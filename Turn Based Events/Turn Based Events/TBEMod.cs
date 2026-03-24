using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using BepInEx;

using ReikaKalseki.DIDrones;

using HarmonyLib;

using UnityEngine;
using UnityEngine.Experimental.Director;

namespace ReikaKalseki.TBE {

    [BepInPlugin("ReikaKalseki.TBEMod", "Turn-Based Events", "1.0.0")]
	[BepInDependency("ReikaKalseki.DIMod")]
	public class TBEMod : DIModBase {
        
        public static TBEMod instance;

        public TBEMod() : base() {
            instance = this;
			config.addSettings(typeof(TBConfig.ConfigEntries));
		}

		protected override void init() {
            DIMod.onCommandEvent += (ic, cmd, multi) => EventRateControlSystem.incrementCommandCounter(cmd);
        }

	}
}
