using System;
using System.IO;    //For data read/write methods
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using ReikaKalseki.DIDrones;

namespace ReikaKalseki.BalanceTweaks {

	public static class BTPatches {
		/*
		[HarmonyPatch(typeof(DungeonInfo))]
		[HarmonyPatch("get_HaveVisited")]
		
		public static class DisableRevisitCheck {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
					codes.Add(new CodeInstruction(OpCodes.Ret));
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
			*/
		[HarmonyPatch(typeof(GalaxyMapManager))]
		[HarmonyPatch("ExecuteBoardOrTravel")]
		
		public static class DisableRevisitCheck1 {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					PatchLib.redirectShipVisited(codes);
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(GalaxyMapManager))]
		[HarmonyPatch("BoardCurrentDungeon")]
		
		public static class DisableRevisitCheck2 {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					PatchLib.redirectShipVisited(codes);
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(GalaxyMapManager))]
		[HarmonyPatch("GetDistanceToClosestVisitableDungeon")]
		
		public static class DisableRevisitCheck3 {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					PatchLib.redirectShipVisited(codes);
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(DungeonRoom))]
		[HarmonyPatch(MethodType.Constructor, typeof(Coordinate2D), typeof(Coordinate2D), typeof(System.Random))]
		
		public static class HookWreckRoomInit {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.BalanceTweaks.BTMod", "initializeWreckRoomConstants", new Type[] { typeof(DungeonRoom) }));
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(DungeonBuilder))]
		[HarmonyPatch("BuildDungeon")]
		
		public static class FuelAmountGuarantee1 {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "Room", "roomItems");
					idx = InstructionHandlers.getFirstOpcode(codes, idx, OpCodes.Callvirt);
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.BalanceTweaks.BTMod", "addFuelNode", new Type[] { typeof(List<RoomItem>), typeof(FuelAccess) });
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		internal static class PatchLib {

			internal static bool redirectScrapCost(Type t) {
				PropertyInfo p = t.GetProperty("ScrapCost");
				if (p == null)
					return false;
				InstructionHandlers.patchMethod(BTMod.instance.harmony, p.GetGetMethod(), BTMod.modDLL, li => {
					li.Clear();
					li.Add(new CodeInstruction(OpCodes.Ldarg_0));
					li.Add(InstructionHandlers.createMethodCall("ReikaKalseki.BalanceTweaks.BTMod", "getModScrapCost", new Type[] { typeof(IModification) }));
					li.Add(new CodeInstruction(OpCodes.Ret));
				});
				return true;
			}

			internal static void redirectShipVisited(List<CodeInstruction> codes) {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "DungeonInfo", "get_HaveVisited", new Type[0]);
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.BalanceTweaks.BTMod", "isWreckVisited", new Type[] { typeof(DungeonInfo)});
			}
			
		}
	}
}
