using System.ComponentModel;
using DG.Tweening;

namespace DancingLineSample.Gameplay.Animation
{
	public class TriggerAnimationComponentBase<T> : AnimationComponentBase<T>, ITriggerAnimation 
	{
		/// <summary>
		/// 该动画的触发时间
		/// </summary>
		public int TriggerTime { get; private set; }
		
		public override void SetAnimationStatusByTime(float time)
		{
			_elapsedTime = time - TriggerTime / 1000f;
			if (_elapsedTime < 0) _elapsedTime = float.NegativeInfinity;
			OnSetAnimationStatusByTime(_elapsedTime);
		}

		/// <summary>
		/// 重置动画 
		/// </summary>
		public void ResetAnimation(bool onCheckPoint)
		{
			int triggerTime = onCheckPoint ? TriggerTime : 0;
			OnResetAnimation();
			TriggerTime = triggerTime;
			Actived = false;
		}

		protected override void OnActiveAnimation()
		{
			if (GameplayManager.Instance.LineStatus != PlayerStatus.Playing)
				return;
			TriggerTime = GameplayManager.Instance.CurrentTiming;
		}

		protected override void OnResetAnimation()
		{
			_elapsedTime = 0;
			TriggerTime = 0;
		}
	}
}