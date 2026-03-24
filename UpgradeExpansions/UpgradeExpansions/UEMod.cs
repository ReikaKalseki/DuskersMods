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
				new DismantleUpgradeContainer().register();

				ModUpgradeManager.Manager.RegisterModificationFor(typeof(NonVisualDrone), new AddDroneSlotMod());
			}
			catch (Exception e) {
				DSUtil.log("Failed to load UpgradeExpansions: " + e);
			}
			DSUtil.log("Finished Initializing UpgradeExpansions");
		}

		public static void onCreateUpgrade() {
			//DSUtil.log("Recorded MakeUpgrade() call from\n" + new StackTrace().GetFrames().getTrace());
		}

		public static void runPryUpgrade(PryUpgrade upg, ExecutedCommand cmd, bool multi) {
			if (cmd.Command.CommandName != "pry")
				return;
			string arg = cmd.Arguments.Count == 0 ? null : cmd.Arguments[cmd.Arguments.Count-1];
			if (arg == "upgrade") {
				cmd.Handled = true;
				Room room = upg.drone.CurrentRoom;
				TargetableRoomObject target = new TargetableRoomObject(WorldUtil.findTowableInRoom<ShipUpgradeInGameObject>(room, u => !u.ThisUpgrade.IsPermanentUpgrade && u.ShipUpgradeStatus == ShipUpgradeInGameObject.ShipUpgradeStatusEnum.InstalledWorking));
				if (target.roomObject == null) {
					upg.SendConsoleResponseMessage("No pry-able upgrade found in room: " + room.LabelSimple, ConsoleMessageType.Warning);
					return;
				}
				if (target.checkAtElseNavToAndTryAgain(upg.drone, cmd)) {
					ShipUpgradeInGameObject obj = (ShipUpgradeInGameObject)target.roomObject;
					obj.ShipUpgradeStatus = ShipUpgradeInGameObject.ShipUpgradeStatusEnum.InstalledWorkingLoose;
					obj.CanBeTowed = true;
					upg.SendConsoleResponseMessage("Successfully pried upgrade in room " + room.LabelSimple, ConsoleMessageType.Benefit);
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
