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
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ReikaKalseki.Upgrades {

	[BepInPlugin("ReikaKalseki.UEMod", "UpgradeExpansions", "1.0.0")]
	[BepInDependency("ReikaKalseki.DIMod")]
	public class UEMod : DIModBase {

		public static UEMod instance;

		public UEMod() : base() {
			instance = this;
			config.addSettings(typeof(UEConfig.ConfigEntries));
		}

		protected override void init() {
			if (config.getBoolean(UEConfig.ConfigEntries.enableDoorCharge))
				new DoorChargerUpgradeContainer().register();
			if (config.getBoolean(UEConfig.ConfigEntries.enableRepair))
				new ObjectRepairUpgradeContainer().register();
			if (config.getBoolean(UEConfig.ConfigEntries.enableDismantle))
				new DismantleUpgradeContainer().register();
			if (config.getBoolean(UEConfig.ConfigEntries.enableHack))
				new TerminalHackUpgradeContainer().register();

			if (config.getBoolean(UEConfig.ConfigEntries.enableModSlot))
				ModUpgradeManager.Manager.RegisterModificationFor(typeof(NonVisualDrone), new AddDroneSlotMod());
			if (config.getBoolean(UEConfig.ConfigEntries.enableReplaceSlot))
				ModUpgradeManager.Manager.RegisterModificationFor(typeof(SlotInfo), new ReplaceShipSlotMod());
		}

		public static void onCreateUpgrade() {
			//DSUtil.log("Recorded MakeUpgrade() call from\n" + new StackTrace().GetFrames().getTrace());
		}

		public static void runPryUpgrade(PryUpgrade upg, ExecutedCommand cmd, bool multi) {
			if (cmd.Command.CommandName != "pry")
				return;
			string arg = cmd.Arguments.Count == 0 ? null : cmd.Arguments[cmd.Arguments.Count-1];
			if (arg == "upgrade") {
				if (!instance.config.getBoolean(UEConfig.ConfigEntries.enableUpgradePry)) {
					upg.SendConsoleResponseMessage("Pry is not enabled on upgrades in mod config.", ConsoleMessageType.Warning);
					return;
				}
				cmd.Handled = true;
				Room room = upg.drone.CurrentRoom;
				TargetableRoomObject target = new TargetableRoomObject(WorldUtil.findTowableInRoom<ShipUpgradeInGameObject>(room, u => !u.ThisUpgrade.IsPermanentUpgrade && u.ShipUpgradeStatus == ShipUpgradeInGameObject.ShipUpgradeStatusEnum.InstalledWorking));
				if (target.roomObject == null) {
					upg.SendConsoleResponseMessage("No pry-able upgrade found in room: " + room.Label, ConsoleMessageType.Warning);
					return;
				}
				if (target.checkAtElseNavToAndTryAgain(upg.drone, cmd)) {
					ShipUpgradeInGameObject obj = (ShipUpgradeInGameObject)target.roomObject;
					obj.ShipUpgradeStatus = ShipUpgradeInGameObject.ShipUpgradeStatusEnum.InstalledWorkingLoose;
					obj.CanBeTowed = true;
					upg.SendConsoleResponseMessage("Successfully pried upgrade in room " + room.Label, ConsoleMessageType.Benefit);
				}
			}
			else if (arg != null && DSUtil.isDoorArg(arg)) {
				cmd.Handled = true;
				originalPryLogic(upg, arg, cmd, multi);
			}
			else {
				upg.SendConsoleResponseMessage("Invalid parameter '" + arg + "'. Must be either a door/airlock or 'upgrade'.", ConsoleMessageType.Warning);
			}
		}

		public static void originalPryLogic(PryUpgrade upg, string text, ExecutedCommand command, bool partOfMultiCommand) {
			DungeonManager instance = DungeonManager.Instance;
			TargetableRoomObject door = new TargetableRoomObject(WorldUtil.findDoor(text));
			if (door.roomObject == null || !((Door)door.roomObject).corridor.containsRoom(upg.drone.CurrentRoom)) {
				upg.SendConsoleResponseMessage("Specified door not found: " + text, ConsoleMessageType.Warning);
				return;
			}
			if (door.checkAtElseNavToAndTryAgain(upg.drone, command)) {
				Door d = (Door)door.roomObject;
				if (d.state == DoorState.Open) {
					upg.SendConsoleResponseMessage("Door already open: " + text, ConsoleMessageType.Info);
				}
				else {
					if (!upg.UpgradeUsed())
						return;
					d.PryOpen();
					if (GlobalSettings.cameraMode == CameraMode.Drone) {
						upg.drone.prySound.volume = GameAudio.RemoteVolume * 1f;
						upg.drone.prySound.Play();
					}
					upg.SendConsoleResponseMessage("Successfully pried door " + text, ConsoleMessageType.Benefit);
				}
			}
		}
	}
}
