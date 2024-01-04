using System.Collections.Generic;
using System.Linq;
using DancingLineSample.Utility;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using UnityEditor;

namespace DancingLineSample.EditorUtility
{
	public static class GizmosExtend
	{
		private const float _halfPi = 1.5707963267948966f;
		
		private static float CubicOut(float a, float b, float t)
		{
			return a + (b - a) * (1 - Mathf.Pow(t, 3));
		}
		
		private static float CubicIn(float a, float b, float t)
		{
			return a + (b - a) * Mathf.Pow(t, 3);
		}

#if UNITY_EDITOR
		public static void DrawParabola(Vector3 pos, Vector3 targetPos, float height, Color color)
		{
			var points = MathUtility.CalculateParabolaPoints(pos, targetPos, height);
			Handles.color = color;
			Handles.DrawAAPolyLine(4f, points.ToArray());
		}
#endif
	}
}