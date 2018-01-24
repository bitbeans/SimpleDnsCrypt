namespace SimpleDnsCrypt.Models
{
	public class Language
	{
		/// <summary>
		///		The name of the language.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		///		Example: en
		/// </summary>
		public string ShortCode { get; set; }
		/// <summary>
		///     Example: en-US
		/// </summary>
		public string CultureCode { get; set; }
	}
}
