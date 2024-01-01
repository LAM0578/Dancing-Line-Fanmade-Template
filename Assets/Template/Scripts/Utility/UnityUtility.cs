using UnityEngine;

namespace DancingLineSample.Utility
{
	public static class UnityUtility
	{
		public static void SaveDestroy(Object obj)
		{
			if (!obj) return;
			if (Application.isEditor && !Application.isPlaying)
			{
				Object.DestroyImmediate(obj);
				return;
			}
			Object.Destroy(obj);
		}
	}
}
