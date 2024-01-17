using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using DancingLineSample.Setting;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using SuperBlur;
using DancingLineSample.Attributes;
using DancingLineSample.Objects;
using UnityEngine.Serialization;

namespace DancingLineSample.UI
{
#pragma warning disable 0649
	public class UIManager : Singleton<UIManager>
	{
		[SerializeField] internal ReadyUI ReadyUI;
		[SerializeField] internal ResultUI ResultUI;
		[SerializeField] internal SettingUI SettingUI;
		[SerializeField] internal OffsetWizardUI OffsetWizardUI;
		[SerializeField] internal PauseUI PauseUI;
		[Space] 
		[SerializeField] private SuperBlurBase m_BlurController;
		[SerializeField] private Image m_FakeFogImage;
		public SuperBlurBase BlurController => m_BlurController;
		
		protected override void OnAwake()
		{
			ReadyUI.OnAwake();
			ResultUI.OnAwake();
			SettingUI.OnAwake();
			PauseUI.OnAwake();
			OffsetWizardUI.OnAwake();
			BlurController.interpolation = 1;
		}

		/// <summary>
		/// 更改准备 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		/// <param name="play"></param>
		public Tween ChangeReadyUI(bool visible, bool play = true)
		{
			return ReadyUI.ChangeStatus(visible, play);
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
			SettingUI.ChangeStatus(visible, true);
		}
		
		/// <summary>
		/// 更改设置 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		/// <param name="play"></param>
		public Tween ChangeSettingUI(bool visible, bool play)
		{
			return SettingUI.ChangeStatus(visible, play);
		}

		/// <summary>
		/// 更改暂停 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public void ChangePauseUI(bool visible)
		{
			PauseUI.ChangeStatus(visible);
		}
		
		/// <summary>
		/// 更改偏移向导 UI 的可见状态
		/// </summary>
		/// <param name="visible">可见状态</param>
		public void ChangeOffsetWizardUI(bool visible)
		{
			OffsetWizardUI.ChangeStatus(visible);
		}

		/// <summary>
		/// 当需要过渡时调用此方法
		/// </summary>
		public void DOFakeFog()
		{
			var col = RenderSettings.fogColor;
			col.a = 1;
			var endCol = new Color(col.r, col.g, col.b, 0);
			var img = m_FakeFogImage;

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetValue<T>(this ObjectStatus<T> objStat, bool isEndVal)
		{
			return isEndVal ? objStat.EndValue : objStat.StartValue;
		}
	}
}
