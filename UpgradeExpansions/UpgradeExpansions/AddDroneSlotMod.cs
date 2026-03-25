using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using UnityEngine;

namespace ReikaKalseki.DIDrones {
	public class AddDroneSlotMod : ApplyableDroneUpgrade {

		public AddDroneSlotMod() : base("Add mod slot", "Adds an upgrade slot to the drone.", 50) {

		}

		public override int MaxAllowed {
			get {
				return _targetDrone == null ? 2 : 5- _targetDrone.NumberOfUpgradeSlots;
			}
		}

		protected override bool isValid(NonVisualDrone drone) {
			return drone.NumberOfUpgradeSlots < 5;
		}

		protected override void apply(NonVisualDrone drone) {
			drone.NumberOfUpgradeSlots++;
		}
	}

}
