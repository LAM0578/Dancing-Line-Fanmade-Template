using System.Collections.Generic;
using System.IO;
using DancingLineSample;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text;
using DancingLineSample.Gameplay;

public class OSUToRoad : EditorWindow
{
	private AutoPlayManager _autoPlayManager;
	private GameplayManager _gameplayManager;

	private void SetTimingsFromChart(string chartData)
	{
		int audioOffset = 0;
		bool isReading = false;
		var timings = new List<int>();

		var lines = chartData.Replace("\r", "").Split('\n').ToList();
		int splitIndex = lines.Count;

		for (int i = 0; i < splitIndex; i++)
		{
			var line = lines[i];
			if (line == "[TimingPoints]")
            {
				//第一个时间点视为歌曲偏移值
				audioOffset = (int)float.Parse(lines[i + 1].Split(',')[0]);
			}
            else if (line == "[HitObjects]" && (!isReading))
			{
				isReading = true;
			}
			else if (isReading)
			{
				if (string.IsNullOrEmpty(line)) break;

				int timing;

				timing = (int)float.Parse(line.Split(',')[2]);

				if (!timings.Contains(timing))
				{
					timings.Add(timing);
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

	private void ReadChart()
	{
		var path = EditorUtility.OpenFilePanel("Select a Osu Chart File (*.osu)", "", "osu");
		if (string.IsNullOrEmpty(path)) return;
		var chartData = File.ReadAllText(path);
		SetTimingsFromChart(chartData);
	}

	[MenuItem("EditorTools/ChartConvert/Osu")]
	public static void ShowWindow()
	{
		var window = GetWindow<OSUToRoad>();
		window.titleContent = new GUIContent("Chart Convert - Osu");
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