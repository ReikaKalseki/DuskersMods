using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;
using System.Linq;

namespace ReikaKalseki.BalanceTweaks {

	[BepInPlugin("ReikaKalseki.BTMod", "BalanceTweaks", "1.0.0")]
	[BepInDependency("ReikaKalseki.DIMod")]
	public class BTMod : BaseUnityPlugin {

		public static BTMod instance;

		public Harmony harmony;

		public static Assembly modDLL;

		public static bool forceAllowVisit = true;

		public static readonly Dictionary<string, int> modScrapCosts = new Dictionary<string, int>();

		public BTMod() : base() {
			instance = this;
			DSUtil.log("Constructed BalanceTweaks object", DSUtil.diDLL);

			modDLL = Assembly.GetExecutingAssembly();
		}

		public void Awake() {
			DSUtil.log("Begin Initializing BalanceTweaks");
			try {
				harmony = new Harmony("BalanceTweaks");
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

				List<string> mods = new List<string>() {
					"AddRepairJuiceMod",
					"BaseResupplyMod",
					"CannonRechargeMod",
					"CraftFuelMod",
					"CraftGathererMod",
					"CraftGeneratorMod",
					"CraftTowMod",
					"DecontaminateRechargeMod",
					"DroneSpeedMod",
					"IncreaseMaxHpMod",
					"IncreaseProbeHpMod",
					"MagneticMod",
					"OverloadRechargeMod",
					"ProbeStealthMod",
					"RadiationDetectorMod",
					//"RepairDroneUpgradeMod",
					"RepairDroneVideoMod",
					//"RepairFullHpMod",
					"RepairHpMod",
					"RepairShieldMod",
					"RepairShipSlotMod",
					//"RepairShipUpgradeMod",
					"RepairShipVisualMod",
					//"ScrapMod",
					"ShieldRadiationMod",
					"ShieldRechargeMod",
					"SonicRechargeMod",
					"StealthRechargeMod",
					"TeleportMod",
				};
				foreach (string mod in mods)
					getModDefaultCostAndPatchClass(mod);

				string dir = Path.GetDirectoryName(modDLL.Location);
				string cfg = Path.Combine(dir, "ModCosts.txt");
				if (File.Exists(cfg)) {
					string[] lines = File.ReadAllLines(cfg);
					foreach (string s in lines) {
						string[] parts = s.Split('=');
						string mod = parts[0];
						if (!modScrapCosts.ContainsKey(mod)) {
							DSUtil.log("Unrecognized scrap cost in config: '"+mod+"'; set:\n"+modScrapCosts.Keys.toDebugString());
							continue;
						}
						int at = modScrapCosts[mod];
						int put = int.Parse(parts[1]);
						if (at != put) {
							DSUtil.log(string.Format("Overriding cost of '{0}' modification: {1} -> {2}", mod, at, put));
							modScrapCosts[mod] = put;
						}
					}
				}
				else {
					List<string> li = new List<string>();
					foreach (KeyValuePair<string, int> kvp in modScrapCosts)
						li.Add(kvp.Key+"="+kvp.Value);
					File.WriteAllLines(cfg, li.ToArray());
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
			DSUtil.log(string.Format("Adjusted room survey-broken probabilities: M={0}%, S={1}%", r.roomMotionBrokenProbability * 100, r.roomScannerBrokenProbability * 100));
		}

		public static void addFuelNode(List<RoomItem> li, FuelAccess f) {
			int has = GlobalSettings.GameState.ThePlayer.Inventory.TotalPropulsionFuel;
			DungeonInfo dg = WorldUtil.getClosestVisitableDungeon(false, false, true);
			StarSystemInfo sys = GlobalSettings.GameState.ThePlayer.CurrentStarSystem;
			DSUtil.log(string.Format("Wreck-gen fuel check A: Player in system '{1}' has {0} fuel", has, sys == null ? "<None>" : sys.Name));
			if (dg == null) {
				if (sys == null || sys.Dungeons == null) {
					DSUtil.log("No system dungeon data?!");
					return;
				}
				DSUtil.log("No valid wrecks to jump to in-system. Wreck set: " + sys.Dungeons.Select(d => string.Format("{0}={1} [{2},{3}]", d.Name, d.getDistance(), d.HaveVisited, d.HasRequiredEquipment)).toDebugString());
				return;
			}
			int dist = dg.getDistance();
			DSUtil.log(string.Format("Wreck-gen fuel check B: Nearest valid wreck ('{1}') is {0} days away", dist, dg.Name));
			if (has < dist) {
				int min = dist-has;
				DSUtil.log(string.Format("Forced wreck to have at least {0} units of P-fuel (from {1})", min, f.countPropulsionFuel));
				f.countPropulsionFuel = Math.Max(f.countPropulsionFuel, min);
			}
			li.Add(f);
		}

		private static void getModDefaultCostAndPatchClass(string classname) {
			getModDefaultCostAndPatchClass(InstructionHandlers.getTypeBySimpleName(classname));
		}

		private static void getModDefaultCostAndPatchClass(Type t) {
			string name = t.Name;
			name = name.Substring(0, name.Length - 3);
			IModification mod = t == typeof(BaseResupplyMod) ? new AddMotionSensorsMod() : (IModification)Activator.CreateInstance(t);
			int ret = -mod.ScrapCost;
			if (BTPatches.PatchLib.redirectScrapCost(t)) {
				modScrapCosts[name] = ret;
			}
		}

		public static int getModScrapCost(IModification mod) {
			string n = mod.GetType().Name;
			n = n.Substring(0, n.Length - 3);
			if (!modScrapCosts.ContainsKey(n) && mod is BaseResupplyMod)
				n = "BaseResupply";
			if (!modScrapCosts.ContainsKey(n)) {
				DSUtil.log("Failed to fetch scrap cost of modification "+mod.DisplayName+" ("+mod.GetType().Name+") ["+n+"]!");
				return 5;
			}
			return -modScrapCosts[n];
		}

	}
}
