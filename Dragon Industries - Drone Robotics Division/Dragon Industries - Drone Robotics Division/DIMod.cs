using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;
using DSMFramework.Modding;
using System.Text.RegularExpressions;

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
				FileLog.logPath = Path.Combine(Path.GetDirectoryName(DSUtil.diDLL.Location), "harmony-log_" + Path.GetFileName(Assembly.GetExecutingAssembly().Location) + ".txt");
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
				DSUtil.log("Failed to load DI: " + e, DSUtil.diDLL);
			}
			DSUtil.log("Finished Initializing Dragon Industries", DSUtil.diDLL);
		}

		public static void onSystemMessage(SystemMessageManager mgr, string msg, ConsoleMessageType type, SystemMessageImageType img) {
			DSUtil.log(string.Format("Logged system message: {0}/'{1}'", type, msg), DSUtil.diDLL);
			mgr.ShowSystemMessageInternal(msg, type, img);
		}

		public static void onCommand(ICommandable ic, ExecutedCommand cmd, bool multi) {
			if (cmd.Command.CommandName == "flag" && ic is DungeonManager dm) { //override flag
				if (cmd.Arguments.Count == 0) {
					ConsoleWindow3.SendConsoleResponse("Invalid parameters to 'flag'. Must have at least one room specified, and only zero or one color(s).", ConsoleMessageType.Warning);
					return;
				}
				GameAudio.SoundEnum sound = GameAudio.SoundEnum.None;
				string meta = cmd.Arguments[cmd.Arguments.Count - 1];
				Color use = Color.clear;
				HashSet<string> rooms = new HashSet<string>(cmd.Arguments);
				if (DSUtil.roomRegex.IsMatch(meta)) {

				}
				else {
					rooms.Remove(meta);
					if (meta == "clear") {
						sound = GameAudio.SoundEnum.FlagRemoved;
					}
					else if (new Regex("^[0-9]+,[0-9]+,[0-9]+$").IsMatch(meta)) {
						string[] rgb = meta.Split(',');
						use = new Color(int.Parse(rgb[0]) / 255F, int.Parse(rgb[1]) / 255F, int.Parse(rgb[2]) / 255F, 1);
						sound = GameAudio.SoundEnum.FlagPlaced;
					}
					else {
						ConsoleWindow3.SendConsoleResponse("Invalid parameters to 'flag'. Final parameter must be either a room, a color, or 'clear'.", ConsoleMessageType.Warning);
						return;
					}
				}
				bool any = false;
				foreach (Room room4 in dm.rooms) {
					if (rooms.Contains(room4.Label))
						any |= room4.toggleFlagWithColor(use);
				}
				if (sound == GameAudio.SoundEnum.None)
					sound = any ? GameAudio.SoundEnum.FlagPlaced : GameAudio.SoundEnum.FlagRemoved;
				GameAudio.Play2DSFX(sound);
				cmd.Handled = true;
			}
			else {
				ic.ExecuteCommand(cmd, multi);
			}
			if (cmd.Handled) {
				DSUtil.log(string.Format("Logged command '{0} {1}'", cmd.Command.CommandName, string.Join(" ", cmd.Arguments.ToArray())), DSUtil.diDLL);
				CommandHistory.addCommand(cmd);
				if (onCommandEvent != null)
					onCommandEvent.Invoke(ic, cmd, multi);
			}
		}

		public static void onSetBoardingUISlot(BoardingConfigInventorySlot slot, IInventoryItem ii) {
			DSUtil.log("Setting inventoryitem '" + ii + "' in boarding UI slot '" + slot + "'");
			if (slot.label)
				DSUtil.log("Slot text: " + slot.label.text);
			if (ii != null)
				DSUtil.log("Item data: name=" + ii.Name + " class=" + ii.GetType().Name + ", inv type=" + Enum.GetName(typeof(InventoryTypeEnum), ii.InventoryType));
		}

		public static void hookDroneSounds(Drone d) {
			// no longer necessary d.setCallsign(GameAudio.SoundEnum.DroneCS_12);
		}

		public static void onDroneInListSelected(UIDroneItem ui) {
			//DSUtil.log("Setting select on "+ui+" with drone "+ui.Drone+": "+ui.IsHighlighted+" & "+(ui.Drone != null ? ui.Drone.DroneName : ""));
			if (!ui.IsHighlighted && ui.Drone != null && ModificationUI.Instance.selectedList == ModificationUI.Instance.DroneList) { //not highlighted because this is called BEFORE the flag is set!
				ui.Drone.playCallsign();
			}
		}

		public static void onDroneInListSelected(BoardingConfigDronePanel ui, bool sel) {
			if (sel && ui.ThisDrone != null) {
				ui.ThisDrone.playCallsign();
			}
		}

	}
}
