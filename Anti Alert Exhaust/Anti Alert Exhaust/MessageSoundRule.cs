using System;
using System.IO;

using System.Text.RegularExpressions;

using System.Reflection;

using System.Xml;

using ReikaKalseki.DIDrones;

namespace ReikaKalseki.AAE {
	public abstract class MessageSoundRule {

		public readonly Func<string, ConsoleMessageType, bool> matcher;

		public readonly GameAudio.SoundEnum? soundOverride;
		public readonly bool mute;

		public MessageSoundRule(string s, bool mute, Func<string, ConsoleMessageType, bool> rule) {
			soundOverride = null;
			if (!string.IsNullOrEmpty(s)) {
				try {
					soundOverride = (GameAudio.SoundEnum)Enum.Parse(typeof(GameAudio.SoundEnum), s);
				}
				catch {
					DSUtil.log("Message sound rule specified a nonexistent sound as an override. No override will be applied.");
				}
			}
			this.mute = mute;
			matcher = rule;
		}

		public abstract void writeToFile(XmlElement doc);

		public static void writeToFile(string folder, string name, MessageSoundRule rule) {
			XmlDocument doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("root"));
			rule.writeToFile(doc.DocumentElement);
			string path = Path.Combine(folder, name+".xml");
			doc.DocumentElement.addProperty("type", rule.GetType().Name.Substring("Message".Length));
			doc.DocumentElement.addProperty("mute", rule.mute);
			if (rule.soundOverride.HasValue)
				doc.DocumentElement.addProperty("override", Enum.GetName(typeof(GameAudio.SoundEnum), rule.soundOverride.Value));
			doc.Save(path);
		}

		public static MessageSoundRule readFromFile(string path) {
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			XmlElement root = doc.DocumentElement;
			string tname = "Message"+root.getProperty("type");
			Type t = InstructionHandlers.getTypeBySimpleName("ReikaKalseki.AAE."+tname);
			if (t == null)
				throw new Exception(string.Format("Unknown message rule type '{0}'!", tname));
			if (!t.IsSubclassOf(typeof(MessageSoundRule)))
				throw new Exception(string.Format("Invalid message rule type '{0}' is not a subclass of the rule base class!", tname));
			MethodInfo m = t.GetMethod("readFile", new Type[]{ typeof(XmlElement) });
			if (m == null || !m.IsStatic)
				throw new Exception(string.Format("Message rule type '{0}' is missing a static readFile method!", t.Name));
			return (MessageSoundRule)m.Invoke(null, new object[] { root });
		}

	}

	public class MessageRegexRule : MessageSoundRule {

		private readonly string regex;

		public MessageRegexRule(string s, bool mute, Regex pattern) : base(s, mute, (msg, type) => pattern.IsMatch(msg)) {
			regex = pattern.ToString();
		}

		public override void writeToFile(XmlElement doc) {
			doc.addProperty("pattern", regex);
		}

		public static MessageRegexRule readFile(XmlElement doc) {
			return new MessageRegexRule(doc.getProperty("override", true), doc.getBoolean("mute"), new Regex(doc.getProperty("pattern")));
		}
	}
}
