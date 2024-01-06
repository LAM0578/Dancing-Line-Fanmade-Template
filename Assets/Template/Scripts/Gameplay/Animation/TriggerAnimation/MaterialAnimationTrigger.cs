using System.Linq;
using DancingLineSample.Objects;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public class MaterialAnimationTrigger : TriggerAnimationComponentBase<Material>
	{
#pragma warning disable

		[Tooltip("使用高级 (完整)")]
		[SerializeField] 
		private bool m_Advanced;

		[Space]
		
		[Tooltip("动画持续时间 (s)")]
		[PropertyActive("m_Advanced", true)]
		[SerializeField] 
		private float m_Duration;

		[Tooltip("动画缓动类型")]
		[PropertyActive("m_Advanced", true)]
		[SerializeField] 
		private Ease m_Easing;

		[PropertyActive("m_Advanced", true)]
		[SerializeField] 
		internal ColorStatus m_Color;

		[PropertyActive("m_Advanced", false)] 
		[SerializeField]
		internal AnimMaterialProperties m_AdvancedProperties;
		
#pragma warning restore

		private static readonly int _m_Color = Shader.PropertyToID("_Color");

		protected override void OnActiveAnimation()
		{
			base.OnActiveAnimation();
			// _tween?.Kill();
			_tween = m_Advanced
				? m_AdvancedProperties.DoMaterialProperties(TargetObject)
				: TargetObject.DOColor(m_Color.EndValue, _m_Color, m_Duration)
					.SetEase(m_Easing);
		}

		protected override void OnResetAnimation()
		{
			base.OnResetAnimation();
			if (m_Advanced)
				m_AdvancedProperties.ResetProperties(TargetObject);
			else
				TargetObject.color = m_Color.StartValue;
		}

		protected override void OnFinishAnimation()
		{
			if (m_Advanced)
				m_AdvancedProperties.ApplyProperties(TargetObject);
			else
				TargetObject.color = m_Color.EndValue;
		}

		protected override void OnSetAnimationStatusByTime(float time)
		{
			if (m_Advanced)
				m_AdvancedProperties.ApplyPropertiesByTime(TargetObject, time);
			else
			{
				float p = AnimLerpHelper.Evaluate(m_Easing, time, m_Duration);
				TargetObject.color = Color.Lerp(m_Color.StartValue, m_Color.EndValue, p);
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
	}
	
#if UNITY_EDITOR
	[CustomEditor(typeof(MaterialAnimationTrigger))]
	public class MaterialAnimationTriggerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var item = (MaterialAnimationTrigger)target;
			
			if (GUILayout.Button("Preview Animation"))
			{
				item.PreviewAnimation();
			}
			
			if (GUILayout.Button("Copy color from target material"))
			{
				var mat = item.TargetObject;
				if (!mat) return;
				
				item.m_Color.StartValue = mat.color;
				var properties = item.m_AdvancedProperties.Properties;

				if (!properties.Any(t =>
					    t.Name == "_Color" &&
					    t.Type == AnimMaterialProperty.PropertyType.Color))
				{
					properties.Add(new AnimMaterialProperty(
						"_Color", 
						AnimMaterialProperty.PropertyType.Color));
				}
				
				var property = properties.First(t => 
					t.Name == "_Color" && 
					t.Type == AnimMaterialProperty.PropertyType.Color);
				
				if (property.m_Color == null) 
					property.m_Color = new ColorStatus();
				
				property.m_Color.StartValue = mat.color;
			}
		}
	}
#endif
}