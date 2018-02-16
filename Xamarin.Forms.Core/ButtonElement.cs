using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Xamarin.Forms
{

	internal interface IButtonElement
	{
		object CommandParameter { get; set; }
		ICommand Command { get; set; }
		event EventHandler<BindablePropertyArgs> CommandChanged;
		event EventHandler<BindablePropertyArgs> CommandChanging;
		bool IsEnabledCore { set; }

		void OnClicked();
		void OnPressed();
		void OnReleased();
	}
	
	// fix closures
    internal static class ButtonElement
    {

		public static void CommandCanExecuteChanged(VisualElement visual, IButtonElement buttonElement)
		{
			ICommand cmd = buttonElement.Command;
			if (cmd != null)
				buttonElement.IsEnabledCore = cmd.CanExecute(buttonElement.CommandParameter);
		}

		public static void Initialize(VisualElement visualElement, IButtonElement buttonElement)
		{
			EventHandler CommandCanExecuteChangedHandler = (_, __) =>
			{
				CommandCanExecuteChanged(visualElement, buttonElement);
			};


			buttonElement.CommandChanged += (_, __) =>
			{
				if (buttonElement.Command != null)
				{
					buttonElement.Command.CanExecuteChanged += CommandCanExecuteChangedHandler;
					CommandCanExecuteChanged(visualElement, buttonElement);
				}
				else
					buttonElement.IsEnabledCore = true;

			};

			buttonElement.CommandChanging += (_, __) =>
			{
				ICommand cmd = buttonElement.Command;
				if (cmd != null)
					cmd.CanExecuteChanged -= CommandCanExecuteChangedHandler;

			};
		}
		 
		public static void SendClicked(VisualElement visualElement, IButtonElement buttonElement)
		{
			if (visualElement.IsEnabled == true)
			{
				buttonElement.Command?.Execute(buttonElement.CommandParameter);
				buttonElement.OnClicked();
			}
		}
		 
		public static void SendPressed(VisualElement visualElement, IButtonElement buttonElement)
		{
			if (visualElement.IsEnabled == true)
			{
				buttonElement.OnPressed();
			}
		}
		 
		public static void SendReleased(VisualElement visualElement, IButtonElement buttonElement)
		{
			if (visualElement.IsEnabled == true)
			{
				buttonElement.OnReleased();
			}
		}


	}
}
