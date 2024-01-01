using System;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public abstract class AnimationBase : MonoBehaviour, IAnimation
	{
		/// <summary>
		/// 表示该动画已被激活 (播放) 过
		/// </summary>
		public bool Actived { get; protected set; }

		protected float _elapsedTime = float.NegativeInfinity;
		protected Tween _tween;
		
		[Button("Preview Animation")]
		public void PreviewAnimation()
		{
			OnResetAnimation();
			OnActiveAnimation();
		}

		/// <summary>
		/// 激活 (播放) 动画
		/// </summary>
		public void ActiveAnimation()
		{
			if (!float.IsNegativeInfinity(_elapsedTime))
			{
				ContinueByElapsedTime();
				return;
			}
			OnActiveAnimation();
			Actived = true;
		}

		/// <summary>
		/// 重置动画 
		/// </summary>
		public void ResetAnimation()
		{
			_elapsedTime = float.NegativeInfinity;
			OnResetAnimation();
			Actived = false;
		}

		/// <summary>
		/// 强制结束动画 (更改被动画对象为结束状态)
		/// </summary>
		[Obsolete("Use AnimationBase.SetAnimationStatusByTime() instead.")]
		public void FinishAnimation()
		{
			OnFinishAnimation();
			Actived = true;
		}

		/// <summary>
		/// 根据时间设置动画状态
		/// </summary>
		/// <param name="time"></param>
		public abstract void SetAnimationStatusByTime(float time);
		
		/// <summary>
		/// 在当前经过的动画时间继续
		/// </summary>
		public void ContinueByElapsedTime()
		{
			if (Actived) return;
			OnContinueByElapsedTime();
			Actived = true;
		}

		/// <summary>
		/// 在激活 (播放) 动画时调用
		/// </summary>
		protected virtual void OnActiveAnimation() { }
		/// <summary>
		/// 在重置动画时调用
		/// </summary>
		protected virtual void OnResetAnimation() { }
		/// <summary>
		/// 在强制结束动画时调用
		/// </summary>
		protected virtual void OnFinishAnimation() { }
		/// <summary>
		/// 在根据时间设置动画状态时调用
		/// </summary>
		/// <param name="time"></param>
		protected virtual void OnSetAnimationStatusByTime(float time) { }
		/// <summary>
		/// 在当前经过的动画时间继续时调用
		/// </summary>
		protected virtual void OnContinueByElapsedTime() { }
		
		
		/// <summary>
		/// 暂停动画
		/// </summary>
		public virtual void Pause() { }
		/// <summary>
		/// 继续动画
		/// </summary>
		public virtual void Continue() { }
	}
}