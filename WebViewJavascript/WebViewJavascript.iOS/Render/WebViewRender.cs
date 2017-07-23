using WebViewJavascript.iOS.Render;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(WebViewer), typeof(WebViewRender))]
namespace WebViewJavascript.iOS.Render
{
	using UIKit;
	using Xamarin.Forms.Platform.iOS;

	public class WebViewRender : WebViewRenderer
	{

		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			if (NativeView != null && e.NewElement != null)
				InitializeCommands((WebViewer)e.NewElement);

			var webView = e.NewElement as WebViewer;
			if (webView != null)
				webView.EvaluateJavascript = (js) =>
				{
					return Task.FromResult(this.EvaluateJavascript(js));
				};
		}

		private void InitializeCommands(WebViewer element)
		{
			element.RefreshCommand = new Command(() =>
			{
				((UIWebView)NativeView).Reload();
			});

			element.GoBackCommand = new Command(() =>
			{
				var control = ((UIWebView)NativeView);
				if (control.CanGoBack)
				{
					element.IsBackNavigating = true;
					control.GoBack();
				}
			});

			element.CanGoBackFunction = () =>
			{
				return ((UIWebView)NativeView).CanGoBack;
			};

			var ctl = ((UIWebView)NativeView);

			ctl.ScalesPageToFit = true;

		}

	}

}
