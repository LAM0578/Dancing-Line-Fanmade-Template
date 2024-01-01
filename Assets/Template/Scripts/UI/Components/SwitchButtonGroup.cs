using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DancingLineSample.UI.Components
{
	
	public class SwitchButtonGroup : MonoBehaviour
	{
		[Serializable]
		public class ValueChangedEvent : UnityEvent<int> { }

#pragma warning disable

		[SerializeField] private bool m_InitOnAwake;
		[Space]
		[SerializeField] private Button m_IncreaseButton;
		[SerializeField] private Button m_DecreaseButton;
		[Space]
		[SerializeField] private int m_MinLimit;
		[SerializeField] private int m_MaxLimit;
		[Space] 
		[SerializeField] private ValueChangedEvent m_OnValueChanged = new ValueChangedEvent();
		
#pragma warning restore
		
		private int _currentValue;
		private bool _inited;

		/// <summary>
		/// 在值更新时调用该事件
		/// </summary>
		public ValueChangedEvent onValueChanged
		{
			get => m_OnValueChanged;
			set => m_OnValueChanged = value;
		}

		/// <summary>
		/// 最小值
		/// </summary>
		public int MinLimit
		{
			get => m_MinLimit;
			set
			{
				m_MinLimit = value;
				UpdateButtonStatus();
			}
		}

		/// <summary>
		/// 最大值
		/// </summary>
		public int MaxLimit
		{
			get => m_MaxLimit;
			set
			{
				m_MaxLimit = value;
				UpdateButtonStatus();
			}
		}
		
		/// <summary>
		/// 当前值
		/// </summary>
		public int CurrentValue
		{
			get => _currentValue;
			set
			{
				_currentValue = value;
				m_OnValueChanged?.Invoke(_currentValue);
				UpdateButtonStatus();
			}
		}

		private void Awake()
		{
			if (!m_InitOnAwake) return;
			Init();
		}

		/// <summary>
		/// 载入 (添加监听到所有按钮中)
		/// </summary>
		public void Init()
		{
			if (_inited) return;
			m_IncreaseButton.onClick.AddListener(Increase);
			m_DecreaseButton.onClick.AddListener(Decrease);
			_inited = true;
		}

		/// <summary>
		/// 在值更新后调用，用于更新按钮状态
		/// </summary>
		private void UpdateButtonStatus()
		{
			m_IncreaseButton.interactable = _currentValue < m_MaxLimit;
			m_DecreaseButton.interactable = _currentValue > m_MinLimit;
		}

		/// <summary>
		/// 当按下 m_IncreaseButton 时调用并增加值 
		/// </summary>
		public void Increase()
		{
			if (_currentValue >= m_MaxLimit) return;
			_currentValue++;
			m_OnValueChanged?.Invoke(_currentValue);
			UpdateButtonStatus();
		}
		
		/// <summary>
		/// 当按下 m_DecreaseButton 时调用并减少值
		/// </summary>
		public void Decrease()
		{
			if (_currentValue <= m_MinLimit) return;
			_currentValue--;
			m_OnValueChanged?.Invoke(_currentValue);
			UpdateButtonStatus();
		}

		/// <summary>
		/// 设置值范围
		/// </summary>
		/// <param name="min">最小值</param>
		/// <param name="max">最大值</param>
		public void SetValueRange(int min, int max)
		{
			m_MinLimit = min;
			m_MaxLimit = max;
			int curVal = Mathf.Clamp(_currentValue, min, max);
			if (_currentValue != curVal)
			{
				_currentValue = curVal;
				onValueChanged?.Invoke(_currentValue);
				UpdateButtonStatus();
			}
		}
	}
}