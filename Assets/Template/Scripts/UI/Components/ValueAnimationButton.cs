using System;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DancingLineSample.UI.Components
{
	public class ValueAnimationButton : MonoBehaviour
	{
		[Serializable]
		public class OnClickEvent : UnityEvent<bool> { }

#pragma warning disable
		
		[SerializeField] private RectTransform m_HandleRectTrans;
		[SerializeField] private Vector2Status m_HandlePositions;
		[Space]
		[SerializeField] private Ease m_AnimationEasing = Ease.OutCubic;
		[SerializeField] private float m_AnimationDuration = .5f;
		[Space]
		[SerializeField] private OnClickEvent m_OnClick = new OnClickEvent();	
		
#pragma warning restore

		public bool Active { get; private set; }

		public OnClickEvent onClick
		{
			get => m_OnClick;
			set => m_OnClick = value;
		}
		
		private void DoHandleAnimationInternal(bool active)
		{
			var targetPos = active ? m_HandlePositions.EndValue : m_HandlePositions.StartValue;
			m_HandleRectTrans.DOAnchorPos(targetPos, m_AnimationDuration).SetEase(m_AnimationEasing);
		}
		
		/// <summary>
		/// 更改按钮值状态
		/// </summary>
		/// <param name="active"></param>
		public void SetActive(bool active)
		{
			Active = active;
			var targetPos = active ? m_HandlePositions.EndValue : m_HandlePositions.StartValue;
			m_HandleRectTrans.anchoredPosition = targetPos;
		}

		/// <summary>
		/// 更改值状态并触发 Handle 移动动画
		/// </summary>
		public void OnButtonClick()
		{
			Active = !Active;
			DoHandleAnimationInternal(Active);
			m_OnClick?.Invoke(Active);
		}
	}
}