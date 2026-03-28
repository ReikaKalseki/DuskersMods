using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using UnityEngine;
using JetBrains.Annotations;

namespace ReikaKalseki.DIDrones {
	public abstract class ApplyableDroneUpgrade : IModification {

		private static readonly HashSet<Type> registeredTypes = new HashSet<Type>();

		public NonVisualDrone _targetDrone { get; private set; }

		public readonly string name;
		public readonly string description;
		public readonly int cost;

		protected ApplyableDroneUpgrade(string name, string desc, int cost) {
			this.name = name;
			this.description = desc;
			this.cost = cost;
			Type t = GetType();
			if (!registeredTypes.Contains(t)) {
				DSUtil.log(string.Format("Registered one-off drone upgrade '{0}'='{1}'", t.Name, name));
				registeredTypes.Add(t);
			}
		}

		public string DisplayName {
			get {
				return name;
			}
		}

		public string Description {
			get {
				return description;
			}
		}

		public int ScrapCost {
			get {
				return -cost;
			}
		}

		public virtual int MaxAllowed {
			get {
				return 1;
			}
		}

		public virtual ModificationStorageIdEnum ModificationStorageId {
			get {
				return ModificationStorageIdEnum.None;
			}
		}

		public string TargetName {
			get {
				return ((IInventoryItem)_targetDrone).Name;
			}
		}

		public void SetTarget(object itemToReceiveMod) {
			_targetDrone = (itemToReceiveMod as NonVisualDrone);
		}

		public bool CanApplyModToTarget() {
			return _targetDrone != null && isValid(_targetDrone);
		}

		protected abstract bool isValid(NonVisualDrone drone);

		public void ApplyModToTarget() {
			if (_targetDrone == null) {
				return;
			}
			apply(_targetDrone);
		}

		protected abstract void apply(NonVisualDrone drone);

		public IModification CopyModification() {
			IModification ret = (ApplyableDroneUpgrade)Activator.CreateInstance(GetType());
			ret.SetTarget(_targetDrone);
			return ret;
		}
	}

}
