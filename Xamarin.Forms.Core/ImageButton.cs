using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ImageButtonRenderer))]
	public class ImageButton : View, 
		IImageController, 
		IElementConfiguration<ImageButton>, 
		IBorderElement, 
		IButtonController,
		IImageElement, 
		IButtonElement, 
		IBackgroundView
	{
		const int DefaultBorderRadius = 5;
		const int DefaultCornerRadius = -1;

		public static readonly BindableProperty CommandProperty 
			= BindableProperty.Create(
					nameof(Command), 
					typeof(ICommand), 
					typeof(ImageButton), 
					null, 
					propertyChanging: (bo, o, n) => ((ImageButton)bo).CommandChanging?.Invoke(bo, new BindablePropertyArgs(bo, o, n)),
					propertyChanged: (bo, o, n) => ((ImageButton)bo).CommandChanged?.Invoke(bo, new BindablePropertyArgs(bo, o, n))
				);

		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create("CornerRadius", typeof(int), typeof(Button), defaultValue: DefaultCornerRadius,
			propertyChanged: CornerRadiusPropertyChanged);


		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ImageButton), null,
			propertyChanged: (bindable, oldvalue, newvalue) => ButtonElement.CommandCanExecuteChanged(((ImageButton)bindable), ((ImageButton)bindable)));

		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(Button), -1d);

		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		public static readonly BindableProperty BorderRadiusProperty = 
			BindableProperty.Create(nameof(BorderRadius), typeof(int), typeof(Button), 5,
				propertyChanged: BorderRadiusPropertyChanged);


		public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(ImageButton), default(ImageSource), 
			propertyChanging: OnSourcePropertyChanging, propertyChanged: OnSourcePropertyChanged);

		public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(ImageButton), Aspect.AspectFit);

		public static readonly BindableProperty IsOpaqueProperty = BindableProperty.Create(nameof(IsOpaque), typeof(bool), typeof(ImageButton), false);

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(ImageButton), default(bool));

		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

		readonly Lazy<PlatformConfigurationRegistry<ImageButton>> _platformConfigurationRegistry;

		public event EventHandler Clicked;

		public event EventHandler Pressed;

		public event EventHandler Released;

		public ImageButton()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ImageButton>>(() => new PlatformConfigurationRegistry<ImageButton>(this));
			ImageElement.Initialize(this);
			ButtonElement.Initialize(this, this);
		}

		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		public int BorderRadius
		{
			get { return (int)GetValue(BorderRadiusProperty); }
			set { SetValue(BorderRadiusProperty, value); }
		}
		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		public bool IsLoading
		{
			get { return (bool)GetValue(IsLoadingProperty); }
		}

		public bool IsOpaque
		{
			get { return (bool)GetValue(IsOpaqueProperty); }
			set { SetValue(IsOpaqueProperty, value); }
		}
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		bool IButtonElement.IsEnabledCore
		{
			set { SetValueCore(IsEnabledProperty, value); }
		}


		[TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource Source
		{
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			ImageElement.OnBindingContextChanged(this, this);
			base.OnBindingContextChanged();
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			SizeRequest desiredSize = base.OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
			return ImageElement.Measure(this, desiredSize, widthConstraint, heightConstraint);
		}

		public event EventHandler<BindablePropertyArgs> ImageSourceChanged;
		public event EventHandler<BindablePropertyArgs> ImageSourceChanging;

		static void OnSourcePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((ImageButton)bindable).ImageSourceChanged?.Invoke(bindable, new BindablePropertyArgs(bindable, oldvalue, newvalue));
		}

		static void OnSourcePropertyChanging(BindableObject bindable, object oldvalue, object newvalue)
		{
			((ImageButton)bindable).ImageSourceChanging?.Invoke(bindable, new BindablePropertyArgs(bindable, oldvalue, newvalue));
		}


		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsLoading(bool isLoading)
		{
			SetValue(IsLoadingPropertyKey, isLoading);
		}

		public IPlatformElementConfiguration<T, ImageButton> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
		 

		public event EventHandler<BindablePropertyArgs> CommandChanged;
		public event EventHandler<BindablePropertyArgs> CommandChanging;

		static void BorderRadiusPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			if (newvalue == oldvalue)
				return;

			var val = (int)newvalue;
			if (val == DefaultBorderRadius)
				val = DefaultCornerRadius;

			var oldVal = (int)bindable.GetValue(Button.CornerRadiusProperty);

			if (oldVal == val)
				return;

			bindable.SetValue(Button.CornerRadiusProperty, val);
		}

		static void CornerRadiusPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			if (newvalue == oldvalue)
				return;

			var val = (int)newvalue;
			if (val == DefaultCornerRadius)
				val = DefaultBorderRadius;

#pragma warning disable 0618 // retain until BorderRadiusProperty removed
			var oldVal = (int)bindable.GetValue(Button.BorderRadiusProperty);
#pragma warning restore

			if (oldVal == val)
				return;

#pragma warning disable 0618 // retain until BorderRadiusProperty removed
			bindable.SetValue(Button.BorderRadiusProperty, val);
#pragma warning restore
		}


		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked() =>
			ButtonElement.SendClicked(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed() =>
			ButtonElement.SendPressed(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased() =>
			ButtonElement.SendReleased(this, this);

		public void OnClicked() =>
			Clicked?.Invoke(this, EventArgs.Empty);

		public void OnPressed() =>
			Pressed?.Invoke(this, EventArgs.Empty);

		public void OnReleased() =>
			Released?.Invoke(this, EventArgs.Empty);

		public void RaiseImageSourcePropertyChanged() =>
			OnPropertyChanged(nameof(Source));
		BindableProperty IBackgroundView.CornerRadiusProperty => ImageButton.CornerRadiusProperty;

		BindableProperty IBackgroundView.BorderColorProperty => ImageButton.BorderColorProperty;

		BindableProperty IBackgroundView.BorderWidthProperty => ImageButton.BorderWidthProperty;
	}
}