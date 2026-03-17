using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;

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
			}
			catch (Exception e) {
				DSUtil.log("Failed to load AAE: " + e);
			}
			DSUtil.log("Finished Initializing AAE");
		}

	}
}
