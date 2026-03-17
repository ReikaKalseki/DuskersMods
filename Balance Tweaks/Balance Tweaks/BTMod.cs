using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;

namespace ReikaKalseki.BalanceTweaks {

    [BepInPlugin("ReikaKalseki.BTMod", "BalanceTweaks", "1.0.0")]
	[BepInDependency("ReikaKalseki.DIMod")]
	public class BTMod : BaseUnityPlugin {

        public static BTMod instance;

		public static Assembly modDLL;

		public static bool forceAllowVisit = true;

        public BTMod() : base() {
            instance = this;
            DSUtil.log("Constructed BalanceTweaks object", DSUtil.diDLL);

			modDLL = Assembly.GetExecutingAssembly();
		}

		public void Awake() {
			DSUtil.log("Begin Initializing BalanceTweaks");
			try {
				Harmony harmony = new Harmony("BalanceTweaks");
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
			}
			catch (Exception e) {
				DSUtil.log("Failed to load BalanceTweaks: " + e);
			}
			DSUtil.log("Finished Initializing BalanceTweaks");
		}

        public static bool isWreckVisited(DungeonInfo ship) {
            return ship.HaveVisited && !forceAllowVisit;
		}

		public static void initializeWreckRoomConstants(DungeonRoom r) {
            r.roomMotionBrokenProbability = UnityEngine.Random.Range(0.25F, 0.5F); //from flat 50%
			r.roomScannerBrokenProbability = UnityEngine.Random.Range(0F, 0.2F); //from 0-30%
			DSUtil.log(string.Format("Adjusted room survey-broken probabilities: M={0}%, S={1}%", r.roomMotionBrokenProbability*100, r.roomScannerBrokenProbability*100));
		}

		public static void addFuelNode(List<RoomItem> li, FuelAccess f) {
			int dist = new GalaxyMapManager().GetDistanceToClosestVisitableDungeon();
			int has = GlobalSettings.GameState.ThePlayer.Inventory.TotalPropulsionFuel;
			if (has < dist) {
				f.countPropulsionFuel = Math.Max(f.countPropulsionFuel, dist-has);
			}
			li.Add(f);
		}

	}
}
