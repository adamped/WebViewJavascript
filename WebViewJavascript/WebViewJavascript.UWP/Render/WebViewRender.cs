using WebViewJavascript;
using WebViewJavascript.UWP.Render;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(WebViewer), typeof(WebViewRender))]
namespace WebViewJavascript.UWP.Render
{
	using Xamarin.Forms;
	using System;

	public class WebViewRender : WebViewRenderer
	{

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			base.OnElementChanged(e);

			if (Control != null && e.NewElement != null)
				InitializeCommands((WebViewer)e.NewElement);

			var oldWebView = e.OldElement as WebViewer;
			if (oldWebView != null)
				oldWebView.EvaluateJavascript = null;

			var newWebView = e.NewElement as WebViewer;
			if (newWebView != null)
				newWebView.EvaluateJavascript = async (js) =>
				{
					try
					{
						return await Control.InvokeScriptAsync("eval", new[] { js });
					}
					catch
					{
						return string.Empty;
					}
				};
		}


		/// <summary>
		/// Will wire up the commands in the WebViewer control to the native method calls
		/// </summary>
		/// <param name="element"></param>
		private void InitializeCommands(WebViewer element)
		{
			element.Refresh = () =>
			{
				Control.Refresh();
			};

			element.GoBack = () =>
			{
				if (Control.CanGoBack)
					Control.GoBack();
			};

			element.CanGoBackFunction = () =>
			{
				return Control.CanGoBack;
			};


		}
	}

}

