using DG.Tweening;
using SuperBlur;
using UnityEngine;
using UnityEngine.UI;

namespace DancingLineSample.Utility
{
	public static class DOTweenUtility
	{
		public static Tweener DOBlurUI(
			this SuperBlurBase superBlurBase, 
			Image img, 
			Color blurUIColor, 
			float duration, 
			bool show
		)
		{
			bool hasImg = img;
			float interpolation = show ? 0 : 1;
			return DOTween.To(
				() => interpolation,
				val =>
				{
					superBlurBase.interpolation = val;
					if (hasImg) img.color = Color.Lerp(Color.white, blurUIColor, val);
					interpolation = val;
				},
				show ? 1 : 0,
				duration);
		}
	}
}
