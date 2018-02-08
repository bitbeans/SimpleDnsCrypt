using System;
using System.ComponentModel;
using Version = System.Version;

namespace SimpleDnsCrypt.Models
{
	/// <summary>
	///     Outer remote update class.
	/// </summary>
	public class RemoteUpdate
	{
		/// <summary>
		///     Indicates if the requested update can be done.
		/// </summary>
		public bool CanUpdate { get; set; }

		/// <summary>
		///     The update data.
		/// </summary>
		public Update Update { get; set; }
	}

	/// <summary>
	///     Update types.
	/// </summary>
	public enum UpdateType
	{
		/// <summary>
		///     A critical update, which should be done as soon as possible.
		/// </summary>
		[Description("Critical")]
		Critical = 0,

		/// <summary>
		///     A stable update.
		/// </summary>
		[Description("Stable")]
		Stable = 1,

		/// <summary>
		///     A preview version.
		/// </summary>
		[Description("Preview")]
		Preview = 2
	}

	/// <summary>
	///     The update data.
	/// </summary>
	public class Update
	{
		/// <summary>
		///     The available version.
		/// </summary>
		public Version Version { get; set; }

		/// <summary>
		///     The update type.
		/// </summary>
		public UpdateType Type { get; set; }

		/// <summary>
		///     The date of this release.
		/// </summary>
		public DateTime Release { get; set; }

		/// <summary>
		///     The minisign public key to validate the installer.
		/// </summary>
		public string Publickey { get; set; }

		/// <summary>
		///     The installer object.
		/// </summary>
		public Installer Installer { get; set; }

		/// <summary>
		///     The signature object.
		/// </summary>
		public Signature Signature { get; set; }
	}

	/// <summary>
	///     The installer information.
	/// </summary>
	public class Installer
	{
		/// <summary>
		///     Name of the installer file.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///     Uri to download the installer file.
		/// </summary>
		public Uri Uri { get; set; }
	}

	/// <summary>
	///     The signature information.
	/// </summary>
	public class Signature
	{
		/// <summary>
		///     Uri to download the signature.
		/// </summary>
		public Uri Uri { get; set; }
	}
}
