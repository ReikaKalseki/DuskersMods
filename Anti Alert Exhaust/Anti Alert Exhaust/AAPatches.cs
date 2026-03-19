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

namespace ReikaKalseki.AAE {

	public static class AAPatches {

		[HarmonyPatch(typeof(SystemMessageManager))]
		[HarmonyPatch("ShowSystemMessageInternal")]
		
		public static class MessageSoundHook {

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
				try {
					for (int i = codes.Count - 1; i >= 0; i--) {
						CodeInstruction ci = codes[i];
						if (ci.opcode == OpCodes.Call) {
							MethodInfo mi = (MethodInfo)ci.operand;
							if (mi.DeclaringType.Name == "GameAudio" && mi.Name == "Play2DSFX") {
								codes[i] = InstructionHandlers.createMethodCall("ReikaKalseki.AAE.AAEMod", "queueMessageAlertSound", new Type[] { typeof(GameAudio.SoundEnum), typeof(string), typeof(ConsoleMessageType) });
								codes.InsertRange(i, new List<CodeInstruction> {
									new CodeInstruction(OpCodes.Ldarg_1),
									new CodeInstruction(OpCodes.Ldarg_2)
								});
							}
						}
					}
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
