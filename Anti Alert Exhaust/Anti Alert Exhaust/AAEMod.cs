using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;
using System.Collections.ObjectModel;

using System.Text.RegularExpressions;

namespace ReikaKalseki.AAE {

	[BepInPlugin("ReikaKalseki.AAEMod", "Anti Alert Exhaustion", "1.0.0")]
	[BepInDependency("ReikaKalseki.DIMod")]
	public class AAEMod : BaseUnityPlugin {

		public static AAEMod instance;

		public static Assembly modDLL;

		public static bool forceAllowVisit;

		public AAEMod() : base() {
			instance = this;
			DSUtil.log("Constructed AAE object", DSUtil.diDLL);

			modDLL = Assembly.GetExecutingAssembly();
		}

		public static readonly HashSet<ConsoleMessageType> silenced = new HashSet<ConsoleMessageType>() {

		};

		public static readonly Dictionary<ConsoleMessageType, GameAudio.SoundEnum> soundOverrides = new Dictionary<ConsoleMessageType, GameAudio.SoundEnum>(){

		};

		public static readonly List<MessageSoundRule> rules = new List<MessageSoundRule>();

		public void Awake() {
			DSUtil.log("Begin Initializing AAE");
			try {
				Harmony harmony = new Harmony("AAE");
				Harmony.DEBUG = true;
				FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log_" + Path.GetFileName(Assembly.GetExecutingAssembly().Location) + ".txt");
				FileLog.Log("Ran mod register, started harmony (harmony log)");
				DSUtil.log("Ran mod register, started harmony");
				try {
					harmony.PatchAll(modDLL);
				}
				catch (Exception ex) {
					FileLog.Log("Caught exception when running patchers!");
					FileLog.Log(ex.Message);
					FileLog.Log(ex.StackTrace);
					FileLog.Log(ex.ToString());
				}

				rules.Add(new MessageRegexRule(null, false, new Regex("(?i)transporter[a-zA-Z0-9 ]*signal(?-i)")));
				rules.Add(new MessageRegexRule(null, false, new Regex("(?i)video[a-zA-Z0-9 ]*signal(?-i)")));
			}
			catch (Exception e) {
				DSUtil.log("Failed to load AAE: " + e);
			}
			DSUtil.log("Finished Initializing AAE");
		}

		public static void queueMessageAlertSound(GameAudio.SoundEnum snd, string msg, ConsoleMessageType type) {
			bool flag = true;
			GameAudio.SoundEnum original = snd;
			if (silenced.Contains(type))
				flag = false;
			else if (soundOverrides.ContainsKey(type))
				snd = soundOverrides[type];
			else {
				foreach (MessageSoundRule mr in rules) {
					if (mr.matcher.Invoke(msg, type)) {
						if (!mr.allowAudio)
							flag = false;
						else if (mr.soundOverride.HasValue)
							snd = mr.soundOverride.Value;
					}
				}
			}
			string sndname = Enum.GetName(typeof(GameAudio.SoundEnum), snd);
			if (snd != original)
				sndname = string.Format("{0} -> {1}", Enum.GetName(typeof(GameAudio.SoundEnum), original), Enum.GetName(typeof(GameAudio.SoundEnum), snd));
			string log = string.Format("Attempted to play sound '{0}' for message - success: {1}", sndname, flag);
			DSUtil.log(log);
			if (flag)
				GameAudio.Play2DSFX(snd);
		}

		public class MessageRegexRule : MessageSoundRule {

			public MessageRegexRule(GameAudio.SoundEnum? s, bool allow, Regex pattern) : base(s, allow, (msg, type) => pattern.IsMatch(msg)) {

			}
		}

		public class MessageSoundRule {

			public readonly Func<string, ConsoleMessageType, bool> matcher;

			public readonly GameAudio.SoundEnum? soundOverride;
			public readonly bool allowAudio;

			public MessageSoundRule(GameAudio.SoundEnum? s, bool allow, Func<string, ConsoleMessageType, bool> rule) {
				soundOverride = s;
				allowAudio = allow;
				matcher = rule;
			}

		}

	}
}
