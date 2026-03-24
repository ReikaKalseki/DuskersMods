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

	public class TerminalHackUpgrade : CustomDroneUpgrade {

		public TerminalHackUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def, c) {

		}

		protected override bool performAction(ExecutedCommand cmd) {
			Room room = drone.CurrentRoom;
			TargetableRoomObject target = new TargetableRoomObject(room.GetRoomItem(typeof(DungeonTerminal), false));
			if (target.roomObject == null) {
				SendConsoleResponseMessage("No terminals in room: "+room.LabelSimple, ConsoleMessageType.Warning);
				return false;
			}

			if (target.checkAtElseNavToAndTryAgain(drone, cmd)) {
				DungeonTerminal term = (DungeonTerminal)target.roomObject;
				if (term.supportsDefenseCommand && term.supportsShipScanCommand && term.supportsSurveyCommand) {
					SendConsoleResponseMessage("Terminal already fully unlocked", ConsoleMessageType.Info);
					return false;
				}
				else {
					term.supportsDefenseCommand = true;
					term.supportsShipScanCommand = true;
					term.supportsSurveyCommand = true;
					TerminalManager.Instance.hasDefenses = true;
					TerminalManager.Instance.hasShipScan = true;
					TerminalManager.Instance.hasSurvey = true;
					SendConsoleResponseMessage("Successfully hacked terminal system in room " + room.LabelSimple, ConsoleMessageType.Benefit);
					return true;
				}
			}
			else {
				return false;
			}
		}
	}

	public class TerminalHackUpgradeContainer : CustomDroneUpgradeContainer<TerminalHackUpgrade> {

		public TerminalHackUpgradeContainer() : base(
			"Hack",
			DroneUpgradeClass.Other,
			new CustomCommandDefinition("hack",
				"A suite of military-grade software designed to grant access to all functions within computer systems.",
				""),
			5, //purchase cost
			0,
			0
			) {

		}
	}
}
