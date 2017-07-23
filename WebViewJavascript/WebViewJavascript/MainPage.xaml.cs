using Xamarin.Forms;

namespace WebViewJavascript
{
	public partial class MainPage: ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			BindingContext = new MainPageViewModel();
		}
	}
}
