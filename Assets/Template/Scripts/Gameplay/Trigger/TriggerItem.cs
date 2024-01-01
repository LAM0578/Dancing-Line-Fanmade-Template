using System;
using DG.Tweening;
using UnityEngine;

namespace DancingLineSample.Gameplay.Trigger
{
	[Serializable]
	public class TriggerItem<T>
	{
		public bool Enable;
		[Space]
		public T Value;
		public float Duration;
		public Ease Easing = Ease.Linear;
		public bool IsAdded;
	}
		
	[Serializable] public class TriggerItemVector3 : TriggerItem<Vector3> { }
	[Serializable] public class TriggerItemFloat : TriggerItem<float> { }
}