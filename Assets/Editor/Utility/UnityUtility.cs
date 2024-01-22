using UnityEngine;

public static class UnityUtility
{
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