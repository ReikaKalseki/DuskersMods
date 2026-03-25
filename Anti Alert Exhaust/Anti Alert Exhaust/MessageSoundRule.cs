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

		public override string ToString() {
			return string.Format("{0}: S={1}/M={2}", GetType().Name, soundOverride == null ? "null" : soundOverride.ToString(), mute.ToString());
		}

		public abstract void writeToFile(XmlElement doc);

		public static void writeToFileIfNew(string folder, string name, MessageSoundRule rule) {
			string path = Path.Combine(folder, name+".xml");
			if (File.Exists(path))
				return;
			writeToFile(folder, name, rule);
		}

		public static void writeToFile(string folder, string name, MessageSoundRule rule) {
			XmlDocument doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("root"));
			rule.writeToFile(doc.DocumentElement);
			string path = Path.Combine(folder, name+".xml");
			int s1 = "Message".Length;
			string n = rule.GetType().Name;
			doc.DocumentElement.addProperty("matchType", n.Substring(s1, n.Length-s1-"Rule".Length));
			doc.DocumentElement.addProperty("muteSound", rule.mute);
			if (rule.soundOverride.HasValue)
				doc.DocumentElement.addProperty("soundOverride", Enum.GetName(typeof(GameAudio.SoundEnum), rule.soundOverride.Value));
			doc.Save(path);
		}

		public static MessageSoundRule readFromFile(string path) {
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			XmlElement root = doc.DocumentElement;
			string tname = "Message"+root.getProperty("matchType")+"Rule";
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

	public class MessageTypeRule : MessageSoundRule {

		private readonly ConsoleMessageType type;

		public MessageTypeRule(string s, bool mute, ConsoleMessageType mtype) : base(s, mute, (msg, type) => type == mtype) {
			type = mtype;
		}

		public override void writeToFile(XmlElement doc) {
			doc.addProperty("type", Enum.GetName(typeof(ConsoleMessageType), type));
		}

		public static MessageTypeRule readFile(XmlElement doc) {
			string type = doc.getProperty("type");
			ConsoleMessageType mtype = ConsoleMessageType.None;
			try {
				mtype = (ConsoleMessageType)Enum.Parse(typeof(ConsoleMessageType), type);
			}
			catch (Exception e) {
				throw new Exception(string.Format("Invalid console message type '{0}'; options are {1}", type, Enum.GetValues(typeof(ConsoleMessageType)).toDebugStringAlt()));
			}
			return new MessageTypeRule(doc.getProperty("soundOverride", true), doc.getBoolean("muteSound"), mtype);
		}

		public override string ToString() {
			return string.Format("{0}: Type={1}", base.ToString(), type.ToString());
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
			return new MessageRegexRule(doc.getProperty("soundOverride", true), doc.getBoolean("muteSound"), new Regex(doc.getProperty("pattern")));
		}

		public override string ToString() {
			return string.Format("{0}: '{1}'", base.ToString(), regex);
		}
	}

	public class MessageAlwaysRule : MessageSoundRule {

		public MessageAlwaysRule(string s, bool mute) : base(s, mute, (msg, type) => true) {

		}

		public override void writeToFile(XmlElement doc) {

		}

		public static MessageAlwaysRule readFile(XmlElement doc) {
			return new MessageAlwaysRule(doc.getProperty("soundOverride", true), doc.getBoolean("muteSound"));
		}
	}
}
