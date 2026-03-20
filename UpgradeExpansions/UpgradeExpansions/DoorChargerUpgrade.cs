using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using DSMFramework;
using DSMFramework.Modding;
using ReikaKalseki.DIDrones;

using System.Text.RegularExpressions;

namespace ReikaKalseki.Upgrades {

	public class DoorChargerUpgrade : RefillableCustomDroneUpgrade {

		public DoorChargerUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def, c) {

		}

		protected override bool performAction(ExecutedCommand cmd) {
			string target = cmd.Arguments[cmd.Arguments.Count-1];
			TargetableRoomObject door = new TargetableRoomObject(WorldUtil.findDoor(target));
			if (door.roomObject != null && ((Door)door.roomObject).corridor.containsRoom(drone.CurrentRoom)) {
				Door d = (Door)door.roomObject;
				if (door.checkAtElseNavToAndTryAgain(drone, cmd)) {
					if (d.powered) {
						SendConsoleResponseMessage("Door " + target + " already powered", ConsoleMessageType.Info);
						return false;
					}
					else {
						SendConsoleResponseMessage("Successfully powered door " + target, ConsoleMessageType.Benefit);
						d.power(true);
						d.RefreshSchematicColor();
						return true;
					}
				}
				else {
					return false;
				}
			}
			else {
				SendConsoleResponseMessage("Specified door '" + target + "' not found!", ConsoleMessageType.Warning);
				return false;
			}
		}
	}

	public class DoorChargerUpgradeContainer : RefillableCustomDroneUpgradeContainer<DoorChargerUpgrade> {

		public DoorChargerUpgradeContainer() : base(
			new UpgradeRefillDefinition(4, 2, "charge"),
			"Door Charger",
			DroneUpgradeClass.Exploration,
			new CustomCommandDefinition("chargedoor", "Allows your drone to connect to a door and charge it", "d14", DSUtil.doorRegex),
			8, //purchase cost
			0,
			0
			) {

		}
	}
}
