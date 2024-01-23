using DancingLineSample.Attributes;
using DG.Tweening;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public class TransformAnimationTrigger : TriggerAnimationComponentBase<Transform>
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
			base.OnActiveAnimation();
			
			// _tween?.Kill();
			var sequence = DOTween.Sequence();
			if (m_Position.Enable)
			{
				var targetPos = m_Position.IsAdd 
					? m_Position.StartValue + m_Position.TargetValue 
					: m_Position.TargetValue;
				sequence.Join(
					(m_Position.IsLocal
						? TargetObject.DOLocalMove(targetPos, m_Position.Duration)
						: TargetObject.DOMove(targetPos, m_Position.Duration))
					.SetEase(m_Position.Easing)
				);
			}

			if (m_Rotation.Enable)
			{
				var targetRot = m_Rotation.IsAdd
					? m_Rotation.StartValue + m_Rotation.TargetValue
					: m_Rotation.TargetValue;
				sequence.Join(
					(m_Rotation.IsLocal
						? AnimTweenHelper.DOLocalRotate(TargetObject, targetRot, m_Rotation.Duration)
						: AnimTweenHelper.DORotate(TargetObject, targetRot, m_Rotation.Duration))
					.SetEase(m_Rotation.Easing)
				);
				
			}

			if (m_Scale.Enable)
			{
				var targetScale = m_Scale.IsAdd
					? m_Scale.StartValue + m_Scale.TargetValue
					: m_Scale.TargetValue;
				sequence.Join(
					TargetObject.DOScale(targetScale, m_Scale.Duration)
						.SetEase(m_Scale.Easing)
				);
			}

			_tween = sequence.Play();
		}
		
		protected override void OnResetAnimation()
		{
			base.OnResetAnimation();
			
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

		private void OnTriggerEnter(Collider other)
		{
			if (!other.gameObject.CompareTag("Player")) return;
			ActiveAnimation();
		}
		
		/// <summary>
		/// 交换值位置
		/// </summary>
		[MethodButton("Swap Values", true)]
		public void SwapValues()
		{
			if (m_Position.Enable && !m_Position.IsAdd)
			{
				(m_Position.StartValue, m_Position.TargetValue) = 
					(m_Position.TargetValue, m_Position.StartValue);
			}
			
			if (m_Rotation.Enable && !m_Rotation.IsAdd)
			{
				(m_Rotation.StartValue, m_Rotation.TargetValue) = 
					(m_Rotation.TargetValue, m_Rotation.StartValue);
			}
			
			if (m_Scale.Enable && !m_Scale.IsAdd)
			{
				(m_Scale.StartValue, m_Scale.TargetValue) = 
					(m_Scale.TargetValue, m_Scale.StartValue);
			}
		}
		
		/// <summary>
		/// 从对象设置值
		/// </summary>
		[MethodButton("Set Values From Object", true)]
		public void SetValuesFromObject()
		{
			m_Position.StartValue = m_Position.IsLocal ? 
				TargetObject.localPosition : TargetObject.position;
			m_Rotation.StartValue = m_Rotation.IsLocal ? 
				TargetObject.localRotation.eulerAngles : TargetObject.rotation.eulerAngles;
			m_Scale.StartValue = TargetObject.localScale;
		}
	}
}