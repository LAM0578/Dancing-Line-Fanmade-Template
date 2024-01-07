using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using MsgPack;
using DancingLineSample.Attributes;

namespace DancingLineSample.Gameplay
{
	public class ResultManager : Singleton<ResultManager>
	{
		[Header("Result UI Components")] 
		public Image[] CrownImages;
		public Material CrownMaterial;
		[Space]
		public Text CollectCountText;
		public Text TitleText;
		public Text ProgressText;
		public Slider ProgressSlider;

		public LevelData CurrentLevelData { get; private set; }
		
		private static readonly List<Material> m_CrownMaterialInstances = new List<Material>();
		private static readonly int _m_Enable = Shader.PropertyToID("_Enable");

		protected override void OnAwake()
		{
			foreach (var img in CrownImages)
			{
				var mat = Instantiate(CrownMaterial);
				img.material = mat;
				m_CrownMaterialInstances.Add(mat);
			}
		}

#if UNITY_EDITOR
		private void OnApplicationQuit()
		{
			foreach (var mat in m_CrownMaterialInstances)
			{
				Destroy(mat);
			}
		}
#endif

		private void EnableCrowns(int count)
		{
			for (int i = 0; i < m_CrownMaterialInstances.Count; i++)
			{
				var mat = m_CrownMaterialInstances[i];
				mat.SetFloat(_m_Enable, 0);
				if (i + 1 <= count)
					mat.DOFloat(1f, _m_Enable, 0.5f)
						.SetEase(Ease.OutExpo)
						.SetDelay(0.4f * (i + 1));
			}
		}

		public void LoadLevel(LevelData data)
		{
			CurrentLevelData = data;
			TitleText.text = data.Name;
		}

		/// <summary>
		/// 从关卡游玩数据设置 UI 元素
		/// </summary>
		/// <param name="data">从关卡游玩数据</param>
		public void SetResult(LevelGameplayData data)
		{
			string progressStr = $"{(data.Progress * 100):F0}%";
			string collectCubeCountStr = $"{data.CollectCount} / 10";
			ProgressSlider.value = 0;
			ProgressSlider.DOValue(data.Progress, 1f).SetDelay(0.5f).SetEase(Ease.OutExpo);
			ProgressText.text = progressStr;
			CollectCountText.text = collectCubeCountStr;
			EnableCrowns(data.CheckpointCount);
			ShareManager.Instance.SetData(TitleText.text, data);
			CurrentLevelData.LoadData(data);
		}

		private void Add<T>(ref T[] items, T item)
		{
			var lst = items.ToList();
			lst.Add(item);
			items = lst.ToArray();
		}

		[MethodButton("Test crown")]
		public void TestCrown()
		{
			EnableCrowns(3);
		}
	}
}
