using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using UnityEngine;

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
			GameAudio.LoadSFXIntoDict(enm);
			DroneCallsignPreview dr = GalaxyMapManager.Instance.gameObject.AddComponent<DroneCallsignPreview>();
			dr.sound = enm;
			dr.StartCoroutine(dr.playAndRemove());
		}

		public IModification CopyModification() {
			IModification ret = new ChangeDroneCallsignMod();
			ret.SetTarget(_targetDrone);
			return ret;
		}

		class DroneCallsignPreview : MonoBehaviour {

			public GameAudio.SoundEnum sound;

			public IEnumerator playAndRemove() {
				yield return new WaitForSeconds(0.25F);
				//DSUtil.log("Playing sound "+sound+" after delay 0.25");
				GameAudio.Play2DSFX(sound, GameAudio.VolumeMultiplier(sound, GameAudio.DroneCallSignalVolume));
				UnityEngine.Object.Destroy(this);
			}

		}
	}

}
