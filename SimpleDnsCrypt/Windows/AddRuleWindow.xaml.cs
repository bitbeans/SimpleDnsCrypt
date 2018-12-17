using MahApps.Metro.SimpleChildWindow;
using System.Windows;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Windows
{
	public partial class AddRuleWindow : ChildWindow
	{
		public AddRuleWindow()
		{
			InitializeComponent();
		}

		private void AddButtonClick(object sender, RoutedEventArgs e)
		{
			var addRuleWindowResult = new AddRuleWindowResult
			{
				Result = true, RuleKey = RuleKey.Text, RuleValue = RuleValue.Text
			};
			Close(addRuleWindowResult);
		}

		private void AbortButtonClick(object sender, RoutedEventArgs e)
		{
			var addRuleWindowResult = new AddRuleWindowResult {Result = false};
			Close(addRuleWindowResult);
		}
	}
}
