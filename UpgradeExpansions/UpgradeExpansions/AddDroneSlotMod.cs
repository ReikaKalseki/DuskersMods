using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using UnityEngine;

namespace ReikaKalseki.DIDrones {
	public class AddDroneSlotMod : IModification {

		private NonVisualDrone _targetDrone;

		public ModificationStorageIdEnum ModificationStorageId {
			get {
				return ModificationStorageIdEnum.None;
			}
		}

		public string DisplayName {
			get {
				return "Add mod slot";
			}
		}

		public string Description {
			get {
				return "Adds an upgrade slot to the drone.";
			}
		}

		public string TargetName {
			get {
				return ((IInventoryItem)_targetDrone).Name;
			}
		}

		public int ScrapCost {
			get {
				return 50;
			}
		}

		public int MaxAllowed {
			get {
				return _targetDrone == null ? 2 : 5- _targetDrone.NumberOfUpgradeSlots;
			}
		}

		public void SetTarget(object itemToReceiveMod) {
			_targetDrone = (itemToReceiveMod as NonVisualDrone);
		}

		public bool CanApplyModToTarget() {
			return _targetDrone != null && _targetDrone.NumberOfUpgradeSlots < 5;
		}

		public void ApplyModToTarget() {
			if (_targetDrone == null) {
				return;
			}
			_targetDrone.NumberOfUpgradeSlots++;
		}

		public IModification CopyModification() {
			IModification ret = new AddDroneSlotMod();
			ret.SetTarget(_targetDrone);
			return ret;
		}
	}

}
