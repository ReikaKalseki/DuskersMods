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
	public class DIMod : DIModBase {

		public static DIMod instance;

		public static event Action<ICommandable, ExecutedCommand, bool> onCommandEvent;

		public DIMod() : base() {
			instance = this;
		}

		protected override void init() {
			ModUpgradeManager.Manager.RegisterModificationFor(typeof(NonVisualDrone), new ChangeDroneCallsignMod());
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
				bool forceRemove = false;
				if (DSUtil.roomRegex.IsMatch(meta)) {

				}
				else {
					rooms.Remove(meta);
					if (meta == "clear") {
						sound = GameAudio.SoundEnum.FlagRemoved;
						rooms.Add("*");
						forceRemove = true;
					}
					else if (new Regex("^[0-9a-fA-F]{6}$").IsMatch(meta)) {
						ColorUtility.TryParseHtmlString("#"+meta, out use);
						sound = GameAudio.SoundEnum.FlagPlaced;
					}
					else {
						ConsoleWindow3.SendConsoleResponse("Invalid parameters to 'flag'. Final parameter must be either a room, a color, or 'clear'.", ConsoleMessageType.Warning);
						return;
					}
				}
				bool any = false;
				foreach (Room room4 in dm.rooms) {
					if (rooms.Contains(room4.Label) || rooms.Contains("*")) {
						if (forceRemove)
							room4.ClearRoomFlag();
						else
							any |= room4.toggleFlagWithColor(use);
					}
				}
				if (sound == GameAudio.SoundEnum.None)
					sound = any ? GameAudio.SoundEnum.FlagPlaced : GameAudio.SoundEnum.FlagRemoved;
				GameAudio.Play2DSFX(sound);
				cmd.Handled = true;
			}
			else {
				try {
					GlobalSettings.GameState.ThePlayer.MyShip.AddEmptySlot();
					GlobalSettings.GameState.ThePlayer.MyShip.AddEmptySlot();
					ic.ExecuteCommand(cmd, multi);
				}
				catch (Exception ex) {
					ConsoleWindow3.SendConsoleResponse(string.Format("Game/mod error: Command threw exception: {0}", ex.GetType().Name), ConsoleMessageType.Error);
					ConsoleWindow3.SendConsoleResponse("Check your output_log.txt for more information.", ConsoleMessageType.Info);
					DSUtil.log(string.Format("Threw exception processing command '{0}': {1}", cmd.Command.CommandName, ex.ToString()), DSUtil.diDLL);
					cmd.Handled = false;
					return;
				}
			}
			if (cmd.Handled) {
				DSUtil.log(string.Format("Logged command '{0} {1}'", cmd.Command.CommandName, string.Join(" ", cmd.Arguments.ToArray())), DSUtil.diDLL);
				CommandHistory.addCommand(cmd);
				if (onCommandEvent != null)
					onCommandEvent.Invoke(ic, cmd, multi);
			}
		}

		public static void onLoadShipSlotsA(DungeonInfo dg) {
			
		}

		public static void onLoadShipSlotsB(DungeonInfo dg) {
			
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
