using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIDrones;

namespace ReikaKalseki.TBE {
	public class TBConfig {
		public enum ConfigEntries {
			[ConfigEntry("General", "Global command threshold modifier", 1F, 0F)]THRESHOLD_SCALAR,
			[ConfigEntry("General", "Event command threshold modifier", 1F, 0F)]EVENT_SCALAR,
			[ConfigEntry("General", "Transporter command threshold modifier", 1F, 0F)]TRANSPORTER_SCALAR,
			[ConfigEntry("General", "Swarm command threshold modifier", 1F, 0F)]SWARM_SCALAR,

			[ConfigEntry("Command Threshold Modifiers", "Minimum 'bonus' moves to threshold (flat)", 0, 0)]MIN_EXTRA_MOVES_FLAT,
			[ConfigEntry("Command Threshold Modifiers", "Minimum 'bonus' moves to threshold (percentage)", 0F, 0F)]MIN_EXTRA_MOVES_PCT,
			[ConfigEntry("Command Threshold Modifiers", "Maximum 'bonus' moves to threshold (flat)", 8, 0)]MAX_EXTRA_MOVES_FLAT,
			[ConfigEntry("Command Threshold Modifiers", "Maximum 'bonus' moves to threshold (percentage)", 30F, 0F)]MAX_EXTRA_MOVES_PCT,

			[ConfigEntry("Command Thresholds - Airlocks", "Minimum number of moves for airlocks to completely fail", 2, 0)]IMMINENT_AIRLOCK_FAILURE,
			[ConfigEntry("Command Thresholds - Airlocks", "Minimum number of moves for airlocks to enter imminent failure", 6, 0)]AIRLOCK_FAILURE_SECOND_STAGE,
			[ConfigEntry("Command Thresholds - Airlocks", "Minimum number of moves for airlocks to begin to fail", 15, 0)]QUEUED_AIRLOCK_FAILURE,
			[ConfigEntry("Command Thresholds - Airlocks", "Minimum number of moves for airlocks to start failing again after stabilization", 5, 0)]AIRLOCK_RETRY_FAILURE,

			[ConfigEntry("Command Thresholds - Asteroids", "Minimum number of moves for asteroids to impact", 7, 0)]ASTEROID_HIT, // + 3-16 = 10-23
			[ConfigEntry("Command Thresholds - Asteroids", "Minimum number of moves for asteroids to move inwards", 5, 0)]ASTEROID_WARNING,
			[ConfigEntry("Command Thresholds - Asteroids", "Minimum number of moves for asteroids to spawn", 30, 0)]ASTEROID_QUEUE,

			[ConfigEntry("Command Thresholds - Doors", "Minimum number of moves for door to fail", 5, 0)]DOOR_FAILURE,

			[ConfigEntry("Command Thresholds - Doors", "Minimum number of moves for 'close' to completely fail", 20, 0)]CLOSE_FAILURE,
			[ConfigEntry("Command Thresholds - Doors", "Minimum number of moves for 'close' to begin to fail", 80, 0)]CLOSE_FAILURE_WARN,

			[ConfigEntry("Command Thresholds - Radiation", "Minimum number of moves for reactor pipe to break", 4, 0)]RADIATION_PIPE_BREAK,
			[ConfigEntry("Command Thresholds - Radiation", "Minimum number of moves for reactor pipe to overpressurize", 50, 0)]RADIATION_PIPE_GROAN,

			[ConfigEntry("Command Thresholds - Swarms", "Minimum number of moves for swarms to begin breaking a door", 20, 0)]CHEW_START,
			[ConfigEntry("Command Thresholds - Swarms", "Minimum number of moves for swarms to destroy a door", 10, 0)]CHEW_BREAK,
		}
	}
}
