using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DancingLineSample.Gameplay.Trigger
{
	public class CollectCubeTrigger : MonoBehaviour
	{
		private void Collect()
		{
			if (GameplayManager.Instance.LineStatus != PlayerStatus.Playing)
				return;
			GameplayManager.Instance.PlayingGameplayData.CollectCount++;
			ParticleManager.Instance.PlayCollectCubeEffect(transform.position);
			gameObject.SetActive(false);
		}

		public void ResetTrigger()
		{
			gameObject.SetActive(true);
		}

		public void OnTriggerEnter(Collider other)
		{
			Collect();
		}
	}
}
