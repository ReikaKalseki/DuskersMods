using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Duskers.EnemyStates;

using ReikaKalseki.DIDrones;

using UnityEngine;

namespace ReikaKalseki.TBE {
	internal class EventRateControlSystem {

		private static readonly Dictionary<string, int> commandCostTable = new Dictionary<string, int>{ //default not-specified is 1
			{ "alias", 0 }, //meta-commands, not in-universe actions
			{ "time", 0 },
			{ "help", 0 },
			{ "clear", 0 },

			{ "info", 0 }, //basic ship info commands
			{ "status", 0 },
			{ "flag", 0 },

			{ "degauss", 1 }, //simple in-world actions
			{ "interface", 1 }, //simple in-world actions
			{ "survey", 1 },
			{ "shipscan", 1 },
			
			//{ "defense", 2 }, //moderate actions

			{ "navigate", 0 }, //complex actions
			//do not penalize navigate or players will just drive by hand!
			//might be able to restore this if can add a penalty per unit time driving by hand

			{ "dock", 4 },
			{ "teleport", 5 },
		};

		private static readonly HashSet<string> freeRepeats = new HashSet<string>{
			"degauss",
			"flag",
			//"generator",
			"interface",
			"scan",
			"shipscan",
			"survey",
		};

		public static int commandsSinceEventProgression { get; private set; }
		public static int commandsSinceTransporterProgression { get; private set; }
		public static int commandsSinceDoorChewProgression { get; private set; }

		public static float currentBonusFactor { get; private set; }

		private static readonly Dictionary<TransporterShipUpgrade.ReceiverData, float> lastTransporterMessage = new Dictionary<TransporterShipUpgrade.ReceiverData, float>();

		static EventRateControlSystem() {
			randomizeBonus();
		}

		public static void incrementCommandCounter(ExecutedCommand cmd) {
			string id = cmd.Command.CommandName;
			if (cmd.Handled) {
				CommandHistory.CommandHistoryItem previous = CommandHistory.getNthFromLastCommand(1); //not immediate last, since that is this one
				if (previous != null && previous.equivalent(cmd) && freeRepeats.Contains(id)) //some actions are free for infinite repeat
					return;
				int cost = commandCostTable.ContainsKey(id) ? commandCostTable[id] : 1;
				commandsSinceEventProgression += cost;
				commandsSinceTransporterProgression += cost;
				DSUtil.log(string.Format("Processing command type '{0}' (+{1}), now {2}/{3} since last progression", id, cost, commandsSinceEventProgression, commandsSinceTransporterProgression));
			}
		}

		public static void baseEventTick(BaseGameEvent evt) {
			if (!GlobalSettings.MissionStarted || GlobalSettings.IsGamePaused || GlobalSettings.GameIsOver) {
				return;
			}

			if (evt is CloseCommandEvent) {
				tickCloseCommand((CloseCommandEvent)evt);
			}
			evt.eventTimerCurrent += Time.deltaTime;
			if (evt.isCoolingDown) {
				if (evt.eventTimerCurrent > evt.Cooldown) {
					evt.isCoolingDown = false;
					evt.eventTimerCurrent = 0f;
				}
			}
			else if (evt.eventTimerCurrent > evt.CheckFrequency) {
				evt.eventTimerCurrent = 0f;
				if (evt.rnd.NextFloat(0f, 1f) < evt.Probability) {
					evt.ExecuteEvent();
					if (!evt.OneTimeEvent) {
						evt.isCoolingDown = true;
					}
					else {
						GameEventManager.Instance.RemoveEvent(evt);
					}
				}
			}
		}

		private static int getBaseCommandThreshold(TBConfig.ConfigEntries cfg) {
			return TBEMod.instance.config.getInt(cfg);
		}

		private static bool tryAdvanceEvent(BaseGameEvent evt, int necessaryCommands) {
			return tryAdvanceEvent(evt.GetType().Name, necessaryCommands);
		}

