using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Reflection;

using BepInEx;
using BepInEx.Configuration;

using UnityEngine;

namespace ReikaKalseki.DIDrones {

	public class ConfigSet {

		private readonly Dictionary<string, ConfigValue> values = new Dictionary<string, ConfigValue>();
		private readonly Dictionary<object, string> enumKeys = new Dictionary<object, string>();

		public readonly DIModBase ownerMod;

		public ConfigSet(DIModBase mod) {
			ownerMod = mod;
		}

		public ConfigSet addSettings(Type t) {
			foreach (object key in Enum.GetValues(t)) {
				string name = Enum.GetName(t, key);
				MemberInfo info = t.GetField(Enum.GetName(t, key));
				ConfigEntry cfg = (ConfigEntry)Attribute.GetCustomAttribute(info, typeof(ConfigEntry));
				enumKeys[key] = name;
				Type ty = cfg.defaultValue.GetType();
				Type ty2 = cfg.vanillaValue.GetType();
				if (ty != ty2)
					throw new Exception(string.Format("Mismatched setting value types {0}&{1} for setting {2}/{3}", ty.Name, ty2.Name, cfg.category, name));
				if (ty == typeof(bool))
					addSetting(cfg.category, name, (bool)cfg.defaultValue, cfg.desc, (bool)cfg.vanillaValue);
				else if (ty == typeof(int))
					addSetting(cfg.category, name, (int)cfg.defaultValue, cfg.desc, (int)cfg.vanillaValue);
				else if (ty == typeof(float))
					addSetting(cfg.category, name, (float)cfg.defaultValue, cfg.desc, (float)cfg.vanillaValue);
				else if (ty == typeof(string))
					addSetting(cfg.category, name, (string)cfg.defaultValue, cfg.desc, (string)cfg.vanillaValue);
				else
					throw new Exception(string.Format("Invalid setting value type {0} for setting {1}/{2}", ty.Name, cfg.category, name));
			}
			return this;
		}

		public ConfigSet addSettings<V>(string cat, IDictionary<string, V> dict, IDictionary<string, V> vans = null) {
			foreach (KeyValuePair<string, V> kvp in dict) {
				addSetting(cat, kvp.Key, kvp.Value, "", vans == null ? kvp.Value : vans[kvp.Key]);
			}
			return this;
		}

		public ConfigSet addSetting<V>(string category, string name, V defaultValue, string desc, V van) {
			values[name] = new ConfigValue<V>(this, category, name, defaultValue, desc, van);
			return this;
		}

		public V getValue<V>(string key) {
			return getConfig<V>(key).value;
		}

		public V getValue<V>(object key) {
			if (!enumKeys.ContainsKey(key))
				throw new Exception("No such config key: '" + key + "'");
			return getConfig<V>(enumKeys[key]).value;
		}

		public bool getBoolean(object key) {
			return getValue<bool>(key);
		}

		public int getInt(object key) {
			return getValue<int>(key);
		}

		public float getFloat(object key) {
			return getValue<float>(key);
		}

		public ConfigValue<V> getConfig<V>(string key) {
			if (!values.ContainsKey(key))
				throw new Exception("No such config key: '" + key + "'");
			return (ConfigValue<V>)values[key];
		}

		public void register() {
			foreach (ConfigValue cfg in values.Values) {
				cfg.register();
			}
		}
	}

	public abstract class ConfigValue {

		public readonly ConfigSet set;
		public readonly string category;
		public readonly string name;
		public readonly string description;

		internal ConfigValue(ConfigSet s, string cat, string nm, string desc) {
			set = s;
			category = cat;
			name = nm;
			description = desc;
		}

		public abstract bool isTrue { get; }

		public abstract string asString { get; }

		public abstract void register();

		public static bool operator true(ConfigValue cfg) {
			return cfg != null && cfg.isTrue;
		}

		public static bool operator false(ConfigValue cfg) {
			return cfg == null || !cfg.isTrue;
		}

	}

	public class ConfigValue<V> : ConfigValue {

		public readonly V defaultValue;
		public readonly V vanillaValue;

		internal ConfigValue(ConfigSet s, string cat, string nm, V def, string desc, V van) : base(s, cat, nm, string.Format("{0} (vanilla value={1})", desc, van.ToString().ToLowerInvariant())) {
			defaultValue = def;
			vanillaValue = van;
		}

		public ConfigEntry<V> handle { get; private set; }

		public V value { get { return handle.Value; } }

		public override string asString { get { return handle.Value.ToString().ToLowerInvariant(); } }

		public override bool isTrue {
			get {
				object val = value;
				if (typeof(V) == typeof(bool))
					return (bool)val;
				else if (typeof(V) == typeof(int))
					return ((int)val) != 0;
				else if (typeof(V) == typeof(float))
					return !Mathf.Approximately((float)val, 0);
				if (typeof(V) == typeof(string))
					return !string.IsNullOrEmpty((string)val);
				return false;
			}
		}

		public override string ToString() {
			string def = defaultValue.ToString();
			string van = vanillaValue.ToString();
			if (typeof(V) == typeof(bool)) {
				def = (bool)(object)defaultValue ? "T" : "F";
				van = (bool)(object)vanillaValue ? "T" : "F";
			}
			return string.Format("{0}/{1} = {2} ({3} & {4})", category, name, asString, def, van);
		}

		public override void register() {
			handle = set.ownerMod.Config.Bind<V>(category, name, defaultValue, description);
			DSUtil.log("Registered config: "+this, set.ownerMod.modDLL);
		}

	}
}
