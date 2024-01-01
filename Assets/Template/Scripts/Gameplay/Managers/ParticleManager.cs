

using DancingLineSample.Utility;
using UnityEngine;

namespace DancingLineSample.Gameplay
{
	public class ParticleManager : Singleton<ParticleManager>
	{
#pragma warning disable
		
		[Header("Collect Cube")]
		[Space]
		[SerializeField] private ParticleSystem m_collectFlyingParticle;
		[SerializeField] private ParticleSystem m_collectShinyParticle;
		[SerializeField] private ParticleSystem m_collectCubeSegmentParticle;
		[Space] 
		[Header("Checkpoint")]
		[SerializeField] private ParticleSystem m_checkpointParticle;
		[Space] 
		[Header("Other")]
		[Space] 
		[SerializeField] private Transform m_particleLayer;
		
#pragma warning restore

		public void PlayEffect(Vector3 worldPos)
		{
			var flying = Instantiate(m_collectFlyingParticle, m_particleLayer);
			var shiny = Instantiate(m_collectShinyParticle, m_particleLayer);
			flying.transform.position = worldPos;
			shiny.transform.position = worldPos;
			flying.Play();
			shiny.Play();
		}

		public void PlayCollectCubeEffect(Vector3 worldPos)
		{
			var flying = Instantiate(m_collectFlyingParticle, m_particleLayer);
			var shiny = Instantiate(m_collectShinyParticle, m_particleLayer);
			var cubeSegment = Instantiate(m_collectCubeSegmentParticle, m_particleLayer);
			flying.transform.position = worldPos;
			shiny.transform.position = worldPos;
			cubeSegment.transform.position = worldPos;
			flying.Play();
			shiny.Play();
			cubeSegment.Play();
		}

		public ParticleSystem GetCheckpointParticle(Transform trans)
		{
			return Instantiate(m_checkpointParticle, trans);
		}
	}
}