		private static bool tryAdvanceEvent(string evtn, int necessaryCommands) {
			int minflat = TBEMod.instance.config.getInt(TBConfig.ConfigEntries.MIN_EXTRA_MOVES_FLAT);
			int maxflat = TBEMod.instance.config.getInt(TBConfig.ConfigEntries.MAX_EXTRA_MOVES_FLAT);
			float minpct = TBEMod.instance.config.getFloat(TBConfig.ConfigEntries.MIN_EXTRA_MOVES_PCT);
			float maxpct = TBEMod.instance.config.getFloat(TBConfig.ConfigEntries.MAX_EXTRA_MOVES_PCT);
			int bonusFlat = Mathf.RoundToInt(Mathf.Lerp(minflat, maxflat, currentBonusFactor));
			float bonusPct = Mathf.Lerp(minpct, maxpct, currentBonusFactor);
			int bonusNet = Mathf.Max(bonusFlat, Mathf.RoundToInt(bonusPct/100F*necessaryCommands));
			int needed = necessaryCommands+bonusNet;
			needed = (int)(needed * TBEMod.instance.config.getFloat(TBConfig.ConfigEntries.THRESHOLD_SCALAR));
			needed = (int)(needed * TBEMod.instance.config.getFloat(TBConfig.ConfigEntries.EVENT_SCALAR));
			string frac = string.Format("{0}/[{1} (@ {2} -> +{3}={4})]", commandsSinceEventProgression, necessaryCommands, currentBonusFactor, bonusNet, needed);
			if (commandsSinceEventProgression < needed) {
				DSUtil.log("Event " + evtn + " failed to progress because command threshold not met: " + frac);
				return false;
			}
			DSUtil.log("Event " + evtn + " progressed at command threshold " + frac);
			randomizeBonus();
			commandsSinceEventProgression = 0;
			return true;
		}

		private static bool tryAdvanceTransporter(TransporterShipUpgrade.ReceiverData data) {
			int needed = Mathf.RoundToInt(data.delayUntilEventChange/20); //one move per 30s of original delay, typically [120-300] or [240-600] -> [4-10] or [8-20]
			needed = (int)(needed * TBEMod.instance.config.getFloat(TBConfig.ConfigEntries.THRESHOLD_SCALAR));
			needed = (int)(needed * TBEMod.instance.config.getFloat(TBConfig.ConfigEntries.TRANSPORTER_SCALAR));
			string frac = string.Format("{0}/{1}", commandsSinceTransporterProgression, needed);
			if (commandsSinceTransporterProgression < needed) {
				float time = Time.time;
				float last = lastTransporterMessage.ContainsKey(data) ? lastTransporterMessage[data] : -1;
				if (time - last > 1) {
					DSUtil.log("Transporter link with room "+data.receiver.roomLocation.Label+" failed to progress because command threshold not met: " + frac);
					lastTransporterMessage[data] = time;
				}
				return false;
			}
			DSUtil.log("Transporter link with room "+data.receiver.roomLocation.Label+" progressed at command threshold " + frac);
			commandsSinceTransporterProgression = 0;
			return true;
		}

		private static bool tryAdvanceDoorChew(BaseEnemyState state, int needed) {
			needed = (int)(needed * TBEMod.instance.config.getFloat(TBConfig.ConfigEntries.THRESHOLD_SCALAR));
			needed = (int)(needed * TBEMod.instance.config.getFloat(TBConfig.ConfigEntries.SWARM_SCALAR));
			string frac = string.Format("{0}/{1}", commandsSinceDoorChewProgression, needed);
			string loc = string.Format("{0} in room {1}", state._brain.ThisEnemy.GetType().Name, state._brain.ThisEnemy.CurrentRoom.Label);
			if (commandsSinceDoorChewProgression < needed) {
				DSUtil.log("Door chew for " + loc + " failed to progress because command threshold not met: " + frac);
				return false;
			}
			DSUtil.log("Door chew for " + loc + " progressed at command threshold " + frac);
			commandsSinceDoorChewProgression = 0;
			return true;
		}

