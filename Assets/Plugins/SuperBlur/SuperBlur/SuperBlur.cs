using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SuperBlur
{

	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Effects/Super Blur", -1)]
	public class SuperBlur : SuperBlurBase
	{
		// public bool UseSourceFormat = true;
		// public RenderTextureFormat TextureFormat = RenderTextureFormat.ARGB32;
		
		void OnRenderImage (RenderTexture source, RenderTexture destination) 
		{
			if (blurMaterial == null || UIMaterial == null)
			{
				Graphics.Blit(source, destination);
				return;
			}

			int tw = source.width >> downsample;
			int th = source.height >> downsample;
			
			var rt = RenderTexture.GetTemporary(
				tw, 
				th, 
				0, 
				source.format // UseSourceFormat ? source.format : TextureFormat
			);

			Graphics.Blit(source, rt);

			if (renderMode == RenderMode.Screen)
			{
				Blur(rt, destination);
			}
			else if (renderMode == RenderMode.UI)
			{
				Blur(rt, rt);
				UIMaterial.SetTexture(Uniforms._BackgroundTexture, rt);
				Graphics.Blit(source, destination);
			}
			else if (renderMode == RenderMode.OnlyUI)
			{
				Blur(rt, rt);
				UIMaterial.SetTexture(Uniforms._BackgroundTexture, rt);
			}

			RenderTexture.ReleaseTemporary(rt);
		}
			
	}

}
