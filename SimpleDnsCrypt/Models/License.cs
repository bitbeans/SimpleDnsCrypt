using System;

namespace SimpleDnsCrypt.Models
{
	/// <summary>
	///     Object to represent a license in the license view.
	/// </summary>
	public class License
	{
		/// <summary>
		///     Visible header Text.
		/// </summary>
		public string LicenseHeaderText { get; set; }

		/// <summary>
		///     Link to the code (github).
		/// </summary>
		public LicenseLink LicenseCodeLink { get; set; }

		/// <summary>
		///     Link to publisher website.
		/// </summary>
		public LicenseLink LicenseRegularLink { get; set; }

		/// <summary>
		///     The license text.
		/// </summary>
		public string LicenseText { get; set; }
	}

	/// <summary>
	///     A license link inside the a license.
	/// </summary>
	public class LicenseLink
	{
		/// <summary>
		///     Visible Text.
		/// </summary>
		public string LinkText { get; set; }

		/// <summary>
		///     Uri to browse.
		/// </summary>
		public Uri LinkUri { get; set; }
	}
}
