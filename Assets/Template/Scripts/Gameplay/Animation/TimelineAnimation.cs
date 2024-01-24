using System.Collections;
using System.Collections.Generic;
using DancingLineSample.Attributes;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineAnimation : MonoBehaviour
{
#pragma warning disable

	[SerializeField] private PlayableDirector m_PlayableDirector;
	
	
#pragma warning restore

	[MethodButton("Play", true)]
	private void Play()
	{
		m_PlayableDirector.time = 0;
		m_PlayableDirector.Play();
	}

	[MethodButton("Pause", true)]
	private void Pause()
	{
		m_PlayableDirector.Pause();
	}

	[MethodButton("Stop", true)]
	private void Stop()
	{
		m_PlayableDirector.Stop();
	}
	
	private void PlayAt(double time)
	{
		m_PlayableDirector.Stop();
		m_PlayableDirector.time = time;
		m_PlayableDirector.Play();
	}

	private void StayAt(double time)
	{
		m_PlayableDirector.Pause();
		m_PlayableDirector.time = time;
		m_PlayableDirector.Evaluate();
	}
	
	[MethodButton("PlayAtTest", true)]
	private void PlayAtTest()
	{
		PlayAt(2.550);
	}
	
	[MethodButton("StayAtTest", true)]
	private void StayAtTest()
	{
		StayAt(3.400);
	}
	
	
}
