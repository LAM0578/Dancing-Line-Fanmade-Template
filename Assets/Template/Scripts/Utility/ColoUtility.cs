using UnityEngine;

namespace DancingLineSample.Utility
{
	public static class ColoUtility
	{
		public static Color WithAlpha(this Color col, float a)
		{
			return new Color(col.r, col.g, col.b, a);
		}
		
		public static Color WithAlpha(this Color col, byte a)
		{
			Color32 ret = col;
			ret.a = a;
			return ret;
		}
	}
}