using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DSMFramework.Modding;

namespace ReikaKalseki.DIDrones {
	public abstract class RefillableCustomDroneUpgrade : CustomDroneUpgrade, IStorageUpgrade {

		public RefillableCustomDroneUpgrade(DroneUpgradeDefinition def, ModDroneUpgradeContainer c) : base(def, c) {
			
		}

		public int Capacity { get { return ((RefillableContainer)container).refill.maximumCapacity; } }

		public int Quantity { get; internal set; }

		public void AddItem(int count) {
			Quantity = Math.Min(Quantity + count, Capacity);
		}

		public void OverrideQuantity(int qty) {
			Quantity = Math.Min(qty, Capacity);
		}
	}

	public class RefillableCustomDroneUpgradeContainer<T> : CustomDroneUpgradeContainer<T>, RefillableContainer where T : RefillableCustomDroneUpgrade {

		public readonly UpgradeRefillDefinition refill;

		public RefillableCustomDroneUpgradeContainer(UpgradeRefillDefinition def, string name, DroneUpgradeClass type, CustomCommandDefinition cmd, int cost, int modifier = 0, int duration = 0) : base(name, type, cmd, cost, modifier, duration) {
			refill = def;
		}

		UpgradeRefillDefinition RefillableContainer.refill { get { return this.refill; } }

		public override void register() {
			base.register();

			//Adds a modification for the modification menu allowing player to buy refills
			ModUpgradeManager.Manager.RegisterModificationFor(typeof(T), new RefillCustomUpgrade(refill));
		}
	}

	public interface RefillableContainer {

		UpgradeRefillDefinition refill { get; }

	}

	public class UpgradeRefillDefinition {

		public readonly int maximumCapacity;
		public readonly int increaseAmount;
		public readonly string unitName;

		public UpgradeRefillDefinition(int cap, int amt, string n) {
			maximumCapacity = cap;
			increaseAmount = amt;
			unitName = n;
		}
	}

	public sealed class RefillCustomUpgrade : BaseResupplyMod {
		
		public readonly UpgradeRefillDefinition definition;

		public RefillCustomUpgrade(UpgradeRefillDefinition def) {
			_name = string.Format("Add {0} {1}{2}", def.increaseAmount, def.unitName, def.increaseAmount == 1 ? "" : "s"); //Label of the modification in the menu
			definition = def;
		}

		public override int ResourceIncreaseValue { get { return definition.increaseAmount; } }

		public override string Description { get { return _name; } }

		public override int MaxAllowed { get { return ((IStorageUpgrade)_targetUpgrade).Capacity/ResourceIncreaseValue; } }

		public override IModification CopyModification() {
			RefillCustomUpgrade ret = new RefillCustomUpgrade(definition);
			ret.SetTarget(_targetUpgrade);
			return ret;
		}
		/*
		public static RefillCustomUpgrade autoConstruct<R>() where R : RefillCustomUpgrade {
				ConstructorInfo[] ctrs = GetType().GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
				RefillCustomUpgrade ret = null;
				foreach (ConstructorInfo ci in ctrs) {
					ParameterInfo[] args = ci.GetParameters();
					if (args.Length == 0)
						ret = (RefillCustomUpgrade)ci.Invoke(new object[0]);
					else if (args.Length == 1)
						ret = (RefillCustomUpgrade)ci.Invoke(new object[] { increaseAmount });
					else if (args.Length == 2)
						ret = (RefillCustomUpgrade)ci.Invoke(new object[] { increaseAmount, "" });
				}
				if (ret == null)
					throw new Exception("Upgrade refill '" + this + "' has no valid autoconstructors");
			}*/
		}
}
