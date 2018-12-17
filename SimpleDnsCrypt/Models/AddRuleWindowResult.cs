namespace SimpleDnsCrypt.Models
{
	public class AddRuleWindowResult
	{
		public bool Result { get; set; } = false;
		public string RuleKey { get; set; } = string.Empty;
		public string RuleValue { get; set; } = string.Empty;
	}
}
