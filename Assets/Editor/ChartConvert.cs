using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DancingLineSample.Gameplay;
using UnityEditor;
using UnityEngine;

public class ChartConvert : EditorWindow
{
	private AutoPlayManager _autoPlayManager;
	private GameplayManager _gameplayManager;

	private bool TryGetInstances()
	{
		if (!_autoPlayManager)
		{
			_autoPlayManager = UnityUtility.FindObjectFromCurrentScene<AutoPlayManager>();
			if (!_autoPlayManager)
			{
				return false;
			}
		}
		
		if (!_gameplayManager)
		{
			_gameplayManager = UnityUtility.FindObjectFromCurrentScene<GameplayManager>();
			if (!_gameplayManager)
			{
				return false;
			}
		}
		
		return true;
	}

	private void SetDatasInternal((int audioOffset, List<int> timings) datas)
	{
		if (!TryGetInstances()) return;
		
		_gameplayManager.AudioOffset = datas.audioOffset;
		_autoPlayManager.LoadAutoData(datas.timings);
	}

	[MenuItem("EditorTools/Chart Converter")]
	public static void ShowWindow()
	{
		var window = GetWindow<ChartConvert>();
		window.titleContent = new GUIContent("Chart Convert to Road");
		window.Show();
	}

	private void OnGUI()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Open Arcaea (*.aff) Chart"))
		{
			var reader = new AFFToRoad();
			var datas = reader.ReadChart();
			SetDatasInternal(datas);
		}

		if (GUILayout.Button("Open Osu (*.osu) Chart"))
		{
			var reader = new OSUChartToRoad();
			var datas = reader.ReadChart();
			SetDatasInternal(datas);
		}
		GUILayout.EndHorizontal();
	}
}