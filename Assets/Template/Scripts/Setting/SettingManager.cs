using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using DancingLineSample.Gameplay;
using DancingLineSample.UI.Components;
using DancingLineSample.Utility;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Newtonsoft.Json;
using DancingLineSample.Attributes;
using DancingLineSample.UI;
using UnityEngine.Events;

namespace DancingLineSample.Setting
{
	[Serializable]
	public class GameSettings
	{
		public int QualityLevel;
		public int FPSLimitIndex;
		public bool EnablePostProcessing;
		public int AudioOffset;
		public int DSPBufferSizeIndex = 1;
		public bool FullScreen = true;
		public bool EnableUIBlur = true;
	}
	[Serializable]
	public class QualitySettingEvent : UnityEvent<int> { }
	public class SettingManager : Singleton<SettingManager>
	{
#pragma warning disable
		
		[Space]
		[Header("Quality Settings")]
		[SerializeField] private SwitchButtonGroup m_QualityButtonButtonGroup;
		[SerializeField] private Text m_QualityNameText;
		[SerializeField] private string[] m_QualityNames;
		[Space]
		[Header("FPS Limit Settings")]
		[SerializeField] private SwitchButtonGroup m_FPSLimitButtonButtonGroup;
		[SerializeField] private Text m_FPSLimitText;
		[SerializeField] private int[] m_FPSLimitValues;
		[Space]
		[Header("Post Processing Settings")]
		[SerializeField] private ValueAnimationButton m_PostProcessingToggleButton;
		[SerializeField] private PostProcessVolume m_PostProcessingVolume;
		[Space]
		[Header("Full Screen Settings")]
		[SerializeField] private GameObject m_EnableFullScreenPanel;
		[SerializeField] private ValueAnimationButton m_EnableFullScreenButton;
		[Space]
		[Header("UI Blur Settings")]
		[SerializeField] private ValueAnimationButton m_EnableUIBlurButton;
		[Space]
		[Header("Other")]
		[SerializeField] private GameSettings m_DefaultSettings;
		[SerializeField] private QualitySettingEvent m_OnQualitySet = new QualitySettingEvent();
		
#pragma warning restore
		
		private static string _settingDataPath => Path.Combine(Application.persistentDataPath, "Setting.data");

		private static int _maxQualityLevel;
		private static int _maxFPSIndex;
		private GameSettings _settings;
		
		private Resolution _lastResolution;
		private Resolution _fullScreenResolution;

		protected override void OnAwake()
		{
#if UNITY_ANDROID || UNITY_IOS
			m_EnableFullScreenPanel.SetActive(false);
#endif
			_fullScreenResolution = Screen.currentResolution;
			_lastResolution = new Resolution() { width = Screen.width, height = Screen.height };
			_maxQualityLevel = QualitySettings.names.Length - 1;
			_maxFPSIndex = m_FPSLimitValues.Length - 1;
			// Quality Button Group
			m_QualityButtonButtonGroup.Init();
			m_QualityButtonButtonGroup.SetValueRange(0, _maxQualityLevel);
			// FPS Limit Button Group
			m_FPSLimitButtonButtonGroup.Init();
			m_FPSLimitButtonButtonGroup.SetValueRange(0, _maxFPSIndex);
			// Init
			AddButtonsListeners();
			LoadSettings();
		}
		
		private void AddButtonsListeners()
		{
			m_QualityButtonButtonGroup.onValueChanged.AddListener(SetQualityLevelInternal);
			m_FPSLimitButtonButtonGroup.onValueChanged.AddListener(SetFPSLimitInternal);
			m_PostProcessingToggleButton.onClick.AddListener(SetPostProcessingEnableInternal);
			m_EnableFullScreenButton.onClick.AddListener(SetFullScreenInternal);
			m_EnableUIBlurButton.onClick.AddListener(SetUIBlurEnableInternal);
		}

		#region Read / Write Settings From / To File
		
