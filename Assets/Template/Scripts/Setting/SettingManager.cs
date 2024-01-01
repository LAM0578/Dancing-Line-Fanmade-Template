using System;
using System.IO;
using DancingLineSample.Gameplay;
using DancingLineSample.UI.Components;
using DancingLineSample.Utility;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace DancingLineSample.Setting
{
	[Serializable]
	public class GameSettings
	{
		public int QualityLevel;
		public int FPSLimitIndex;
		public bool EnablePostProcessing;
	}
	public class SettingManager : Singleton<SettingManager>
	{
#pragma warning disable
		
		[Header("Quality Settings")]
		[Space]
		[SerializeField] private SwitchButtonGroup m_QualityButtonButtonGroup;
		[SerializeField] private Text m_QualityNameText;
		[SerializeField] private string[] m_QualityNames;
		[Space]
		[Header("FPS Limit Settings")]
		[SerializeField] private SwitchButtonGroup m_FPSLimitButtonButtonGroup;
		[SerializeField] private Text m_FPSLimitText;
		[SerializeField] private int[] m_FPSLimitValues;
		[Header("Post Processing Settings")]
		[SerializeField] private ValueAnimationButton m_PostProcessingToggleButton;
		[SerializeField] private PostProcessVolume m_PostProcessingVolume;
		[Space]
		[SerializeField] private GameSettings m_DefaultSettings;
		
#pragma warning restore
		
		private static string _settingDataPath => Path.Combine(Application.persistentDataPath, "Setting.data");

		private static int _maxQualityLevel;
		private static int _maxFPSLimitCount;
		private GameSettings _settings;

		protected override void OnAwake()
		{
			_maxQualityLevel = QualitySettings.names.Length - 1;
			_maxFPSLimitCount = m_FPSLimitValues.Length - 1;
			// Quality Button Group
			m_QualityButtonButtonGroup.Init();
			m_QualityButtonButtonGroup.SetValueRange(0, _maxQualityLevel);
			// FPS Limit Button Group
			m_FPSLimitButtonButtonGroup.Init();
			m_FPSLimitButtonButtonGroup.SetValueRange(0, _maxFPSLimitCount);
			// Init
			AddButtonsListeners();
			LoadSettings();
		}
		
		private void AddButtonsListeners()
		{
			m_QualityButtonButtonGroup.onValueChanged.AddListener(SetQualityLevelInternal);
			m_FPSLimitButtonButtonGroup.onValueChanged.AddListener(SetFPSLimitInternal);
			m_PostProcessingToggleButton.onClick.AddListener(SetPostProcessingEnable);
		}

		#region Read / Write Settings From / To File
		
		private void AfterLoadSettings()
		{
			m_QualityButtonButtonGroup.CurrentValue = _settings.QualityLevel;
			m_FPSLimitButtonButtonGroup.CurrentValue = _settings.FPSLimitIndex;
			m_PostProcessingToggleButton.SetActive(_settings.EnablePostProcessing);
			SetPostProcessingEnableInternal(_settings.EnablePostProcessing);
		}
		
		private void LoadSettings()
		{
			_settings = MsgPackHelper
				.TryReadAndDeserializeFromFile(_settingDataPath, m_DefaultSettings);
			AfterLoadSettings();
		}
		
		public void SaveSettings()
		{
#if UNITY_EDITOR
			byte[] bdata = MsgPackHelper.Serialize(m_DefaultSettings);
#else
			byte[] bdata = MsgPackHelper.Serialize(_settings);
			print(JsonConvert.SerializeObject(_settings));
#endif
			File.WriteAllBytes(_settingDataPath, bdata);
		}
		
		#endregion

		#region Quality Settings
		
		private void SetQualityLevelInternal(int qualityLevel)
		{
			if (qualityLevel < 0 || qualityLevel > _maxQualityLevel) return;
			
			_settings.QualityLevel = qualityLevel;
			QualitySettings.SetQualityLevel(qualityLevel);

			if (qualityLevel >= m_QualityNames.Length) return;
			m_QualityNameText.text = m_QualityNames[qualityLevel];
		}

		#endregion

		#region FPS Limit Settings
		
		private void SetFPSLimitInternal(int index)
		{
			if (index < 0 || index > _maxFPSLimitCount) return;
			int fpsLimit = m_FPSLimitValues[index];
			_settings.FPSLimitIndex = index;
#if UNITY_ANDROID || UNITY_IOS
			Application.targetFrameRate = fpsLimit == -1 ? int.MaxValue : fpsLimit;
#else
			Application.targetFrameRate = fpsLimit;
#endif
			QualitySettings.vSyncCount = fpsLimit == -1 ? 1 : 0;
			m_FPSLimitText.text = fpsLimit == -1 ? "Unlimited" : fpsLimit.ToString();
		}
		
		#endregion
		
		#region Post Processing Settings
		
		private void SetPostProcessingEnableInternal(bool enable)
		{
			_settings.EnablePostProcessing = enable;
			m_PostProcessingVolume.enabled = enable;
		}
		
		/// <summary>
		/// 设置后处理效果的状态
		/// </summary>
		public void SetPostProcessingEnable(bool enable)
		{
			_settings.EnablePostProcessing = enable;
			SetPostProcessingEnableInternal(_settings.EnablePostProcessing);
		}
		
		#endregion

		private void OnApplicationQuit()
		{
			SaveSettings();
		}

#if UNITY_EDITOR
		[Button("ToggleQuality")]
		public void ToggleQuality()
		{
			int qualityLevel = (_settings.QualityLevel + 1) % _maxQualityLevel;
			_settings.QualityLevel = qualityLevel;
			QualitySettings.SetQualityLevel(qualityLevel);
		}
#endif
	}
}