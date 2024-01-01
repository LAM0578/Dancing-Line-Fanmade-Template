using UnityEngine;
using UnityEngine.Events;

namespace DancingLineSample.Gameplay.Trigger
{
	public class ChangeTurnableTrigger : MonoBehaviour
	{
		[SerializeField] private bool m_IsTurnable = true;
		[SerializeField] private UnityEvent m_OnChangeTurnable = new UnityEvent();
		
		private void ChangeTurnable()
		{
			GameplayManager.Instance.AllowTurn = m_IsTurnable;
			m_OnChangeTurnable?.Invoke();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.gameObject.CompareTag("Player")) return;
			ChangeTurnable();
		} 
	}
}