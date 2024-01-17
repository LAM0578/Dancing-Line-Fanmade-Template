using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DancingLineSample.Attributes;
using DancingLineSample.UI.Components;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DancingLineSample.Gameplay
{
	public class AudioManager : Singleton<AudioManager>
	{
#pragma warning disable
		[Space] 
		[Header("Audio Offset")] 
		[SerializeField] private SwitchHoldButtonGroup m_OffsetButtonGroup;
		[SerializeField] private Text m_OffsetText;
		[SerializeField] private Button m_ResetOffsetButton;
		[Space] 
		[Header("Offset Calibration Wizard")] 
		[SerializeField] private AudioSource m_WizardSource;
		[Space] 
		[SerializeField] private Text m_CurrentAVGOffsetText;
		[SerializeField] private Button m_OpenOffsetWizardButton;
		[SerializeField] private Button m_CloseOffsetWizardButton;
		[Space] 
		[SerializeField] private Button m_OffsetWizardButton;
		[SerializeField] private Button m_CancelOffsetWizardButton;
		[Space] 
		[SerializeField] private RectTransform m_OffsetWizardButtonOutline;
		[SerializeField] private RectTransform m_PreviewNote;
		[SerializeField] private Image m_CancelOffsetWizardImage;
		[Space]
		[Header("DSP Buffer")]
		[SerializeField] private SwitchButtonGroup m_DspBufferButtonGroup;
		[SerializeField] private Text m_DspBufferText;
#pragma warning restore

		private bool _offsetWizardClick;
		private bool _forceQuitOffsetWizard;

		private int _audioOffset;

		private static readonly int[] _dspBufferSizeInternal = new int[]
		{
			128, 256, 512, 1024, 2048, 4096
		};

		private int _dspBufferSizeIndex;

		public int DSPBufferSizeIndex
		{
			get => _dspBufferSizeIndex;
			set
			{
				_dspBufferSizeIndex = value;
				int size = _dspBufferSizeInternal[value];
				m_DspBufferText.text = size.ToString();
				SetDSPBufferSize(size);
				m_DspBufferButtonGroup.SetValueWithoutNotify(value);
			}
		}
		
		public int AudioOffset
		{
			get => _audioOffset;
			private set
			{
				if (_audioOffset == value) return;
				m_OffsetText.text = (value >= 0 ? "+" : "") + value;
				_audioOffset = value;
				m_OffsetButtonGroup.SetValueWithoutNotify(value);
			}
		}

		protected override void OnAwake()
		{
			m_OffsetButtonGroup.onValueChanged.AddListener(SetOffset);
			m_OffsetButtonGroup.Init();
			m_ResetOffsetButton.onClick.AddListener(ResetOffset);

			m_OpenOffsetWizardButton.onClick.AddListener(() => m_CurrentAVGOffsetText.text = "Click");
			m_CloseOffsetWizardButton.onClick.AddListener(() => _forceQuitOffsetWizard = true);
			m_OffsetWizardButton.onClick.AddListener(StartOffsetWizard);
			m_CancelOffsetWizardButton.onClick.AddListener(() => _forceQuitOffsetWizard = true);
			
			m_DspBufferButtonGroup.Init();
			m_DspBufferButtonGroup.SetValueRange(0, _dspBufferSizeInternal.Length);
			m_DspBufferButtonGroup.onValueChanged.AddListener(idx => DSPBufferSizeIndex = idx);
		}

		private void GenerateClickNote()
		{
			var clickNote = Instantiate(m_PreviewNote, m_PreviewNote.transform.parent);
			var img = clickNote.GetComponent<Image>();
			img.color = img.color.WithAlpha(0.5f);
			img.DOFade(0, .5f).OnComplete(() => Destroy(clickNote.gameObject));
		}

		private void SetAVGOffsetText(float offset)
		{
			m_CurrentAVGOffsetText.text = (offset >= 0 ? "+" : "") + (int)(offset * 1000);
		}

		private void SetOffsetWizardButtonAction(UnityAction action)
		{
			m_OffsetWizardButton.onClick.RemoveAllListeners();
			m_OffsetWizardButton.onClick.AddListener(action);
		}

		private void ChangeOffsetWizardButtonStatus(bool isOn)
		{
			float wizardButtonY = isOn ? 140 : 0;
			float cancelButtonY = isOn ? -190 : 0;
			m_OffsetWizardButtonOutline
				.DOAnchorPosY(wizardButtonY, .5f)
				.SetEase(Ease.OutCubic);
			m_CancelOffsetWizardButton.GetComponent<RectTransform>()
				.DOAnchorPosY(cancelButtonY, .5f)
				.SetEase(Ease.OutCubic);
			m_CancelOffsetWizardImage
				.DOFade(isOn ? 1 : 0, .5f);
		}

		private async UniTaskVoid StartOffsetWizardTask()
		{
			const float groupInterval = 2f;
			const float clickOffset = -1;

			int hitCount = 0;
			float offsetSum = 0;
			var keyboardKey = GameplayManager.TurnKeybordKey;

			// 需要在这里重置对应条件的状态，否则在某个情况下会直接退出
			_offsetWizardClick = false;
			_forceQuitOffsetWizard = false;

			SetAVGOffsetText(0);
			ChangeOffsetWizardButtonStatus(true);
			SetOffsetWizardButtonAction(() => _offsetWizardClick = true);

			m_WizardSource.Play();
			float time = Time.time;

			for (int i = 0; i < 4; i++)
			{
				float currentOffset = 0;
				bool isClick = false;
				while (true)
				{
					float interval = Time.time - time;

					if (interval > groupInterval) break;

					if (_offsetWizardClick || Input.GetKeyDown(keyboardKey))
					{
						_offsetWizardClick = false;
						currentOffset = interval + clickOffset;

						isClick = true;
						await UniTask.WaitForEndOfFrame(this);

						float currentAvgOffset = (offsetSum + currentOffset) / (hitCount + 1);
						SetAVGOffsetText(currentAvgOffset);

						GenerateClickNote();
					}

					float progress = interval / groupInterval;

					float noteY = -800 * progress;
					m_PreviewNote.anchoredPosition = new Vector2(0, noteY);

					if (_forceQuitOffsetWizard)
					{
						_forceQuitOffsetWizard = false;
						m_WizardSource.Stop();

						m_PreviewNote.anchoredPosition = new Vector2(0, 0);

						m_CurrentAVGOffsetText.text = "Click";

						SetOffsetWizardButtonAction(StartOffsetWizard);
						ChangeOffsetWizardButtonStatus(false);

						return;
					}

					await UniTask.Yield();
				}

				if (isClick)
				{
					offsetSum += currentOffset;
					hitCount++;
				}

				time = Time.time;
			}

			if (hitCount > 0)
			{
				float avgOffset = offsetSum / hitCount;
				SetOffset(avgOffset);
			}

			SetOffsetWizardButtonAction(StartOffsetWizard);
			ChangeOffsetWizardButtonStatus(false);
		}

		/// <summary>
		/// 重置延迟 (将延迟重置为 0)
		/// </summary>
		public void ResetOffset()
		{
			AudioOffset = 0;
			m_OffsetButtonGroup.CurrentValue = 0;
		}

		/// <summary>
		/// 设置延迟
		/// </summary>
		/// <param name="offset"></param>
		public void SetOffset(float offset) => AudioOffset = (int)(offset * 1000f);

		/// <summary>
		/// 设置延迟
		/// </summary>
		/// <param name="offset"></param>
		public void SetOffset(int offset) => AudioOffset = offset;

		/// <summary>
		/// 开始运行偏移校准向导
		/// </summary>
		public void StartOffsetWizard()
		{
			StartOffsetWizardTask().Forget();
		}

		/// <summary>
		/// 设置 DSP 缓冲区大小
		/// </summary>
		/// <param name="size"></param>
		public void SetDSPBufferSize(int size)
		{
			var config = AudioSettings.GetConfiguration();
			config.dspBufferSize = size;
			AudioSettings.Reset(config);
		}
	}
}