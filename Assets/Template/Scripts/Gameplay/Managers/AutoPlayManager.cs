using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DancingLineSample.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay
{
	[Serializable]
	public class AutoHitData
	{
		public AutoHitData(int timing) => Timing = timing;

		public int Timing;
		public bool Actived { get; private set; }

		public void Reset() => Actived = false;
		public void Active() => Actived = true;
	}
	public class AutoPlayManager : Singleton<AutoPlayManager>
	{
		[SerializeField]
		private List<AutoHitData> currentHitData = new List<AutoHitData>();

		public bool EnableAuto;
		public int AutoPlayOffset;
		
#if UNITY_EDITOR // 用于测试自动打击的准确度（？

		[Space] 
		public bool PlayHitSound;
		
		[PropertyActive("PlayHitSound", false)]
		public AudioSource TestSource;
		[PropertyActive("PlayHitSound", false)]
		public AudioClip TestClip;
		
#endif

		public void LoadAutoData(IEnumerable<int> hitTimings)
		{
			currentHitData = hitTimings.Where(t => t > 0).Select(t => new AutoHitData(t)).ToList();
		}
		
		public List<int> GetTimingsFromCurrentHitData()
		{
			return currentHitData.Select(t => t.Timing).ToList();
		}

		private void Update()
		{
			if (!EnableAuto || GameplayManager.Instance.LineStatus != PlayerStatus.Playing) return;
			int curTiming = GameplayManager.Instance.CurrentTiming - AutoPlayOffset;
			foreach (var data in currentHitData)
			{
				if (curTiming < data.Timing || data.Actived) continue;
				data.Active();
				GameplayManager.Instance.Line.Turn();
#if UNITY_EDITOR
				if (!PlayHitSound) continue;
				TestSource.PlayOneShot(TestClip);
#endif
			}
		}

		public void ResetAutoHitDataStatus()
		{
			foreach (var data in currentHitData)
			{
				data.Reset();
			}
		}

		public void ActiveAutoHitDatasByTiming(int checkpointCheckpointTime)
		{
			int curTiming = checkpointCheckpointTime - AutoPlayOffset;
			foreach (var data in currentHitData)
			{
				if (curTiming < data.Timing || data.Actived) continue;
				data.Active();
			}
		}
	}
	#if UNITY_EDITOR
	[CustomEditor(typeof(AutoPlayManager))]
	public class AutoPlayManagerEditor : Editor
	{
		private static IEnumerable<int> ParseString(string s)
		{
			s = s.Trim('[', ']');
			string[] parts = s.Split(',');
			var result = new List<int>();
			foreach (string part in parts)
			{
				if (!int.TryParse(part.Trim(), out int value)) continue;
				result.Add(value);
			}
			return result;
		}

		private string data;
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (AutoPlayManager)target;

			data = GUILayout.TextArea(data);
			if (GUILayout.Button("Parse"))
			{
				var lst = ParseString(data);
				obj.LoadAutoData(lst);
			}
		}
	}
	#endif
}