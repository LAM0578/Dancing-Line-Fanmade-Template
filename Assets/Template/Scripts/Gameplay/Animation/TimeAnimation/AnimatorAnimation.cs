using System;
using System.Collections.Generic;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public class AnimatorAnimation : TimeAnimationComponentBase<Animator>
	{
#pragma warning disable
		
		[SerializeField]
		[Tooltip("在 Awake 方法被调用时激活动画")]
		private bool m_ActiveOnAwake;
		
		[SerializeField]
		[Tooltip("手动设置 Animator 的动画名称")]
		private bool m_UseManualAnimName;

		[PropertyActive("m_UseManualAnimName", false)]
		[SerializeField] 
		private string m_AnimName;
		
#pragma warning restore
		
		private Animator _animator;
		private AnimationClip _clip;
		private int _curAnimStateInfoHash;
		private bool _isLoop;

		protected override void OnActiveAnimation()
		{
			_animator.Play(_curAnimStateInfoHash, 0, 0);
			_animator.speed = 1;
		}
		
		protected override void OnResetAnimation()
		{
			_animator.Play(_curAnimStateInfoHash, 0, 0);
			_animator.speed = 0;
		}

		protected override void OnFinishAnimation()
		{
			_animator.Play(_curAnimStateInfoHash, 0, _isLoop ? 0 : 1);
			_animator.speed = _isLoop ? 1 : 0;
		}

		protected override void OnSetAnimationStatusByTime(float time)
		{
			float p = Mathf.Clamp01(time / _clip.length);
			if (!_isLoop && Math.Abs(p - 1) < float.Epsilon)
			{
				Actived = true;
			}
			_animator.Play(_curAnimStateInfoHash, 0, p);
			_animator.speed = 0;
		}

		protected override void OnContinueByElapsedTime()
		{
			OnSetAnimationStatusByTime(_elapsedTime);
			_animator.speed = 1;
		}

		public override void Pause()
		{
			_animator.speed = 0;
		}
		
		public override void Continue()
		{
			_animator.speed = 1;
		}

		private void Awake()
		{
			_animator = TargetObject;
			_animator.speed = m_ActiveOnAwake ? 1 : 0;
			
			_curAnimStateInfoHash = m_UseManualAnimName ? 
				Animator.StringToHash(m_AnimName) : 
				_animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
			
			var animClipInfos = _animator.GetCurrentAnimatorClipInfo(0);
			if (animClipInfos.Length < 1) return;
			_clip = animClipInfos[0].clip;
			_isLoop = _clip.isLooping;
		}
	}
}