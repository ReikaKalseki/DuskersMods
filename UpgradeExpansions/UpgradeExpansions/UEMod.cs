using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;
using System.Collections.ObjectModel;

using DSMFramework;
using DSMFramework.Modding;
using UpgradeExpansions;

namespace ReikaKalseki.Upgrades {

	[BepInPlugin("ReikaKalseki.UEMod", "UpgradeExpansions", "1.0.0")]
	[BepInDependency("ReikaKalseki.DIMod")]
	public class UEMod : BaseUnityPlugin {

		public static UEMod instance;

		public static Assembly modDLL;

		public static bool forceAllowVisit;

		public UEMod() : base() {
			instance = this;
			DSUtil.log("Constructed UpgradeExpansions object", DSUtil.diDLL);

			modDLL = Assembly.GetExecutingAssembly();
		}

		public void Awake() {
			DSUtil.log("Begin Initializing UpgradeExpansions");
			try {
				Harmony harmony = new Harmony("UpgradeExpansions");
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

				new DoorChargerUpgradeContainer().register();
				new ObjectRepairUpgradeContainer().register();
			}
			catch (Exception e) {
				DSUtil.log("Failed to load UpgradeExpansions: " + e);
			}
			DSUtil.log("Finished Initializing UpgradeExpansions");
		}

	}
}
