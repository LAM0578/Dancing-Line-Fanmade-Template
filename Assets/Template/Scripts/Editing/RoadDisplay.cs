using System;
using System.Collections.Generic;
using System.Linq;
using DancingLineSample.Editing.Utility;
using DancingLineSample.Gameplay;
using DancingLineSample.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.XR;

namespace DancingLineSample.Editing
{
	public class RoadDisplay : Singleton<RoadDisplay>
	{
#pragma warning disable
		
		[SerializeField] private bool m_Enable;
		[Space]
		[SerializeField] private Vector3 m_StartPosition;
		[Space]
		[SerializeField] private Color m_LineColor;
		[SerializeField] private Color m_TextColor;
		[SerializeField] private Color m_TextBackgroundColor;
		[Space]
		[SerializeField] private Vector3 m_StartForward;
		[SerializeField] private Vector3 m_TurnForward;
		[Space]
		[SerializeField] private float m_LineSpeed;
		[SerializeField] private int m_Offset;
		[Space] 
		[SerializeField] private float m_PointRadius;
		[SerializeField] private bool m_ShowText;
		
#pragma warning restore

		[Serializable]
		public class Point
		{
			public Point(int timing, Vector3 position)
			{
				Timing = timing;
				Position = position;
			}
			
			public int Timing;
			public Vector3 Position;
		}
		
		[Serializable]
		public class Line
		{
			public Line(Point from, Vector3 to)
			{
				From = from;
				To = to;
			}
			
			public Point From;
			public Vector3 To;
		}

		[HideInInspector] public List<int> Timings = new List<int>();
		[HideInInspector] public List<Line> Lines = new List<Line>();
		
		private Vector3 _startPosition;
		private Color _textColor;
		private Color _textBackgroundColor;
		private Vector3 _startForward;
		private Vector3 _turnForward;
		private float _lineSpeed;
		private int _offset;
		private Texture2D _textBackground;
		private GUIStyle _style;

#if UNITY_EDITOR
		
		internal void SetTimings(IEnumerable<int> timings)
		{
			Timings = timings.OrderBy(t => t).ToList();
			CalculateLines();
		}

		public void CalculateLines()
		{
			Lines.Clear();
			int sameTimeCount = 0;
			var curPos = m_StartPosition;
			for (int i = 0; i < Timings.Count; i++)
			{
				int time = i == 0 ? 0 : Timings[i - 1] + m_Offset;
				int nextTime = Timings[i] + m_Offset;
				if (time < 0)
					time = 0;
				if (nextTime < 0)
					nextTime = 0;
				if (nextTime == time)
				{
					sameTimeCount++;
					continue;
				}
				int difTime = nextTime - time;
				float length = difTime / 1000f * m_LineSpeed;
				var targetForward = (i + sameTimeCount) % 2 == 0 ? m_StartForward : m_TurnForward;
				var nextPos = curPos + targetForward * length;
				Lines.Add(new Line(new Point(time, curPos), nextPos));
				curPos = nextPos;
			}
		}

		private void UpdateValues()
		{
			if (m_ShowText)
			{
				if (_textBackgroundColor != m_TextBackgroundColor)
				{
					if (_textBackground)
					{
						DestroyImmediate(_textBackground);
					}
					_textBackgroundColor = m_TextBackgroundColor;
					_textBackground = _textBackgroundColor.ToTexture2D();
					createNewGUIStyle();
				}

				if (_textColor != m_TextColor)
				{
					_textColor = m_TextColor;
					createNewGUIStyle();
				}
			}

			bool hasChange = false;

			if (_startPosition != m_StartPosition)
			{
				_startPosition = m_StartPosition;
				hasChange = true;
			}

			if (_startForward != m_StartForward)
			{
				_startForward = m_StartForward;
				hasChange = true;
			}

			if (_turnForward != m_TurnForward)
			{
				_turnForward = m_TurnForward;
				hasChange = true;
			}
			
			if (Math.Abs(_lineSpeed - m_LineSpeed) > float.Epsilon)
			{
				_lineSpeed = m_LineSpeed;
				if (_lineSpeed < 0)
				{
					print("LineSpeed can not be negative");
					m_LineSpeed = 0;
				}
				hasChange = true;
			}
			
			if (_offset != m_Offset)
			{
				_offset = m_Offset;
				hasChange = true;
			}
			
			if (hasChange)
				CalculateLines();
			
			return;

			void createNewGUIStyle()
			{
				_style = new GUIStyle
				{
					normal = new GUIStyleState
					{
						textColor = m_TextColor,
						background = _textBackground
					}
				};
			}
		}

		public void OnDrawGizmos()
		{
			if (!m_Enable || Timings.Count == 0) return;
			
			if (Lines.Count == 0)
				CalculateLines();
			
			if (Lines.Count == 0) return;
			
			UpdateValues();
			
			Gizmos.color = m_LineColor;

			for (int i = 0; i < Lines.Count; i++)
			{
				var line = Lines[i];
				Gizmos.DrawLine(line.From.Position, line.To);
				Gizmos.DrawCube(line.To, Vector3.one * m_PointRadius);
				if (m_ShowText)
				{
					Handles.Label(
						line.From.Position, 
						line.From.Timing.ToString(), 
						_style
					);
				}
				if (i + 1 >= Lines.Count)
				{
					var targetForward = Lines.Count % 2 != 0 ? m_StartForward : m_TurnForward;
					Gizmos.DrawRay(line.To, targetForward * 1000f);
				}
			}
			if (m_ShowText)
			{
				Handles.Label(
					Lines[Lines.Count - 1].To, 
					Timings[Timings.Count - 1].ToString(),
					_style
				);
			}
		}		

#endif 
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(RoadDisplay))]
	public class RoadDisplayEditor : Editor
	{
		private AutoPlayManager _autoPlayManager;
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var item = (RoadDisplay)target;
			if (GUILayout.Button("Copy timings from AutoPlayManager"))
			{
				if (!_autoPlayManager)
				{
					_autoPlayManager = Resources.FindObjectsOfTypeAll<AutoPlayManager>()[0];
				}
				item.SetTimings(_autoPlayManager.GetTimingsFromCurrentHitData());
			}

			if (GUILayout.Button("Calculate lines"))
			{
				item.CalculateLines();
			}
		}
	}
#endif
}