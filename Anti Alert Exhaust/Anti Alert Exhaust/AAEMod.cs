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

		public static readonly HashSet<ConsoleMessageType> silenced = new HashSet<ConsoleMessageType>() {

		};

		public static readonly Dictionary<ConsoleMessageType, GameAudio.SoundEnum> soundOverrides = new Dictionary<ConsoleMessageType, GameAudio.SoundEnum>(){

		};

		public static readonly List<MessageSoundRule> rules = new List<MessageSoundRule>();

		protected override void init() {
			rules.Add(new MessageRegexRule(null, false, new Regex("(?i)transporter[a-zA-Z0-9 ]*signal(?-i)")));
			rules.Add(new MessageRegexRule(null, false, new Regex("(?i)video[a-zA-Z0-9 ]*signal(?-i)")));
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
