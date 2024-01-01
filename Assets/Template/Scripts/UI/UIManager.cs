using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DancingLineSample.Setting;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using SuperBlur;

namespace DancingLineSample.UI
{
#pragma warning disable 0649
	[Serializable]
	public class ResultUI
	{
#pragma warning disable
		
		public RectTransform Top;
		public RectTransform Bottom;
		public CanvasGroup CanvasGroup;
		[Space] 
		public Vector2Status TopPositions = new Vector2Status();
		public Vector2Status BottomPositions = new Vector2Status();
		[Space]
		[SerializeField] private SuperBlurBase m_SuperBlurBase;
		[Space] 
		[SerializeField] private Button m_RestartButton;
		[SerializeField] private Button m_ContinueButton;
		
#pragma warning restore
		
		private CancellationTokenSource _cts = new CancellationTokenSource();
		private Tween _tween;

		internal void OnAwake()
		{
			SetStatus(false);
			m_SuperBlurBase.interpolation = 1;
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
						visible ? TopPositions.EndValue : TopPositions.StartValue, 
						UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(Bottom
					.DOAnchorPos(
						visible ? BottomPositions.EndValue : BottomPositions.StartValue, 
						UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(CanvasGroup
					.DOFade(visible ? 1 : 0, UIComponentHelper.AnimationDuration)
				);

			if (m_SuperBlurBase)
			{
				var img = CanvasGroup.GetComponent<Image>();
				sequence
				.Join(m_SuperBlurBase.DOBlurUI(img, 
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
			Top.anchoredPosition = visible ? TopPositions.EndValue : TopPositions.StartValue;
			Bottom.anchoredPosition = visible ? BottomPositions.EndValue : BottomPositions.StartValue;
			CanvasGroup.alpha = visible ? 1 : 0;
			CanvasGroup.gameObject.SetActive(visible);
			if (!m_SuperBlurBase) return;
			var img = CanvasGroup.GetComponent<Image>();
			img.color = visible ? UIComponentHelper.BlurUIColor : Color.white;
			m_SuperBlurBase.interpolation = visible ? 1 : 0;
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
	public class SettingUI
	{
#pragma warning disable
		
		[SerializeField] private CanvasGroup m_CanvasGroup;
		[SerializeField] private RectTransform m_TopRectTrans;
		[SerializeField] private RectTransform m_BottomRectTrans;
		[Space] 
		[SerializeField] private Vector2Status m_TopRectTransPositions;
		[SerializeField] private Vector2Status m_BottomRectTransPositions;
		[Space]
		[SerializeField] private SuperBlurBase m_SuperBlurBase;
		
#pragma warning restore
		
		private CancellationTokenSource _cts = new CancellationTokenSource();
		private Tween _tween;

		internal void OnAwake()
		{
			SetStatus(false, true);
			m_SuperBlurBase.interpolation = 1;
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
			var readyUIVisible = UIManager.Instance.ReadyUIVisible;

			if (readyUIVisible)
			{
				sequence.Join(UIManager.Instance.ChangeReadyUI(false, false));
				sequence.AppendInterval(UIComponentHelper.AnimationDuration);
			}

			sequence.Join(m_TopRectTrans.DOAnchorPos(
						visible ? m_TopRectTransPositions.EndValue : m_TopRectTransPositions.StartValue, 
						UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_BottomRectTrans.DOAnchorPos(
						visible ? m_BottomRectTransPositions.EndValue : m_BottomRectTransPositions.StartValue,
						UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(m_CanvasGroup.DOFade(
						visible ? 1 : 0, UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase)
				);
			
			if (m_SuperBlurBase)
			{
				var img = m_CanvasGroup.GetComponent<Image>();
				sequence
					.Join(m_SuperBlurBase.DOBlurUI(img, 
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
		public void SetStatus(bool visible, bool onAwake = false)
		{
			if (!visible && !onAwake)
			{
				SettingManager.Instance.SaveSettings();
			}
			m_TopRectTrans.anchoredPosition = 
				visible ? m_TopRectTransPositions.EndValue : m_TopRectTransPositions.StartValue;
			m_BottomRectTrans.anchoredPosition = 
				visible ? m_BottomRectTransPositions.EndValue : m_BottomRectTransPositions.StartValue;
			m_CanvasGroup.alpha = visible ? 1 : 0;
			m_CanvasGroup.gameObject.SetActive(visible);
			if (!m_SuperBlurBase) return;
			var img = m_CanvasGroup.GetComponent<Image>();
			img.color = visible ? UIComponentHelper.BlurUIColor : Color.white;
			m_SuperBlurBase.interpolation = visible ? 1 : 0;
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
	public class UIManager : Singleton<UIManager>
	{
		public CanvasGroup ReadyUI;
		public ResultUI ResultUI;
		public SettingUI SettingUI;
		[Space] 
		public SuperBlurBase SuperBlurBase;
		public Image FakeFogImage;

		public bool ReadyUIVisible { get; private set; } = true;

		private Tween _tween;

		protected override void OnAwake()
		{
			ResultUI.OnAwake();
			SettingUI.OnAwake();
		}

		/// <summary>
		/// 更改准备 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public Tween ChangeReadyUI(bool visible, bool play = true)
		{
			_tween?.Kill();
			
			var squence = DOTween.Sequence();

			squence
				.Join(ReadyUI.DOFade(visible ? 1 : 0, UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase))
				.Join(ReadyUI.GetComponent<RectTransform>()
					.DOAnchorPosY(visible ? 0 : -300f, UIComponentHelper.AnimationDuration)
					.SetEase(UIComponentHelper.AnimationEase));

			if (SuperBlurBase)
			{
				squence.Join(
					SuperBlurBase
						.DOBlurUI(
							ReadyUI.GetComponent<Image>(), 
							UIComponentHelper.BlurUIColor, 
							UIComponentHelper.AnimationDuration, 
							visible
						)
						.SetEase(UIComponentHelper.AnimationEase));
			}

			_tween =  play ? squence.Play() : squence;

			ReadyUIVisible = visible;

			return _tween;
		}
		
		/// <summary>
		/// 更改结算 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public void ChangeResultUI(bool visible)
		{
			ResultUI.ChangeStatus(visible);
		}
		
		/// <summary>
		/// 更改设置 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public void ChangeSettingUI(bool visible)
		{
			SettingUI.ChangeStatus(visible);
		}

#if UNITY_EDITOR
		[Button("ResultUITest")]
		public void ResultUITest()
		{
			ResultUI.SetStatus(false);
			ResultUI.ChangeStatus(true);
		}
#endif

		/// <summary>
		/// 当需要过渡时调用此方法
		/// </summary>
		public void DOFakeFog()
		{
			var col = RenderSettings.fogColor;
			col.a = 1;
			var endCol = new Color(col.r, col.g, col.b, 0);
			var img = FakeFogImage;

			var sequence = DOTween.Sequence();
			sequence.Append(img.DOColor(col, 0.5f));
			sequence.Append(img.DOColor(endCol, 0.5f));
			sequence.Play();
		}
	}
	
	internal static class UIComponentHelper
	{
		public static readonly Color BlurUIColor = new Color32(0xe7, 0xe7, 0xe7, 0xff);
		public const Ease AnimationEase = Ease.OutCubic;
		public const float AnimationDuration = 0.5f;
	}
}
