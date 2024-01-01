using System;
using System.Collections;
using System.Collections.Generic;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace DancingLineSample.Gameplay.Trigger
{
	public class CameraTrigger : MonoBehaviour
	{
		public TriggerItemVector3 Move = new TriggerItemVector3();
		public TriggerItemVector3 Rotate = new TriggerItemVector3();
		[Space] 
		public TriggerItemFloat FieldOfView = new TriggerItemFloat();
		[Space] 
		[Tooltip("表示该相机触发器可循环使用")] public bool IsRecyclable;

		private bool _isActived;
		private Tween _moveTween, _rotateTween, _fovTween;
		
		[Button("Active")]
		private void Active()
		{
			if (_isActived && !IsRecyclable || 
			    GameplayManager.Instance.LineStatus != PlayerStatus.Playing) return;
			_isActived = true;
			
			if (Move.Enable)
			{
				var move = new Vector3();
				_moveTween = DOTween.To(() => move, x =>
				{
					if (Move.IsAdded) CameraManager.Instance.TriggerOffset += x;
					else CameraManager.Instance.TriggerOffset = x;
				}, Move.Value, Move.Duration).
					SetEase(Move.Easing);
			}

			if (Rotate.Enable)
			{
				var camTransform = CameraManager.Instance.TargetCamera.transform;
				_rotateTween = Rotate.IsAdded ? 
					camTransform.DOBlendableLocalRotateBy(Rotate.Value, Rotate.Duration) : 
					camTransform.DOLocalRotate(Rotate.Value, Rotate.Duration).
						SetEase(Rotate.Easing);
			}

			if (FieldOfView.Enable)
			{
				var cam = CameraManager.Instance.TargetCamera;
				_fovTween = FieldOfView.IsAdded ? 
					cam.DOFieldOfView(cam.fieldOfView + FieldOfView.Value, FieldOfView.Duration) : 
					cam.DOFieldOfView(FieldOfView.Value, FieldOfView.Duration).
					SetEase(FieldOfView.Easing);
			}
		}

		/// <summary>
		/// 更改此相机触发器 Tween 的播放状态
		/// </summary>
		/// <param name="play">播放状态</param>
		public void ChangeTweenStatus(bool play)
		{
			if (play)
			{
				_moveTween?.Play();
				_rotateTween?.Play();
				_fovTween?.Play();
			}
			else
			{
				_moveTween?.Pause();
				_rotateTween?.Pause();
				_fovTween?.Pause();
			}
		}

		/// <summary>
		/// 重置此相机触发器的状态
		/// </summary>
		public void ResetStatus()
		{
			ChangeTweenStatus(false);
			_isActived = false;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.gameObject.CompareTag("Player")) return;
			Active();
		}
	}
}