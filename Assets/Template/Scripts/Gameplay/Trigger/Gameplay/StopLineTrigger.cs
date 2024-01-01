using UnityEngine;

namespace DancingLineSample.Gameplay.Trigger
{
	public class StopLineTrigger : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if (!other.gameObject.CompareTag("Player")) return;
			GameplayManager.Instance.Line.Pause();
		}
	}
}