using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DancingLineSample.Objects;
using DancingLineSample.Utility;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay.Animation
{
	[Serializable]
	public abstract class AnimValueBase<T>
	{
#pragma warning disable
		
		[Tooltip("启用动画")]
		public bool Enable;
		
		[Space]
		
		[Tooltip("动画持续时间 (s)")]
		public float Duration;
		
		[Tooltip("动画缓动类型")]
		public Ease Easing;
		
		[Space]
		
		[Tooltip("起始值")]
		public T StartValue;
		
		[Tooltip("目标值")]
		public T TargetValue;
		
#pragma warning restore

		/// <summary>
		/// 根据时间过渡值
		/// </summary>
		/// <param name="t">时间 (s)</param>
		/// <returns>根据时间取 StartValue 和 TargetValue 之间的值</returns>
		public abstract T LerpByTime(float t);

		public abstract T EndValue { get; }
	}

	[Serializable]
	public class AnimValueBaseVector3 : AnimValueBase<Vector3>
	{
		[Space]
		
		[Tooltip("是否为局部")]
		public bool IsLocal;
		
		[Tooltip("是否为累加\n当该值为 true 时该值为 StartValue + TargetValue\n否则作为为 EndValue 使用")]
		public bool IsAdd;

		public override Vector3 LerpByTime(float t)
		{
			float p = AnimLerpHelper.Evaluate(Easing, t, Duration);
			return IsAdd 
				? Vector3.LerpUnclamped(StartValue, StartValue + TargetValue, p) 
				: Vector3.LerpUnclamped(StartValue, TargetValue, p);
		}

		public override Vector3 EndValue => IsAdd ? StartValue + TargetValue : TargetValue;
	}
	
	[Serializable]
	public class AnimValueBaseVector3WithoutLocal : AnimValueBase<Vector3>
	{
		[Tooltip("是否为累加\n当该值为 true 时该值为 StartValue + TargetValue\n否则作为为 EndValue 使用")]
		public bool IsAdd;

		public override Vector3 LerpByTime(float t)
		{
			float p = AnimLerpHelper.Evaluate(Easing, t, Duration);
			return IsAdd 
				? Vector3.LerpUnclamped(StartValue, StartValue + TargetValue, p) 
				: Vector3.LerpUnclamped(StartValue, TargetValue, p);
		}

		public override Vector3 EndValue => IsAdd ? StartValue + TargetValue : TargetValue;
	}

	[Serializable]
	public class AnimMaterialProperty
	{
		public AnimMaterialProperty(string name, PropertyType type)
		{
			m_PropertyName = name;
			m_PropertyType = type;
		}
		
		[Serializable]
		public enum PropertyType
		{
			Color,
			Float,
			Vector4
		}

#pragma warning disable
		
		[Tooltip("材质球特性名称")]
		[SerializeField] 
		private string m_PropertyName;
		
		[Tooltip("材质球特性类型")]
		[SerializeField] 
		private PropertyType m_PropertyType;

		[Tooltip("动画持续时间 (s)")]
		[SerializeField] 
		private float m_Duration;

		[Tooltip("动画缓动类型")]
		[SerializeField] 
		private Ease m_Easing;
		
		[PropertyActive("m_PropertyType", PropertyType.Color, CompareType.NotEqul)] 
		[SerializeField]
		internal ColorStatus m_Color;
		
		[PropertyActive("m_PropertyType", PropertyType.Float, CompareType.NotEqul)] 
		[SerializeField]
		internal FloatStatus m_Float;
		
		[PropertyActive("m_PropertyType", PropertyType.Vector4, CompareType.NotEqul)] 
		[SerializeField]
		internal Vector4Status m_Vector4;
		
#pragma warning restore

		public string Name => m_PropertyName;
		public PropertyType Type => m_PropertyType;

		public Tween DoValue(Material material)
		{
			if (m_PropertyType == PropertyType.Color)
				return material.DOColor(
						m_Color.EndValue, 
						m_PropertyName, 
						m_Duration)
					.SetEase(m_Easing);
			if (m_PropertyType == PropertyType.Float)
				return material.DOFloat(
						m_Float.EndValue,
						m_PropertyName,
						m_Duration)
					.SetEase(m_Easing);
			if (m_PropertyType == PropertyType.Vector4) 
				return material.DOVector(
						m_Vector4.EndValue, 
						m_PropertyName,
						m_Duration)
					.SetEase(m_Easing);
			return null;
		}

		public void ResetValue(Material material)
		{
			if (m_PropertyType == PropertyType.Color)
				material.SetColor(m_PropertyName, m_Color.StartValue);
			if (m_PropertyType == PropertyType.Float)
				material.SetFloat(m_PropertyName, m_Float.StartValue);
			if (m_PropertyType == PropertyType.Vector4)
				material.SetVector(m_PropertyName, m_Vector4.StartValue);
		}

		public void ApplyValue(Material material)
		{
			if (m_PropertyType == PropertyType.Color)
				material.SetColor(m_PropertyName, m_Color.EndValue);
			if (m_PropertyType == PropertyType.Float)
				material.SetFloat(m_PropertyName, m_Float.EndValue);
			if (m_PropertyType == PropertyType.Vector4)
				material.SetVector(m_PropertyName, m_Vector4.EndValue);
		}

		public void ApplyByTime(Material material, float t)
		{
			float p = AnimLerpHelper.Evaluate(m_Easing, t, m_Duration);
			if (m_PropertyType == PropertyType.Color)
				material.SetColor(m_PropertyName, 
					Color.Lerp(m_Color.StartValue, m_Color.EndValue, p));
			if (m_PropertyType == PropertyType.Float)
				material.SetFloat(m_PropertyName,
					Mathf.Lerp(m_Float.StartValue, m_Float.EndValue, p));
			if (m_PropertyType == PropertyType.Vector4)
				material.SetVector(m_PropertyName,
					Vector4.Lerp(m_Vector4.StartValue, m_Vector4.EndValue, p));
		}
	}

	[Serializable]
	public class AnimMaterialProperties
	{
		public List<AnimMaterialProperty> Properties = new List<AnimMaterialProperty>();

		/// <summary>
		/// 播放特性值变化动画并设置指定材质球上
		/// </summary>
		/// <param name="material">指定材质球</param>
		/// <returns>Tween 对象</returns>
		public Tween DoMaterialProperties(Material material)
		{
			var sequence = DOTween.Sequence();
			foreach (var property in Properties)
			{
				sequence.Join(property.DoValue(material));
			}

			return sequence.Play();
		}

		/// <summary>
		/// 重置特性值到指定材质球上
		/// </summary>
		/// <param name="material">指定材质球</param>
		public void ResetProperties(Material material)
		{
			foreach (var property in Properties)
			{
				property.ResetValue(material);
			}
		}

		/// <summary>
		/// 应用特性值到指定材质球上
		/// </summary>
		/// <param name="material">指定材质球</param>
		public void ApplyProperties(Material material)
		{
			foreach (var property in Properties)
			{
				property.ApplyValue(material);
			}
		}
		
		/// <summary>
		/// 根据时间应用特性值到指定材质球上
		/// </summary>
		/// <param name="material">指定材质球</param>
		/// <param name="t">时间 (s)</param>
		public void ApplyPropertiesByTime(Material material, float t)
		{
			foreach (var property in Properties)
			{
				property.ApplyByTime(material, t);
			}
		}
		
	}

	[Serializable]
	public class AnimFogAdvanced
	{
#pragma warning disable
		
		[SerializeField]
		internal FogMode m_FogMode = FogMode.Linear;
		
		[PropertyActive("m_FogMode", FogMode.Linear, CompareType.NotEqul)]
		[SerializeField]
		internal FloatStatus m_Start;
		
		[PropertyActive("m_FogMode", FogMode.Linear, CompareType.NotEqul)]
		[SerializeField]
		internal FloatStatus m_End;

		[PropertyActive("m_FogMode", FogMode.Linear)] 
		[SerializeField]
		internal FloatStatus m_Density;
		
#pragma warning restore

		public Tween DoValues(float duration, Ease easing)
		{
			if (m_FogMode == FogMode.Linear)
			{
				var sequence = DOTween.Sequence();
				sequence.Join(AnimTweenHelper.DOFogStartDistance(
					m_Start.EndValue,
					duration).SetEase(easing));
				sequence.Join(AnimTweenHelper.DOFogEndDistance(
					m_End.EndValue,
					duration).SetEase(easing));
				return sequence;
			}

			return AnimTweenHelper.DOFogDensity(
				m_Density.EndValue, 
				duration).SetEase(easing);
		}

		public void ResetValues()
		{
			if (m_FogMode == FogMode.Linear)
			{
				RenderSettings.fogStartDistance = m_Start.StartValue;
				RenderSettings.fogEndDistance = m_End.StartValue;
			}
			else
			{
				RenderSettings.fogDensity = m_Density.StartValue;
			}
		}

		public void ApplyValues()
		{
			if (m_FogMode == FogMode.Linear)
			{
				RenderSettings.fogStartDistance = m_Start.EndValue;
				RenderSettings.fogEndDistance = m_End.EndValue;
			}
			else
			{
				RenderSettings.fogDensity = m_Density.EndValue;
			}
		}

		public void ApplyByProgress(float p)
		{
			if (m_FogMode == FogMode.Linear)
			{
				RenderSettings.fogStartDistance = m_Start.Lerp(p);
				RenderSettings.fogEndDistance = m_End.Lerp(p);
			}
			else
			{
				RenderSettings.fogDensity = m_Density.Lerp(p);
			}
		}
	}
	
	public static class AnimTweenHelper
	{
		public static Tween DOLocalRotate(Transform trans, Vector3 endValue, float duration)
		{
			return DOTween.To(
				() => trans.localRotation.eulerAngles,
				x => trans.localRotation = Quaternion.Euler(x),
				endValue,
				duration
			).SetTarget(trans.rotation.eulerAngles);
		}
		
		public static Tween DORotate(Transform trans, Vector3 endValue, float duration)
		{
			return DOTween.To(
				() => trans.rotation.eulerAngles,
				x => trans.rotation = Quaternion.Euler(x),
				endValue,
				duration
			).SetTarget(trans.rotation.eulerAngles);
		}

		public static Tween DOFloat(float val, float endValue, float duration)
		{
			return DOTween.To(
				() => val,
				x => val = x,
				endValue,
				duration
			).SetTarget(val);
		}

		public static Tween DOColor(Color val, Color endValue, float duration)
		{
			return DOTween.To(
				() => val,
				x => val = x,
				endValue,
				duration
			).SetTarget(val);
		}

		#region FogSettings

		public static Tween DOFogColor(Color endValue, float duration)
		{
			var val = RenderSettings.fogColor;
			return DOTween.To(
				() => val,
				x => RenderSettings.fogColor = x,
				endValue,
				duration
			).SetTarget(val);
		}

		public static Tween DOFogStartDistance(float endValue, float duration)
		{
			float val = RenderSettings.fogStartDistance;
			return DOTween.To(
				() => val,
				x => RenderSettings.fogStartDistance = x,
				endValue,
				duration
			).SetTarget(val);
		}

		public static Tween DOFogEndDistance(float endValue, float duration)
		{
			float val = RenderSettings.fogEndDistance;
			return DOTween.To(
				() => val,
				x => RenderSettings.fogEndDistance = x,
				endValue,
				duration
			).SetTarget(val);
		}
		
		public static Tween DOFogDensity(float endValue, float duration)
		{
			float val = RenderSettings.fogDensity;
			return DOTween.To(
				() => val,
				x => RenderSettings.fogDensity = x,
				endValue,
				duration
			).SetTarget(val);
		}

		#endregion
	}

	public static class AnimLerpHelper
	{
		private static readonly EaseFunction _internalEaseFunction = (time, duration, amplitude, period) => time / duration;

		internal static float Evaluate(Ease easing, float elapsedTime, float duration)
		{
			if (elapsedTime < 0) return 0;
			if (elapsedTime > duration) return 1;
			return EaseManager.Evaluate(
				easing, 
				_internalEaseFunction, 
				elapsedTime, 
				duration, 
				1, 
				0
			);
		}
		
		public static Color LerpByTime(this ColorStatus colStat, float elapsedTime, float duration, Ease easing)
		{
			float p = Evaluate(easing, elapsedTime, duration);
			return Color.LerpUnclamped(colStat.StartValue, colStat.EndValue, p);
		}

		public static float LerpByTime(this FloatStatus floatStat, float elapsedTime, float duration, Ease easing)
		{
			float p = Evaluate(easing, elapsedTime, duration);
			return Mathf.LerpUnclamped(floatStat.StartValue, floatStat.EndValue, p);
		}

		public static Vector3 LerpByTime(this Vector3Status vecStat, float elapsedTime, float duration, Ease easing)
		{
			float p = Evaluate(easing, elapsedTime, duration);
			return Vector3.LerpUnclamped(vecStat.StartValue, vecStat.EndValue, p);
		}

		public static float Lerp(this FloatStatus floatStat, float progress)
		{
			return Mathf.LerpUnclamped(floatStat.StartValue, floatStat.EndValue, progress);
		}

		public static Vector3 Lerp(this Vector3Status vecStat, float progress)
		{
			return Vector3.LerpUnclamped(vecStat.StartValue, vecStat.EndValue, progress);
		}
	}
	
}