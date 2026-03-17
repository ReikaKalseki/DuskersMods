using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;

using UnityEngine;

namespace ReikaKalseki.DIDrones
{
	public static class TextureManager {
		
		private static readonly Dictionary<Assembly, Dictionary<string, Texture2D>> textures = new Dictionary<Assembly, Dictionary<string, Texture2D>>();
		//private static readonly Texture2D NOT_FOUND = ImageUtils.LoadTextureFromFile(path); 
		
		static TextureManager() {
			
		}
		
		public static void refresh() {
			textures.Clear();
		}
		
		public static Texture2D getTexture(Assembly a, string path) {
			if (a == null)
				throw new Exception("You must specify a mod to load the texture for!");
			if (!textures.ContainsKey(a))
				textures[a] = new Dictionary<string, Texture2D>();
			if (!textures[a].ContainsKey(path)) {
				textures[a][path] = loadTexture(a, path);
			}
			return textures[a][path];
		}
		
		private static Texture2D loadTexture(Assembly a, string relative) {
			string folder = Path.GetDirectoryName(a.Location);
			string path = Path.Combine(folder, relative+".png");
			DSUtil.log("Loading texture from '"+path+"'", a);
			Texture2D newTex = null;//ImageUtils.LoadTextureFromFile(path); //TODO
			if (newTex == null) {
				//newTex = NOT_FOUND;
				DSUtil.log("Texture not found @ "+path, a);
			}
			return newTex;
		}
		
	}
}
