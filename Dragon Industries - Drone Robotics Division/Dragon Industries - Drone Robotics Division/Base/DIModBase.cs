using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.IO;

using HarmonyLib;

using BepInEx;
using BepInEx.Configuration;

namespace ReikaKalseki.DIDrones {
	public abstract class DIModBase : BaseUnityPlugin {

		public readonly ConfigSet config;

		public Harmony harmony { get; private set; }

		public Assembly modDLL;

		public new string name {  get { return Info.Metadata.Name; } }

		protected DIModBase() : base() {
			modDLL = DSUtil.tryGetModDLL(this.GetType() == typeof(DIMod));

			DSUtil.log("Constructed "+name+" object", modDLL);

			config = new ConfigSet(this);
		}

		private void applyHarmony() {
			harmony = new Harmony(name);
			Harmony.DEBUG = true;
			FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log_" + Path.GetFileName(Assembly.GetExecutingAssembly().Location) + ".txt");
			FileLog.Log(name+": Ran mod register, started harmony (harmony log)");
			DSUtil.log("Ran mod register, started harmony", modDLL);
			try {
				harmony.PatchAll(modDLL);
			}
			catch (Exception ex) {
				FileLog.Log("Caught exception when running patchers!");
				FileLog.Log(ex.Message);
				FileLog.Log(ex.StackTrace);
				FileLog.Log(ex.ToString());
			}
		}

		public void Awake() {
			DSUtil.log("Begin Initializing "+name, modDLL);
			try {
				config.register();

				applyHarmony();

				init();
			}
			catch (Exception e) {
				DSUtil.log("Failed to load "+ name + ": " + e, modDLL);
			}
			DSUtil.log("Finished Initializing "+name, modDLL);
		}

		protected virtual void addAdditionalConfig() { }

		protected abstract void init();
	}
}
