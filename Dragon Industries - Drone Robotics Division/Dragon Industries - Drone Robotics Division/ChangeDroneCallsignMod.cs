using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReikaKalseki.DIDrones {
	public class ChangeDroneCallsignMod : IModification {

		public readonly int callsignIndex;

		public ChangeDroneCallsignMod(int idx) {
			callsignIndex = idx;
		}

		public ModificationStorageIdEnum ModificationStorageId {
			get {
				return ModificationStorageIdEnum.None;
			}
		}

		public string DisplayName {
			get {
				return "Callout number "+callsignIndex;
			}
		}

		public string Description {
			get {
				return "Changes a drone's callout to format number \"+callsignIndex";
			}
		}

		public string TargetName {
			get {
				return ((IInventoryItem)_targetDrone).Name;
			}
		}

		public int ScrapCost {
			get {
				return 0;
			}
		}

		public int MaxAllowed {
			get {
				return 1;
			}
		}

		public void SetTarget(object itemToReceiveMod) {
			_targetDrone = (itemToReceiveMod as NonVisualDrone);
		}

		public bool CanApplyModToTarget() {
			return _targetDrone != null;
		}

		public void ApplyModToTarget() {
			if (_targetDrone == null) {
				return;
			}
			_targetDrone.CSID = callsignIndex;
			GameAudio.Play2DSFX((GameAudio.SoundEnum)((int)GameAudio.SoundEnum.DroneCS_1 + callsignIndex));
		}

		public IModification CopyModification() {
			IModification ret = new ChangeDroneCallsignMod(callsignIndex);
			ret.SetTarget(_targetDrone);
			return ret;
		}

		private NonVisualDrone _targetDrone;
	}

}
