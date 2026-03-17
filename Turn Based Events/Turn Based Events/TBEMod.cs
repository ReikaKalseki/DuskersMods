using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;
using UnityEngine.Experimental.Director;

namespace ReikaKalseki.TBE {

    [BepInPlugin("ReikaKalseki.TBEMod", "Turn-Based Events", "1.0.0")]
	[BepInDependency("ReikaKalseki.DIMod")]
	public class TBEMod : BaseUnityPlugin {
        
        public static TBEMod instance;

        public static Assembly modDLL;

        public TBEMod() : base() {
            instance = this;
            DSUtil.log("Constructed TBEMod object");

            modDLL = Assembly.GetExecutingAssembly();
		}

        public void Awake() {
            DSUtil.log("Begin Initializing Turn-Based Events");
            try {
                Harmony harmony = new Harmony("Turn-Based Events");
                Harmony.DEBUG = true;
                FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log_"+Path.GetFileName(Assembly.GetExecutingAssembly().Location)+".txt");
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
                DSUtil.log("Failed to load TBE: "+e);
            }
            DSUtil.log("Finished Initializing Turn-Based Events");

            DIMod.onCommandEvent += (ic, cmd, multi) => EventRateControlSystem.incrementCommandCounter(cmd);
        }

	}
}
