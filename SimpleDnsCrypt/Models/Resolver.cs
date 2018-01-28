namespace SimpleDnsCrypt.Models
{
	public class Resolver
	{
		public string Group { get; set; }
		public string Name { get; set; }
		public string Comment { get; set; }
		public Stamp Stamp { get; set; }

		public bool IsInServerList { get; set; }
	}
}
