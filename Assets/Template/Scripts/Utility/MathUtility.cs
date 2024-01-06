using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace DancingLineSample.Utility
{
	public static class MathUtility
	{
		private static double[] Solve(double[,] A, double[] y)
		{
			int n = y.Length;

			for (int i = 0; i < n; i++)
			{
				int max = i;
				for (int j = i + 1; j < n; j++)
					if (Mathf.Abs((float)A[j, i]) > Mathf.Abs((float)A[max, i]))
						max = j;

				double[] temp = new double[n + 1];
				for (int k = 0; k < n; k++)
					temp[k] = A[max, k];
				temp[n] = y[max];
				for (int k = 0; k < n; k++)
					A[max, k] = A[i, k];
				y[max] = y[i];
				for (int k = 0; k < n; k++)
					A[i, k] = temp[k];
				y[i] = temp[n];

				for (int j = i + 1; j < n; j++)
				{
					double f = A[j, i] / A[i, i];
					for (int k = i + 1; k < n; k++)
						A[j, k] -= A[i, k] * f;
					y[j] -= y[i] * f;
				}
			}

			double[] x = new double[n];
			for (int i = n - 1; i >= 0; i--)
			{
				x[i] = y[i] / A[i, i];
				for (int j = i - 1; j >= 0; j--)
					y[j] -= A[j, i] * x[i];
			}

			return x;
		}
		
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
			double[,] A = new double[3, 3] {
				{ pointA.x * pointA.x, pointA.x, 1 },
				{ pointB.x * pointB.x, pointB.x, 1 },
				{ pointC.x * pointC.x, pointC.x, 1 }
			};

			double[] y = new double[3] { pointA.y, pointB.y, pointC.y };

			// 求解线性方程组
			double[] coefficients = Solve(A, y);

			return coefficients;
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