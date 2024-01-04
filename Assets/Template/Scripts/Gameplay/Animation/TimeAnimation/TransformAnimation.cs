using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public class TransformAnimation : TimeAnimationComponentBase<Transform>
	{
#pragma warning disable
		
		[Space]
		[SerializeField] 
		private AnimValueBaseVector3 m_Position;
		
		[Space]
		[SerializeField] 
		private AnimValueBaseVector3 m_Rotation;
		
		[Space]
		[SerializeField] 
		private AnimValueBaseVector3WithoutLocal m_Scale;
		
#pragma warning restore

		protected override void OnActiveAnimation()
		{
			// _tween?.Kill(); // 不知道为啥加了这个就 index out of range
			var sequence = DOTween.Sequence();
			if (m_Position.Enable)
			{
				var targetPos = m_Position.EndValue;
				sequence.Join(
					(m_Position.IsLocal
						? TargetObject.DOLocalMove(targetPos, m_Position.Duration)
						: TargetObject.DOMove(targetPos, m_Position.Duration))
					.SetEase(m_Position.Easing)
				);
			}

			if (m_Rotation.Enable)
			{
				var targetRot = m_Rotation.EndValue;
				sequence.Join(
					(m_Rotation.IsLocal
						? AnimTweenHelper.DOLocalRotate(TargetObject, targetRot, m_Rotation.Duration)
						: AnimTweenHelper.DORotate(TargetObject, targetRot, m_Rotation.Duration))
					.SetEase(m_Rotation.Easing)
				);
				
			}

			if (m_Scale.Enable)
			{
				var targetScale = m_Scale.EndValue;
				sequence.Join(
					TargetObject.DOScale(targetScale, m_Scale.Duration)
						.SetEase(m_Scale.Easing)
				);
			}

			_tween = sequence.Play();
		}
		
		protected override void OnResetAnimation()
		{
			if (m_Position.Enable)
			{
				if (m_Position.IsLocal)
					TargetObject.localPosition = m_Position.StartValue;
				else
					TargetObject.position = m_Position.StartValue;
			}

			if (m_Rotation.Enable)
			{
				if (m_Rotation.IsLocal)
					TargetObject.localRotation = Quaternion.Euler(m_Rotation.StartValue);
				else
					TargetObject.rotation = Quaternion.Euler(m_Rotation.StartValue);
			}

			if (m_Scale.Enable)
			{
				TargetObject.localScale = m_Scale.StartValue;
			}
		}
		
		protected override void OnFinishAnimation()
		{
			if (m_Position.Enable)
			{
				var targetPos = m_Position.EndValue;
				
				if (m_Position.IsLocal)
					TargetObject.localPosition = targetPos;
				else
					TargetObject.position = targetPos;
			}

			if (m_Rotation.Enable)
			{
				var targetRot = m_Rotation.EndValue;
				
				if (m_Rotation.IsLocal)
					TargetObject.localRotation = Quaternion.Euler(targetRot);
				else
					TargetObject.rotation = Quaternion.Euler(targetRot);
			}

			if (m_Scale.Enable)
			{
				var targetScale = m_Scale.EndValue;
				
				TargetObject.localScale = targetScale;
			}
		}

		protected override void OnSetAnimationStatusByTime(float time)
		{
			if (m_Position.Enable)
			{
				var targetPos = m_Position.LerpByTime(time);
				
				if (m_Position.IsLocal)
					TargetObject.localPosition = targetPos;
				else
					TargetObject.position = targetPos;
			}
			
			if (m_Rotation.Enable)
			{
				var targetRot = m_Rotation.LerpByTime(time);
				
				if (m_Rotation.IsLocal)
					TargetObject.localRotation = Quaternion.Euler(targetRot);
				else
					TargetObject.rotation = Quaternion.Euler(targetRot);
			}
			
			if (m_Scale.Enable)
			{
				var targetScale = m_Scale.LerpByTime(time);
				
				TargetObject.localScale = targetScale;
			}
		}

		protected override void OnContinueByElapsedTime()
		{
			OnActiveAnimation();
			if (_tween == null) return;
			_tween.fullPosition = _elapsedTime;
		}

		public override void Pause()
		{
			_tween?.Pause();
		}

		public override void Continue()
		{
			_tween?.Play();
		}

		// public float p = .5f;
		//
		// [Button("SetTime")]
		// private void SetTime()
		// {
		// 	ResetAnimation();
		// 	ActiveAnimation();
		// 	if (_tween == null) return;
		// 	_tween.fullPosition = p;
		// 	print(_tween.IsComplete());
		// 	_tween.Kill();
		// }
		//
		// [Button("PlayAtTime")]
		// private void PlayAtTime()
		// {
		// 	ResetAnimation();
		// 	ActiveAnimation();
		// 	if (_tween == null) return;
		// 	_tween.fullPosition = p;
		// 	print(_tween.IsComplete());
		// }
	}
}