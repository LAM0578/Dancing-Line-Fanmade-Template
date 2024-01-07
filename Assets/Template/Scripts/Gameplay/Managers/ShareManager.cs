using System;
using System.Collections.Generic;
using System.IO;
using DancingLineSample.Utility;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Path = System.IO.Path;
using DancingLineSample.Attributes;

namespace DancingLineSample.Gameplay
{
	public class ShareManager : Singleton<ShareManager>
	{
#pragma warning disable
		[SerializeField] private RawImage m_TargetRawImage;
		[SerializeField] private Camera m_ResultUICamera;
		[SerializeField] private Camera m_GameplayCamera;
		[Space]
		[SerializeField] private Text m_TitleText;
		[SerializeField] private Text m_CollectCubeCountText;
		[SerializeField] private Image[] m_CrownImages;
		[SerializeField] private Slider m_ProgressSlider;
		[SerializeField] private Text m_ProgressText;
		[Space] 
		[SerializeField] private Material m_CrownMaterial;
#pragma warning restore

		private RenderTexture m_ShareTempRenderTexture, m_ShareRenderTexture;
		private List<PostProcessEffectSettings> m_PostEffectSettings = new List<PostProcessEffectSettings>();
		private readonly List<Material> m_CrownMaterialInstances = new List<Material>();
		
		private static readonly int _m_Enable = Shader.PropertyToID("_Enable");
		private LevelGameplayData _currentLevelGameplayData;
		private Texture _shareTexture;

		protected override void OnAwake()
		{
			foreach (var img in m_CrownImages)
			{
				var mat = Instantiate(m_CrownMaterial);
				img.material = mat;
				m_CrownMaterialInstances.Add(mat);
			}
			
			Directory.CreateDirectory(ShareImageDirectory);
			
			var postVol = m_GameplayCamera.GetComponent<PostProcessVolume>();
			if (!postVol) return;
			var profile = postVol.profile;
			if (!profile) return;
			m_PostEffectSettings = profile.settings;
		}

		private PostProcessEffectSettings MotionBlurSetting
			=> m_PostEffectSettings.Find(t => t is MotionBlur);

		private static string ShareImageDirectory
			=> Path.Combine(
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				Application.persistentDataPath, 
#else
				Path.GetDirectoryName(Application.dataPath) ?? Application.dataPath, 
#endif
				"ShareImages"
			);

		private void SetTexture(out bool hasMontionBlurSetting, out bool settingActive)
		{
			if (m_ShareTempRenderTexture)
			{
				RenderTexture.ReleaseTemporary(m_ShareTempRenderTexture);
			}

			var mbs = MotionBlurSetting;
			settingActive = mbs && mbs.active;

			if (mbs) mbs.active = false;
			hasMontionBlurSetting = mbs;

			var cam = m_GameplayCamera;
			
			m_ShareTempRenderTexture = RenderTexture.GetTemporary(1024, 1024, 0);
			
			cam.targetTexture = m_ShareTempRenderTexture;
			cam.Render();
			cam.targetTexture = null;
		}

		/// <summary>
		/// 保存关卡分享图
		/// </summary>
		[MethodButton("SaveShareImage")]
		public void SaveShareImage()
		{
			bool useShareImage = _currentLevelGameplayData.Progress >= 1 && _shareTexture;
			string path = Path.Combine(ShareImageDirectory, $"{DateTime.Now.Ticks}.jpg");
			
			#region setBaseImage

			if (useShareImage)
			{
				m_TargetRawImage.texture = _shareTexture;
				m_TargetRawImage.rectTransform.sizeDelta = new Vector2(1024, 1024);
			}
			else
			{
				SetTexture(out bool hasMontionBlurSetting, out bool settingActive);

				m_TargetRawImage.texture = m_ShareTempRenderTexture;
				m_TargetRawImage.SetNativeSize();
			
				if (hasMontionBlurSetting) MotionBlurSetting.active = settingActive;
			}
			
			if (m_ShareRenderTexture) RenderTexture.ReleaseTemporary(m_ShareRenderTexture);
			
			#endregion End of setBaseImage
			
			#region m_ResultUICamera

			var cam = m_ResultUICamera;
			cam.gameObject.SetActive(true);
			
			m_ShareRenderTexture = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 0);

			var lRt = cam.targetTexture;
			
			cam.targetTexture = m_ShareRenderTexture;
			cam.Render();
			m_ShareRenderTexture.SaveTo(path, true);
			
			cam.targetTexture = lRt;
			cam.gameObject.SetActive(false);
			
			#endregion End of m_ResultUICamera
		}

		/// <summary>
		/// 加载关卡分享图
		/// </summary>
		/// <param name="tex">关卡分享图</param>
		public void LoadShareTexture(Texture tex)
		{
			_shareTexture = tex;
		}

		/// <summary>
		/// 从关卡游玩数据设置分享图元素
		/// </summary>
		/// <param name="title">关卡标题</param>
		/// <param name="data">从关卡游玩数据</param>
		public void SetData(string title, LevelGameplayData data)
		{
			_currentLevelGameplayData = data;
			
			m_TitleText.text = title;

			m_CollectCubeCountText.text = $"{data.CollectCount} / 10";
			
			m_ProgressText.text = $"{(data.Progress * 100):F0}%";
			m_ProgressSlider.value = data.Progress;
			
			for (int i = 0; i < m_CrownMaterialInstances.Count; i++)
			{
				var mat = m_CrownMaterialInstances[i];
				mat.SetFloat(_m_Enable, 0);
				if (i + 1 > data.CheckpointCount) continue;
				mat.SetFloat(_m_Enable, 1);
			}
		}
	}
}

