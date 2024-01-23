using System;
using System.Collections.Generic;
using DancingLineSample.Attributes;
using DancingLineSample.Editing.Utility;
using DancingLineSample.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BeatlineDisplay : MonoBehaviour
{
#pragma warning disable

	[SerializeField] private bool m_Enable;
	[SerializeField] private bool m_EnableText;
	[Header("Beatline Generate Settings")]
	[SerializeField] private int m_StartTiming;
	[SerializeField] private int m_RenderRange;
	[SerializeField] private int m_Offset;
	[Space]
	[SerializeField] private float m_Bpm;
	[SerializeField] private float m_Speed;
	[SerializeField] private float m_BeatlineDensity = 4;
	[Header("Beatline Settings")]
	[SerializeField] private Vector3 m_StartPosition;
	[SerializeField] private float m_XOffset;
	[SerializeField] private Vector3 m_Rotation;
	[SerializeField] private float m_Scale = 100;
	[SerializeField] private float m_LabelPosition = 100;
	[Header("Fake Line")]
	[SerializeField] private Vector3 m_StartForward = new Vector3(0, 0, 1);
	[SerializeField] private Vector3 m_TurnForward = new Vector3(1, 0, 0);
	[Header("Color")] 
	[SerializeField] private Color32[] ImportanceColors = new []
	{
		new Color32(0x0c, 0xd4, 0xd4, 0xff),
		new Color32(0x40, 0xd6, 0x57, 0xff),
		new Color32(0xff, 0x7d, 0x7d, 0xff),
		new Color32(0xaa, 0x54, 0xa0, 0xff),
	};
	
#pragma warning restore

	private struct BeatlineData
	{
		public BeatlineData(int timing, int importance)
		{
			Timing = timing;
			Importance = importance;
		}
		
		public readonly int Timing;
		public readonly int Importance;
	}
	
	[Serializable]
	public struct RenderBeatlineData
	{
		public RenderBeatlineData(int timing, Vector3[] linePoints, Vector3 labelPosition, Color color)
		{
			Timing = timing;
			LinePoints = linePoints;
			LabelPosition = labelPosition;
			Color = color;
		}

		public int Timing;
		public Vector3[] LinePoints;
		public Vector3 LabelPosition;
		public Color Color;
	}
	
	[HideInInspector] public List<RenderBeatlineData> RenderBeatlines = new List<RenderBeatlineData>();

	private readonly List<BeatlineData> _beatlines = new List<BeatlineData>();
	private GUIStyle _textStyle;

	/// <summary>
	/// 计算小节线时间点并计算需要渲染的小节线的详细数据
	/// </summary>
	[MethodButton("Calculate Beatline Timings", true)]
	private void CalculateBeatlineTimings()
	{
		_beatlines.Clear();
		RenderBeatlines.Clear();
		
		if (m_BeatlineDensity <= 0 || m_BeatlineDensity > 64) return;
		
		float currentTiming = 0;
		float interval = 60000 / m_Bpm / m_BeatlineDensity;
		int primaryCount = 0;
		int endTiming = m_StartTiming + m_RenderRange;
		while (currentTiming < endTiming)
		{
			int importance = primaryCount % m_BeatlineDensity == 0 ? 
				0 : 
				primaryCount % (m_BeatlineDensity / 2) == 0 ? 
					1 : primaryCount % (m_BeatlineDensity / 4) == 0 ? 
						2 : 
						3;
			_beatlines.Add(new BeatlineData((int)currentTiming, importance));
			currentTiming += interval;
			primaryCount++;
		}
		
		var currentPos = m_StartPosition;
		var rot = (RotationFromForward(m_TurnForward) - RotationFromForward(m_StartForward)) / 2;
		for (int i = 0; i < _beatlines.Count - 1; i++)
		{
			int timing = _beatlines[i].Timing;
			int nextTiming = _beatlines[i + 1].Timing;
			
			int dirTime = nextTiming - timing;
			float length = (dirTime + (i == 0 ? m_Offset : 0)) / 1000f * m_Speed;
			
			var targetForward = i % 2 == 0 ? m_StartForward : m_TurnForward;
			var nextPos = currentPos + targetForward * length;
			
			if (timing >= m_StartTiming)
			{
				var noRotCurrentPos = RotatePointAroundPivot(
					currentPos, 
					m_StartPosition, 
					-rot
				);
				var p1 = noRotCurrentPos;
				p1.x = -m_Scale + m_XOffset;
				var p2 = noRotCurrentPos;
				p2.x = m_Scale + m_XOffset;
				p1 = RotatePointAroundPivot(p1, m_StartPosition, m_Rotation);
				p2 = RotatePointAroundPivot(p2, m_StartPosition, m_Rotation);
				var color = ImportanceColors[_beatlines[i].Importance];
				var labelPos = noRotCurrentPos;
				labelPos.x = m_LabelPosition + m_XOffset;
				labelPos = RotatePointAroundPivot(labelPos, m_StartPosition, m_Rotation);
				RenderBeatlines.Add(new RenderBeatlineData(timing, new []{p1, p2}, labelPos, color));
			}
			
			currentPos = nextPos;
		}
	}
	
	/// <summary>
	/// 围绕一个中心点旋转一个点
	/// </summary>
	/// <param name="point">点</param>
	/// <param name="pivot">中心点</param>
	/// <param name="angles">角度</param>
	/// <returns>围绕中心点旋转后的点</returns>
	private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 dir = point - pivot;
		dir = Quaternion.Euler(angles) * dir;
		return dir + pivot;
	}
	
	/// <summary>
	/// 通过朝向获取旋转角度
	/// </summary>
	/// <param name="forward">朝向</param>
	/// <returns>由朝向获取的旋转角度</returns>
	private static Vector3 RotationFromForward(Vector3 forward)
	{
		return Quaternion.LookRotation(forward).eulerAngles;
	}

	private void RenderBeatline(RenderBeatlineData beatline)
	{
		Gizmos.color = beatline.Color;
		// Handles.DrawAAPolyLine(5, beatline.LinePoints);
		Gizmos.DrawLine(beatline.LinePoints[0], beatline.LinePoints[1]);
		// Gizmos.DrawCube(beatline.LinePoints[1], Vector3.one * 0.3f);
		if (!m_EnableText) return;
		Handles.Label(
			beatline.LabelPosition, 
			beatline.Timing.ToString(),
			_textStyle
		);
	}

	private void OnDrawGizmosUpdate()
	{
		if (_textStyle != null) return;
		_textStyle = new GUIStyle
		{
			normal = new GUIStyleState
			{
				textColor = Color.white,
				background = Color.black.WithAlpha(200).ToTexture2D()
			}
		};
	}

	private void OnDrawGizmos()
	{
		if (!m_Enable) return;
		OnDrawGizmosUpdate();
		foreach (var beatline in RenderBeatlines)
		{
			RenderBeatline(beatline);
		}
	}
}
