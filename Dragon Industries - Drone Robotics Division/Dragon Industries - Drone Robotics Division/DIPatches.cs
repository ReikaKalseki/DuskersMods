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

namespace ReikaKalseki.DIDrones {

	public static class DIPatches {

		[HarmonyPatch(typeof(ConsoleWindow3))]
		[HarmonyPatch("SendCommandToObject")]
		
		public static class CommandHook {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {/*
					List<CodeInstruction> add = new List<CodeInstruction>(){
						new CodeInstruction(OpCodes.Ldarg_0),
						new CodeInstruction(OpCodes.Ldarg_1),
						new CodeInstruction(OpCodes.Ldarg_2),
						InstructionHandlers.createMethodCall("ReikaKalseki.DIDrones.DIMod", "onCommand", new Type[] { typeof(DungeonManager), typeof(ExecutedCommand), typeof(bool) })
					};
					//InstructionHandlers.patchEveryReturnPre(codes, add.ToArray());
					int idx = InstructionHandlers.getLastOpcodeBefore(codes, codes.Count, OpCodes.Ret);
					codes.InsertRange(idx, add);
					*/
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "ICommandable", "ExecuteCommand", new Type[]{ typeof(ExecutedCommand), typeof(bool)});
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIDrones.DIMod", "onCommand", new Type[] { typeof(ICommandable), typeof(ExecutedCommand), typeof(bool) });
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

		[HarmonyPatch(typeof(SystemMessageManager))]
		[HarmonyPatch("ShowSystemMessage", typeof(string), typeof(ConsoleMessageType))]
		
		public static class MessageHook1 {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					PatchLib.redirectSysMsgInternal(codes);
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

		[HarmonyPatch(typeof(SystemMessageManager))]
		[HarmonyPatch("ShowSystemMessage", typeof(string), typeof(ConsoleMessageType), typeof(SystemMessageImageType))]
		
		public static class MessageHook2 {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					PatchLib.redirectSysMsgInternal(codes);
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

		[HarmonyPatch(typeof(BoardingConfigInventorySlot))]
		[HarmonyPatch("SetInventoryItem", typeof(IInventoryItem))]
		public static class BoardingUIInvSlotSetFix {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					/*
					InstructionHandlers.patchInitialHook(codes,
						new CodeInstruction(OpCodes.Ldarg_0),
						new CodeInstruction(OpCodes.Ldarg_1),
						InstructionHandlers.createMethodCall("ReikaKalseki.DIDrones.DIMod", "onSetBoardingUISlot", new Type[] { typeof(BoardingConfigInventorySlot), typeof(IInventoryItem) })
					);*/
					PatchLib.fixInvSlotItemTypeDetection(codes);
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

		[HarmonyPatch(typeof(Drone))]
		[HarmonyPatch("AddSoundSources")]
		public static class DroneSoundHook {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					InstructionHandlers.patchEveryReturnPre(codes,
						new CodeInstruction(OpCodes.Ldarg_0),
						InstructionHandlers.createMethodCall("ReikaKalseki.DIDrones.DIMod", "hookDroneSounds", new Type[] { typeof(Drone) })
					);
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

		[HarmonyPatch(typeof(UIDroneItem))]
		[HarmonyPatch("Highlight")]
		public static class DroneMenuSelectHook {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					InstructionHandlers.patchInitialHook(codes,
						new CodeInstruction(OpCodes.Ldarg_0),
						InstructionHandlers.createMethodCall("ReikaKalseki.DIDrones.DIMod", "onDroneInListSelected", new Type[] { typeof(UIDroneItem) })
					);
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

		[HarmonyPatch(typeof(BoardingConfigDronePanel))]
		[HarmonyPatch("SetHighlighted")]
		public static class DroneMenu2SelectHook {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					InstructionHandlers.patchInitialHook(codes,
						new CodeInstruction(OpCodes.Ldarg_0),
						new CodeInstruction(OpCodes.Ldarg_1),
						InstructionHandlers.createMethodCall("ReikaKalseki.DIDrones.DIMod", "onDroneInListSelected", new Type[] { typeof(BoardingConfigDronePanel), typeof(bool) })
					);
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

		[HarmonyPatch(typeof(BoardingConfigDronePanel))]
		[HarmonyPatch("SetCursorHere")]
		public static class DroneMenu2bSelectHook {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					InstructionHandlers.patchInitialHook(codes,
						new CodeInstruction(OpCodes.Ldarg_0),
						new CodeInstruction(OpCodes.Ldarg_1),
						InstructionHandlers.createMethodCall("ReikaKalseki.DIDrones.DIMod", "onDroneInListSelected", new Type[] { typeof(BoardingConfigDronePanel), typeof(bool) })
					);
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

			internal static void redirectSysMsgInternal(List<CodeInstruction> li) {
				int idx = InstructionHandlers.getLastOpcodeBefore(li, li.Count, OpCodes.Callvirt);
				li[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIDrones.DIMod", "onSystemMessage", new Type[] { typeof(SystemMessageManager), typeof(string), typeof(ConsoleMessageType), typeof(SystemMessageImageType) });
			}

			internal static void fixInvSlotItemTypeDetection(List<CodeInstruction> codes) {
				int idx1 = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "System.Object", "GetType", new Type[0]);
				int idx2 = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "System.Type", "GetTypeFromHandle", new Type[]{ typeof(RuntimeTypeHandle) });
				codes.RemoveRange(idx1, idx2 - idx1 + 1);
				codes.InsertRange(idx1, new List<CodeInstruction>{
					InstructionHandlers.createMethodCall("ReikaKalseki.DIDrones.DIExtensions", "isDroneUpgrade", new Type[] { typeof(IInventoryItem) }),
					new CodeInstruction(OpCodes.Ldc_I4_1) //true, so != true
				});
			}

			public static void replaceMaybeInlinedFieldWithConstant(List<CodeInstruction> codes, string owner, string name, float origVal, float newVal) {
				int idx = -1;
				try {
					idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldsfld, owner, name);
				}
				catch (Exception e) {
					idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, origVal);
				}
				codes[idx] = new CodeInstruction(OpCodes.Ldc_R4, newVal);
			}
			
		}
	}
}
