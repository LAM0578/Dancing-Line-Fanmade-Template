using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DancingLineSample.Gameplay.Trigger;
using DancingLineSample.UI;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay
{
	public class MainLine : MonoBehaviour
	{
		public GameObject LineTailPrefab;
		public GameObject DeadCubePrefab;
		public Transform LineTailLayer;
		public GameObject LandingParticle;
		public float Speed;
		[Space]
		public Vector3 ResetPosition;
		[Space]
		public Vector3 StartForward;
		public Vector3 TurnForward;
		[Space] 
		public LayerMask GroundLayerMask;
		
		private bool _isTurned;
		private Vector3 _currentPosition;
		private Vector3 _lastPosition;
		private GameObject _currentTail;
		
		private Rigidbody _lineRigidbody;
		
		private readonly LinkedList<GameObject> _tails = new LinkedList<GameObject>();
		private readonly LinkedList<GameObject> _deadCubeInstances = new LinkedList<GameObject>();

		private bool _onGround;
		private Quaternion _currentQuaternion;

#pragma warning disable
		
		[SerializeField] private float m_CheckGroundMaxDistance;
		[SerializeField] private float m_CheckGroundRadius;
		
#pragma warning restore

		private bool _isOnGround
		{
			get => _onGround;
			set
			{
				if (_onGround == value) return;
				if (value)
				{
					CreateTail();
					var particle = Instantiate(LandingParticle, LineTailLayer);
					particle.transform.localPosition = transform.localPosition;
					particle.GetComponent<ParticleSystem>().Play();
				}
				else
				{
					CollectTail();
					_currentTail = null;
				}
				_onGround = value;
			}
		}

		public bool AllowDead { get; set; }

		private bool _isPlaying;
		private bool _isDead;

		private void Awake()
		{
			_lineRigidbody = GetComponent<Rigidbody>();
			_lineRigidbody.isKinematic = true;
		}

		private void Update()
		{
			if (!_isPlaying) return;
			_isOnGround = UpdateOnGroupStatus();
			UpdateLinePosition();
			UpdateCurrentTail();
		}

		private void UpdateLinePosition()
		{
			Transform lineTransform;
			(lineTransform = transform).localRotation = _currentQuaternion;
			
			var localPosition = lineTransform.localPosition;
			var pos = localPosition + lineTransform.forward * (Time.deltaTime * Speed);
			
			_currentPosition = pos;
			lineTransform.localPosition = pos;
		}

		private bool UpdateOnGroupStatus()
		{
			var lineTranform = transform;
			var up = lineTranform.up;
			bool result = Physics.SphereCast(
				lineTranform.position + up, 
				m_CheckGroundRadius, 
				-up,
				out _, 
				m_CheckGroundMaxDistance, 
				GroundLayerMask
			);
			return result;
		}

		private void UpdateCurrentTail()
		{
			if (!_currentTail) return;
			
			var tailTransform = _currentTail.transform;
			tailTransform.localRotation = _currentQuaternion; 
			
			var dir = _currentPosition - _lastPosition;
			var pos = _lastPosition + dir / 2f;
			pos.y = transform.localPosition.y;
			tailTransform.localPosition = pos;
			
			float scaleZ = Vector3.Distance(_currentPosition, _lastPosition) + transform.localScale.z;
			var scale = tailTransform.localScale;
			
			scale.z = scaleZ;
			tailTransform.localScale = scale;
		}

		private void CollectTail()
		{
			if (!_currentTail) return;
			_tails.AddLast(_currentTail);
		}

		private void CreateTail()
		{
			CollectTail();

			_currentTail = Instantiate(LineTailPrefab, LineTailLayer);
			_currentTail.SetActive(true);
			
			var localPosition = transform.localPosition;
			_currentTail.transform.position = localPosition;
			_lastPosition = localPosition;
		}

		private void ClearTails()
		{
			if (_currentTail)
			{
				Destroy(_currentTail);
				_currentTail = null;
			}
			foreach (var obj in _tails)
			{
				Destroy(obj);
			}
			_tails.Clear();
		}

		/// <summary>
		/// 当开始游戏时调用
		/// </summary>
		public void Play()
		{
			_isDead = false;
			_isPlaying = true;
			if (_onGround)
				CreateTail();
			_lineRigidbody.isKinematic = false;
		}

		/// <summary>
		/// 当游戏暂停时调用
		/// </summary>
		public void Pause()
		{
			_isPlaying = false;
			_lineRigidbody.isKinematic = true;
		}

		/// <summary>
		/// 清除线生成的所有对象
		/// </summary>
		public void ClearLineObjects()
		{
			ClearTails();
			foreach (
				var obj in 
				_deadCubeInstances.Where(t => t != null)
			) Destroy(obj);
			_deadCubeInstances.Clear();
		}

		/// <summary>
		/// 当线重置时调用
		/// </summary>
		public void Restart()
		{
			foreach (
				var obj in 
				_deadCubeInstances.Where(t => t != null)
			) Destroy(obj);
			_deadCubeInstances.Clear();
			
			Pause();
			ClearTails();
			_isDead = false;
			_isTurned = false;
			_currentQuaternion = Quaternion.Euler(StartForward);
			
			transform.position = ResetPosition;
			transform.localRotation = Quaternion.Euler(StartForward);
		}

		/// <summary>
		/// 当游戏继续时调用
		/// </summary>
		public void Continue()
		{
			if (_isDead) return;
			_isPlaying = true;
			_lineRigidbody.isKinematic = false;
		}

		/// <summary>
		/// 当点击时调用
		/// </summary>
		public void Turn()
		{
			if (!_isOnGround || _isDead) return;
			_isTurned = !_isTurned;
			_currentQuaternion = Quaternion.Euler(_isTurned ? TurnForward : StartForward);
			CreateTail();
		}

		private void OnCollisionEnter(Collision other)
		{
			if (!other.gameObject.CompareTag("Wall") || !AllowDead) return;
			
			Pause();
			_isDead = true;
			GameplayManager.Instance.LineStatus = PlayerStatus.Dead;
			var deadObj = Instantiate(DeadCubePrefab, LineTailLayer);
			deadObj.SetActive(true);
			deadObj.transform.localPosition = transform.localPosition;
			_deadCubeInstances.AddLast(deadObj);
			GameplayManager.Instance.CalculateResult();
			UIManager.Instance.ChangeResultUI(true);
			GameplayManager.Instance.FadeOutMusic();
		}

		public void ChangeForward(bool isTurnedForward)
		{
			_isTurned = isTurnedForward;
			_currentQuaternion = Quaternion.Euler(_isTurned ? TurnForward : StartForward);
			transform.localRotation = _currentQuaternion;
		}

		public void ChangeStatusByCheckpoint(CheckpointTrigger checkpoint)
		{
			_isTurned = checkpoint.TurnedForward;
			_currentPosition = checkpoint.CheckpointPosition;
			_currentQuaternion = Quaternion.Euler(_isTurned ? TurnForward : StartForward);
			var lineTrans = transform;
			lineTrans.localPosition = _currentPosition;
			lineTrans.localRotation = _currentQuaternion;
		}
	}
}
