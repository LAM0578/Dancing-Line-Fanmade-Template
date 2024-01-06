using DancingLineSample.UI.Components;
using DancingLineSample.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace DancingLineSample.Gameplay
{
	public class AudioOffsetManager : Singleton<AudioOffsetManager>
	{
#pragma warning disable
		[Header("UI Components")] 
		[SerializeField] private SwitchHoldButtonGroup m_OffsetButtonGroup;
		[SerializeField] private Text m_OffsetText;
		[SerializeField] private Button m_ResetOffsetButton;
#pragma warning restore
		
		private int _audioOffset;

		public int AudioOffset
		{
			get => _audioOffset;
			private set
			{
				if (_audioOffset == value) return;
				m_OffsetText.text = value.ToString();
				_audioOffset = value;
			}
		}

		protected override void OnAwake()
		{
			m_OffsetButtonGroup.onValueChanged.AddListener(SetOffset);
			m_OffsetButtonGroup.Init();
			m_ResetOffsetButton.onClick.AddListener(ResetOffset);
		}

		/// <summary>
		/// 增加延迟 (当感觉 LATE 较多时调用)
		/// </summary>
		public void AddOffset() => AudioOffset++;

		/// <summary>
		/// 减少延迟 (当感觉 EARLY 较多时调用)
		/// </summary>
		public void ReduceOffset() => AudioOffset--;
		
		/// <summary>
		/// 重置延迟 (将延迟重置为 0)
		/// </summary>
		public void ResetOffset() => AudioOffset = 0;
		
		/// <summary>
		/// 设置延迟
		/// </summary>
		/// <param name="offset"></param>
		public void SetOffset(int offset) => AudioOffset = offset;
	}
}