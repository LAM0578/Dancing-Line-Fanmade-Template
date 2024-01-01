using UnityEngine;

namespace DancingLineSample.Editing.Utility
{
	public static class ScreenUtility
	{
		public static bool CompareResolution(this Resolution resolution, Resolution other)
		{
			return resolution.width == other.width && resolution.height == other.height;
		}
	}
}