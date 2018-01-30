using Caliburn.Micro;

namespace SimpleDnsCrypt.Models
{
	public class Resolver : PropertyChangedBase
	{
		private Stamp _stamp;
		private bool _isInServerList;
		private string _comment;
		private string _name;
		private string _group;

		public string Group
		{
			get => _group;
			set
			{
				_group = value;
				NotifyOfPropertyChange(() => Group);
			}
		}

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				NotifyOfPropertyChange(() => Name);
			}
		}

		public string Comment
		{
			get => _comment;
			set
			{
				_comment = value;
				NotifyOfPropertyChange(() => Comment);
			}
		}

		public bool IsInServerList
		{
			get => _isInServerList;
			set
			{
				_isInServerList = value;
				NotifyOfPropertyChange(() => IsInServerList);
			}
		}

		public Stamp Stamp
		{
			get => _stamp;
			set
			{
				_stamp = value;
				NotifyOfPropertyChange(() => Stamp);
			}
		}
	}
}
