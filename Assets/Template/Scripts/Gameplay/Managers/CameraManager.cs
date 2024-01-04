using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DancingLineSample.Editing.Utility;
using DancingLineSample.Gameplay.Trigger;
using DancingLineSample.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay
{
	#pragma warning disable 0649
	public class CameraManager : Singleton<CameraManager>
	{
		[Header("Camera")]
		public Camera TargetCamera;
		public Transform FollowObject;
		public CameraResetStatus CameraResetStatus;
		public float Smooth;
		
		[Header("Debug")] 
		[SerializeField] private bool m_EnableDebug;
		[SerializeField] private Color m_LineColor;
		[SerializeField] private Color m_OriginLineColor;
		[SerializeField] private Color m_TextColor;
		[SerializeField] private Color m_TextBackgroundColor;
		[SerializeField] private Color m_FollowObjectColor;
		[SerializeField] private float m_PointRadius;
		
		public bool IsAnimCamera { get; set; }
		[HideInInspector] public Vector3 TriggerOffset;

		private List<CameraTrigger> _cameraTriggers = new List<CameraTrigger>();
		private CameraResetStatus _currentCameraResetStatus;
		private Vector3 _currentFollowPos;
		private Vector3 _currentTriggerOffset;

		public bool UpdateFollowPos { get; set; } = true;

		protected override void OnAwake()
		{
			_currentCameraResetStatus = CameraResetStatus;
			FindAllTriggers();
		}

		private void LateUpdate()
		{
			if (IsAnimCamera || !GameplayManager.Instance.IsPlaying) return;
			
			var camTransform = TargetCamera.transform;
			if (UpdateFollowPos)
			{
				_currentFollowPos = FollowObject.position;
				_currentTriggerOffset = TriggerOffset;
			}
			var pos = _currentFollowPos + _currentCameraResetStatus.ResetOffset + _currentTriggerOffset;
			if (Smooth <= 0)
			{
				camTransform.position = pos;
			}
			else
			{
				var position = camTransform.position;
				position = Vector3.Lerp(position, pos, Time.deltaTime * Smooth);
				camTransform.position = position;
			}
		}

		/// <summary>
		/// 重置相机状态
		/// </summary>
		public void ResetStatus()
		{
			ResetTriggerStatus();
			_currentCameraResetStatus = CameraResetStatus;
			var camTransform = TargetCamera.transform;
			TriggerOffset = new Vector3();
			camTransform.position = FollowObject.position + _currentCameraResetStatus.ResetOffset;
			camTransform.rotation = Quaternion.Euler(_currentCameraResetStatus.ResetRotation);
			TargetCamera.fieldOfView = _currentCameraResetStatus.ResetFieldOfView;
			UpdateFollowPos = true;
		}

		/// <summary>
		/// 根据 CameraResetStatus 对象重置相机状态
		/// </summary>
		/// <param name="resetStatus">CameraResetStatus 对象</param>
		public void ResetStatus(CameraResetStatus resetStatus)
		{
			ResetTriggerStatus();
			var camTransform = TargetCamera.transform;
			TriggerOffset = new Vector3();
			camTransform.position = FollowObject.position + resetStatus.ResetOffset;
			camTransform.rotation = Quaternion.Euler(resetStatus.ResetRotation);
			TargetCamera.fieldOfView = CameraResetStatus.ResetFieldOfView;
			_currentCameraResetStatus = resetStatus;
			UpdateFollowPos = true;
		}
		
		/// <summary>
		/// 在当前场景中查找所有相机触发器
		/// </summary>
		public void FindAllTriggers()
		{
			ClearTriggers();
			_cameraTriggers = Resources.FindObjectsOfTypeAll<CameraTrigger>().ToList();
		}

		/// <summary>
		/// 清除所有触发器
		/// </summary>
		public void ClearTriggers()
		{
			_cameraTriggers.Clear();
		}

		private void ResetTriggerStatus()
		{
			foreach (var t in _cameraTriggers)
			{
				t.ResetStatus();
			}
		}

		/// <summary>
		/// 继续所有触发器动画
		/// </summary>
		public void ContinueTriggers()
		{
			foreach (var t in _cameraTriggers)
			{
				t.ChangeTweenStatus(true);
			}
		}

		/// <summary>
		/// 暂停所有触发器动画
		/// </summary>
		public void PauseTriggers()
		{
			foreach (var t in _cameraTriggers)
			{
				t.ChangeTweenStatus(false);
			}
		}
		
#if UNITY_EDITOR

		private Color _textColor;
		private Color _textBackgroundColor;
		private Texture2D _textBackground;
		private static GUIStyle _style;

		private void OnDrawGizmosUpdate()
		{
			if (_style == null)
			{
				updateStyle();
				return;
			}
			
			if (_textColor != m_TextColor)
			{
				updateStyle();
				_textColor = m_TextColor;
				return;
			}
			
			if (_textBackgroundColor != m_TextBackgroundColor)
			{
				if (_textBackground)
				{
					DestroyImmediate(_textBackground);
				}
				_textBackgroundColor = m_TextBackgroundColor;
				_textBackground = _textBackgroundColor.ToTexture2D();
				updateStyle();
				return;
			}

			return;
			
			void updateStyle()
			{
				_style = new GUIStyle
				{
					normal = new GUIStyleState
					{
						textColor = _textColor, // Color.white,
						background = _textBackground // _backgroundColor.ToTexture2D()
					}
				};
			}
		}

		private void OnDrawGizmos()
		{
			if (!m_EnableDebug || !TargetCamera) return;
			OnDrawGizmosUpdate();
			
			var camPos = TargetCamera.transform.position;
			
			Gizmos.color = m_LineColor; // Color.yellow;
			Gizmos.DrawLine(camPos, _currentFollowPos);
			Gizmos.DrawCube(camPos, Vector3.one * m_PointRadius);
			Handles.Label(camPos,$"Position {camPos}", _style);

			Gizmos.color = m_FollowObjectColor; // Color.magenta;
			Gizmos.DrawCube(_currentFollowPos, Vector3.one * m_PointRadius);
			Handles.Label(_currentFollowPos,$"Follow {_currentFollowPos}", _style);
			
			var pos = camPos - _currentTriggerOffset;
			if (camPos == pos) return;

			Gizmos.color = m_OriginLineColor; // _orange;
			Gizmos.DrawLine(pos, _currentFollowPos);
			Gizmos.DrawCube(pos, Vector3.one * m_PointRadius);
			Handles.Label(pos,$"Position {pos}\nOffset {_currentTriggerOffset}", _style);
			
		}

#endif
	}
}
