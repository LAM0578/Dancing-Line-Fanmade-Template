using DancingLineSample.Attributes;
using DancingLineSample.Gameplay.Objects;
using DancingLineSample.Objects;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public class FogAnimation : TimeAnimationBase
	{
#pragma warning disable
		
		[Space]
		
		[Tooltip("使用高级 (完整)")]
		[SerializeField] 
		private bool m_Advanced;

		[Tooltip("动画持续时间 (s)")]
		[SerializeField] 
		private float m_Duration;
		
		[Tooltip("动画缓动类型")]
		[SerializeField] 
		private Ease m_Easing;
		
		[Space] 
		
		[SerializeField]
		private ColorStatus m_Color;
		
		[Space]
		
		[PropertyActive("m_Advanced", false)]
		[SerializeField] 
		private AnimFogAdvanced m_AnimFogAdvanced;
		
#pragma warning restore

		protected override void OnActiveAnimation()
		{
			// _tween?.Kill();
			
			var sequence = DOTween.Sequence();
			sequence.Join(AnimTweenHelper.DOFogColor(
				m_Color.EndValue,
				m_Duration).SetEase(m_Easing));

			if (m_Advanced)
			{
				sequence.Join(m_AnimFogAdvanced.DoValues(
					m_Duration,
					m_Easing));
			}
			
			_tween = sequence.Play();
		}
		
		protected override void OnResetAnimation()
		{
			RenderSettings.fogColor = m_Color.StartValue;
			if (m_Advanced)
				m_AnimFogAdvanced.ResetValues();
		}

		protected override void OnFinishAnimation()
		{
			RenderSettings.fogColor = m_Color.EndValue;
			if (m_Advanced)
				m_AnimFogAdvanced.ApplyValues();
		}

		protected override void OnSetAnimationStatusByTime(float time)
		{
			float p = AnimLerpHelper.Evaluate(m_Easing, time, m_Duration);
			RenderSettings.fogColor = Color.Lerp(m_Color.StartValue, m_Color.EndValue, p);
			if (m_Advanced)
				m_AnimFogAdvanced.ApplyByProgress(p);
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

		[Button("Set FogSettings from RenderSettings", true)]
		private void SetFogSettingsFromRenderSettings()
		{
			m_Color.StartValue = RenderSettings.fogColor;
			m_AnimFogAdvanced.m_FogMode = RenderSettings.fogMode;
			m_AnimFogAdvanced.m_Start.StartValue = RenderSettings.fogStartDistance;
			m_AnimFogAdvanced.m_End.StartValue = RenderSettings.fogEndDistance;
			m_AnimFogAdvanced.m_Density.StartValue = RenderSettings.fogDensity;
		}
	}
}