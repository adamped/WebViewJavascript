using Xamarin.Forms;
using WebViewJavascript;
using WebViewJavascript.Droid.Render;

[assembly: ExportRenderer(typeof(WebViewer), typeof(WebViewRender))]
namespace WebViewJavascript.Droid.Render
{

	using Android.OS;
	using System;
	using Xamarin.Forms.Platform.Android;
	using Android.Content;
	using Android.Webkit;
	using System.Threading;
	using System.Threading.Tasks;

	public class WebViewRender : WebViewRenderer
	{

		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
		{
			base.OnElementChanged(e);

			var oldWebView = e.OldElement as WebViewer;
			if (oldWebView != null)
				oldWebView.EvaluateJavascript = null;

			var newWebView = e.NewElement as WebViewer;
			if (newWebView != null)
				newWebView.EvaluateJavascript = async (js) =>
				{

					ManualResetEvent reset = new ManualResetEvent(false);
					var response = "";
					Device.BeginInvokeOnMainThread(() =>
					{
						System.Diagnostics.Debug.WriteLine("Javascript Send: " + js);
						Control?.EvaluateJavascript(js, new JavascriptCallback((r) => { response = r; reset.Set(); }));
					});
					await Task.Run(() => { reset.WaitOne(); });
					if (response == "null")
						response = string.Empty;

					return response;
				};

			if (Control != null && e.NewElement != null)
			{
				InitializeCommands((WebViewer)e.NewElement);
				SetupControl();
			}
		}

		/// <summary>
		/// Sets up various settings for the Android WebView
		/// </summary>
		private void SetupControl()
		{
			// Ensure common functionality is enabled
			Control.Settings.DomStorageEnabled = true;
			Control.Settings.JavaScriptEnabled = true;

			// Must remove minimum font size otherwise SAP PDF's go massive
			Control.Settings.MinimumFontSize = 0;

			// Because Android 4.4 and below doesn't respect ViewPort in HTML
			if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
				Control.Settings.UseWideViewPort = true;

		}

		/// <summary>
		/// Will wire up the commands in the WebViewer control to the native method calls
		/// </summary>
		/// <param name="element"></param>
		private void InitializeCommands(WebViewer element)
		{

			element.RefreshCommand = new Command(() =>
			{
				Control?.Reload();
			});

			element.GoBackCommand = new Command(() =>
			{
				var ctrl = Control;
				if (ctrl == null)
					return;

				if (ctrl.CanGoBack())
					ctrl.GoBack();
			});

			element.CanGoBackFunction = () =>
			{
				var ctrl = Control;
				if (ctrl == null)
					return false;

				return ctrl.CanGoBack();
			};

			// This allows you to show a file chooser dialog from the WebView
			Control.SetWebChromeClient(new WebViewChromeClient((uploadMsg, acceptType, capture) =>
			{
				MainActivity.UploadMessage = uploadMsg;
				if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
				{
					var i = new Intent(Intent.ActionGetContent);

					//To set all type of files
					i.SetType("image/*");

					//Here File Chooser dialog is started as Activity, and it gives result while coming back from that Activity.
					((MainActivity)this.Context).StartActivityForResult(Intent.CreateChooser(i, "File Chooser"), MainActivity.FILECHOOSER_RESULTCODE);
				}
				else
				{
					var i = new Intent(Intent.ActionOpenDocument);
					i.AddCategory(Intent.CategoryOpenable);

					//To set all image file types. You can change to whatever you need
					i.SetType("image/*");

					//Here File Chooser dialog is started as Activity, and it gives result while coming back from that Activity.
					((MainActivity)this.Context).StartActivityForResult(Intent.CreateChooser(i, "File Chooser"), MainActivity.FILECHOOSER_RESULTCODE);
				}
			}));

		}

	}

	internal class JavascriptCallback : Java.Lang.Object, IValueCallback
	{
		public JavascriptCallback(Action<string> callback)
		{
			_callback = callback;
		}

		private Action<string> _callback;
		public void OnReceiveValue(Java.Lang.Object value)
		{
			System.Diagnostics.Debug.WriteLine("Javascript Return: " + Convert.ToString(value));
			_callback?.Invoke(Convert.ToString(value));
		}
	}

	public class WebViewChromeClient : WebChromeClient
	{

		Action<IValueCallback, Java.Lang.String, Java.Lang.String> callback;

		public WebViewChromeClient(Action<IValueCallback, Java.Lang.String, Java.Lang.String> callback)
		{
			this.callback = callback;
		}

		//For Android 4.1+
		[Java.Interop.Export]
		public void openFileChooser(IValueCallback uploadMsg, Java.Lang.String acceptType, Java.Lang.String capture)
		{
			callback(uploadMsg, acceptType, capture);
		}

		// For Android 5.0+
		public override bool OnShowFileChooser(WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
		{
			return base.OnShowFileChooser(webView, filePathCallback, fileChooserParams);
		}
	}

}