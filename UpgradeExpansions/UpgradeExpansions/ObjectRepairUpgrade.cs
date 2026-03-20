using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using DSMFramework;
using DSMFramework.Modding;
using ReikaKalseki.DIDrones;

using System.Text.RegularExpressions;
using Steamworks;

namespace ReikaKalseki.Upgrades {

	public class ObjectRepairUpgrade : RefillableCustomDroneUpgrade {

		private static Dictionary<string, Type> typeMap = new Dictionary<string, Type>(){
			//{"door", typeof(Door) },
			{"airlock", typeof(Door) },
			{"generator", typeof(DungeonPowerInlet) },
			{"power", typeof(DungeonPowerInlet) },
			{"terminal", typeof(DungeonTerminal) },
			{"console", typeof(DungeonTerminal) },
			{"gun", typeof(DungeonDefense) },
			{"turret", typeof(DungeonDefense) },
			{"upgrade", typeof(ShipUpgradeInGameObject) },
		};

		public ObjectRepairUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def, c) {

		}

		protected override bool performAction(ExecutedCommand cmd) {
			string arg = cmd.Arguments[cmd.Arguments.Count-1];
			bool wantDoor = DSUtil.isDoorArg(arg);
			TargetableRoomObject target;
			Room room = drone.CurrentRoom;
			if (wantDoor) {
				target = new TargetableRoomObject(WorldUtil.findDoor(arg));
			}
			else {
				Type type = typeMap.ContainsKey(arg) ? typeMap[arg] : null;
				if (type == null) {
					SendConsoleResponseMessage("Unknown object type '" + arg + "'. Valid types: " + typeMap.Keys.toDebugString(), ConsoleMessageType.Warning);
					return false;
				}
				if (type == typeof(ShipUpgradeInGameObject))
					target = new TargetableRoomObject(WorldUtil.findTowableInRoom<ShipUpgradeInGameObject>(room, u => u.isBroken()));
				else
					target = new TargetableRoomObject(room.GetRoomItem(type, false));
			}
			
			string rname = room.LabelSimple;
			if (target.roomObject == null) {
				SendConsoleResponseMessage(string.Format("No {0} of {1} '{2}' found in room {3}", wantDoor ? "door" : "object", wantDoor ? "name" : "type", arg, rname), ConsoleMessageType.Warning);
				return false;
			}

			if (target.checkAtElseNavToAndTryAgain(drone, cmd)) {
				if (target.isDead) {
					if (target.roomObject is Door dr2) {
						dr2.repair();
					}
					else if (target.roomObject is ShipUpgradeInGameObject u) {
						u.repair();
					}
					else {
						((IBreakable)target.roomObject).Fix(out string msg);
					}
					SendConsoleResponseMessage("Successfully repaired " + arg + " in room " + rname, ConsoleMessageType.Benefit);
					return true;
				}
				else {
					SendConsoleResponseMessage("Object does not need repair", ConsoleMessageType.Info);
					return false;
				}
			}
			else {
				return false;
			}
		}
	}

	public class ObjectRepairUpgradeContainer : RefillableCustomDroneUpgradeContainer<ObjectRepairUpgrade> {

		public ObjectRepairUpgradeContainer() : base(
			new UpgradeRefillDefinition(1, 1, "repair kit"),
			"Repair",
			DroneUpgradeClass.Other,
			new CustomCommandDefinition("repair",
				"Allows your drone to repair broken installations like doors, power taps and terminals.",
				"terminal",
				new Regex("^[0-9a-zA-Z]+$")),
			8, //purchase cost
			0,
			0
			) {

		}
	}
}
