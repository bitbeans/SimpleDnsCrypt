using Caliburn.Micro;

namespace SimpleDnsCrypt.Models
{
	public class BlacklistRule : PropertyChangedBase
	{
		private string _content;

		public string Content
		{
			get => _content;
			set
			{
				if (value.Equals(_content)) return;
				_content = value;
				NotifyOfPropertyChange(() => Content);
			}
		}
	}
}
