using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using BepInEx;
using BepInEx.Configuration;

using ReikaKalseki.DIDrones;

namespace ReikaKalseki.DIDrones {

	public class ConfigEntry : Attribute {

		public readonly string category;
		public readonly string desc;
		public readonly object defaultValue;
		public readonly object vanillaValue;

		public ConfigEntry(string cat, string d, object def, object v) {
			category = cat;
			desc = d;
			defaultValue = def;
			vanillaValue = v;
		}
	}
}
