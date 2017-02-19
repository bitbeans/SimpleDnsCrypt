using System.Collections.Generic;

namespace SimpleDnsCrypt.Tools
{
	public static class ListHelpers
	{
		public static void AddUnique<T>(this IList<T> self, IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				if (!self.Contains(item))
				{
					self.Add(item);
				}
			}
		}
	}
}
