using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;

namespace ReikaKalseki.DIDrones {

    [BepInPlugin("ReikaKalseki.DIMod", "Dragon Industries Drone Robotics Division", "1.0.0")]
    public class DIMod : BaseUnityPlugin {

        public static DIMod instance;

        public static bool forceAllowVisit;

        public static event Action<ICommandable, ExecutedCommand, bool> onCommandEvent;

        public DIMod() : base() {
            instance = this;
            DSUtil.log("Constructed DI object", DSUtil.diDLL);
        }

        public void Awake() {
            DSUtil.log("Begin Initializing Dragon Industries", DSUtil.diDLL);
            try {
                Harmony harmony = new Harmony("Dragon Industries");
                Harmony.DEBUG = true;
                FileLog.logPath = Path.Combine(Path.GetDirectoryName(DSUtil.diDLL.Location), "harmony-log_"+Path.GetFileName(Assembly.GetExecutingAssembly().Location)+".txt");
                FileLog.Log("Ran mod register, started harmony (harmony log)");
                DSUtil.log("Ran mod register, started harmony", DSUtil.diDLL);
                try {
                    harmony.PatchAll(DSUtil.diDLL);
                }
                catch (Exception ex) {
                    FileLog.Log("Caught exception when running patchers!");
                    FileLog.Log(ex.Message);
                    FileLog.Log(ex.StackTrace);
                    FileLog.Log(ex.ToString());
                }
            }
            catch (Exception e) {
                DSUtil.log("Failed to load DI: "+e, DSUtil.diDLL);
            }
            DSUtil.log("Finished Initializing Dragon Industries", DSUtil.diDLL);
        }

        public static void onCommand(ICommandable ic, ExecutedCommand cmd, bool multi) {
			ic.ExecuteCommand(cmd, multi);
            if (cmd.Handled)
                CommandHistory.addCommand(cmd);
            if (onCommandEvent != null)
                onCommandEvent.Invoke(ic, cmd, multi);
		}

	}
}
