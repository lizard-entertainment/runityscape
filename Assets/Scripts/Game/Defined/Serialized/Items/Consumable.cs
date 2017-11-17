using System;
using System.Collections.Generic;
using Scripts.Model.Items;
using Scripts.Model.Spells;
using Scripts.Game.Defined.Spells;
using Scripts.Model.Stats;
using Scripts.Model.Characters;
using Scripts.Model.Pages;

namespace Scripts.Game.Defined.Unserialized.Items {

    public abstract class RestoreItem : ConsumableItem {
        private readonly int restoreAmount;
        private readonly StatType stat;

        public RestoreItem(
            StatType stat,
            string name,
            string spriteLoc,
            string description,
            int basePrice,
            int restoreAmount,
            string extraText = "")
            : base(
                  Util.GetSprite(spriteLoc),
                  basePrice,
                  TargetType.ONE_ALLY,
                  name,
                  string.Format("{0} Restores {1} {2}.{3}",
                      description,
                      restoreAmount,
                      stat,
                      extraText
                      )) {
            this.restoreAmount = restoreAmount;
            this.stat = stat;
        }

        public sealed override IList<SpellEffect> GetEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, stat, restoreAmount)
            };
        }

        protected sealed override bool IsMeetOtherRequirements(Character caster, Character target) {
            return target.Stats.HasStat(stat)
                && target.Stats.GetMissingStatCount(stat) > 0
                && IsMeetRequirements(caster, target);
        }

        protected virtual bool IsMeetRequirements(Character caster, Character target) {
            return true;
        }
    }

    public abstract class HealingItem : RestoreItem {
        private readonly int healingAmount;
        private readonly bool canRevive;

        public HealingItem(
            string name,
            string spriteLoc,
            string description,
            int basePrice,
            int healingAmount,
            bool canRevive = false)
            : base(
                  StatType.HEALTH,
                  name,
                  spriteLoc,
                  description,
                  basePrice,
                  healingAmount,
                  canRevive ? " Can be used on fallen targets." : string.Empty) {
            this.healingAmount = healingAmount;
            this.canRevive = canRevive;
        }

        protected sealed override bool IsMeetRequirements(Character caster, Character target) {
            return canRevive || target.Stats.State == State.ALIVE;
        }
    }

    public abstract class ManaItem : RestoreItem {

        public ManaItem(
            string name,
            string spriteLoc,
            string description,
            int basePrice,
            int manaAmount
            )
            : base(StatType.MANA, name, spriteLoc, description, basePrice, manaAmount) { }
    }
}