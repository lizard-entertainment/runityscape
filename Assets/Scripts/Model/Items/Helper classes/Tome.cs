using System;
using System.Collections.Generic;
using Scripts.Model.Characters;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using Scripts.Game.Defined.Spells;

namespace Scripts.Model.Items {

    /// <summary>
    /// Consumable item that teaches a spellbook.
    /// </summary>
    /// <seealso cref="Scripts.Model.Items.ConsumableItem" />
    public abstract class Tome : ConsumableItem {
        private int levelRequirement;
        private SpellBook spellToTeach;

        public Tome(int levelRequirement, int basePrice, SpellBook spellToTeach)
            : base(basePrice,
                  TargetType.SINGLE_ALLY,
                  string.Format("Tome: {0}", spellToTeach.Name),
                  string.Format("<color=grey>Requires level {0}.</color>\nTeaches <color=cyan>{1}</color>\n{2}",
                      levelRequirement,
                      spellToTeach.Name,
                      spellToTeach.TextboxTooltip.Tooltip.Text)
                  ) {
            this.levelRequirement = levelRequirement;
            this.spellToTeach = spellToTeach;
        }

        public override IList<SpellEffect> GetEffects(Character caster, Character target) {
            return new SpellEffect[] { new LearnSpellEffect(target.Spells, spellToTeach) };
        }

        protected override bool IsMeetOtherRequirements(Character caster, Character target) {
            return base.IsMeetOtherRequirements(caster, target)
                && target.Stats.Level >= levelRequirement && LearnerHasResourcesNeededToCastSpell(target.Stats, spellToTeach);
        }

        private static bool LearnerHasResourcesNeededToCastSpell(Characters.Stats learner, SpellBook book) {
            foreach (StatType type in book.Costs.Keys) {
                if (!learner.HasStat(type)) {
                    return false;
                }
            }
            return true;
        }
    }
}