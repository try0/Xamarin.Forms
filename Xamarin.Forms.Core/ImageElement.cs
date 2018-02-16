using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	public interface IImageElement
	{
		Aspect Aspect { get; set; }
		ImageSource Source { get; set; }
		bool IsOpaque { get; set; }

		event EventHandler<BindablePropertyArgs> ImageSourceChanged;
		event EventHandler<BindablePropertyArgs> ImageSourceChanging;

		void RaiseImageSourcePropertyChanged();
	}

	internal static class ImageElement
	{
		public static SizeRequest Measure(
			IImageElement imageElement,
			SizeRequest desiredSize, 
			double widthConstraint, 
			double heightConstraint)
		{ 
			double desiredAspect = desiredSize.Request.Width / desiredSize.Request.Height;
			double constraintAspect = widthConstraint / heightConstraint;

			double desiredWidth = desiredSize.Request.Width;
			double desiredHeight = desiredSize.Request.Height;

			if (desiredWidth == 0 || desiredHeight == 0)
				return new SizeRequest(new Size(0, 0));

			double width = desiredWidth;
			double height = desiredHeight;
			if (constraintAspect > desiredAspect)
			{
				// constraint area is proportionally wider than image
				switch (imageElement.Aspect)
				{
					case Aspect.AspectFit:
					case Aspect.AspectFill:
						height = Math.Min(desiredHeight, heightConstraint);
						width = desiredWidth * (height / desiredHeight);
						break;
					case Aspect.Fill:
						width = Math.Min(desiredWidth, widthConstraint);
						height = desiredHeight * (width / desiredWidth);
						break;
				}
			}
			else if (constraintAspect < desiredAspect)
			{
				// constraint area is proportionally taller than image
				switch (imageElement.Aspect)
				{
					case Aspect.AspectFit:
					case Aspect.AspectFill:
						width = Math.Min(desiredWidth, widthConstraint);
						height = desiredHeight * (width / desiredWidth);
						break;
					case Aspect.Fill:
						height = Math.Min(desiredHeight, heightConstraint);
						width = desiredWidth * (height / desiredHeight);
						break;
				}
			}
			else
			{
				// constraint area is same aspect as image
				width = Math.Min(desiredWidth, widthConstraint);
				height = desiredHeight * (width / desiredWidth);
			}

			return new SizeRequest(new Size(width, height));
		}

		internal static void OnBindingContextChanged(IImageElement image, VisualElement visualElement)
		{
			if (image.Source != null)
				BindableObject.SetInheritedBindingContext(image.Source, visualElement?.BindingContext);
		}


		public static async void ImageSourceChanging(
			object sender, 
			BindablePropertyArgs eventArgs,
			EventHandler onSourceChanged)
		{
			var oldvalue = (ImageSource)eventArgs.OldValue;
			if (oldvalue == null)
				return;

			oldvalue.SourceChanged -= onSourceChanged;
			try
			{
				await oldvalue.Cancel().ConfigureAwait(false);
			}
			catch (ObjectDisposedException)
			{
				// Workaround bugzilla 37792 https://bugzilla.xamarin.com/show_bug.cgi?id=37792
			}
		}

		public static void ImageSourceChanged(
			object sender,
			BindablePropertyArgs eventArgs,
			EventHandler onSourceChanged)
		{
			var newvalue = (ImageSource)eventArgs.NewValue;
			var visualElement = (VisualElement)eventArgs.Owner;
			if (newvalue != null)
			{
				newvalue.SourceChanged += onSourceChanged;
				BindableObject.SetInheritedBindingContext(newvalue, visualElement?.BindingContext);
			}

			visualElement?.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		public static void Initialize(IImageElement image)
		{
			// work on closures 
			EventHandler onSourceChanged = (object sender, EventArgs eventArgs) =>
			{
				image.RaiseImageSourcePropertyChanged();
				// move concept to interface
				((VisualElement)image).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			};

			image.ImageSourceChanging += (x,y) => ImageSourceChanging(x, y, onSourceChanged);
			image.ImageSourceChanged += (x, y) => ImageSourceChanged(x, y, onSourceChanged);
		}
	}
}