		private static void randomizeBonus() {
			currentBonusFactor = UnityEngine.Random.Range(0F, 1F);
			DSUtil.log("New bonus-move factor: " + (currentBonusFactor * 100).ToString("0.0") + "%");
		}

		public static void tickAirlockSeal(AirlockSealFailEvent evt) {
			if (evt.processingAirlocks != null) {
				int count = evt.processingAirlocks.Count;
				for (int i = count - 1; i >= 0; i--) {
					AirlockSealFailEvent.BreakingAirlock breakingAirlock = evt.processingAirlocks[i];
					if (breakingAirlock.isBreaking) {
						if (breakingAirlock.hasShownSecondWarning) {
							if (tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.IMMINENT_AIRLOCK_FAILURE))) {
								breakingAirlock.airlock.door.EndSealFailureVisual();
								breakingAirlock.airlock.door.TakeDamage(1000f, DamageType.Physical, null);
								if (breakingAirlock.airlock.onSchematic) {
									breakingAirlock.airlock.UpdateCameraView(true);
									breakingAirlock.airlock.droneUIObject.UpdateCameraView();
								}
								if (breakingAirlock.airlock.door.onSchematic) {
									SystemMessageManager.ShowSystemMessage(string.Format("'{0}' airlock seal failed!", breakingAirlock.airlock.door.Label), ConsoleMessageType.Warning);
								}
								else {
									SystemMessageManager.ShowSystemMessage("'unknown' airlock seal failed!", ConsoleMessageType.Warning);
								}
								evt.processingAirlocks.RemoveAt(i);
								if (evt.processingAirlocks.Count == 0) {
									evt.processingAirlocks = null;
								}
							}
						}
						else if (BoardingShip.Instance.CurrentAirlock == breakingAirlock.airlock) {
							breakingAirlock.isBreaking = false;
							breakingAirlock.isPendingRestartEvent = false;
							breakingAirlock.airlock.door.EndSealFailureVisual();
							if (breakingAirlock.airlock.door.onSchematic) {
								SystemMessageManager.ShowSystemMessage(string.Format("Airlock '{0}': seal stabilized", breakingAirlock.airlock.door.Label), ConsoleMessageType.Benefit);
							}
							else {
								SystemMessageManager.ShowSystemMessage("Airlock 'unknown': seal stabilized", ConsoleMessageType.Benefit);
							}
						}
						else {
							if (tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.AIRLOCK_FAILURE_SECOND_STAGE))) {
								breakingAirlock.hasShownSecondWarning = true;
								if (breakingAirlock.airlock.door.onSchematic) {
									SystemMessageManager.ShowSystemMessage(string.Format("Airlock '{0}': seal failure imminent!", breakingAirlock.airlock.door.Label), ConsoleMessageType.Warning);
								}
								else {
									SystemMessageManager.ShowSystemMessage("Airlock 'unknown': seal failure imminent!", ConsoleMessageType.Warning);
								}
							}
						}
					}
					else if (breakingAirlock.isPendingRestartEvent) {
						if (tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.AIRLOCK_RETRY_FAILURE))) {
							breakingAirlock.isPendingRestartEvent = false;
							breakingAirlock.isBreaking = true;
							breakingAirlock.airlock.door.BeginSealFailureVisual();
							breakingAirlock.hasShownSecondWarning = false;
							breakingAirlock.timerToBreak = 60f;
							if (breakingAirlock.airlock.door.onSchematic) {
								SystemMessageManager.ShowSystemMessage(string.Format("Airlock '{0}': seal integrity failing once more.", breakingAirlock.airlock.door.Label), ConsoleMessageType.Warning);
							}
							else {
								SystemMessageManager.ShowSystemMessage("Airlock 'unknown': seal integrity failing once more.", ConsoleMessageType.Warning);
							}
						}
					}
					else if (BoardingShip.Instance.CurrentAirlock != breakingAirlock.airlock) {
						breakingAirlock.isPendingRestartEvent = true;
						breakingAirlock.timerToRestartEvent = evt.rnd.NextFloat(40f, 60f);
					}
				}
			}
			baseEventTick(evt);
		}

		public static void performAirlockSeal(AirlockSealFailEvent evt) {
			Corridor corridor = null;
			int num = 0;
			do {
				int num2 = evt.rnd.Next(0, DungeonManager.Instance.corridors.Length);
				Corridor corridor2 = DungeonManager.Instance.corridors[num2];
				if (corridor2.IsAirlock && BoardingShip.Instance.CurrentAirlock != corridor2 && !corridor2.door.IsDead && !corridor2.door.IsDisconnected) {
					corridor = corridor2;
				}
				num++;
			}
			while (corridor == null && num < 100);

			if (corridor != null && tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.QUEUED_AIRLOCK_FAILURE))) {
				if (evt.processingAirlocks == null) {
					evt.processingAirlocks = new List<AirlockSealFailEvent.BreakingAirlock>();
				}
				evt.processingAirlocks.Add(new AirlockSealFailEvent.BreakingAirlock {
					airlock = corridor,
					timerToBreak = 60f,
					timerToRestartEvent = 0f,
					isBreaking = true,
					isPendingRestartEvent = false
				});
				corridor.door.BeginSealFailureVisual();
				if (corridor.door.onSchematic) {
					SystemMessageManager.ShowSystemMessage(string.Format("Airlock '{0}': seal integrity failing.", corridor.door.Label), ConsoleMessageType.Warning);
				}
				else {
					SystemMessageManager.ShowSystemMessage("Airlock 'unknown': seal integrity failing.", ConsoleMessageType.Warning);
				}
			}
		}

		public static void tickAsteroids(AsteroidEvent evt) {
			if (evt.asteroidGroupList != null && evt.asteroidGroupList.Count > 0) {
				int count = evt.asteroidGroupList.Count;
				for (int i = count - 1; i >= 0; i--) {
					AsteroidEvent.SingleAsteroidEvent singleAsteroidEvent = evt.asteroidGroupList[i];
					if (singleAsteroidEvent.warningFired) {
						if (tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.ASTEROID_HIT))) {
							evt.asteroidGroupList.RemoveAt(i);
							bool flag = false;
							int count2 = singleAsteroidEvent.potRoomList.Count;
							List<int> list = new List<int>();
							for (int j = 0; j < count2; j++) {
								List<float> potPerList;
								List<float> list2 = potPerList = singleAsteroidEvent.potPerList;
								int index2;
								int index = index2 = j;
								float num = potPerList[index2];
								list2[index] = num + evt.rnd.NextFloat(0f, 0.9f);
								if (singleAsteroidEvent.potPerList[j] >= 1f) {
									list.Add(j);
								}
							}
							if (list.Count > 0) {
								foreach (int num2 in list) {
									if (evt.rnd.Next(0, 100) > 0) {
										if (!singleAsteroidEvent.potRoomList[num2].boardingVessel || !(((BoardingShip)singleAsteroidEvent.potRoomList[num2]).CurrentAirlock != singleAsteroidEvent.dockedCorridor)) {
											flag = true;
											SystemMessageManager.ShowSystemMessage(string.Format("Room {0} hit by asteroid", singleAsteroidEvent.potRoomList[num2].Label), ConsoleMessageType.Warning);
											GameAudio.Play2DSFX(GameAudio.SoundEnum.AsteroidHit);
											if (singleAsteroidEvent.potRoomList.Count > num2 && singleAsteroidEvent.potRoomList[num2] != null) {
												singleAsteroidEvent.potRoomList[num2].DestroyByImpact("due to asteroid collision", 50, 25, 30);
											}
										}
									}
								}
							}
							if (!flag) {
								SystemMessageManager.ShowSystemMessage(string.Format("Asteroids missed the derelict", new object[0]), ConsoleMessageType.Healthy);
							}
						}
					}
					else if (tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.ASTEROID_WARNING) + (int)(singleAsteroidEvent.timerIncomming / 30F))) { //event originally has a 90s-8min timer, apply some of that -> add 3-16 moves
						singleAsteroidEvent.warningFired = true;
						singleAsteroidEvent.AdjustProbabilities();
						//singleAsteroidEvent.DisplayProbabilities();
						displayAsteroidWarning(singleAsteroidEvent);
					}
				}
			}
			baseEventTick(evt);
		}

		public static void performAsteroids(AsteroidEvent evt) {
			if (!tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.ASTEROID_QUEUE)))
				return;
			AsteroidEvent.SingleAsteroidEvent singleAsteroidEvent = new AsteroidEvent.SingleAsteroidEvent(evt.rnd)
		{
				timerIncomming = evt.rnd.NextFloat(90f, 480f)
			};
			DungeonManager instance = DungeonManager.Instance;
			int num = evt.rnd.Next(1, instance.rooms.Length);
			Room room = instance.rooms[num];
			singleAsteroidEvent.potRoomList = new List<Room>();
			singleAsteroidEvent.potRoomList.Add(room);
			int num2 = evt.rnd.Next(0, 4);
			if (num2 > 0) {
				List<Room> adjacentRooms = room.getAdjacentRooms();
				if (adjacentRooms.Count > 0) {
					for (int i = 0; i < num2; i++) {
						int num3 = 0;
						Room room2;
						do {
							int index = evt.rnd.Next(0, adjacentRooms.Count);
							room2 = adjacentRooms[index];
							num3++;
						}
						while (num3 < 100 && room2 != null && singleAsteroidEvent.potRoomList.Contains(room2) && !(room2 is BoardingShip));
						if (num3 >= 100) {
							break;
						}
						singleAsteroidEvent.potRoomList.Add(room2);
					}
				}
			}
			int count = singleAsteroidEvent.potRoomList.Count;
			for (int j = 0; j < count; j++) {
				Room room3 = singleAsteroidEvent.potRoomList[j];
				if (room3 != null && room3.boardingVessel) {
					singleAsteroidEvent.dockedCorridor = ((BoardingShip)room3).CurrentAirlock;
					break;
				}
			}
			singleAsteroidEvent.CalculateProbabilities();
			//singleAsteroidEvent.DisplayProbabilities();
			displayAsteroidWarning(singleAsteroidEvent);
			if (evt.asteroidGroupList == null) {
				evt.asteroidGroupList = new List<AsteroidEvent.SingleAsteroidEvent>();
			}
			evt.asteroidGroupList.Add(singleAsteroidEvent);
		}

		public static void displayAsteroidWarning(AsteroidEvent.SingleAsteroidEvent evt) {
			float num = evt.timerIncomming / 30f; //range is 90-480 -> num 3-16
			string warning = "undefined";
			if (num <= 5) { //3-5
				warning = "Immediate";
			}
			else if (num <= 9) { //6-9
				warning = "Short";
			}
			else if (num <= 13) { // 10-13
				warning = "Moderate";
			}
			else { //14-16
				warning = "Long";
			}
			SystemMessageManager.ShowSystemMessage($"Incoming asteroid detected at {warning} range", ConsoleMessageType.Warning);
			int count = evt.potRoomList.Count;
			for (int i = 0; i < count; i++) {
				if (evt.potRoomList[i] != null) {
					if (evt.potRoomList[i].boardingVessel && ((BoardingShip)evt.potRoomList[i]).CurrentAirlock != evt.dockedCorridor) {
						DungeonManager.Instance.SendConsoleMessage($"\tBoarding ship has moved out of range of incomming asteroids", ConsoleMessageType.Info);
					}
					else {
						DungeonManager.Instance.SendConsoleMessage($"\tRoom {evt.potRoomList[i].Label} has a {evt.potPerList[i],4:P0} chance of being hit", ConsoleMessageType.Info);
					}
				}
			}
		}

		public static string warnedCloseFailure = "diclosewarnplaceholder";

		//this is called not with a transpiler but with direct code in the base update above
		public static void tickCloseCommand(CloseCommandEvent evt) {
			if (GlobalSettings.CrippledCommandList != null && GlobalSettings.CrippledCommandList.Contains(warnedCloseFailure)) {
				if (tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.CLOSE_FAILURE))) {
					GlobalSettings.CrippledCommandList.Remove(warnedCloseFailure);
					GlobalSettings.CrippledCommandList.Add("close");
					SystemMessageManager.ShowSystemMessage("Derelict no longer responding to 'close' command.", ConsoleMessageType.Error);
				}
			}
		}

		public static void performCloseCommandEvent(CloseCommandEvent evt) {
			if (GlobalSettings.CrippledCommandList == null) {
				GlobalSettings.CrippledCommandList = new List<string>();
			}
			if (GlobalSettings.CrippledCommandList.Contains(warnedCloseFailure))
				return;
			if (tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.CLOSE_FAILURE_WARN))) {
				GlobalSettings.CrippledCommandList.Add(warnedCloseFailure);
				SystemMessageManager.ShowSystemMessage("Corruption detected in derelict command registry: 'close' command reliability impacted.", ConsoleMessageType.Warning);
			}
		}

		public static void performDoorFailEvent(DoorFailEvent evt) {
			DungeonManager instance = DungeonManager.Instance;
			Door door = null;
			int num = 0;
			do {
				int num2 = evt.rnd.Next(0, instance.doors.Length);
				Door door2 = instance.doors[num2];
				if (!door2.IsDead && !door2.IsDisconnected && (!door2.corridor.IsAirlock || door2.corridor != BoardingShip.Instance.CurrentAirlock)) {
					door = door2;
				}
				num++;
			}
			while (door == null && num < 100);
			if (door != null && tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.DOOR_FAILURE))) {
				door.DisconnectDoor();
				SystemMessageManager.ShowSystemMessage(string.Format("System error: door '{0}' no longer responding", door.Label), ConsoleMessageType.Error);
			}
		}

		public static void performPipeBreakEvent(RoomDestroyEvent evt) {
			Room room = null;
			int num = 0;
			while (room == null && num < 100) {
				List<Room> list;
				if (evt.yellowRooms == null || evt.rnd.Next(0, 100) >= evt.chanceOfPickingYellowRoom) {
					list = evt.redRooms;
				}
				else {
					list = evt.yellowRooms;
				}
				int index = evt.rnd.Next(0, list.Count);
				Room room2 = list[index];
				if (room2.onSchematic && !room2.boardingVessel && !room2.IsFillingWithRadiation && !room2.IsRadiated) {
					room = room2;
				}
				num++;
			}
			if (room != null && tryAdvanceEvent(evt, getBaseCommandThreshold(TBConfig.ConfigEntries.RADIATION_PIPE_GROAN))) {
				//redirect room.NaturalRadiateEvent();
				SystemMessageManager.ShowSystemMessage(string.Format("Overpressure in reactor coolant pipe in room '{0}'", room.Label), ConsoleMessageType.Warning);
				setupPipeBreakInRoom(room);
			}
		}

		public static void setupPipeBreakInRoom(Room r) {
			if (!r.IsFillingWithRadiation && !r.IsRadiated && !r.IsInPreNaturalRadiationState) {
				r.IsInPreNaturalRadiationState = true;
				r.isPendingMothershipCreak = true;
				r.preNaturalRadiationTimer = UnityEngine.Random.Range(15f, 30f);
				r.mothershipCreakTimer = 4f;
				if (UnityEngine.Random.Range(0, 100) < 10) {
					r.willNaturalRadiationFail = true;
				}
				if (GlobalSettings.cameraMode == CameraMode.Drone) {
					DroneManager.Instance.PlayDroneShipCreak();
				}
			}
		}

		public static void radiateRoomRedirect(Room r, string msg) {
			if (tryAdvanceEvent("RadiateRoom", getBaseCommandThreshold(TBConfig.ConfigEntries.RADIATION_PIPE_BREAK)))
				r.Radiate(msg);
			else
				r.IsInPreNaturalRadiationState = true; //re-set state and try again next time
		}

		public static void tickTransporterUpgrade(TransporterShipUpgrade upg) {
			if (upg.isProcessingDelayedJump) {
				upg.delayBeforeJump -= Time.deltaTime;
				if (upg.delayBeforeJump <= 0f) {
					foreach (Drone drone in upg.delayedTransportDroneList) {
						upg.JumpDroneToRoom(drone, upg.delayedTransportDestinationRoom);
					}
					upg.isProcessingDelayedJump = false;
					upg.delayedTransportDestinationRoom = null;
					upg.delayedTransportDroneList.Clear();
				}
			}
			if (GlobalSettings.MissionStarted && !GlobalSettings.IsGamePaused && upg.CanTransport) {
				int count = upg.receiverDataList.Count;
				for (int i = 0; i < count; i++) {
					TransporterShipUpgrade.ReceiverData receiverData = upg.receiverDataList[i];
					//receiverData.delayUntilEventChange -= Time.deltaTime;
					if (!receiverData.receiver.IsOffline) {
						switch (receiverData.CurrentStrength) {
							case TransporterShipUpgrade.ReceiverStrengthEnum.Strong:
								if (tryAdvanceTransporter(receiverData)) {
									receiverData.delayUntilEventChange = UnityEngine.Random.Range(120f, 300f) * upg.strengthErraticFactor;
									receiverData.CurrentStrength = TransporterShipUpgrade.ReceiverStrengthEnum.Weak;
									receiverData.receiver.SetDamaged();
									receiverData.receiver.roomLocation.UpdateTransporterReceiver(TransporterShipUpgrade.ReceiverStrengthEnum.Weak);
									SystemMessageManager.ShowSystemMessage("Transporter receiver signal in room " + receiverData.receiver.roomLocation.Label + " is weak.", ConsoleMessageType.Warning);
								}
								break;
							case TransporterShipUpgrade.ReceiverStrengthEnum.Weak:
								if (tryAdvanceTransporter(receiverData)) {
									if (UnityEngine.Random.Range(0, 4) == 0) {
										receiverData.CurrentStrength = TransporterShipUpgrade.ReceiverStrengthEnum.Strong;
										receiverData.delayUntilEventChange = UnityEngine.Random.Range(240f, 600f) * upg.strengthErraticFactor;
										receiverData.receiver.SetActive();
										receiverData.receiver.roomLocation.UpdateTransporterReceiver(TransporterShipUpgrade.ReceiverStrengthEnum.Strong);
										SystemMessageManager.ShowSystemMessage("Transporter receiver signal in room " + receiverData.receiver.roomLocation.Label + " is strong.", ConsoleMessageType.Notification);
									}
									else {
										receiverData.CurrentStrength = TransporterShipUpgrade.ReceiverStrengthEnum.None;
										receiverData.delayUntilEventChange = UnityEngine.Random.Range(120f, 360f);
										receiverData.receiver.IsResponding = false;
										upg.ReceiverWentOffline();
										receiverData.receiver.roomLocation.UpdateTransporterReceiver(TransporterShipUpgrade.ReceiverStrengthEnum.None);
										SystemMessageManager.ShowSystemMessage("Lost transporter receiver signal in room " + receiverData.receiver.roomLocation.Label + ".", ConsoleMessageType.Warning);
									}
								}
								break;
							case TransporterShipUpgrade.ReceiverStrengthEnum.None:
								if (tryAdvanceTransporter(receiverData)) {
									if (UnityEngine.Random.Range(0, 4) == 0) {
										receiverData.CurrentStrength = TransporterShipUpgrade.ReceiverStrengthEnum.Weak;
										receiverData.delayUntilEventChange = UnityEngine.Random.Range(120f, 300f) * upg.strengthErraticFactor;
										receiverData.receiver.SetDamaged();
										receiverData.receiver.roomLocation.UpdateTransporterReceiver(TransporterShipUpgrade.ReceiverStrengthEnum.Weak);
										SystemMessageManager.ShowSystemMessage("Reacquired a weak transporter receiver signal in room " + receiverData.receiver.roomLocation.Label + ".", ConsoleMessageType.Benefit);
										receiverData.receiver.IsResponding = true;
										upg.ReceiverCameOnline();
									}
									else {
										receiverData.CurrentStrength = TransporterShipUpgrade.ReceiverStrengthEnum.None;
										receiverData.receiver.roomLocation.UpdateTransporterReceiver(TransporterShipUpgrade.ReceiverStrengthEnum.None);
										receiverData.delayUntilEventChange = UnityEngine.Random.Range(120f, 360f) * upg.strengthErraticFactor;
									}
								}
								break;
						}
					}
					else if (tryAdvanceTransporter(receiverData)) {
						receiverData.receiver.BringOnline();
						receiverData.receiver.roomLocation.ExternallyMarkAsOnSchematic();
						receiverData.CurrentStrength = TransporterShipUpgrade.ReceiverStrengthEnum.Weak;
						receiverData.receiver.roomLocation.UpdateTransporterReceiver(TransporterShipUpgrade.ReceiverStrengthEnum.Weak);
						receiverData.delayUntilEventChange = UnityEngine.Random.Range(120f, 300f) * upg.strengthErraticFactor;
						receiverData.receiver.SetDamaged();
						receiverData.receiver.RefreshIcon();
						SystemMessageManager.ShowSystemMessage("New transporter receiver signal in room " + receiverData.receiver.roomLocation.Label + " detected.", ConsoleMessageType.Notification);
						upg.ReceiverCameOnline();
					}
				}
				if (upg.isDeadAir) {
					upg.timerForceReceiverOnline -= Time.deltaTime;
					if (upg.timerForceReceiverOnline <= 0f) {
						Debug.Log("******** Forced Receiver Online ********");
						int index = UnityEngine.Random.Range(0, upg.receiverDataList.Count);
						TransporterShipUpgrade.ReceiverData receiverData2 = upg.receiverDataList[index];
						receiverData2.CurrentStrength = TransporterShipUpgrade.ReceiverStrengthEnum.Weak;
						receiverData2.receiver.roomLocation.UpdateTransporterReceiver(TransporterShipUpgrade.ReceiverStrengthEnum.Weak);
						receiverData2.delayUntilEventChange = UnityEngine.Random.Range(120f, 300f) * upg.strengthErraticFactor;
						receiverData2.receiver.SetDamaged();
						SystemMessageManager.ShowSystemMessage("Reacquired a weak transporter receiver signal in room " + receiverData2.receiver.roomLocation.Label + ".", ConsoleMessageType.Benefit);
						receiverData2.receiver.IsResponding = true;
						upg.ReceiverCameOnline();
					}
				}
			}
			if (!GlobalSettings.IsGamePaused && !upg.canJump) {
				upg.timeTilNextJump -= Time.deltaTime;
				if (upg.timeTilNextJump <= 0f) {
					upg.timeTilNextJump = 0f;
					upg.canJump = true;
				}
			}
			if (upg.failSafe) {
				upg.timerLastAttempt -= Time.deltaTime;
			}
		}

		public static Door checkForDoorToChew(StatePatrolIdle state) {
			if (!tryAdvanceDoorChew(state, getBaseCommandThreshold(TBConfig.ConfigEntries.CHEW_START)))
				return null;
			Door d = state.CheckForDoorToChewGeneral();
			if (d != null && !d.corridor.IsAirlock)
				ConsoleWindow3.SendConsoleResponse(state._brain.ThisEnemy.GetType().Name+" has begun targeting door "+d.Label, ConsoleMessageType.Warning);
			return d;
		}

		public static void dealDoorChewDamage(Door d, float amt, DamageType type, ICombatTarget dealer, StatePatrolChewDoor state) {
			if (amt >= d.CurrentHitPoints && !tryAdvanceDoorChew(state, getBaseCommandThreshold(TBConfig.ConfigEntries.CHEW_BREAK)))
				return;
			d.TakeDamage(amt, type, dealer);
		}
	}
}
