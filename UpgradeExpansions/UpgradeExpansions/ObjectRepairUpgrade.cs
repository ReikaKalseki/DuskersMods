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

		private static Dictionary<string, Type> typeMap = new Dictionary<string, Type>(){
			{"door", typeof(Door) },
			{"airlock", typeof(Door) },
			{"generator", typeof(DungeonPowerInlet) },
			{"power", typeof(DungeonPowerInlet) },
			{"terminal", typeof(DungeonTerminal) },
			{"console", typeof(DungeonTerminal) },
			{"gun", typeof(DungeonDefense) },
			{"turret", typeof(DungeonDefense) },
		};

		public ObjectRepairUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def, c) {

		}

		protected override bool performAction(ExecutedCommand cmd) {
			string type = cmd.Arguments[cmd.Arguments.Count-1];
			Type obj = typeMap.ContainsKey(type) ? typeMap[type] : null;
			if (obj == null) {
				SendConsoleResponseMessage("Unknown object type '" + type + "'. Valid types: " + typeMap.Keys.toDebugString(), ConsoleMessageType.Warning);
				return false;
			}
			object target = obj == typeof(Door) ? null : drone.CurrentRoom.GetRoomItem(obj, false);
			string rname = drone.CurrentRoom.LabelSimple;
			if (target == null) {
				SendConsoleResponseMessage("No object of type '" + type + "' found in room " + rname, ConsoleMessageType.Warning);
				return false;
			}
			Bounds bounds = (target is Door d ? d.corridor : (MonoBehaviour)target).GetComponent<Collider>().bounds;
			bounds.Expand(new Vector3(0.3f, 0.3f, 0.3f));
			if (bounds.Intersects(drone.GetComponent<Collider>().bounds)) {
				bool dead = target is Door dr ? dr.isDead : ((RoomItem)target).IsDead;
				if (dead) {
					if (target is Door dr2)
						dr2.repair();
					else
						((IBreakable)target).Fix(out string msg);
					SendConsoleResponseMessage("Successfully repaired " + type + " in room " + rname, ConsoleMessageType.Benefit);
					return true;
				}
				else {
					SendConsoleResponseMessage("Object does not need repair", ConsoleMessageType.Info);
					return false;
				}
			}
			else {
				if (target is Door dr2)
					drone.NavigateToAndExecuteCommand(dr2.corridor.gameObject, cmd, CollisionType.BoundsIntesect);
				else
					drone.NavigateToAndExecuteCommand((RoomItem)target, cmd, CollisionType.BoundsIntesect);
				return false;
			}
		}
	}

	public class ObjectRepairUpgradeContainer : RefillableCustomDroneUpgradeContainer<ObjectRepairUpgrade> {

		public ObjectRepairUpgradeContainer() : base(
			new UpgradeRefillDefinition(1, 1, "repair kit"),
			"Installation Restoration",
			DroneUpgradeClass.Other,
			new CustomCommandDefinition("repair",
				"Allows your drone to repair broken installations like doors, power taps and terminals.",
				"terminal",
				new Regex("^[a-zA-Z]+$")),
			8, //purchase cost
			0,
			0
			) {

		}
	}
}
