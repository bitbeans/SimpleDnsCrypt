using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SimpleDnsCrypt.Helper
{
	/// <summary>
	///     Class to make inline URLs clickable (UI).
	/// </summary>
	public static class NavigationService
	{
		/// <summary>
		///     Copied from http://geekswithblogs.net/casualjim/archive/2005/12/01/61722.aspx
		/// </summary>
		private static readonly Regex ReUrl =
			new Regex(
				@"(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~/|/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|de|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?");

		public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
			"Text",
			typeof(string),
			typeof(NavigationService),
			new PropertyMetadata(null, OnTextChanged)
			);

		public static string GetText(DependencyObject d)
		{
			return d.GetValue(TextProperty) as string;
		}

		public static void SetText(DependencyObject d, string value)
		{
			d.SetValue(TextProperty, value);
		}

		private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var textBlock = d as TextBlock;
			if (textBlock == null)
				return;

			textBlock.Inlines.Clear();

			var newText = (string)e.NewValue;
			if (string.IsNullOrEmpty(newText))
				return;

			// Find all URLs using a regular expression
			var lastPos = 0;
			foreach (Match match in ReUrl.Matches(newText))
			{
				// Copy raw string from the last position up to the match
				if (match.Index != lastPos)
				{
					var rawText = newText.Substring(lastPos, match.Index - lastPos);
					textBlock.Inlines.Add(new Run(rawText));
				}

				// Create a hyperlink for the match
				var hyperLink = new ResourceDictionary
				{
					Source = new Uri("/SimpleDnsCrypt;component/Styles/HyperLink.xaml", UriKind.RelativeOrAbsolute)
				};

				var linkHyperLinkStyle = hyperLink["LinkHyperLinkStyle"] as Style;

				var link = new Hyperlink(new Run(match.Value))
				{
					NavigateUri = new Uri(match.Value),
					Style = linkHyperLinkStyle
				};

				link.Click += OnUrlClick;
				textBlock.Inlines.Add(link);

				// Update the last matched position
				lastPos = match.Index + match.Length;
			}

			// Finally, copy the remainder of the string
			if (lastPos < newText.Length)
				textBlock.Inlines.Add(new Run(newText.Substring(lastPos)));
		}

		private static void OnUrlClick(object sender, RoutedEventArgs e)
		{
			var link = (Hyperlink)sender;
			// Do something with link.NavigateUri like:
			Process.Start(link.NavigateUri.ToString());
		}
	}
}
