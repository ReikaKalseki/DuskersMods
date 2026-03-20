using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReikaKalseki.DIDrones {
	public class WorldUtil {

		public static DungeonInfo getClosestVisitableDungeon(bool allowVisited, bool ignoreEquipment) {
			if (GlobalSettings.GameState.ThePlayer.CurrentStarSystem == null || GlobalSettings.GameState.ThePlayer.CurrentStarSystem.Dungeons == null) {
				return null;
			}
			int dist = -1;
			DungeonInfo ret = null;
			DungeonInfo at = GlobalSettings.GameState.ThePlayer.CurrentDockedDungeon;
			foreach (DungeonInfo dg in GlobalSettings.GameState.ThePlayer.CurrentStarSystem.Dungeons) {
				if (dg != null && dg != at && (!dg.HaveVisited || allowVisited) && (ignoreEquipment || dg.HasRequiredEquipment)) {
					int d0 = GalaxyMapManager.CalculateDungeonDistanceInDays(at.Coordinates, dg.Coordinates);
					if (dist == -1 || d0 < dist) {
						dist = d0;
						ret = dg;
					}
				}
			}
			return ret;
		}

		public static Door findDoor(string target, bool knownOnly = true) {
			foreach (Door d in DungeonManager.Instance.doors) {
				if (d.LabelSimple == target && (!knownOnly || d.corridor.isExplored))
					return d;
			}
			return null;
		}

		public static Room findRoom(string target, bool knownOnly = true) {
			foreach (Room d in DungeonManager.Instance.rooms) {
				if (d.LabelSimple == target && (!knownOnly || d.isExplored))
					return d;
			}
			return null;
		}

		public static T findTowableInRoom<T>(Room r, Predicate<T> condition) where T : class, ITowItem {
			foreach (ITowItem obj in TowManager.Instance.knownTowableItems) {
				if (obj is T drone && obj.getRoom() == r && (condition == null || condition.Invoke(drone)))
					return drone;
			}
			return null;
		}
	}
}
