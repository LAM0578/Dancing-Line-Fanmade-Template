using DG.Tweening;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public class TimeAnimationBase : AnimationBase, ITimeAnimation
	{
		[Tooltip("触发时间 (ms)")] public int TriggerTime;
		
		public override void SetAnimationStatusByTime(float time)
		{
			_elapsedTime = time - TriggerTime / 1000f;
			if (_elapsedTime < 0) _elapsedTime = float.NegativeInfinity;
			OnSetAnimationStatusByTime(_elapsedTime);
		}
		
		protected override void OnActiveAnimation() { }
		protected override void OnResetAnimation() { }
		
		public override void Pause() { }
		public override void Continue() { }
	}
}