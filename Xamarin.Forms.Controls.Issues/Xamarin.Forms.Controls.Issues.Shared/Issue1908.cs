using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.TestCasesPages
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1908, "Image reuse", PlatformAffected.Android)]
	public class Issue1908 : ContentPage
	{

		public Issue1908()
		{
			StackLayout listView = new StackLayout();

			for (int i = 0; i < 1000; i++)
			{
				listView.Children.Add(new Image() { Source = "oasis.jpg" });
			}

			Content = new ScrollView() { Content = listView };
		}



		protected override void OnAppearing()
		{

			base.OnAppearing();
		}

	}
}
