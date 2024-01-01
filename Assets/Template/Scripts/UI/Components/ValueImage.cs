using UnityEngine;
using UnityEngine.UI;

namespace DancingLineSample.UI.Components
{
	[RequireComponent(typeof(Button))]
	public class ValueImage : MonoBehaviour
	{
#pragma warning disable
		
		[SerializeField] private Image m_TargetImage;
		[Space]
		[SerializeField] private Sprite m_InactiveSprite;
		[SerializeField] private Sprite m_ActiveSprite;
		
#pragma warning restore
		
		public bool Active { get; private set; }

		/// <summary>
		/// 根据激活状态来设置此目标 Image 的 Sprite
		/// </summary>
		/// <param name="active">激活状态</param>
		public void SetActive(bool active)
		{
			Active = active;
			m_TargetImage.sprite = Active ? m_ActiveSprite : m_InactiveSprite;
		}

		/// <summary>
		/// 按钮被按下时调用此方法来切换目标 Image 的 Sprite
		/// </summary>
		public void OnButtonClick()
		{
			SetActive(!Active);
		}
	}
}