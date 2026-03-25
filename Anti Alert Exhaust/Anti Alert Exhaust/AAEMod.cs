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
	public class AAEMod : DIModBase {

		public static AAEMod instance;

		public AAEMod() : base() {
			instance = this;
		}

		public static readonly List<MessageSoundRule> rules = new List<MessageSoundRule>();

		protected override void init() {
			string path = modDLL.Location;
			while (!path.EndsWith("BepInEx"))
				path = Path.GetDirectoryName(path);
			string folder = Path.Combine(Path.Combine(path, "config"), "AAE_Rules");
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			MessageSoundRule.writeToFileIfNew(folder, "EXAMPLE_UniversalMute", new MessageAlwaysRule(null, true));
			MessageSoundRule.writeToFileIfNew(folder, "EXAMPLE_ReplaceBellsWithBeep", new MessageTypeRule(GameAudio.SoundEnum.AlertWarning.ToString(), false, ConsoleMessageType.DisasterWarning));
			MessageSoundRule.writeToFileIfNew(folder, "EXAMPLE_TransporterMute", new MessageRegexRule(null, true, new Regex("(?i)transporter[a-zA-Z0-9 ]*signal(?-i)")));
			MessageSoundRule.writeToFileIfNew(folder, "EXAMPLE_VideoSignalLessAlarming", new MessageRegexRule(GameAudio.SoundEnum.SensorUntriggered.ToString(), false, new Regex("(?i)video[a-zA-Z0-9 ]*signal(?-i)")));
			foreach (string file in Directory.GetFiles(folder)) {
				if (file.StartsWith("EXAMPLE_"))
					continue;
				try {
					MessageSoundRule rule = MessageSoundRule.readFromFile(file);
					rules.Add(rule);
					DSUtil.log(string.Format("Added message sound rule from config file '{0}': {1}", Path.GetFileName(file), rule.ToString()));
				}
				catch (Exception ex) {
					DSUtil.log(string.Format("Could not construct message sound rule from config file '{0}': {1}", Path.GetFileName(file), ex.ToString()));
				}
			}
		}

		public static void queueMessageAlertSound(GameAudio.SoundEnum snd, string msg, ConsoleMessageType type) {
			bool flag = true;
			GameAudio.SoundEnum original = snd;
			foreach (MessageSoundRule mr in rules) {
				if (mr.matcher.Invoke(msg, type)) {
					if (mr.mute)
						flag = false;
					else if (mr.soundOverride.HasValue)
						snd = mr.soundOverride.Value;
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

	}
}
