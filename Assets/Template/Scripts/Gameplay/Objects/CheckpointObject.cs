using System;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay.Objects
{
	public class CheckpointObject : MonoBehaviour
	{
#pragma warning disable
		
		[Tooltip("检查点对象 (粒子效果起始位置, 可为自身)")]
		[SerializeField] 
		private GameObject m_CheckpointObject;
		
		[Tooltip("检查点粒子效果的 Transform")]
		[SerializeField] 
		private Transform m_ParticleTransform;
		
		[Tooltip("检查点粒子效果最终位置的 Transform")]
		[SerializeField] 
		private Transform m_ParticleTargetTransform;
		
		[Tooltip("粒子效果的持续时间")]
		[SerializeField] 
		private float m_ParticleEffectDuration;
		
		[Tooltip("皇冠标识对象")]
		[SerializeField] private GameObject m_CrownIconObject;
		
#pragma warning restore
		
		private const float _effectHeight = 5f;
		
		private static readonly int _m_Progress = Shader.PropertyToID("_Progress");

		private bool _hasCollect;
		private bool _hasLost;
		private Material _crownMaterial;

		private void SetCrownMaterialInstance()
		{
			if (_crownMaterial) return;
			var renderer = m_CrownIconObject.GetComponent<MeshRenderer>();
			if (!renderer) return;
			_crownMaterial = Instantiate(renderer.material);
			renderer.material = _crownMaterial;
		}

		private void DoParabola(Vector3 originalPos, Vector3 targetPos)
		{
			var middlePos = Vector3.Lerp(originalPos, targetPos, .5f);
			var sequence = DOTween.Sequence();
			var trans = m_ParticleTransform;
			float halfDuration = m_ParticleEffectDuration / 2f;
			float maxHeight = Mathf.Max(originalPos.y, targetPos.y);
			middlePos.y = maxHeight + _effectHeight;
			// part 0
			sequence.Join(trans.DOMoveX(middlePos.x, halfDuration).SetEase(Ease.Linear));
			sequence.Join(trans.DOMoveY(middlePos.y, halfDuration).SetEase(Ease.OutCubic));
			sequence.Join(trans.DOMoveZ(middlePos.z, halfDuration).SetEase(Ease.Linear));
			// wait
			sequence.AppendInterval(halfDuration);
			// part 1
			sequence.Join(trans.DOMoveX(targetPos.x, halfDuration).SetEase(Ease.Linear));
			sequence.Join(trans.DOMoveY(targetPos.y, halfDuration).SetEase(Ease.InCubic));
			sequence.Join(trans.DOMoveZ(targetPos.z, halfDuration).SetEase(Ease.Linear));
			// wait
			sequence.AppendInterval(halfDuration);
			// crown
			sequence.Join(
				_crownMaterial
					.DOFloat(1, _m_Progress, .2f)
					.SetEase(Ease.OutCubic));
			// play
			sequence.Play();
		}

		private ParticleSystem GetParticle(float duration)
		{
			var particle = ParticleManager.Instance.GetCheckpointParticle(m_ParticleTransform);
			var particleMain = particle.main;
			particleMain.simulationSpeed = particleMain.duration / duration;
			particle.gameObject.transform.localPosition = new Vector3();
			return particle;
		}

		/// <summary>
		/// 播放收集此检查点时的效果
		/// </summary>
		public void DoCollectEffect()
		{
			if (_hasCollect) return;
			m_CheckpointObject.SetActive(false);
			GetParticle(m_ParticleEffectDuration).Play();
			var originalPos = m_CheckpointObject.transform.position;
			var targetPos = m_ParticleTargetTransform.position;
			SetCrownMaterialInstance();
			DoParabola(originalPos, targetPos);
			_hasCollect = true;
		}

		/// <summary>
		/// 播放失去此检查点时的效果
		/// </summary>
		public void DoLostEffect()
		{
			if (_hasLost) return;
			GetParticle(m_ParticleEffectDuration).Play();
			SetCrownMaterialInstance();
			var trans = m_ParticleTransform;
			trans.position = m_ParticleTargetTransform.position;
			trans.DOMoveY(trans.position.y + _effectHeight * 2, m_ParticleEffectDuration * 2);
			_crownMaterial
				.DOFloat(0, _m_Progress, .2f)
				.SetEase(Ease.InCubic);
			_hasLost = true;
		}

		/// <summary>
		/// 重置此检查点效果为初始状态
		/// </summary>
		public void ResetEffect()
		{
			m_ParticleTransform.position = m_CheckpointObject.transform.position;
			_hasCollect = false;
			_hasLost = false;
			m_CheckpointObject.SetActive(true);
			if (!_crownMaterial) return;
			_crownMaterial.SetFloat(_m_Progress, 0);
		}

#if UNITY_EDITOR
		[Button("DoEffectTest")]
		public void DoEffectTest()
		{
			var trans = m_ParticleTransform;
			trans.position = m_CheckpointObject.transform.position;
			_hasCollect = false;
			m_CheckpointObject.SetActive(true);
			if (_crownMaterial) _crownMaterial.SetFloat(_m_Progress, 0);
			DoCollectEffect();
		}
#endif
	}
}