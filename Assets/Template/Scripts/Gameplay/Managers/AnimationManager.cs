using System;
using System.Collections.Generic;
using System.Linq;
using DancingLineSample.Gameplay.Animation;
using DancingLineSample.Utility;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;

namespace DancingLineSample.Gameplay
{
	public class AnimationManager : Singleton<AnimationManager>
	{
		private List<TimeAnimationComponentBase<Transform>> _transformAnimations = new List<TimeAnimationComponentBase<Transform>>();
		private List<TimeAnimationComponentBase<Material>> _materialAnimations = new List<TimeAnimationComponentBase<Material>>();

		private List<TriggerAnimationComponentBase<Transform>> _transformAnimationTriggers = new List<TriggerAnimationComponentBase<Transform>>();
		private List<TriggerAnimationComponentBase<Material>> _materialAnimationTriggers = new List<TriggerAnimationComponentBase<Material>>();

		private List<TimeAnimationBase> _timeAnimations = new List<TimeAnimationBase>();
		private List<TriggerAnimationBase> _triggerAnimations = new List<TriggerAnimationBase>();
		
		protected override void OnAwake()
		{
			_transformAnimations = FindObjectsOfType<TimeAnimationComponentBase<Transform>>().ToList();
			// 这里使用了一个 OrderBy 是保证按顺序重置 (避免多个动画对象重置同一个材质球时的问题) 
			_materialAnimations = FindObjectsOfType<TimeAnimationComponentBase<Material>>()
				.OrderBy(t => t.TriggerTime)
				.ToList();
			
			_transformAnimationTriggers = FindObjectsOfType<TriggerAnimationComponentBase<Transform>>().ToList();
			_materialAnimationTriggers = FindObjectsOfType<TriggerAnimationComponentBase<Material>>().ToList();

			// 这里使用了一个 OrderBy 是保证按顺序重置 (避免多个动画对象重置雾设置的问题)
			_timeAnimations = FindObjectsOfType<TimeAnimationBase>()
				.OrderBy(t => t.TriggerTime)
				.ToList();
			_triggerAnimations = FindObjectsOfType<TriggerAnimationBase>().ToList();
			
			DOTween.SetTweensCapacity(32767, 32767);
		}

		private void Update() 
		{
			if (GameplayManager.Instance.LineStatus != PlayerStatus.Playing) return;
			int curTiming = GameplayManager.Instance.CurrentTiming;
			foreach (var anim in _transformAnimations)
			{
				if (anim.Actived || curTiming < anim.TriggerTime) continue;
				anim.ActiveAnimation();
			}
			foreach (var anim in _materialAnimations)
			{
				if (anim.Actived || curTiming < anim.TriggerTime) continue;
				anim.ActiveAnimation();
			}
			foreach (var anim in _timeAnimations)
			{
				if (anim.Actived || curTiming < anim.TriggerTime) continue;
				anim.ActiveAnimation();
			}
		}

		/// <summary>
		/// 重置动画
		/// </summary>
		/// <param name="onCheckpoint">是否为检查点触发重置：用于确定是否重置触发器的触发时间</param>
		/// <param name="reverse">用于拷贝一个新的动画对象列表用于反转</param>
		public void ResetAnimations(bool onCheckpoint, bool reverse)
		{
			// transform & material
			
			var transformAnimations = _transformAnimations.CopyList();
			if (reverse) transformAnimations.Reverse();
			
			var materialAnimations = _materialAnimations.CopyList();
			if (reverse) materialAnimations.Reverse();
			
			foreach (var anim in 
			         transformAnimations)
				anim.ResetAnimation();
			foreach (var anim in 
			         materialAnimations)
				anim.ResetAnimation();
			
			// transform & material (trigger)
			
			var transformAnimationTriggers = _transformAnimationTriggers.CopyList();
			if (reverse) transformAnimationTriggers.Reverse();
			
			var materialAnimationTriggers = _materialAnimationTriggers.CopyList();
			if (reverse) materialAnimationTriggers.Reverse();
			
			foreach (var trigger in transformAnimationTriggers)
				trigger.ResetAnimation(onCheckpoint);
			foreach (var trigger in materialAnimationTriggers)
				trigger.ResetAnimation(onCheckpoint);
			
			// default animation components
			
			var timeAnimations = _timeAnimations.CopyList();
			if (reverse) timeAnimations.Reverse();
			
			var triggerAnimations = _triggerAnimations.CopyList();
			if (reverse) triggerAnimations.Reverse();
			
			foreach (var anim in timeAnimations)
				anim.ResetAnimation();
			foreach (var trigger in triggerAnimations)
				trigger.ResetAnimation(onCheckpoint);
		}

