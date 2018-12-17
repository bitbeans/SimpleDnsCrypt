using System.Threading;
using MahApps.Metro.SimpleChildWindow;
using System.Windows;
using MahApps.Metro.IconPacks;
using SimpleDnsCrypt.Helper;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Windows
{
	public enum RuleWindowType
	{
		Cloaking,
		Forwarding
	}

	public partial class AddRuleWindow : ChildWindow
	{
		public AddRuleWindow(RuleWindowType ruleWindowType)
		{
			InitializeComponent();
			if (ruleWindowType == RuleWindowType.Cloaking)
			{
				Title = LocalizationEx.GetUiString("cloaking", Thread.CurrentThread.CurrentCulture);
				RuleHeaderIcon.Kind = PackIconMaterialKind.AccountConvert;
				RuleHeader.Text = LocalizationEx.GetUiString("rule_window_cloaking_header", Thread.CurrentThread.CurrentCulture);
				RuleKeyDescription.Text = LocalizationEx.GetUiString("rule_window_cloaking_key_text", Thread.CurrentThread.CurrentCulture);
				RuleValueDescription.Text = LocalizationEx.GetUiString("rule_window_cloaking_value_text", Thread.CurrentThread.CurrentCulture);
			}
			else
			{
				Title = LocalizationEx.GetUiString("forwarding", Thread.CurrentThread.CurrentCulture);
				RuleHeaderIcon.Kind = PackIconMaterialKind.Radar;
				RuleHeader.Text = LocalizationEx.GetUiString("rule_window_forwarding_header", Thread.CurrentThread.CurrentCulture);
				RuleKeyDescription.Text = LocalizationEx.GetUiString("rule_window_forwarding_key_text", Thread.CurrentThread.CurrentCulture);
				RuleValueDescription.Text = LocalizationEx.GetUiString("rule_window_forwarding_value_text", Thread.CurrentThread.CurrentCulture);
			}
			AddRule.Content = LocalizationEx.GetUiString("rule_window_add", Thread.CurrentThread.CurrentCulture);
			Abort.Content = LocalizationEx.GetUiString("rule_window_abort", Thread.CurrentThread.CurrentCulture);
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
