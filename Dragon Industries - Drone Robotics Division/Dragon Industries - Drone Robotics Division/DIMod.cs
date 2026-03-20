using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;
using DSMFramework.Modding;

namespace ReikaKalseki.DIDrones {

    [BepInPlugin("ReikaKalseki.DIMod", "Dragon Industries Drone Robotics Division", "1.0.0")]
	[BepInDependency("DSMFramework")]
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

                //for (int i = 0; i <= 12; i++) {
					//    new ChangeDroneCallsignMod(i);
					//too early GameAudio.LoadSFXIntoDict(GameAudio.SoundEnum.DroneCS_1+i);
				//}
                //ModificationsHelper._modificationsByType
                ModUpgradeManager.Manager.RegisterModificationFor(typeof(NonVisualDrone), new ChangeDroneCallsignMod());
			}
            catch (Exception e) {
                DSUtil.log("Failed to load DI: "+e, DSUtil.diDLL);
            }
            DSUtil.log("Finished Initializing Dragon Industries", DSUtil.diDLL);
		}

		public static void onSystemMessage(SystemMessageManager mgr, string msg, ConsoleMessageType type, SystemMessageImageType img) {
			DSUtil.log(string.Format("Logged system message: {0}/'{1}'", type, msg), DSUtil.diDLL);
			mgr.ShowSystemMessageInternal(msg, type, img);
		}

		public static void onCommand(ICommandable ic, ExecutedCommand cmd, bool multi) {
			ic.ExecuteCommand(cmd, multi);
            if (cmd.Handled) {
				DSUtil.log(string.Format("Logged command '{0} {1}'", cmd.Command.CommandName, string.Join(" ", cmd.Arguments.ToArray())), DSUtil.diDLL);
				CommandHistory.addCommand(cmd);
                if (onCommandEvent != null)
                    onCommandEvent.Invoke(ic, cmd, multi);
            }
		}

        public static void onSetBoardingUISlot(BoardingConfigInventorySlot slot, IInventoryItem ii) {
            DSUtil.log("Setting inventoryitem '"+ii+"' in boarding UI slot '"+slot+"'");
            if (slot.label)
			    DSUtil.log("Slot text: "+slot.label.text);
            if (ii != null)
                DSUtil.log("Item data: name="+ii.Name+" class="+ ii.GetType().Name+", inv type="+Enum.GetName(typeof(InventoryTypeEnum), ii.InventoryType));
		}

        public static void hookDroneSounds(Drone d) {
             // no longer necessary d.setCallsign(GameAudio.SoundEnum.DroneCS_12);
        }

	}
}
