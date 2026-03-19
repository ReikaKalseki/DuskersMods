using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using DSMFramework;
using DSMFramework.Modding;
using ReikaKalseki.DIDrones;

using System.Text.RegularExpressions;

namespace UpgradeExpansions {

	public class DoorChargerUpgrade : RefillableCustomDroneUpgrade {

		public DoorChargerUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def, c) {

		}

		protected override bool performAction(ExecutedCommand cmd) {
			string target = cmd.Arguments[cmd.Arguments.Count-1];
			Door door = WorldUtil.findDoor(target);
			if (door != null && door.corridor.containsRoom(drone.CurrentRoom)) {
				var bounds = door.corridor.GetComponent<Collider>().bounds;
				bounds.Expand(new Vector3(0.3f, 0.3f, 0.3f));
				if (bounds.Intersects(drone.GetComponent<Collider>().bounds)) {
					if (door.powered) {
						SendConsoleResponseMessage("Door " + target + " already powered", ConsoleMessageType.Info);
						return false;
					}
					else {
						if (Quantity <= 0 || !UpgradeUsed()) {
							SendConsoleResponseMessage("Charges depleted, unable to power door", ConsoleMessageType.Warning);
							return false;
						}

						SendConsoleResponseMessage("Successfully powered door " + target, ConsoleMessageType.Benefit);
						door.power(true);
						return true;

					}
				}
				else {
					drone.NavigateToAndExecuteCommand(door.corridor.gameObject, cmd, CollisionType.BoundsIntesect);
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
			new CustomCommandDefinition("chargedoor", "Allows your drone to connect to a door and charge it", "d14", new Regex("^[d,a][0-9]+$")),
			8, //purchase cost
			0,
			0
			) {

		}
	}
}
