using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReikaKalseki.Upgrades {

	public class ReplaceShipSlotMod : IModification {

		public ModificationStorageIdEnum ModificationStorageId {
			get {
				return ModificationStorageIdEnum.None;
			}
		}

		public string DisplayName {
			get {
				return "Replaces a broken ship upgrade slot";
			}
		}

		public string Description {
			get {
				return "Replaces the first broken ship upgrade slot.";
			}
		}

		public string TargetName {
			get {
				return ((IInventoryItem)_targetUpgrade).Name;
			}
		}

		public int ScrapCost {
			get {
				return -20;
			}
		}

		public int MaxAllowed {
			get {
				return 1;
			}
		}

		public void SetTarget(object itemToReceiveMod) {
			_targetUpgrade = null;
		}

		public bool CanApplyModToTarget() {
			if (GlobalSettings.GameState.ThePlayer.MyShip.slotList != null) {
				for (int i = 0; i < GlobalSettings.GameState.ThePlayer.MyShip.slotList.Count; i++) {
					SlotInfo s = GlobalSettings.GameState.ThePlayer.MyShip.slotList[i];
					if (s.BrokenState == BrokenStateEnum.Broken) {
						_targetUpgrade = s;
						return true;
					}
				}
			}
			return false;
		}

		public void ApplyModToTarget() {
			if (_targetUpgrade == null)
				return;
			_targetUpgrade.Fix();
		}

		public IModification CopyModification() {
			IModification modification = new ReplaceShipSlotMod();
			modification.SetTarget(_targetUpgrade);
			return modification;
		}

		private SlotInfo _targetUpgrade;
	}
}
