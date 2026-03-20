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

		static class PatchLib {
			
		}
	}
}
