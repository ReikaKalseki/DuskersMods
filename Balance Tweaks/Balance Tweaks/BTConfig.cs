using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIDrones;

namespace ReikaKalseki.BalanceTweaks {
	public class BTConfig {
		public enum ConfigEntries {
			[ConfigEntry("Misc", "Allow derelicts to be revisited.", false, false)]allowShipRevisit,
			[ConfigEntry("Misc", "Derelicts will always have enough fuel to travel to the next POI", false, false)]forceEnoughFuel,
			[ConfigEntry("Sensors", "Minimum Motion-Inconclusive Chance (%) Per Room", 25F, 50F)]minMotionBrokenChance,
			[ConfigEntry("Sensors", "Maximum Motion-Inconclusive Chance (%) Per Room", 50F, 50F)]maxMotionBrokenChance,
			[ConfigEntry("Sensors", "Minimum Scan Error Chance (%) Per Room", 0F, 0F)]minScannerBrokenChance,
			[ConfigEntry("Sensors", "Maximum Scan Error Chance (%) Per Room", 20F, 30F)]maxScannerBrokenChance,
			[ConfigEntry("Camera Failure", "Minimum Initial Drone Camera Failure Duration", 2, 15)]minLengthDroneInitialCameraFail,
			[ConfigEntry("Camera Failure", "Maximum Initial Drone Camera Failure Duration", 5, 30)]maxLengthDroneInitialCameraFail,
			[ConfigEntry("Camera Failure", "Minimum Initial Main Camera Failure Duration", 1, 15)]minLengthMainInitialCameraFail,
			[ConfigEntry("Camera Failure", "Maximum Initial Main Camera Failure Duration", 1, 60)]maxLengthMainInitialCameraFail,
			[ConfigEntry("Camera Failure", "Maximum Drone Camera Failure Duration", 30, 99999)]maxLengthDroneCameraFail,
			[ConfigEntry("Camera Failure", "Maximum Main Camera Failure Duration", 10, 99999)]maxLengthMainCameraFail,
			[ConfigEntry("Camera Failure", "Camera Failure Duration Increase Per Failure", 5, 15)]cameraFailDurationStep,
			[ConfigEntry("Camera Failure", "Minimum Camera Uptime (%)", 66F, 0F)]cameraMinFunctionLevel,
			[ConfigEntry("Scrap Capacity", "Increase scrap capacity at lower values", true, false)]tightenScrapCurve,
			[ConfigEntry("Scrap Capacity", "Minimum Scrap Capacity", 50, 20)]minScrapCapacity,
			[ConfigEntry("Scrap Capacity", "Maximum Scrap Capacity", 200, 160)]maxScrapCapacity,
			[ConfigEntry("Scrap Capacity", "Scrap Capacity Multiplier", 1F, 1F)]scrapScalar,
		}
	}
}
