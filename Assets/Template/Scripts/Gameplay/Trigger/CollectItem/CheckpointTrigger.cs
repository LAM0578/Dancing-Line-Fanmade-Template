using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DancingLineSample.Gameplay.Objects;
using DancingLineSample.UI;
using DancingLineSample.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay.Trigger
{
	public class CheckpointTrigger : MonoBehaviour
	{
#pragma warning disable
		
		[Tooltip("该复活点的复活时间")] 
		[SerializeField] 
		private int m_CheckpointTime;
		
		[Tooltip("该复活点的复活位置")] 
		[SerializeField] 
		private Vector3 m_CheckpointPosition;
		
		[Tooltip("复活时的朝向是否为点击后的朝向")] 
		[SerializeField] 
		private bool m_TurnedForward;
		
		[Space]
		
		[Tooltip("覆盖复活时的相机数据")] 
		[SerializeField]
		private bool m_OverrideCameraResetStatus;

		[Tooltip("复活时的相机数据")] 
		[SerializeField]
		[PropertyActive("m_OverrideCameraResetStatus", false)]
		private CameraResetStatus m_CameraResetStatus;
		
		[Space]
		
		[Tooltip("此检查点的效果对象")] 
		[SerializeField]
		private CheckpointObject m_CheckpointObject;
		
		[Tooltip("在复活时需要重置的场景 / 资源对象")] 
		[SerializeField]
		private ResetObjects m_ResetObjects;
		
		[Space]

		[Tooltip("在收集此检查点时调用该事件")] 
		[SerializeField] 
		private UnityEvent m_OnCollectCheckpoint;
		
		[Tooltip("在复活时调用该事件")] 
		[SerializeField] 
		private UnityEvent m_OnContinueInCheckpoint;
		
#pragma warning restore

		public int CheckpointTime => m_CheckpointTime;
		public Vector3 CheckpointPosition => m_CheckpointPosition;
		public bool TurnedForward => m_TurnedForward;
		public bool OverrideCameraResetStatus => m_OverrideCameraResetStatus;
		public CameraResetStatus CameraResetStatus => m_CameraResetStatus;
		public CheckpointObject CheckpointObject => m_CheckpointObject;
		public ResetObjects ResetObjects { get => m_ResetObjects; set => m_ResetObjects = value; }

		private bool _isActived;
		private bool _isLost;

		/// <summary>
		/// 在此检查点继续时调用该方法
		/// </summary>
		public void ContinueInThisCheckpoint()
		{
			if (!_isLost)
			{
				// if (m_CheckpointObject) 
				// 	m_CheckpointObject.DoLostEffect();
				
				int checkpointCount = GameplayManager.Instance.PlayingGameplayData.CheckpointCount;
				if (checkpointCount > 0) checkpointCount--;
				GameplayManager.Instance.PlayingGameplayData.CheckpointCount = checkpointCount;
			}
			
			GameplayManager.Instance.ContinueByCheckpoint(this);
			
			m_OnCollectCheckpoint?.Invoke();
			
			_isLost = true;
		}

		/// <summary>
		/// 收集此检查点时调用该方法
		/// </summary>
		public void CollectCheckpoint()
		{
			if (_isActived || 
			    GameplayManager.Instance.LineStatus != PlayerStatus.Playing) return;
			
			if (m_CheckpointObject) 
				m_CheckpointObject.DoCollectEffect();
			
			int checkpointCount = GameplayManager.Instance.PlayingGameplayData.CheckpointCount;
			if (checkpointCount < 3) checkpointCount++;
			
			GameplayManager.Instance.PlayingGameplayData.CheckpointCount = checkpointCount;
			GameplayManager.Instance.LastCheckpoint = this;
			
			UIManager.Instance.ResultUI.ChangeButtonStatus(true);
			
			m_OnContinueInCheckpoint?.Invoke();
			
			_isActived = true;
		}

		/// <summary>
		/// 重置此检查点状态
		/// </summary>
		public void ResetTrigger()
		{
			if (m_CheckpointObject) 
				m_CheckpointObject.ResetEffect();
			_isActived = false;
			_isLost = false;
		}

		public void OnTriggerEnter(Collider other)
		{
			if (!other.CompareTag("Player")) return;
			CollectCheckpoint();
		}

		internal void SetCheckpointPosition(Vector3 position)
		{
			m_CheckpointPosition = position;
		}

		internal void SetCameraResetStatus(CameraResetStatus resetStatus)
		{
			m_CameraResetStatus = resetStatus;
		}
	}
#if UNITY_EDITOR
	[CanEditMultipleObjects]
	[CustomEditor(typeof(CheckpointTrigger))]
	public class CheckPointTriggerEditor : Editor
	{
		private CameraManager _camManager;
		private DataManager _dataManager;
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var item = (CheckpointTrigger)target;

			if (GUILayout.Button("Set position from this transform"))
			{
				item.SetCheckpointPosition(item.transform.localPosition);
			}

			if (GUILayout.Button("Copy camera status from CameraFollow"))
			{
				if (!_camManager)
				{
					var camManager = UnityUtility.FindObjectFromCurrentScene<CameraManager>();
					if (!camManager) return;
					_camManager = camManager;
				}
				item.SetCameraResetStatus(_camManager.CameraResetStatus);
			}

			if (GUILayout.Button("Copy reset object data from DataManager"))
			{
				if (!_dataManager)
				{
					var dataManager = UnityUtility.FindObjectFromCurrentScene<DataManager>();
					if (!dataManager) return;
					_dataManager = dataManager;
				}
				
				var levelData = _dataManager.SingleLevel ? _dataManager.Level : _dataManager.Levels[0];
				var resetObjs = levelData.ResetObjects;
				
				item.ResetObjects = resetObjs;
			}
		}
	}
#endif
}
