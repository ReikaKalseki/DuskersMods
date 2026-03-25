using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using UnityEngine;

namespace ReikaKalseki.DIDrones {
	public class ChangeDroneCallsignMod : ApplyableDroneUpgrade {

		public ChangeDroneCallsignMod() : base("Cycle callsign", "Changes a drone's callsign sound", 0) {

		}

		protected override bool isValid(NonVisualDrone drone) {
			return true;
		}

		protected override void apply(NonVisualDrone drone) {
			drone.CSID = (drone.CSID+1)%13;
			GameAudio.SoundEnum enm = (GameAudio.SoundEnum)((int)GameAudio.SoundEnum.DroneCS_1 + drone.CSID);
			GameAudio.LoadSFXIntoDict(enm);
			DroneCallsignPreview dr = GalaxyMapManager.Instance.gameObject.AddComponent<DroneCallsignPreview>();
			dr.sound = enm;
			dr.StartCoroutine(dr.playAndRemove());
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