		/// <summary>
		/// 根据时间设置动画状态
		/// </summary>
		/// <param name="timing">时间 (ms)</param>
		public void SetAnimationStatusByTiming(int timing)
		{
			var fTiming = timing / 1000f;
			
			foreach (var anim in 
			         _transformAnimations)
				anim.SetAnimationStatusByTime(fTiming);
			foreach (var anim in 
			         _materialAnimations)
				anim.SetAnimationStatusByTime(fTiming);
			
			foreach (var trigger in _transformAnimationTriggers)
				trigger.SetAnimationStatusByTime(fTiming);
			foreach (var trigger in _materialAnimationTriggers)
				trigger.SetAnimationStatusByTime(fTiming);
			
			foreach (var anim in _timeAnimations)
				anim.SetAnimationStatusByTime(fTiming);
			foreach (var trigger in _triggerAnimations)
				trigger.SetAnimationStatusByTime(fTiming);
		}

		/// <summary>
		/// 根据时间强制结束动画
		/// </summary>
		/// <param name="timing">时间 (ms)</param>
		[Obsolete("Use AnimationManager.SetAnimationStatusByTiming() instead.")]
		public void ForceFinishAnimationsByTiming(int timing)
		{
			foreach (var anim in 
			         _transformAnimations
				         .Where(t => t && t.TriggerTime <= timing))
				anim.FinishAnimation();
			foreach (var anim in 
			         _materialAnimations
				         .Where(t => t && t.TriggerTime <= timing))
				anim.FinishAnimation();
			
			foreach (var trigger in _transformAnimationTriggers
				         .Where(t => t && t.TriggerTime <= timing))
				trigger.FinishAnimation();
			foreach (var trigger in _materialAnimationTriggers
				         .Where(t => t && t.TriggerTime <= timing))
				trigger.FinishAnimation();
			
			foreach (var anim in _timeAnimations
				         .Where(t => t))
				anim.FinishAnimation();
			foreach (var trigger in _triggerAnimations
				         .Where(t => t))
				trigger.FinishAnimation();
		}

		/// <summary>
		/// 暂停动画
		/// </summary>
		public void Pause()
		{
			foreach (var anim in 
			         _transformAnimations)
				anim.Pause();
			foreach (var anim in 
			         _materialAnimations)
				anim.Pause();
			
			foreach (var trigger in _transformAnimationTriggers)
				trigger.Pause();
			foreach (var trigger in _materialAnimationTriggers)
				trigger.Pause();
			
			foreach (var anim in _timeAnimations)
				anim.Pause();
			foreach (var trigger in _triggerAnimations)
				trigger.Pause();
		}

		/// <summary>
		/// 继续动画
		/// </summary>
		public void Continue()
		{
			foreach (var anim in 
			         _transformAnimations)
				anim.Continue();
			foreach (var anim in 
			         _materialAnimations)
				anim.Continue();
			
			foreach (var trigger in _transformAnimationTriggers)
				trigger.Continue();
			foreach (var trigger in _materialAnimationTriggers)
				trigger.Continue();
			
			foreach (var trigger in _triggerAnimations)
				trigger.Continue();
			foreach (var anim in _timeAnimations)
				anim.Continue();
		}
	}
}