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

	public class ObjectRepairUpgrade : RefillableCustomDroneUpgrade {

		public ObjectRepairUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def, c) {

		}

		protected override bool performAction(ExecutedCommand cmd) {
			string room = cmd.Arguments[cmd.Arguments.Count-2];
			string type = cmd.Arguments[cmd.Arguments.Count-1];
			if (drone.CurrentRoom != null && drone.CurrentRoom.LabelSimple != room) {
				Room target = WorldUtil.findRoom(room);
				if (target != null)
					drone.NavigateToAndExecuteCommand(target);
				else
					SendConsoleResponseMessage("Specified room '" + room + "' not found!", ConsoleMessageType.Warning);
				return false;
			}
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
				SendConsoleResponseMessage("Specified room '" + room + "' not found!", ConsoleMessageType.Warning);
				return false;
			}
		}
	}

	public class ObjectRepairUpgradeContainer : RefillableCustomDroneUpgradeContainer<ObjectRepairUpgrade> {

		public ObjectRepairUpgradeContainer() : base(
			new UpgradeRefillDefinition(1, 1, "repair kits"),
			"Installation Restoration",
			DroneUpgradeClass.Other,
			new CustomCommandDefinition("repair",
				"Allows your drone to repair broken installations like doors, power taps and terminals.",
				"r6 terminal",
				new Regex("^r[0-9]+$"),
				new Regex("^[a-zA-Z]+$")),
			8, //purchase cost
			0,
			0
			) {

		}
	}
}
