using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace DancingLineSample.Utility
{
	public static class MathUtility
	{
		
		/// <summary>
		/// 根据三个点计算抛物线方程系数
		/// </summary>
		/// <param name="pointA"></param>
		/// <param name="pointB"></param>
		/// <param name="pointC"></param>
		/// <returns>抛物线方程的系数</returns>
		public static double[] FitParabola(Vector3 pointA, Vector3 pointB, Vector3 pointC)
		{
			// 构建矩阵
			var A = Matrix<double>.Build.Dense(3, 3);
			var y = Vector<double>.Build.Dense(3);

			var points = new Vector3[] { pointA, pointB, pointC };

			for (int i = 0; i < 3; i++)
			{
				double x = points[i].x;
				A[i, 0] = x * x;
				A[i, 1] = x;
				A[i, 2] = 1;
				y[i] = points[i].y;
			}

			// 求解线性方程组
			var coefficients = A.Solve(y);

			return new double[] { coefficients[0], coefficients[1], coefficients[2] };
		}
		
		/// <summary>
		/// 根据两个点和高度计算一定数量的抛物线上的点
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="targetPos"></param>
		/// <param name="height"></param>
		/// <param name="count"></param>
		/// <returns>一定数量的抛物线上的点</returns>
		public static List<Vector3> CalculateParabolaPoints(Vector3 pos, Vector3 targetPos, float height, int count=20)
		{
			double[] coefficients = FitParabola(
				new Vector3(0, pos.y),
				new Vector3(0.5f, Mathf.Max(pos.y, targetPos.y) + height),
				new Vector3(1, targetPos.y)
			);

			var points = new List<Vector3>();
			for (int i = 0; i < count; i++)
			{
				float p = i / (count - 1f);
				float x = Mathf.Lerp(pos.x, targetPos.x, p);
				float y = (float)calcRealHeight(coefficients[0], coefficients[1], coefficients[2], p);
				float z = Mathf.Lerp(pos.z, targetPos.z, p);
				points.Add(new Vector3(x, y, z));
			}
			
			return points;

			double calcRealHeight(double a, double b, double c, double x)
			{
				return a * x * x + b * x + c;
			}
		}
	}
}