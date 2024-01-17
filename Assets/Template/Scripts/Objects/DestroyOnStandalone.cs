using UnityEngine;

namespace DancingLineSample.Objects
{
	public class DestroyOnStandalone : MonoBehaviour
	{
		public bool Enable = true;
		
#if !UNITY_EDITOR

		private void Awake()
		{
			if (!Enable) return;
			Destroy(gameObject);
		}

#endif
	}
}