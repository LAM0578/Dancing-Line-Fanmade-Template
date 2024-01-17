using System;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineSample.Objects
{
	public class ResolutionChangeListener : MonoBehaviour
	{
		[Serializable]
		public class ResolutionChangeEvent : UnityEvent<Resolution> { }
		
#pragma warning disable
		
		[SerializeField] private ResolutionChangeEvent m_OnResolutionChange = new ResolutionChangeEvent();
		
#pragma warning restore
		
		public ResolutionChangeEvent OnResolutionChange => m_OnResolutionChange;
		
		private void OnRectTransformDimensionsChange()
		{
			m_OnResolutionChange?.Invoke(Screen.currentResolution);
		}
	}
}