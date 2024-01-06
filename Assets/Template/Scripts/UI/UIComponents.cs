using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DancingLineSample.Objects;
using DancingLineSample.Setting;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DancingLineSample.UI
{
#pragma warning disable 0649
	[Serializable]
	internal class ReadyUI
	{
		[SerializeField] private CanvasGroup m_CanvasGroup;
		[SerializeField] private RectTransform m_RectTrans;
		[SerializeField] private FloatStatus m_RectYPositions;
		
		private Tween _tween;

		public bool Visible { get; private set; } = true;

		/// <summary>
		/// 更改准备 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		/// <param name="play"></param>
		public Tween ChangeStatus(bool visible, bool play = true)
		{
			_tween?.Kill();
			
			var squence = DOTween.Sequence();

			squence
				.Join(m_CanvasGroup.DOFade(visible ? 1 : 0, UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_RectTrans
					.DOAnchorPosY(m_RectYPositions.GetValue(visible), UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase));

			squence.Join(
				UIManager.Instance.SuperBlurBase
					.DOBlurUI(
						m_CanvasGroup.GetComponent<Image>(), 
						UIComponentHelper.BlurUIColor, 
						UIComponentHelper.AnimationDuration, 
						visible
					)
					.SetEase(UIComponentHelper.AnimationEase));

			_tween =  play ? squence.Play() : squence;

			Visible = visible;

			return _tween;
		}

	}
	[Serializable]
	internal class ResultUI
	{
		public RectTransform Top;
		public RectTransform Bottom;
		public CanvasGroup CanvasGroup;
		[Space] 
		public Vector2Status TopPositions = new Vector2Status();
		public Vector2Status BottomPositions = new Vector2Status();
		[Space] 
		[SerializeField] private Button m_RestartButton;
		[SerializeField] private Button m_ContinueButton;
		
		private CancellationTokenSource _cts = new CancellationTokenSource();
		private Tween _tween;

		internal void OnAwake()
		{
			SetStatus(false);
		}

		/// <summary>
		/// 更改结算 UI 的可见状态
		/// <remarks>同 SetStatus, 只不过多了过渡效果</remarks>
		/// </summary>
		/// <param name="visible">可见状态</param>
		public void ChangeStatus(bool visible)
		{
			_cts.Cancel();
			_cts.Dispose();
			_cts = new CancellationTokenSource();
			AnimationTask(visible).Forget();
			
			_tween?.Kill();
			
			var sequence = DOTween.Sequence();
			sequence
				.Join(Top
					.DOAnchorPos(
						TopPositions.GetValue(visible), 
						UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(Bottom
					.DOAnchorPos(
						BottomPositions.GetValue(visible), 
						UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(CanvasGroup
					.DOFade(visible ? 1 : 0, UIComponentHelper.AnimationDuration)
				);

			if (UIManager.Instance.SuperBlurBase)
			{
				var img = CanvasGroup.GetComponent<Image>();
				sequence
				.Join(UIManager.Instance.SuperBlurBase.DOBlurUI(img, 
						UIComponentHelper.BlurUIColor, 
						UIComponentHelper.AnimationDuration, visible)
					.SetEase(UIComponentHelper.AnimationEase));
			}

			_tween = sequence.Play();
		}

		/// <summary>
		/// 设置结算 UI 的可见状态
		/// <remarks>同 ChangeStatus, 只不过少了过渡效果</remarks>
		/// </summary>
		/// <param name="visible">可见状态</param>
		public void SetStatus(bool visible)
		{
			Top.anchoredPosition = TopPositions.GetValue(visible);
			Bottom.anchoredPosition = BottomPositions.GetValue(visible);
			CanvasGroup.alpha = visible ? 1 : 0;
			CanvasGroup.gameObject.SetActive(visible);
			var img = CanvasGroup.GetComponent<Image>();
			img.color = visible ? UIComponentHelper.BlurUIColor : Color.white;
			UIManager.Instance.SuperBlurBase.interpolation = visible ? 1 : 0;
		}
		
		private async UniTaskVoid AnimationTask(bool visible)
		{
			bool isCanceled = await UniTask.Delay(
				TimeSpan.FromSeconds(visible ? 0 : UIComponentHelper.AnimationDuration), 
				cancellationToken: _cts.Token)
				.SuppressCancellationThrow();
			
			if (isCanceled) return;
			
			CanvasGroup.gameObject.SetActive(visible);
		}

		/// <summary>
		/// 切换按钮状态
		/// </summary>
		/// <param name="isContinue">切换到 Continue</param>
		public void ChangeButtonStatus(bool isContinue)
		{
			m_RestartButton.gameObject.SetActive(!isContinue);
			m_ContinueButton.gameObject.SetActive(isContinue);
		}
	}

	[Serializable]
	internal class SettingUI
	{
		
		[SerializeField] private CanvasGroup m_CanvasGroup;
		[SerializeField] private RectTransform m_TopRectTrans;
		[SerializeField] private RectTransform m_BottomRectTrans;
		[Space] 
		[SerializeField] private Vector2Status m_TopRectTransPositions;
		[SerializeField] private Vector2Status m_BottomRectTransPositions;
		
		private CancellationTokenSource _cts = new CancellationTokenSource();
		private Tween _tween;

		internal void OnAwake()
		{
			SetStatus(false, true);
		}

		/// <summary>
		/// 更改设置 UI 的可见状态
		/// <remarks>同 SetStatus, 只不过多了过渡效果</remarks>
		/// </summary>
		/// <param name="visible">可见状态</param>
		public void ChangeStatus(bool visible)
		{
			_cts.Cancel();
			_cts.Dispose();
			_cts = new CancellationTokenSource();
			AnimationTask(visible).Forget();
			
			_tween?.Kill();

			if (!visible)
			{
				SettingManager.Instance.SaveSettings();
			}
			
			var sequence = DOTween.Sequence();
			var readyUIVisible = UIManager.Instance.ReadyUI.Visible;

			if (readyUIVisible)
			{
				sequence.Join(UIManager.Instance.ChangeReadyUI(false, false));
				sequence.AppendInterval(UIComponentHelper.AnimationDuration);
			}

			sequence.Join(m_TopRectTrans.DOAnchorPos(
						m_TopRectTransPositions.GetValue(visible), 
						UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_BottomRectTrans.DOAnchorPos(
						m_BottomRectTransPositions.GetValue(visible),
						UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_CanvasGroup.DOFade(
						visible ? 1 : 0, UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase)
				);
			
			if (UIManager.Instance.SuperBlurBase)
			{
				var img = m_CanvasGroup.GetComponent<Image>();
				sequence
					.Join(UIManager.Instance.SuperBlurBase.DOBlurUI(img, 
						UIComponentHelper.BlurUIColor, 
						UIComponentHelper.AnimationDuration, visible));
			}
			
			if (!readyUIVisible)
			{
				sequence.AppendInterval(UIComponentHelper.AnimationDuration);
				sequence.Join(UIManager.Instance.ChangeReadyUI(true, false));
			}

			_tween = sequence.Play();
		}

		/// <summary>
		/// 设置设置 UI 的可见状态
		/// <remarks>同 ChangeStatus, 只不过少了过渡效果</remarks>
		/// </summary>
		/// <param name="visible">可见状态</param>
		/// <param name="onAwake"></param>
		public void SetStatus(bool visible, bool onAwake = false)
		{
			if (!visible && !onAwake)
			{
				SettingManager.Instance.SaveSettings();
			}

			m_TopRectTrans.anchoredPosition = m_TopRectTransPositions.GetValue(visible);
			m_BottomRectTrans.anchoredPosition = m_BottomRectTransPositions.GetValue(visible);
			m_CanvasGroup.alpha = visible ? 1 : 0;
			m_CanvasGroup.gameObject.SetActive(visible);
			var img = m_CanvasGroup.GetComponent<Image>();
			img.color = visible ? UIComponentHelper.BlurUIColor : Color.white;
			UIManager.Instance.SuperBlurBase.interpolation = visible ? 1 : 0;
		}
		
		private async UniTaskVoid AnimationTask(bool visible)
		{
			bool isCanceled = await UniTask.Delay(
					TimeSpan.FromSeconds(visible ? 0 : UIComponentHelper.AnimationDuration), 
					cancellationToken: _cts.Token)
				.SuppressCancellationThrow();
			
			if (isCanceled) return;
			
			m_CanvasGroup.gameObject.SetActive(visible);
		}
	}

	[Serializable]
	internal class PauseUI
	{
		[SerializeField] private CanvasGroup m_CanvasGroup;
		[SerializeField] private RectTransform m_RectTrans;
		[SerializeField] private FloatStatus m_RectYPositions;
		
		private Tween _tween;
		
		internal void OnAwake()
		{
			SetStatus(false);
		}

		/// <summary>
		/// 更改暂停 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public void ChangeStatus(bool visible)
		{
			_tween?.Kill();
			
			var squence = DOTween.Sequence();

			// m_CanvasGroup.interactable = visible;

			squence
				.Join(m_CanvasGroup.DOFade(visible ? 1 : 0, UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_RectTrans
					.DOAnchorPosY(m_RectYPositions.GetValue(visible), UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase));

			_tween = squence.Play();
		}

		/// <summary>
		/// 设置暂停 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public void SetStatus(bool visible)
		{
			m_CanvasGroup.alpha = visible ? 1 : 0;
			var anchorPos = m_RectTrans.anchoredPosition;
			anchorPos.y = m_RectYPositions.GetValue(visible);
			m_RectTrans.anchoredPosition = anchorPos;
		}
	}
}