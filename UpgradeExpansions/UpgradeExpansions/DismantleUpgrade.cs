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

	public class DismantleUpgrade : CustomDroneUpgrade {

		public DismantleUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def, c) {

		}

		protected override bool performAction(ExecutedCommand cmd) {
			TargetableRoomObject target = new TargetableRoomObject(WorldUtil.findTowableInRoom<Drone>(drone.CurrentRoom, d => d.IsDead/* && !d.CanBeTowed*/));
			string rname = drone.CurrentRoom.Label;
			if (target.roomObject == null) {
				SendConsoleResponseMessage("No valid drone found in room " + rname, ConsoleMessageType.Warning);
				return false;
			}
			if (target.checkAtElseNavToAndTryAgain(drone, cmd)) {
				Drone d = (Drone)(target.roomObject);
				int n = d.CanBeTowed ? 7 : UnityEngine.Random.Range(3, 6); //sentry bots are 1-3, drones scrap for 7-9 based on health, so make broken 3-5
				for (int i = 0; i < n; i++) {
					DungeonManager.Instance.PlaceLootInRoom(drone.CurrentRoom, false, MathUtil.getRandomVectorAround(drone.transform.position, 0.5F), false);
				}
				d.Vaporize(false);
				SendConsoleResponseMessage("Successfully dismantled broken drone in room " + rname, ConsoleMessageType.Benefit);
				return true;
			}
			else {
				return false;
			}
		}
	}

	public class DismantleUpgradeContainer : CustomDroneUpgradeContainer<DismantleUpgrade> {

		public DismantleUpgradeContainer() : base(
			"Scrapper",
			DroneUpgradeClass.Other,
			new CustomCommandDefinition("dismantle",
				"Allows your drone to break disabled or destroyed drones down into scrap.",
				""
			),
			4, //purchase cost
			0,
			0
			) {

		}
	}
}
