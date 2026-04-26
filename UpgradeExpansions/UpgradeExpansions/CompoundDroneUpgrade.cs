using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using DSMFramework;
using DSMFramework.Modding;
using ReikaKalseki.DIDrones;

using System.Text.RegularExpressions;
using Steamworks;

namespace ReikaKalseki.Upgrades {

	public class CompoundDroneUpgrade : CustomDroneUpgrade {

		public CompoundDroneUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def, c) {

		}

		protected override bool performAction(ExecutedCommand cmd) {

		}
	}

	public class CompoundDroneUpgradeContainer : CustomDroneUpgradeContainer<CompoundDroneUpgrade> {

		private readonly Dictionary<string, Type> sourceTypes;

		public CompoundDroneUpgradeContainer(string name, params Type[] types) : base(
			"Hack",
			DroneUpgradeClass.Other,
			new CustomCommandDefinition("hack",
				"The combined suite of several ",
				""),
			5, //purchase cost
			0,
			0
			) {

		}
	}
}
