using System.Collections.Generic;
using System.IO;
using DancingLineSample;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text;
using DancingLineSample.Gameplay;

public class OSUChartToRoad
{
	private static (int audioOffset, List<int> timings) GetDatasFromChart(string chartData)
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

				int timing = (int)float.Parse(line.Split(',')[2]);

				if (!timings.Contains(timing))
				{
					timings.Add(timing);
				}
			}
		}
		
		return (audioOffset, timings);
	}

	/// <summary>
	/// 选择一个文件作为 OSU 谱面并解析为时间点
	/// </summary>
	public (int audioOffset, List<int> timings) ReadChart()
	{
		var path = EditorUtility.OpenFilePanel("Select a Osu Chart File (*.osu)", "", "osu");
		if (string.IsNullOrEmpty(path)) throw new FileNotFoundException();
		var chartData = File.ReadAllText(path);
		return GetDatasFromChart(chartData);
	}
}