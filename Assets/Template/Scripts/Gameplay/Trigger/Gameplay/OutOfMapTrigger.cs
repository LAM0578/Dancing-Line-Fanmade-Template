using System;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineSample.Gameplay.Trigger
{
	public class OutOfMapTrigger : MonoBehaviour
	{
		[SerializeField] private UnityEvent m_OnOutOfMap = new UnityEvent();
		
		private void OutOfMap()
		{
			float progress = GameplayManager.Instance.CurrentProgress;
			GameplayManager.Instance.ChangeToResult(progress, PlayerStatus.Dead);
			m_OnOutOfMap?.Invoke();
		}

		private void OnTriggerEnter(Collider other)
		{
			var lineStatus = GameplayManager.Instance.LineStatus;
			if (!other.gameObject.CompareTag("Player") || lineStatus != PlayerStatus.Playing) return;
			OutOfMap();
		}
	}
}