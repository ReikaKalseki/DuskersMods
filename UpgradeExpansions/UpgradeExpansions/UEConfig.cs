using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIDrones;

namespace ReikaKalseki.Upgrades {
	public class UEConfig {
		public enum ConfigEntries {
			[ConfigEntry("New Upgrades", "Enable 'doorcharger' drone upgrade", true, false)]enableDoorCharge,
			[ConfigEntry("New Upgrades", "Enable 'repair' drone upgrade", true, false)]enableRepair,
			[ConfigEntry("New Upgrades", "Enable 'dismantle' drone upgrade", true, false)]enableDismantle,
			[ConfigEntry("New Upgrades", "Enable 'hack' drone upgrade", true, false)]enableHack,
			[ConfigEntry("Misc", "Enable 'Add modification slot' drone upgrade", true, false)]enableModSlot,
			[ConfigEntry("Existing Upgrades", "Expand 'pry' drone upgrade to allow removing firm ship upgrades", true, false)]enableUpgradePry,
		}
	}
}
