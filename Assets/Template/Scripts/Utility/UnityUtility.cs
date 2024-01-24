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
		
		public static T FindObjectFromCurrentScene<T>() where T : Component
		{
			var instances = Resources.FindObjectsOfTypeAll<T>();
			foreach (var instance in instances)
			{
				if (instance.gameObject.scene.isLoaded)
				{
					return instance; 
				}
			}
			Debug.LogError($"{typeof(T)} not found in current scene");
			return null;
		}
	}
}
