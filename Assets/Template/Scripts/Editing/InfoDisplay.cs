using System;
using DancingLineSample.Editing.Utility;
using DancingLineSample.Gameplay;
using DancingLineSample.Utility;
using DG.Tweening;
using UnityEngine;

namespace DancingLineSample.Editing
{
	public class InfoDisplay : Singleton<InfoDisplay>
	{
		
#pragma warning disable

		[SerializeField] 
		private bool m_Enable;
		
		[SerializeField] 
		private Color m_TextColor;
		
		[Range(0, 50)]
		[SerializeField] 
		private int m_FontSize;
		
#pragma warning restore
		
		private GUIStyle _style;
		private Color _textColor;
		private float _fontSize;
		private float _aspectRatio;

		private Resolution _resolution;
	
#if UNITY_EDITOR
		
		private void OnDrawGizmosUpdate()
		{
			
			if (_style == null) updateStyle();
			
			if (_textColor != m_TextColor)
			{
				updateStyle();
				_textColor = m_TextColor;
			}
			
			if (Math.Abs(_fontSize - m_FontSize) > float.Epsilon)
			{
				updateStyle();
				_fontSize = m_FontSize;
			}
			
			float aspectRatio = Mathf.Min(
				1920f / Screen.width,
				1080f / Screen.height
			);
			
			if (Math.Abs(_aspectRatio - aspectRatio) > float.Epsilon)
			{
				_aspectRatio = aspectRatio;
				updateStyle();
			}
			
			return;

			void updateStyle()
			{
				// print($"{m_FontSize} * {_aspectRatio} = {m_FontSize / _aspectRatio}");
				_style = new GUIStyle()
				{
					normal = new GUIStyleState()
					{
						textColor = m_TextColor,
					},
					fontSize = (int)(m_FontSize / _aspectRatio)
				};
			}
		}
		
		private void OnGUI()
		{
			if (!m_Enable) return;
			OnDrawGizmosUpdate();
			var gameplayManager = GameplayManager.Instance;
			var curStatus = gameplayManager.LineStatus;
			var curTiming = gameplayManager.CurrentTiming;
			var curProgress = gameplayManager.CurrentProgress * 100f;
			var curPosition = gameplayManager.Line.transform.position;
			GUI.Label(new Rect(
					m_FontSize / _aspectRatio, 
					m_FontSize / _aspectRatio, 
					Screen.width, 
					Screen.height
				), 
				$"Status: {curStatus}\n" +
				$"Timing: {curTiming}\n" +
				$"Progress: {curProgress}%\n" +
				$"Position: {curPosition}",
				_style);
		}
#endif
	}
}