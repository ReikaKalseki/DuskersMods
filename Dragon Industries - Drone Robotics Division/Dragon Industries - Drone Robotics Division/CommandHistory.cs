using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Messaging;

namespace ReikaKalseki.DIDrones {
	public class CommandHistory {

		private static readonly List<CommandHistoryItem> history = new List<CommandHistoryItem>();

		public static int currentShip { get; private set; }
		public static int currentCommandIndex { get; private set; }

		public static void addCommand(ExecutedCommand cmd) {
			CommandHistoryItem item = new CommandHistoryItem(cmd, history.Count, currentShip);
			currentCommandIndex = item.commandIndex;
			history.Add(item);
			if (item.commandID == "exit")
				currentShip++;
		}

		public static IList<CommandHistoryItem> getFullHistory() {
			return history.AsReadOnly();
		}

		public static IList<CommandHistoryItem> getHistoryOfShip(int ship) {
			return history.Where(h => h.shipIndex == ship).ToList().AsReadOnly();
		}

		public static IList<CommandHistoryItem> getCurrentShipHistory() {
			return getHistoryOfShip(currentShip);
		}

		public static CommandHistoryItem getNthFromLastCommand(int n, bool onShip = true) {
			IList<CommandHistoryItem> set = onShip ? getCurrentShipHistory() : history;
			return set.Count <= n ? null : set[set.Count - 1 - n];
		}

		public static CommandHistoryItem getLastCommand(bool onShip = true) {
			return getNthFromLastCommand(0, onShip);
		}

		// May return < N commands if there are fewer than that many that fit the criteria!
		public static IList<CommandHistoryItem> getLastNCommands(int n, bool onShip = true) {
			IList<CommandHistoryItem> set = onShip ? getCurrentShipHistory() : history;
			return set.getLastNItems(n).AsReadOnly();
		}

		public static CommandHistoryItem getLastUsage(string id, bool onShip = true) {
			IList<CommandHistoryItem> set = onShip ? getCurrentShipHistory() : history;
			return set.Reverse().Where(c => c.commandID == id).FirstOrDefault(null);
		}

		public class CommandHistoryItem {

			public readonly int commandIndex;
			public readonly int shipIndex;
			public readonly string commandID;
			public readonly IList<string> arguments;

			internal CommandHistoryItem(ExecutedCommand cmd, int i, int s) {
				commandID = cmd.Command.CommandName;
				arguments = cmd.Arguments.AsReadOnly();
				commandIndex = i;
				shipIndex = s;
			}

			public bool equivalent(ExecutedCommand cmd) {
				return commandID == cmd.Command.CommandName && arguments.SequenceEqual(cmd.Arguments);
			}
		}

	}
}
