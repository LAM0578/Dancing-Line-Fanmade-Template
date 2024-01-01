using UnityEngine;

namespace DancingLineSample.Editing.Utility
{
	public static class ColorUtility
	{
		public static Texture2D ToTexture2D(this Color color)
		{
			Texture2D texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, color);
			texture.Apply();
			return texture;
		}
	}
}