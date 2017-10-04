using System;
using System.Collections.Generic;
using Scripts.Model.Characters;
using Scripts.Model.Spells;
using Scripts.Game.Defined.Spells;

namespace Scripts.Model.Items {

    /// <summary>
    /// Increases inventory size.
    /// </summary>
    /// <seealso cref="Scripts.Model.Items.ConsumableItem" />
    public abstract class InventoryExpander : ConsumableItem {
        private int capacityIncreaseAmount;
        private int capacityIncreaseLimit;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryExpander"/> class.
        /// </summary>
        /// <param name="capacityIncreaseAmount">The capacity increase amount.</param>
        /// <param name="capacityIncreaseLimit">The capacity increase limit.</param>
        /// <param name="basePrice">The base price.</param>
        /// <param name="name">The name of the item.</param>
        public InventoryExpander(int capacityIncreaseAmount, int capacityIncreaseLimit, int basePrice, string name)
            : base(basePrice,
                  TargetType.SINGLE_ALLY,
                  name,
                  string.Format("Increases inventory capacity by {0}. Cannot increase capacity past {1}.",
                      capacityIncreaseAmount,
                      capacityIncreaseLimit)) {
            this.capacityIncreaseAmount = capacityIncreaseAmount;
            this.capacityIncreaseLimit = capacityIncreaseLimit;
        }

        public override IList<SpellEffect> GetEffects(Character caster, Character target) {
            return new SpellEffect[] {
                new IncreaseInventoryCapacityEffect(target.Inventory, capacityIncreaseAmount)
            };
        }

        protected override bool IsMeetOtherRequirements(Character caster, Character target) {
            return
                base.IsMeetOtherRequirements(caster, target)
                && (target.Inventory.Capacity + capacityIncreaseAmount) <= capacityIncreaseLimit;
        }
    }
}