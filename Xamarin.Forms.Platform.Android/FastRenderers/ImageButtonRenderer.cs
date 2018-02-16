using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using AImageButton = Android.Widget.ImageButton;
using AView = Android.Views.View;
using Android.Views;
using Xamarin.Forms.Internals;
using AMotionEventActions = Android.Views.MotionEventActions;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	internal sealed class ImageButtonRenderer : 
		AppCompatImageButton, 
		IVisualElementRenderer, 
		IImageRendererController, 
		AView.IOnFocusChangeListener, 
		//IEffectControlProvider, 
		AView.IOnClickListener, 
		AView.IOnTouchListener
	{
		bool _disposed; 
		bool _skipInvalidate;
		int? _defaultLabelFor;
		VisualElementTracker _tracker;
		//VisualElementRenderer _visualElementRenderer;
		private BorderBackgroundTracker _backgroundTracker;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		protected override void Dispose(bool disposing)
		{			
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}

				if (Button != null)
				{
					Button.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.GetRenderer(Button) == this)
						Button.ClearValue(Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		public override void Invalidate()
		{
			if (_skipInvalidate)
			{
				_skipInvalidate = false;
				return;
			}

			base.Invalidate();
		}



		async void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
		{
			if (e.OldElement != null)
			{
				_backgroundTracker?.Reset();
			}

			this.EnsureId();

			// Image
			await TryUpdateBitmap(e.OldElement);
			UpdateAspect();

			//UpdateBitmap();
			//UpdateIsEnabled();
			//UpdateInputTransparent();
			UpdateBackgroundColor();
			UpdateDrawable();


			ElevationHelper.SetElevation(this, e.NewElement);
			// Image


			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}




		Size MinimumSize()
		{
			return new Size();
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			if (_disposed)
			{
				return new SizeRequest();
			}

			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), MinimumSize());
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}
			
			if (!(element is ImageButton image))
			{
				throw new ArgumentException("Element is not of type " + typeof(ImageButton), nameof(element));
			}

			ImageButton oldElement = Button;
			Button = image;

			var reference = Guid.NewGuid().ToString();
			Performance.Start(reference);

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (_backgroundTracker == null)
				_backgroundTracker = new BorderBackgroundTracker(Button, this);
			else
				_backgroundTracker.Button = Button;

			Color currentColor = oldElement?.BackgroundColor ?? Color.Default;
			if (element.BackgroundColor != currentColor)
			{
				UpdateBackgroundColor();
			}


			element.PropertyChanged += OnElementPropertyChanged;

			if (_tracker == null)
			{
				_tracker = new VisualElementTracker(this);
			}

			Performance.Stop(reference);

			OnElementChanged(new ElementChangedEventArgs<ImageButton>(oldElement, Button));

			Button?.SendViewInitialized(Control);
		}

		// Automation properties
		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = LabelFor;

			LabelFor = (int)(id ?? _defaultLabelFor);
		}
		// Automation properties



		void IVisualElementRenderer.UpdateLayout() => _tracker?.UpdateLayout();

		VisualElement IVisualElementRenderer.Element => Button;
		AView IVisualElementRenderer.View => this;
		ViewGroup IVisualElementRenderer.ViewGroup => null;
		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

		ImageButton Button { get; set; }



		void IImageRendererController.SkipInvalidate() => _skipInvalidate = true;

		AppCompatImageButton Control => this;

		public ImageButtonRenderer(Context context) : base(context)
		{
			SoundEffectsEnabled = false;
			SetOnClickListener(this);
			SetOnTouchListener(this);
			OnFocusChangeListener = this;
			Tag = this;
		}

		public void UpdateBackgroundColor()
		{
			UpdateDrawable();
		}
		void UpdateDrawable()
		{
			_backgroundTracker?.UpdateDrawable();
		}

		// Image related
		async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// General update blocks
			if (e.PropertyName == ImageButton.SourceProperty.PropertyName)
				await TryUpdateBitmap();
			else if (e.PropertyName == ImageButton.AspectProperty.PropertyName)
				UpdateAspect();
			// General update blocks

			ElementPropertyChanged?.Invoke(this, e);
		}

		async Task TryUpdateBitmap(ImageButton previous = null)
		{
			// By default we'll just catch and log any exceptions thrown by UpdateBitmap so they don't bring down
			// the application; a custom renderer can override this method and handle exceptions from
			// UpdateBitmap differently if it wants to

			try
			{
				await UpdateBitmap(previous);
			}
			catch (Exception ex)
			{
				Log.Warning(nameof(ImageRenderer), "Error loading image: {0}", ex);
			}
			finally
			{
				((IImageController)Button)?.SetIsLoading(false);
			}
		}

		async Task UpdateBitmap(ImageButton previous = null)
		{
			if (Button == null || _disposed)
			{
				return;
			}

			await Control.UpdateBitmap(Button, Button.Source,  previous, previous?.Source);
		}

		void UpdateAspect()
		{
			if (Button == null || _disposed)
			{
				return;
			}

			ScaleType type = Button.Aspect.ToScaleType();
			SetScaleType(type);
		}
		// Image related



		// general state related
		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus)
		{
			((IElementController)Button).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);
		}
		// general state related


		// Button related
		void IOnClickListener.OnClick(AView v)
		{
			((IButtonController)Button)?.SendClicked();
		}

		bool IOnTouchListener.OnTouch(AView v, MotionEvent e)
		{
			var buttonController = Button as IButtonController;
			switch (e.Action)
			{
				case AMotionEventActions.Down:
					buttonController?.SendPressed();
					break;
				case AMotionEventActions.Up:
					buttonController?.SendReleased();
					break;
			}

			return false;
		}
		// Button related
	}
}
