using System;
using DancingLineSample.Gameplay.Objects;
using DancingLineSample.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay
{
	[Serializable]
	public class LevelGameplayData
	{
		private float _progress;
		private int _collectCount;
		private int _crownCount;
		
		[HideInInspector] public float Progress;
		[HideInInspector] public int CollectCount;
		[HideInInspector] public int CheckpointCount;

		public LevelGameplayData(LevelGameplayData data)
		{
			Progress = data.Progress;
			CollectCount = data.CollectCount;
			CheckpointCount = data.CheckpointCount;
		}

		public LevelGameplayData(float progress, int collectCount, int checkpointCount)
		{
			Progress = progress;
			CollectCount = collectCount;
			CheckpointCount = checkpointCount;
		}
		
		public LevelGameplayData() { }
	}
	
	[Serializable]
	[CreateAssetMenu(fileName="LevelData", menuName="ScriptableObject/LevelData")]
	public class LevelData : ScriptableObject
	{
		public string ID;
		public string Name;
		[Space]
		public float LineSpeed;
		public int CollectCubeCount;
		[Space] 
		public Vector3 ResetPosition;
		public Vector3 StartForward;
		public Vector3 TurnForward;
		[Space]
		public AudioClip Music;
		[Space]
		public ResetObjects ResetObjects;

		[HideInInspector] public LevelGameplayData GameplayData;

		public void SaveData()
		{
#if !UNITY_EDITOR
			DataManager.Instance.SaveLevelData(ID, GameplayData);
#endif
		}

		public void LoadData(LevelGameplayData data)
		{
			GameplayData = data;
		}
	}
}
