using System;
using DancingLineSample.Attributes;
using DancingLineSample.EditorUtility;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay.Objects
{
	public class CheckpointObject : MonoBehaviour
	{
#pragma warning disable
		
		[Header("Checkpoint Object Settings")]
		
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

		[Header("Debug")] 
		
		[SerializeField] private bool m_EnableDebug;
		[SerializeField] private Color m_LineColor;
		
#pragma warning restore
		
		private const float _effectHeight = 4f;
		
		private static readonly int _m_Progress = Shader.PropertyToID("_Progress");

		private bool _hasCollect;
		private bool _hasLost;
		private Material _crownMaterial;
		private EaseFunction _easeFunction;

#if UNITY_EDITOR
		
		private Vector3 _originalPos;
		private Vector3 _targetPos;
		private Vector3[] _points;
		
#endif

		// private void Awake()
		// {
		// 	SetCrownMaterialInstance();
		// }

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
			_easeFunction = DOTweenUtility.CalculateParabolaFunction(
				originalPos,
				targetPos,
				_effectHeight
			);
			
			var sequence = DOTween.Sequence();
			var trans = m_ParticleTransform;
			float duration = m_ParticleEffectDuration;
			
			sequence.Join(trans.DOMoveX(targetPos.x, duration).SetEase(Ease.Linear));
			sequence.Join(DOTweenUtility.DOFunction(
				_easeFunction,
				y =>
				{
					var p = trans.position;
					p.y = y;
					trans.position = p;
				},
				duration
			).SetEase(Ease.Linear));
			sequence.Join(trans.DOMoveZ(targetPos.z, duration).SetEase(Ease.Linear));
			if (_crownMaterial)
			{
				sequence.Append(_crownMaterial
					.DOFloat(1, _m_Progress, .2f)
					.SetEase(Ease.OutCubic));
			}
			
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
			// print(string.Join(", ", new Vector3[]{ originalPos, targetPos}));
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
		[MethodButton("DoEffectTest")]
		public void DoEffectTest()
		{
			var trans = m_ParticleTransform;
			trans.position = m_CheckpointObject.transform.position;
			_hasCollect = false;
			m_CheckpointObject.SetActive(true);
			if (_crownMaterial) _crownMaterial.SetFloat(_m_Progress, 0);
			DoCollectEffect();
		}

		private void OnDrawGizmosUpdate()
		{
			if (!m_CheckpointObject || !m_ParticleTargetTransform) return;
			
			var originalPos = m_CheckpointObject.transform.position;
			var targetPos = m_ParticleTargetTransform.position;
			
			if (_originalPos != originalPos)
			{
				_originalPos = originalPos;
				goto updatePoints;
			}

			if (_targetPos != targetPos)
			{
				_targetPos = targetPos;
				goto updatePoints;
			}
			
			return;
			
			updatePoints:
			_points = MathUtility
				.CalculateParabolaPoints(
					originalPos, targetPos, _effectHeight)
				.ToArray();
		}

		private void OnDrawGizmos()
		{
			if (!m_EnableDebug) return;
			
			OnDrawGizmosUpdate();

			Handles.color = m_LineColor;
			Handles.DrawAAPolyLine(4f, _points);
		}
#endif
	}
}