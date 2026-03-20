using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

using UnityEngine;

using DSMFramework;
using DSMFramework.Modding;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ReikaKalseki.DIDrones {

	public abstract class CustomDroneUpgrade : BaseDroneUpgrade {

		protected readonly ModDroneUpgradeContainer container;

		protected CustomDroneUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def) {
			container = c;
		}

		public override sealed string CommandValue { get { return ((CommandableContainer)container).commandData.CommandName; } }

		public sealed override List<CommandDefinition> QueryAvailableCommands() {
			return container.GetCommandDefinitions();
		}

		protected abstract bool performAction(ExecutedCommand cmd);

		public sealed override void ExecuteCommand(ExecutedCommand cmd, bool partOfMultiCommand) {
			var commandName = cmd.Command.CommandName;
			if (commandName != CommandValue)
				return;
			CustomCommandDefinition cmdRef = ((CommandableContainer)container).commandData;
			string err = cmdRef.validateArguments(cmd.Arguments);
			if (err != null) {
				SendConsoleResponseMessage("Invalid parameters. Ex: " + ((CommandableContainer)container).commandData.Example, ConsoleMessageType.Warning);
				return;
			}

			if (this is RefillableCustomDroneUpgrade r) {
				if (r.Quantity <= 0 || !r.UpgradeUsed()) {
					SendConsoleResponseMessage("No "+ ((RefillableContainer)container).refill.unitName + "s available!", ConsoleMessageType.Warning);
					return;
				}
			}

			cmd.Handled = true;

			if (performAction(cmd)) {
				UsedThisMission = true;
				if (this is RefillableCustomDroneUpgrade rg)
					rg.Quantity--;
				DroneManager.Instance.currentDronePanel.UpgradesChanged = true;
			}
		}
	}

	public interface CommandableContainer {

		CustomCommandDefinition commandData { get; }

	}

	public class CustomDroneUpgradeContainer<T> : ModDroneUpgradeContainer, CommandableContainer where T : CustomDroneUpgrade {

		public readonly CustomCommandDefinition commandData;

		CustomCommandDefinition CommandableContainer.commandData { get { return this.commandData; } }

		public CustomDroneUpgradeContainer(string name, DroneUpgradeClass type, CustomCommandDefinition cmd, int cost, int modifier = 0, int duration = 0) : base(name, cmd.Description, cost, modifier, duration, type) {
			commandData = cmd;
		}

		public virtual void register() {
			//Registers our upgrade in the game
			ModUpgradeManager.Manager.RegisterDroneUpgrade(this);
			DSUtil.log("Added custom drone upgrade '"+ GetType().Name+"' ("+(this is RefillableContainer) +")");
		}

		public sealed override BaseDroneUpgrade MakeUpgrade() {
			//Example all implementations should follow for creating a new upgrade instance
			//DSUtil.log("Recorded MakeUpgrade() call for "+this.GetType().Name+" from\n"+ new StackTrace().GetFrames().getTrace());
			ConstructorInfo ctr = typeof(T).GetConstructor(new Type[]{typeof(DroneUpgradeDefinition), typeof(ModDroneUpgradeContainer) });
			if (ctr == null)
				throw new Exception("Drone upgrade '" + Name + "' has no valid constructor");

			return (T)ctr.Invoke(new object[] { MyDefinition, this });
			//return (T)Activator.CreateInstance(typeof(T), new object[] { MyDefinition, this });
		}

		public sealed override List<CommandDefinition> GetCommandDefinitions() {
			return new List<CommandDefinition> { commandData };
		}
	}
}
