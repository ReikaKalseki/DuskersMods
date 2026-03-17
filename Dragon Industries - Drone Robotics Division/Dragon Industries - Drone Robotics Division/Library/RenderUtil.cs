using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ReikaKalseki.DIDrones {

	public static class RenderUtil {

		private static readonly HashSet<string> warnNoTextures = new HashSet<string>();
		
		public static Texture extractTexture(GameObject go, string texType) {
			return go.GetComponentInChildren<Renderer>().materials[0].GetTexture(texType);
		}
		
		public static Texture2D getSpriteTexture(Sprite s) {
			if (!Mathf.Approximately(s.rect.width, s.texture.width)) {
	        	Texture2D newTex = new Texture2D((int)s.rect.width, (int)s.rect.height);
	        	Rect r = s.textureRect;
	            Color[] newColors = s.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
	            newTex.SetPixels(newColors);
	            newTex.Apply();
	            return newTex;
	       	}
			else {
	             return s.texture;
			}
	    }
		
		public static Texture2D duplicateTexture(Texture2D source) {
		    RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);		
		    Graphics.Blit(source, renderTex);
		    RenderTexture previous = RenderTexture.active;
		    RenderTexture.active = renderTex;
		    Texture2D copy = new Texture2D(source.width, source.height);
		    copy.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
		    copy.Apply();
		    RenderTexture.active = previous;
		    RenderTexture.ReleaseTemporary(renderTex);
		    return copy;
		}
		
		public static void dumpTextures(Assembly a, Renderer r) {
			foreach (Material m in r.materials) {
				dumpTextures(a, m, new string(r.gameObject.name.Select(ch => System.Array.IndexOf(Path.GetInvalidPathChars(), ch) >= 0 ? '_' : ch).ToArray())+"_$_");
			}
		}
		
		public static void dumpTextures(Assembly a, Material m, string tex, string prefix = "") {
			string fn = prefix+m.name+"_-_"+tex;
			Texture2D img = (Texture2D)m.GetTexture(tex);
			dumpTexture(a, fn, img);
		}
		
		public static void dumpTexture(Assembly a, string fn, RenderTexture img, string pathOverride = null) {
		    Texture2D copy = new Texture2D(img.width, img.height);
		    copy.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
		    copy.Apply();
		    dumpTexture(a, fn, copy, pathOverride);
		}
		
		public static void dumpTexture(Assembly a, string fn, Texture2D img, string pathOverride = null) {
			if (img != null) {
				byte[] raw = duplicateTexture(img).EncodeToPNG();
				string folder = Path.GetDirectoryName(a.Location);
				string path = Path.Combine(Path.Combine(folder, "TextureDump"), fn+".png");
				if (!string.IsNullOrEmpty(pathOverride))
					path = Path.Combine(pathOverride, fn+".png");
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllBytes(path, raw);
			}
		}
		
	}
}
