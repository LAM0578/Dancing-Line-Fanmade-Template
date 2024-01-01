using System.Collections.Generic;

namespace DancingLineSample.Utility
{
	public static class CollectionUtility
	{
		public static List<T> CopyList<T>(this IEnumerable<T> list)
		{
			return new List<T>(list);
		}
	}
}