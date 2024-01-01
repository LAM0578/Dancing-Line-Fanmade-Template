using System.IO;
using UnityEngine;

namespace DancingLineSample.Utility
{
	public static class RenderTextureUtility
	{
		public static Texture2D ToTexture2D(this RenderTexture rt, bool linear = false)
		{
			var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, linear);
			var old_rt = RenderTexture.active;
			RenderTexture.active = rt;

			tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			tex.Apply();

			RenderTexture.active = old_rt;
			return tex;
		}
		
		public static void SaveTo(this RenderTexture rt, string path, bool linear = false)
		{
			var tex = rt.ToTexture2D(linear);
			byte[] vs = tex.EncodeToPNG();

			var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
			fileStream.Write(vs, 0, vs.Length);
			fileStream.Dispose();
			fileStream.Close();
			
			RenderTexture.active = null;
		}
	}
}
