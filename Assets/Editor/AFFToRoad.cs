using System.Collections.Generic;
using System.IO;
using DancingLineSample;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text;
using DancingLineSample.Gameplay;

public class AFFToRoad : EditorWindow
{
	private AutoPlayManager _autoPlayManager;
	private GameplayManager _gameplayManager;
	
	private static int ReadInternal(string eventContent, int skipCount)
	{
		var rawDatas = eventContent.SubStringByIndex(skipCount, -1).Split(',');
		return int.Parse(rawDatas[0]); 
	}

	private void SetTimingsFromChart(string chartData)
	{
		int audioOffset = 0;
		var timings = new List<int>();

		var lines = chartData.Replace("\r", "").Split('\n').ToList();
		int splitIndex = lines.IndexOf("-");

		for (int i = 0; i < splitIndex; i++)
		{
			var line = lines[i];
			if (line.StartsWith("AudioOffset:"))
			{
				audioOffset = int.Parse(line.Replace("AudioOffset:", "").Trim());
				break;
			}
		}

		for (int i = splitIndex + 1; i < lines.Count; i++)
		{
			var eventSplits = lines[i].Trim().Split(';');
			foreach (string eventSplit in eventSplits)
			{
				var timingGroupSplits = eventSplit.Split('{');
				foreach (string timingGroupSplit in timingGroupSplits)
				{
					var eventContent = timingGroupSplit.Trim();
					int timing;
					if (eventContent.StartsWith("("))
					{
						timing = ReadInternal(eventContent, 1);
					}
					else if (eventContent.StartsWith("hold"))
					{
						timing = ReadInternal(eventContent, 5);
					}
					else if (eventContent.StartsWith("arc"))
					{
						timing = ReadInternal(eventContent, 5);
					}
					else
					{
						continue;
					}

					if (!timings.Contains(timing))
					{
						timings.Add(timing);
					}
				}
			}
		}

		Debug.Log($"AudioOffset: {audioOffset}");
		Debug.Log($"Timings: {string.Join(",", timings)}");

		if (!_autoPlayManager)
		{
			_autoPlayManager = UnityUtility.FindObjectFromCurrentScene<AutoPlayManager>();
			if (!_autoPlayManager)
			{
				return;
			}
		}
		
		if (!_gameplayManager)
		{
			_gameplayManager = UnityUtility.FindObjectFromCurrentScene<GameplayManager>();
			if (!_gameplayManager)
			{
				return;
			}
		}
		
		_gameplayManager.AudioOffset = audioOffset;
		_autoPlayManager.LoadAutoData(timings);
	}

	/// <summary>
	/// 选择一个文件作为 Arcaea 谱面并解析为时间点
	/// </summary>
	private void ReadChart()
	{
		var path = EditorUtility.OpenFilePanel("Select a Arcaea Chart File (*.aff)", "", "aff");
		if (string.IsNullOrEmpty(path)) return;
		var chartData = File.ReadAllText(path);
		SetTimingsFromChart(chartData);
	}

	[MenuItem("EditorTools/ChartConvert/Arcaea")]
	public static void ShowWindow()
	{
		var window = GetWindow<AFFToRoad>();
		window.titleContent = new GUIContent("Chart Convert - Arcaea");
		window.Show();
	}

	private void OnGUI()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Open Chart"))
		{
			ReadChart();
		}
		GUILayout.EndHorizontal();
	}
}