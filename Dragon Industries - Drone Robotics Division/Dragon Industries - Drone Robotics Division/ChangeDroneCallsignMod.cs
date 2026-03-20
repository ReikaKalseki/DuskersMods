using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReikaKalseki.DIDrones {
	public class ChangeDroneCallsignMod : IModification {

		private NonVisualDrone _targetDrone;

		public ModificationStorageIdEnum ModificationStorageId {
			get {
				return ModificationStorageIdEnum.None;
			}
		}

		public string DisplayName {
			get {
				return "Cycle callout";
			}
		}

		public string Description {
			get {
				return "Changes a drone's callout sound";
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
			_targetDrone.CSID = (_targetDrone.CSID+1)%13;
			GameAudio.SoundEnum enm = (GameAudio.SoundEnum)((int)GameAudio.SoundEnum.DroneCS_1 + _targetDrone.CSID);
			float vol = GameAudio.VolumeMultiplier(enm, GameAudio.DroneCallSignalVolume);
			GameAudio.LoadSFXIntoDict(enm);
			GameAudio.Play2DSFX(enm, vol);
			//DSUtil.log("Played sound "+Enum.GetName(typeof(GameAudio.SoundEnum), enm)+" at vol "+vol);
		}

		public IModification CopyModification() {
			IModification ret = new ChangeDroneCallsignMod();
			ret.SetTarget(_targetDrone);
			return ret;
		}
	}

}
