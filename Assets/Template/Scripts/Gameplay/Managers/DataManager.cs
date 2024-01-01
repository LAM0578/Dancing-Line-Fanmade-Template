using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DancingLineSample.Utility;
using UnityEngine;

namespace DancingLineSample.Gameplay
{
	public class DataManager : Singleton<DataManager>
	{

		public bool LoadLevelOnAwake = true;
		public bool SingleLevel = true;

		[PropertyActive("SingleLevel", false)] 
		public LevelData Level;
		
		public LevelData[] Levels = { };
		
		private Dictionary<string, LevelGameplayData> LevelDatas = new Dictionary<string, LevelGameplayData>();
		private static string DataSavePath => Path.Combine(Application.persistentDataPath, "Save.data");

		public void SaveLevelData(string levelId, LevelGameplayData levelData)
		{
			if (LevelDatas.ContainsKey(levelId))
			{
				LevelDatas[levelId] = levelData;
				return;
			}
			LevelDatas.Add(levelId, levelData);
		}

		public void LoadData()
		{
			// print(DataSavePath);
			LevelDatas = MsgPackHelper.TryReadAndDeserializeFromFile(DataSavePath, LevelDatas);
			if (SingleLevel)
			{
				if (!Level || !LevelDatas.ContainsKey(Level.ID)) return;
				Level.LoadData(LevelDatas[Level.ID]);
			}
			else
			{
				foreach (var lvl in Levels.Where(t => t))
				{
					if (!LevelDatas.ContainsKey(lvl.ID)) continue;
					lvl.LoadData(LevelDatas[lvl.ID]);
				}
			}
		}

		public void SaveData()
		{
			if (SingleLevel && Level)
			{
				Level.SaveData();
			}
			else
			{
				foreach (var lvl in Levels.Where(t => t))
				{
					lvl.SaveData();
				}
			}
			
			byte[] bdata = MsgPackHelper.Serialize(LevelDatas);
			File.WriteAllBytes(DataSavePath, bdata);
		}

		private void LoadLevel()
		{
			if (!LoadLevelOnAwake) return;
			if (SingleLevel)
			{
				if (Level == null) return;
				GameplayManager.Instance.LoadLevel(Level);
			}
			else
			{
				if (Levels.Length <= 0 || Levels[0] == null) return;
				GameplayManager.Instance.LoadLevel(Levels[0]);
			}
		}

		protected override void OnAwake()
		{
			LoadData();
		}

		private void Start()
		{
			LoadLevel();
		}

		private void OnApplicationQuit()
		{
			SaveData();
		}
	}
}