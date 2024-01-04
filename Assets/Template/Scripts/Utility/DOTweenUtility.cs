using System;
using DG.Tweening;
using SuperBlur;
using UnityEngine;
using UnityEngine.UI;

namespace DancingLineSample.Utility
{
	public static class DOTweenUtility
	{
		public static Tweener DOBlurUI(
			this SuperBlurBase superBlurBase, 
			Image img, 
			Color blurUIColor, 
			float duration, 
			bool show
		)
		{
			bool hasImg = img;
			float interpolation = show ? 0 : 1;
			return DOTween.To(
				() => interpolation,
				val =>
				{
					superBlurBase.interpolation = val;
					if (hasImg) img.color = Color.Lerp(Color.white, blurUIColor, val);
					interpolation = val;
				},
				show ? 1 : 0,
				duration);
		}

		public static Tween DOFunction(EaseFunction function, Action<float> onUpdate, float duration)
		{
			float target = 0f;
			return DOTween.To(
				() => target,
				val =>
				{
					onUpdate.Invoke(
						function(val, 1f, 0, 0));
					target = val;
				},
				1,
				duration
			);
		}

		/// <summary>
		/// 根据三个点计算抛物线函数
		/// </summary>
		/// <param name="pointA"></param>
		/// <param name="pointB"></param>
		/// <param name="pointC"></param>
		/// <returns>抛物线函数</returns>
		public static EaseFunction CalculateParabolaFunction(
			Vector3 pointA,
			Vector3 pointB,
			Vector3 pointC
		)
		{
			var coefficients = MathUtility.FitParabola(pointA, pointB, pointC);
			// Debug.Log(string.Join(", ", coefficients));
			return (time, duration, amplitude, period) =>
			{
				float p = time / duration;
				return (float)(
					coefficients[0] * p * p +
					coefficients[1] * p +
					coefficients[2]
				);
			};
		}

		/// <summary>
		/// 根据两个点和高度计算抛物线函数
		/// </summary>
		/// <param name="startPos"></param>
		/// <param name="endPos"></param>
		/// <param name="height"></param>
		/// <returns>抛物线函数</returns>
		public static EaseFunction CalculateParabolaFunction(
			Vector3 startPos,
			Vector3 endPos,
			float height
		)
		{
			return CalculateParabolaFunction(
				new Vector3(0, startPos.y),
				new Vector3(0.5f, Mathf.Max(startPos.y, endPos.y) + height),
				new Vector3(1, endPos.y)
			);
		}
	}
}
