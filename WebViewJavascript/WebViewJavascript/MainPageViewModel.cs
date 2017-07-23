using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace WebViewJavascript
{
	public class MainPageViewModel : BindableObject
	{

		public Func<string, Task<string>> EvaluateJavascript { get; set; }
		public Action GoBack { get; set; }
		public Action Refresh { get; set; }

		public ICommand GoBackCommand
		{
			get
			{
				return new Command(() => { GoBack(); });
			}
		}

		public ICommand RefreshCommand
		{
			get
			{
				return new Command(() => { Refresh(); });
			}
		}

		public ICommand EvalJS
		{
			get
			{
				return new Command(async () =>
				{
					var result = await EvaluateJavascript("document.getElementById('html');");
				});
			}
		}

	}
}
