using System;
using System.Collections;
using System.Collections.Generic;
using DancingLineSample.Attributes;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace DancingLineSample.Gameplay.Trigger
{
	public class CameraTrigger : MonoBehaviour
	{
		public TriggerItemVector3 Move = new TriggerItemVector3();
		public TriggerItemVector3WithLocal Rotate = new TriggerItemVector3WithLocal();
		[Space] 
		public TriggerItemFloat FieldOfView = new TriggerItemFloat();
		[Space] 
		[Tooltip("表示该相机触发器可循环使用")] public bool IsRecyclable;

		private bool _isActived;
		private Tween _moveTween, _rotateTween, _fovTween;

		private Tween RotateCamera(Transform trans, Vector3 endValue, float duration, bool isAdded, bool isLocal)
		{
			var startValue = isLocal ? trans.localEulerAngles : trans.eulerAngles;
			var realEndValue = isAdded ? endValue + startValue : endValue;
			return DOTween.To(
				() => 0f,
				x =>
				{
					var rot = Vector3.Lerp(startValue, realEndValue, x);
					if (isLocal) trans.localEulerAngles = rot;
					else trans.eulerAngles = rot;
				},
				1,
				duration
			);
		}
		
		[MethodButton("Active")]
		private void Active()
		{
			if (_isActived && !IsRecyclable) return;
			_isActived = true;
			
			if (GameplayManager.Instance.LineStatus != PlayerStatus.Playing) return;
			
			if (Move.Enable)
			{
				var move = new Vector3();
				var originOffset = CameraManager.Instance.TriggerOffset;
				_moveTween = DOTween.To(() => move, x =>
				{
					if (Move.IsAdded) CameraManager.Instance.TriggerOffset = originOffset + x;
					else CameraManager.Instance.TriggerOffset = x;
				}, Move.Value, Move.Duration).
					SetEase(Move.Easing);
			}

			if (Rotate.Enable)
			{
				var camTransform = CameraManager.Instance.TargetCamera.transform;
				_rotateTween = RotateCamera(
					camTransform, 
					Rotate.Value, 
					Rotate.Duration, 
					Rotate.IsAdded, 
					Rotate.IsLocal)
				.SetEase(Rotate.Easing);
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