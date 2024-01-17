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
	
	internal abstract class UIComponent
	{
		protected UIComponent() { }
		
		public bool Visible { get; protected set; }
		internal virtual void OnAwake() { }
		public virtual void ChangeStatus(bool visible) { }
		public virtual Tween ChangeStatus(bool visible, bool play) => null;
		public virtual void SetStatus(bool visible) { }
		public virtual void SetStatus(bool visible, bool onAwake) { }
	}
	
	[Serializable]
	internal sealed class ReadyUI : UIComponent
	{
		private ReadyUI() { }
		
		[SerializeField] private CanvasGroup m_CanvasGroup;
		[SerializeField] private RectTransform m_RectTrans;
		[SerializeField] private FloatStatus m_RectYPositions;
		
		private Tween _tween; 
		
		internal override void OnAwake()
		{
			Visible = true;
		}

		/// <summary>
		/// 更改准备 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		/// <param name="play"></param>
		public override Tween ChangeStatus(bool visible, bool play)
		{
			_tween?.Kill();

			float duration = UIComponentHelper.AnimationDuration;
			
			var squence = DOTween.Sequence();

			squence
				.Join(m_CanvasGroup.DOFade(visible ? 1 : 0, duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_RectTrans
					.DOAnchorPosY(m_RectYPositions.GetValue(visible), duration)
					.SetEase(UIComponentHelper.AnimationEase));

			squence.Join(
				UIManager.Instance.BlurController
					.DOBlurUI(
						m_CanvasGroup.GetComponent<Image>(), 
						UIComponentHelper.BlurUIColor, 
						duration, 
						visible
					)
					.SetEase(UIComponentHelper.AnimationEase));

			_tween =  play ? squence.Play() : squence;

			Visible = visible;

			return _tween;
		}

		public override void ChangeStatus(bool visible)
		{
			ChangeStatus(visible, true);
		}
	}
	[Serializable]
	internal sealed class ResultUI : UIComponent
	{
		private ResultUI() { }
		
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

		internal override void OnAwake()
		{
			SetStatus(false);
		}

		/// <summary>
		/// 更改结算 UI 的可见状态
		/// <remarks>同 SetStatus, 只不过多了过渡效果</remarks>
		/// </summary>
		/// <param name="visible">可见状态</param>
		public override void ChangeStatus(bool visible)
		{
			_cts.Cancel();
			_cts.Dispose();
			_cts = new CancellationTokenSource();
			AnimationTask(visible).Forget();
			
			float duration = UIComponentHelper.AnimationDuration;
			
			_tween?.Kill();
			
			var sequence = DOTween.Sequence();
			sequence
				.Join(Top
					.DOAnchorPos(
						TopPositions.GetValue(visible), 
						duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(Bottom
					.DOAnchorPos(
						BottomPositions.GetValue(visible), 
						duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(CanvasGroup
					.DOFade(visible ? 1 : 0, duration)
				);

			if (UIManager.Instance.BlurController)
			{
				var img = CanvasGroup.GetComponent<Image>();
				sequence
				.Join(UIManager.Instance.BlurController.DOBlurUI(img, 
						UIComponentHelper.BlurUIColor, 
						duration, visible)
					.SetEase(UIComponentHelper.AnimationEase));
			}

			_tween = sequence.Play();
		}

		/// <summary>
		/// 设置结算 UI 的可见状态
		/// <remarks>同 ChangeStatus, 只不过少了过渡效果</remarks>
		/// </summary>
		/// <param name="visible">可见状态</param>
		public override void SetStatus(bool visible)
		{
			Top.anchoredPosition = TopPositions.GetValue(visible);
			Bottom.anchoredPosition = BottomPositions.GetValue(visible);
			CanvasGroup.alpha = visible ? 1 : 0;
			CanvasGroup.gameObject.SetActive(visible);
			var img = CanvasGroup.GetComponent<Image>();
			img.color = visible ? UIComponentHelper.BlurUIColor : Color.white;
			UIManager.Instance.BlurController.interpolation = visible ? 1 : 0;
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
	internal sealed class SettingUI : UIComponent
	{
		private SettingUI() { }
		
		[SerializeField] private CanvasGroup m_CanvasGroup;
		[SerializeField] private RectTransform m_TopRectTrans;
		[SerializeField] private RectTransform m_BottomRectTrans;
		[Space] 
		[SerializeField] private Vector2Status m_TopRectTransPositions;
		[SerializeField] private Vector2Status m_BottomRectTransPositions;
		
		private CancellationTokenSource _cts = new CancellationTokenSource();
		private Tween _tween;

		internal override void OnAwake()
		{
			SetStatus(false, true);
		}

		public new Tween ChangeStatus(bool visible)
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
			
			float duration = UIComponentHelper.AnimationDuration;

			sequence.Join(m_TopRectTrans.DOAnchorPos(
						m_TopRectTransPositions.GetValue(visible), 
						duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_BottomRectTrans.DOAnchorPos(
						m_BottomRectTransPositions.GetValue(visible),
						duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_CanvasGroup.DOFade(
						visible ? 1 : 0, duration)
					.SetEase(UIComponentHelper.AnimationEase)
				);
			
			if (UIManager.Instance.BlurController)
			{
				var img = m_CanvasGroup.GetComponent<Image>();
				sequence
					.Join(UIManager.Instance.BlurController.DOBlurUI(img, 
						UIComponentHelper.BlurUIColor, 
						duration, visible));
			}

			_tween = sequence;

			Visible = visible;
			
			return _tween;
		}

		/// <summary>
		/// 更改设置 UI 的可见状态
		/// <remarks>同 SetStatus, 只不过多了过渡效果</remarks>
		/// </summary>
		/// <param name="visible">可见状态</param>
		/// <param name="play"></param>
		public override Tween ChangeStatus(bool visible, bool play)
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
			
			float duration = UIComponentHelper.AnimationDuration;

			if (readyUIVisible)
			{
				sequence.Join(UIManager.Instance.ChangeReadyUI(false, false));
				sequence.AppendInterval(duration);
			}

			sequence.Join(m_TopRectTrans.DOAnchorPos(
						m_TopRectTransPositions.GetValue(visible), 
						duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_BottomRectTrans.DOAnchorPos(
						m_BottomRectTransPositions.GetValue(visible),
						duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_CanvasGroup.DOFade(
						visible ? 1 : 0, duration)
					.SetEase(UIComponentHelper.AnimationEase)
				);
			
			if (UIManager.Instance.BlurController)
			{
				var img = m_CanvasGroup.GetComponent<Image>();
				sequence
					.Join(UIManager.Instance.BlurController.DOBlurUI(img, 
						UIComponentHelper.BlurUIColor, 
						duration, visible));
			}
			
			if (!readyUIVisible)
			{
				sequence.AppendInterval(duration);
				sequence.Join(UIManager.Instance.ChangeReadyUI(true, false));
			}

			_tween = play ? sequence.Play() : sequence;

			Visible = visible;
			
			return _tween;
		}

		/// <summary>
		/// 设置设置 UI 的可见状态
		/// <remarks>同 ChangeStatus, 只不过少了过渡效果</remarks>
		/// </summary>
		/// <param name="visible">可见状态</param>
		/// <param name="onAwake"></param>
		public override void SetStatus(bool visible, bool onAwake)
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
			UIManager.Instance.BlurController.interpolation = visible ? 1 : 0;
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
	internal sealed class PauseUI : UIComponent
	{
		private PauseUI() { }
		
		[SerializeField] private CanvasGroup m_CanvasGroup;
		[SerializeField] private RectTransform m_RectTrans;
		[SerializeField] private FloatStatus m_RectYPositions;
		
		private Tween _tween;
		
		internal override void OnAwake()
		{
			SetStatus(false);
		}

		/// <summary>
		/// 更改暂停 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public override void ChangeStatus(bool visible)
		{
			_tween?.Kill();
			
			var squence = DOTween.Sequence();

			float duration = UIComponentHelper.AnimationDuration;

			squence
				.Join(m_CanvasGroup.DOFade(visible ? 1 : 0, duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_RectTrans
					.DOAnchorPosY(m_RectYPositions.GetValue(visible), duration)
					.SetEase(UIComponentHelper.AnimationEase));

			_tween = squence.Play();
		}

		/// <summary>
		/// 设置暂停 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public override void SetStatus(bool visible)
		{
			m_CanvasGroup.alpha = visible ? 1 : 0;
			var anchorPos = m_RectTrans.anchoredPosition;
			anchorPos.y = m_RectYPositions.GetValue(visible);
			m_RectTrans.anchoredPosition = anchorPos;
		}
	}

	[Serializable]
	internal sealed class OffsetWizardUI : UIComponent
	{
		private OffsetWizardUI() { }
		
		[SerializeField] private CanvasGroup m_CanvasGroup;
		[SerializeField] private RectTransform m_TopRectTrans;
		[SerializeField] private RectTransform m_BottomRectTrans;
		[Space]
		[SerializeField] private FloatStatus m_TopYPositions;
		[SerializeField] private FloatStatus m_BottomYPositions;

		private CancellationTokenSource _cts = new CancellationTokenSource();
		private Tween _tween;
		
		internal override void OnAwake()
		{
			SetStatus(false);
		}

		/// <summary>
		/// 更改延迟向导 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public override void ChangeStatus(bool visible)
		{
			_cts.Cancel();
			_cts.Dispose();
			_cts = new CancellationTokenSource();
			AnimationTask(visible).Forget();
			
			_tween?.Kill();
			
			var squence = DOTween.Sequence();

			bool settingUIVisible = UIManager.Instance.SettingUI.Visible;
			float duration = UIComponentHelper.AnimationDuration;

			if (settingUIVisible)
			{
				squence.Join(UIManager.Instance.SettingUI.ChangeStatus(false));
				squence.AppendInterval(duration);
			}

			squence
				.Join(m_CanvasGroup.DOFade(visible ? 1 : 0, duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_TopRectTrans
					.DOAnchorPosY(m_TopYPositions.GetValue(visible), duration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_BottomRectTrans
					.DOAnchorPosY(m_BottomYPositions.GetValue(visible), duration)
					.SetEase(UIComponentHelper.AnimationEase));

			var img = m_CanvasGroup.GetComponent<Image>();
			squence.Join(UIManager.Instance.BlurController.DOBlurUI(img, 
					UIComponentHelper.BlurUIColor, 
					duration, visible)
				.SetEase(UIComponentHelper.AnimationEase));
			
			if (!settingUIVisible)
			{
				squence.Append(UIManager.Instance.SettingUI.ChangeStatus(true));
			}
			
			_tween = squence.Play();
		}

		/// <summary>
		/// 设置延迟向导 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public override void SetStatus(bool visible)
		{
			m_CanvasGroup.alpha = visible ? 1 : 0;
			m_CanvasGroup.gameObject.SetActive(visible);
			
			var anchorPos = m_TopRectTrans.anchoredPosition;
			anchorPos.y = m_TopYPositions.GetValue(visible);
			m_TopRectTrans.anchoredPosition = anchorPos;
			
			anchorPos = m_BottomRectTrans.anchoredPosition;
			anchorPos.y = m_BottomYPositions.GetValue(visible);
			m_BottomRectTrans.anchoredPosition = anchorPos;
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
}