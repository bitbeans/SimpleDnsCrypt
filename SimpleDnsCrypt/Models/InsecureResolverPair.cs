using Caliburn.Micro;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace SimpleDnsCrypt.Models
{
	public class InsecureResolverPair : PropertyChangedBase
	{
		private bool _isSelected;
		private string _name;
		private string _description;
		private List<string> _addresses;
		private int _id;

		[YamlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		[YamlIgnore]
		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (_isSelected == value) return;
				_isSelected = value;
				NotifyOfPropertyChange(() => IsSelected);
			}
		}

		public int Id
		{
			get => _id;
			set
			{
				if (_id == value) return;
				_id = value;
				NotifyOfPropertyChange(() => Id);
			}
		}

		public string Name
		{
			get => _name;
			set
			{
				if (_name == value) return;
				_name = value;
				NotifyOfPropertyChange(() => Name);
			}
		}

		public string Description
		{
			get => _description;
			set
			{
				if (_description == value) return;
				_description = value;
				NotifyOfPropertyChange(() => Description);
			}
		}

		public List<string> Addresses
		{
			get => _addresses;
			set
			{
				if (_addresses == value) return;
				_addresses = value;
				NotifyOfPropertyChange(() => Addresses);
			}
		}

		[YamlIgnore]
		public string AddressesString => _addresses != null && _addresses?.Count > 1 ? string.Join(", ", _addresses) : "-";
	}
}
