using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public abstract class AnimationComponentBase<T> : AnimationBase
	{
		public T TargetObject;

		public abstract override void SetAnimationStatusByTime(float time);
	}
}