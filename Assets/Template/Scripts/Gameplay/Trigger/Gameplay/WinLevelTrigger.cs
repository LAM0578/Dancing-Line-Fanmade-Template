using System;
using DancingLineSample.UI;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineSample.Gameplay.Trigger
{
	public class WinLevelTrigger : MonoBehaviour
	{
		[SerializeField] private UnityEvent m_OnWinLevel = new UnityEvent();
		
		private void WinLevel()
		{
			GameplayManager.Instance.ChangeToResult(1, PlayerStatus.Win);
			UIManager.Instance.ResultUI.ChangeButtonStatus(false);
			m_OnWinLevel?.Invoke();
		}
		
		private void OnTriggerEnter(Collider other)
		{
			if (!other.gameObject.CompareTag("Player")) return;
			WinLevel();
		}
	}
}