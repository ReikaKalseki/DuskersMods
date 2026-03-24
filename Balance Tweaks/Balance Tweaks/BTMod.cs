using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;
using BepInEx.Configuration;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;
using System.Linq;

namespace ReikaKalseki.BalanceTweaks {

	[BepInPlugin("ReikaKalseki.BTMod", "BalanceTweaks", "1.0.0")]
	[BepInDependency("ReikaKalseki.DIMod")]
	public class BTMod : DIModBase {

		public static readonly Dictionary<string, int> modScrapCosts = new Dictionary<string, int>();

		public static BTMod instance;

		public BTMod() : base() {
			instance = this;
			config.addSettings(typeof(BTConfig.ConfigEntries));
		}

		protected override void addAdditionalConfig() {
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

			config.addSettings("Modification Costs", modScrapCosts);
		}

		protected override void init() {
			foreach (string mod in modScrapCosts.Keys) {
				int at = modScrapCosts[mod];
				int put = config.getValue<int>(mod);
				if (at != put) {
					DSUtil.log(string.Format("Overriding cost of '{0}' modification: {1} -> {2}", mod, at, put));
					modScrapCosts[mod] = put;
				}
			}
		}

		public static bool isWreckVisited(DungeonInfo ship) {
			return ship.HaveVisited && !instance.config.getBoolean(BTConfig.ConfigEntries.allowShipRevisit);
		}

		public static void initializeWreckRoomConstants(DungeonRoom r) {
			float mmin = instance.config.getFloat(BTConfig.ConfigEntries.minMotionBrokenChance);
			float mmax = instance.config.getFloat(BTConfig.ConfigEntries.maxMotionBrokenChance);
			float smin = instance.config.getFloat(BTConfig.ConfigEntries.minScannerBrokenChance);
			float smax = instance.config.getFloat(BTConfig.ConfigEntries.maxScannerBrokenChance);
			r.roomMotionBrokenProbability = UnityEngine.Random.Range(mmin / 100F, mmax / 100F); //from flat 50%
			r.roomScannerBrokenProbability = UnityEngine.Random.Range(smin / 100F, smax / 100F); //from 0-30%
			DSUtil.log(string.Format("Adjusted room survey-broken probabilities: M={0}%, S={1}%", r.roomMotionBrokenProbability * 100, r.roomScannerBrokenProbability * 100));
		}

		public static void addFuelNode(List<RoomItem> li, FuelAccess f) {
			if (!instance.config.getBoolean(BTConfig.ConfigEntries.forceEnoughFuel))
				return;
			int has = GlobalSettings.GameState.ThePlayer.Inventory.TotalPropulsionFuel;
			DungeonInfo dg = WorldUtil.getClosestVisitableDungeon(false, false, true);
			if (dg == null)
				dg = WorldUtil.getClosestVisitableDungeon(false, false, false);
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

		private void getModDefaultCostAndPatchClass(string classname) {
			getModDefaultCostAndPatchClass(InstructionHandlers.getTypeBySimpleName(classname));
		}

		private void getModDefaultCostAndPatchClass(Type t) {
			string name = t.Name;
			name = name.Substring(0, name.Length - 3);
			IModification mod = t == typeof(BaseResupplyMod) ? new AddMotionSensorsMod() : (IModification)Activator.CreateInstance(t);
			int ret = -mod.ScrapCost;
			if (BTPatches.PatchLib.redirectScrapCost(this, t)) {
				modScrapCosts[name] = ret;
			}
		}

		public static int getModScrapCost(IModification mod) {
			string n = mod.GetType().Name;
			n = n.Substring(0, n.Length - 3);
			if (!modScrapCosts.ContainsKey(n) && mod is BaseResupplyMod)
				n = "BaseResupply";
			if (!modScrapCosts.ContainsKey(n)) {
				DSUtil.log("Failed to fetch scrap cost of modification " + mod.DisplayName + " (" + mod.GetType().Name + ") [" + n + "]!");
				return 5;
			}
			return -modScrapCosts[n];
		}

		public static void onInitVideoFailure(VideoFailManager mgr) {
			bool main = mgr._videoFailObject is DungeonInfo;
			int tmin = main ? instance.config.getInt(BTConfig.ConfigEntries.minLengthMainInitialCameraFail) : instance.config.getInt(BTConfig.ConfigEntries.minLengthDroneInitialCameraFail);
			int tmax = main ? instance.config.getInt(BTConfig.ConfigEntries.maxLengthMainInitialCameraFail) : instance.config.getInt(BTConfig.ConfigEntries.maxLengthDroneInitialCameraFail);
			mgr._videoFailObject.VideoLossDuration = Mathf.Clamp(VideoFailManager._random.NextFloat(mgr._failDurationMinInitial * 0.75F, mgr._failDurationMaxInitial * 0.5F), tmin, tmax);
			mgr._videoFailObject.TimeOfNextVideoRestore = mgr._videoFailObject.TimeOfNextVideoLoss + mgr._videoFailObject.VideoLossDuration;
		}

		public static void incrementVideoFailureDuration(IHasVideoThatCanFail failer, float put, VideoFailManager mgr) {
			bool main = mgr._videoFailObject is DungeonInfo;
			float orig = put-15;
			float uptime = instance.config.getFloat(BTConfig.ConfigEntries.cameraMinFunctionLevel)/100F;
			float maxDurUptime = mgr._videoFailObject.TimeTilNextFailMin*(1-uptime);
			float hardLimit = main ? instance.config.getInt(BTConfig.ConfigEntries.maxLengthMainCameraFail) : instance.config.getInt(BTConfig.ConfigEntries.maxLengthDroneCameraFail);
			float stepLimit = orig + instance.config.getInt(BTConfig.ConfigEntries.cameraFailDurationStep);

			failer.VideoLossDuration = Mathf.Min(maxDurUptime, stepLimit, hardLimit);
		}

		public static void setShipScrapCapacity(DungeonInfo info, int amt) {
			int min = instance.config.getInt(BTConfig.ConfigEntries.minScrapCapacity);
			int max = instance.config.getInt(BTConfig.ConfigEntries.maxScrapCapacity);
			float f = instance.config.getFloat(BTConfig.ConfigEntries.scrapScalar);
			float raw = instance.config.getBoolean(BTConfig.ConfigEntries.tightenScrapCurve) ? Mathf.Pow(amt, 0.5F)*15 : amt;
			info.ScrapMax = (int)Mathf.Clamp(raw * f, min, max); //(int)Mathf.Clamp(Mathf.Pow(amt, 0.6F)*8, 50, 200);
		}

	}
}
