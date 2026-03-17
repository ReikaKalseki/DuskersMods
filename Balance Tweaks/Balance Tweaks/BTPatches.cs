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
		[HarmonyDebug]
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
		[HarmonyDebug]
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
		[HarmonyDebug]
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
		[HarmonyDebug]
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
		[HarmonyDebug]
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

		static class PatchLib {

			internal static void redirectShipVisited(List<CodeInstruction> codes) {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "DungeonInfo", "get_HaveVisited", new Type[0]);
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.BalanceTweaks.BTMod", "isWreckVisited", new Type[] { typeof(DungeonInfo)});
			}
			
		}
	}
}
