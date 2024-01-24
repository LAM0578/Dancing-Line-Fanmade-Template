using System.Collections.Generic;
using System.IO;
using DancingLineSample;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text;
using DancingLineSample.Gameplay;

public class AFFToRoad
{
	private static int ReadInternal(string eventContent, int skipCount)
	{
		var rawDatas = eventContent.SubStringByIndex(skipCount, -1).Split(',');
		return int.Parse(rawDatas[0]); 
	}

	private static (int audioOffset, List<int> timings) GetDatasFromChart(string chartData)
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
		
		return (audioOffset, timings);
	}

	/// <summary>
	/// 选择一个文件作为 Arcaea 谱面并解析为时间点
	/// </summary>
	public (int audioOffset, List<int> timings) ReadChart()
	{
		var path = EditorUtility.OpenFilePanel("Select a Arcaea Chart File (*.aff)", "", "aff");
		if (string.IsNullOrEmpty(path)) throw new FileNotFoundException();
		var chartData = File.ReadAllText(path);
		return GetDatasFromChart(chartData);
	}
}