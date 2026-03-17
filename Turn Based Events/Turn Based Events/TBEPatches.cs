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

namespace ReikaKalseki.TBE {

	public static class TBEPatches {

		[HarmonyPatch(typeof(BaseGameEvent))]
		[HarmonyPatch("Update")]
		[HarmonyDebug]
		public static class BaseGameEventUpdate {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes = PatchLib.redirectEventMethod("baseEventTick", typeof(BaseGameEvent));
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

		[HarmonyPatch(typeof(AirlockSealFailEvent))]
		[HarmonyPatch("Update")]
		[HarmonyDebug]
		public static class AirlockEventTick {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes = PatchLib.redirectEventMethod("tickAirlockSeal", typeof(AirlockSealFailEvent));
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

		[HarmonyPatch(typeof(AirlockSealFailEvent))]
		[HarmonyPatch("ExecuteEvent")]
		[HarmonyDebug]
		public static class AirlockEventExec {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes = PatchLib.redirectEventMethod("performAirlockSeal", typeof(AirlockSealFailEvent));
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

		[HarmonyPatch(typeof(AsteroidEvent))]
		[HarmonyPatch("Update")]
		[HarmonyDebug]
		public static class AsteroidEventTick {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes = PatchLib.redirectEventMethod("tickAsteroids", typeof(AsteroidEvent));
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

		[HarmonyPatch(typeof(AsteroidEvent))]
		[HarmonyPatch("ExecuteEvent")]
		[HarmonyDebug]
		public static class AsteroidEventExec {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes = PatchLib.redirectEventMethod("performAsteroids", typeof(AsteroidEvent));
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

		[HarmonyPatch(typeof(CloseCommandEvent))]
		[HarmonyPatch("ExecuteEvent")]
		[HarmonyDebug]
		public static class CloseCommandEventExec {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes = PatchLib.redirectEventMethod("performCloseCommandEvent", typeof(CloseCommandEvent));
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

		[HarmonyPatch(typeof(DoorFailEvent))]
		[HarmonyPatch("ExecuteEvent")]
		[HarmonyDebug]
		public static class DoorFailEventExec {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes = PatchLib.redirectEventMethod("performDoorFailEvent", typeof(DoorFailEvent));
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

		[HarmonyPatch(typeof(RoomDestroyEvent))]
		[HarmonyPatch("ExecuteEvent")]
		[HarmonyDebug]
		public static class PipeBreakEventExec {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>();
				try {
					codes = PatchLib.redirectEventMethod("performPipeBreakEvent", typeof(RoomDestroyEvent));
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

		[HarmonyPatch(typeof(Room))]
		[HarmonyPatch("Update")]
		[HarmonyDebug]
		public static class RoomRadiateRedirect {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Room", "Radiate", new Type[]{ typeof(string) });
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.TBE.EventRateControlSystem", "radiateRoomRedirect", new Type[] { typeof(Room), typeof(string) });
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

			internal static List<CodeInstruction> redirectEventMethod(string target, Type t) {
				return new List<CodeInstruction> {
					new CodeInstruction(OpCodes.Ldarg_0),
					InstructionHandlers.createMethodCall("ReikaKalseki.TBE.EventRateControlSystem", target, new Type[] { t }),
					new CodeInstruction(OpCodes.Ret),
				};
			}
			
		}
	}
}
