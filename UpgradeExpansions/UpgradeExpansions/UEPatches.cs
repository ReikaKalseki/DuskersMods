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

namespace ReikaKalseki.Upgrades {

	public static class UEPatches {

		[HarmonyPatch(typeof(DroneUpgradeFactory))]
		[HarmonyPatch("CreateUpgradeInstance", typeof(DroneUpgradeType), typeof(int))]

		public static class DroneUpgradeBuildLog {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					InstructionHandlers.patchInitialHook(codes, InstructionHandlers.createMethodCall("ReikaKalseki.Upgrades.UEMod", "onCreateUpgrade", new Type[0]));
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

		[HarmonyPatch(typeof(PryUpgrade))]
		[HarmonyPatch("ExecuteCommand")]
		public static class PryUpgradeControl {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
					codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
					codes.Add(new CodeInstruction(OpCodes.Ldarg_2));
					codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.Upgrades.UEMod", "runPryUpgrade", new Type[] { typeof(PryUpgrade), typeof(ExecutedCommand), typeof(bool) }));
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
		/*
		[HarmonyPatch(typeof(BoardingConfigSelectedDrone))]
		[HarmonyPatch("SetDrone")]
		public static class DroneUpgradeUISize1 {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					PatchLib.increaseDroneUpgradeListSize(codes, true);
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

		[HarmonyPatch(typeof(BoardingConfigSelectedDrone))]
		[HarmonyPatch("SetCursorAtSlot")]
		public static class DroneUpgradeUISize2 {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					PatchLib.increaseDroneUpgradeListSize(codes, false);
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

		[HarmonyPatch(typeof(BoardingConfigSelectedDrone))]
		[HarmonyPatch("ArrowUp", typeof(bool))]
		public static class DroneUpgradeUISize3 {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					PatchLib.increaseDroneUpgradeListSize(codes, false, 3);
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

		[HarmonyPatch(typeof(BoardingConfigSelectedDrone))]
		[HarmonyPatch("Awake")]
		public static class DroneUpgradeUISizeInit {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.Upgrades.UEMod", "onInitDroneUI", new Type[] { typeof(BoardingConfigSelectedDrone) }));
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
		static class PatchLib {
			
			public static void increaseDroneUpgradeListSize(List<CodeInstruction> codes, bool all, int val = 4) {
				OpCode seek = val == 4 ? OpCodes.Ldc_I4_4 : (OpCode)typeof(OpCodes).GetField("Ldc_I4_"+val).GetValue(null);
				OpCode put = val == 4 ? OpCodes.Ldc_I4_5 : (OpCode)typeof(OpCodes).GetField("Ldc_I4_"+(val+1)).GetValue(null);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction code = codes[i];
					if (code.opcode == seek) {
						code.opcode = put;
						if (!all)
							break;
					}
				}
			}

		}
	}
}
