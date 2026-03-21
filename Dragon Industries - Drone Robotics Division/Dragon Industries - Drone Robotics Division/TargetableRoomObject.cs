using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace ReikaKalseki.DIDrones {
	public class TargetableRoomObject {

		public object roomObject {  get; private set; }

		public TargetableType type { get; private set; }

		public TargetableRoomObject(object o) {
			setObject(o);
		}

		public void setObject(object o) {
			if (o == null)
				return;
			roomObject = o;
			if (o is Drone)
				type = TargetableType.DRONE;
			else if (o is RoomItem)
				type = TargetableType.ROOMITEM;
			else if (o is Door)
				type = TargetableType.DOOR;
			else if (o is ShipUpgradeInGameObject)
				type = TargetableType.UPGRADE;
			else if (o is ITowItem)
				type = TargetableType.MISCTOW;
			else
				throw new Exception("Invalid, non-targetable object: " + o.GetType().Name);
		}

		public Collider collider {
			get {
				switch (type) {
					case TargetableType.DOOR:
						return ((Door)roomObject).corridor.GetComponent<Collider>();
					case TargetableType.ROOMITEM:
						return ((RoomItem)roomObject).GetComponent<Collider>();
					case TargetableType.DRONE:
						return ((Drone)roomObject).GetComponent<Collider>();
					case TargetableType.UPGRADE:
						return ((ShipUpgradeInGameObject)roomObject).GetComponent<Collider>();
					case TargetableType.MISCTOW:
						return ((ITowItem)roomObject).UnderlyingGameObject.GetComponent<Collider>();
					default:
						throw new Exception("Invalid object type!");
				}
			}
		}

		public Bounds proximityBounds {
			get {
				Bounds b = collider.bounds;
				b.Expand(Vector3.one * 0.3F);
				return b;
			}
		}

		public bool isDead {
			get {
				switch (type) {
					case TargetableType.DOOR:
						return ((Door)roomObject).IsDead;
					case TargetableType.ROOMITEM:
						return ((RoomItem)roomObject).IsDead;
					case TargetableType.DRONE:
						return ((Drone)roomObject).IsDead;
					case TargetableType.UPGRADE:
						return false;
					case TargetableType.MISCTOW:
						return false;
					default:
						throw new Exception("Invalid object type!");
				}
			}
		}

		public Room roomLocation {
			get {
				switch (type) {
					case TargetableType.DOOR:
						return ((Door)roomObject).CurrentRoom;
					case TargetableType.ROOMITEM:
						return ((RoomItem)roomObject).roomLocation;
					case TargetableType.DRONE:
						return ((Drone)roomObject).CurrentRoom;
					case TargetableType.UPGRADE:
						return ((ShipUpgradeInGameObject)roomObject).roomLocation;
					case TargetableType.MISCTOW:
						return ((ITowItem)roomObject).getRoom();
					default:
						throw new Exception("Invalid object type!");
				}
			}
		}

		public bool checkDroneBounds(Drone drone) {
			return proximityBounds.Intersects(drone.GetComponent<Collider>().bounds);
		}

		public bool checkAtElseNavToAndTryAgain(Drone drone, ExecutedCommand cmd) {
			if (checkDroneBounds(drone)) {
				return true;
			}
			else {
				navToAndRun(drone, cmd);
				return false;
			}
		}

		public void navToAndRun(Drone drone, ExecutedCommand cmd) {
			switch (type) {
				case TargetableType.DOOR:
					drone.NavigateToAndExecuteCommand(((Door)roomObject).corridor.gameObject, cmd, CollisionType.BoundsIntesect);
					break;
				case TargetableType.ROOMITEM:
					drone.NavigateToAndExecuteCommand((RoomItem)roomObject, cmd, CollisionType.BoundsIntesect);
					break;
				case TargetableType.DRONE:
				case TargetableType.UPGRADE:
				case TargetableType.MISCTOW:
					drone.NavigateToAndExecuteCommand(((ITowItem)roomObject).UnderlyingGameObject, cmd, CollisionType.BoundsIntesect);
					break;
			}
		}

		public enum TargetableType {
			DOOR,
			ROOMITEM,
			DRONE,
			UPGRADE,
			MISCTOW,
		}

	}
}