		private void AfterLoadSettings()
		{
			// 从设置读取值设置按钮状态
			m_QualityButtonButtonGroup.CurrentValue = _settings.QualityLevel;
			m_FPSLimitButtonButtonGroup.CurrentValue = _settings.FPSLimitIndex;
			m_PostProcessingToggleButton.SetActive(_settings.EnablePostProcessing);
			m_EnableFullScreenButton.SetActive(_settings.FullScreen);
			m_EnableUIBlurButton.SetActive(_settings.EnableUIBlur);
			// 从设置读取值设置状态
			m_PostProcessingVolume.enabled = _settings.EnablePostProcessing;
			AudioManager.Instance.SetOffset(_settings.AudioOffset);
			AudioManager.Instance.DSPBufferSizeIndex = _settings.DSPBufferSizeIndex;
			UIManager.Instance.BlurController.enableBlur = _settings.EnableUIBlur;
#if UNITY_STANDALONE_WIN
			Screen.fullScreen = _settings.FullScreen;
#endif
		}
		
		private void LoadSettings()
		{
			try
			{
				_settings = MsgPackHelper
					.TryReadAndDeserializeFromFile(_settingDataPath, m_DefaultSettings);
			}
			catch (Exception e)
			{
				_settings = m_DefaultSettings;
				Debug.LogException(e);
			}
		}

		private void Start()
		{
			// 写在这里避免在某些时候触发 Null Reference Exception
			AfterLoadSettings();
		}

		public void SaveSettings()
		{
#if UNITY_EDITOR
			byte[] bdata = MsgPackHelper.Serialize(m_DefaultSettings);
#else
			_settings.DSPBufferSizeIndex = AudioManager.Instance.DSPBufferSizeIndex;
			_settings.AudioOffset = AudioManager.Instance.AudioOffset;
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
			m_OnQualitySet?.Invoke(qualityLevel);

			if (qualityLevel >= m_QualityNames.Length) return;
			m_QualityNameText.text = m_QualityNames[qualityLevel];
		}
		
		public void AddSetQualityListener(UnityAction<int> listener) => m_OnQualitySet.AddListener(listener);
		public void RemoveSetQualityListener(UnityAction<int> listener) => m_OnQualitySet.RemoveListener(listener);
		public void InvokeSetQualityListener() => m_OnQualitySet?.Invoke(_settings.QualityLevel);

		#endregion

		#region FPS Limit Settings
		
		private void SetFPSLimitInternal(int index)
		{
			if (index < 0 || index > _maxFPSIndex) return;
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
		
		/// <summary>
		/// 设置后处理效果的启用状态
		/// </summary>
		/// <param name="enable">启用状态</param>
		private void SetPostProcessingEnableInternal(bool enable)
		{
			_settings.EnablePostProcessing = enable;
			m_PostProcessingVolume.enabled = enable;
		}
		
		#endregion

		#region Full Screen Settings

		/// <summary>
		/// 设置是否全屏
		/// </summary>
		/// <param name="fullScreen">全屏</param>
		private void SetFullScreenInternal(bool fullScreen)
		{
#if UNITY_STANDALONE_WIN
			if (fullScreen)
			{
				_lastResolution = new Resolution() { width = Screen.width, height = Screen.height };
			}
			_settings.FullScreen = fullScreen;
			var targetResolution = fullScreen ? _fullScreenResolution : _lastResolution;
			Screen.SetResolution(targetResolution.width, targetResolution.height, fullScreen);
#endif
		}

		#endregion

		#region Blur UI Settings

		/// <summary>
		/// 设置 UI 模糊的启用状态
		/// </summary>
		/// <param name="enableBlur">启用状态</param>
		private void SetUIBlurEnableInternal(bool enableBlur)
		{
			_settings.EnableUIBlur = enableBlur;
			UIManager.Instance.BlurController.enableBlur = enableBlur;
		}

		#endregion
		
		private void OnApplicationQuit()
		{
			SaveSettings();
		}
	}
}