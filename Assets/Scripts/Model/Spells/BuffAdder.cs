using Scripts.Game.Defined.Spells;
using Scripts.Model.Buffs;
using Scripts.Model.Characters;
using Scripts.Model.Pages;
using System;
using System.Collections.Generic;

namespace Scripts.Model.Spells {

    /// <summary>
    /// Convenience spellbook extension for creating SpellBooks
    /// that only add a buff.
    /// </summary>
    public abstract class BuffAdder : BasicSpellbook {
        private Buff dummy;

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="target">Type of targets</param>
        /// <param name="spell">Type of spell</param>
        /// <param name="dummy">Dummy buff to create text and copies</param>
        /// <param name="name">Spell name</param>
        /// <param name="priority">Priority of the spell</param>
        public BuffAdder(TargetType target, SpellType spell, Buff dummy, string name, PriorityType priority) : base(name, dummy.Sprite, target, spell, priority) {
            this.dummy = dummy;
        }

        /// <summary>
        /// Does the buff adding
        /// </summary>
        /// <returns>IList with neccessar spell effects for adding a buff</returns>
        protected sealed override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddBuff(
                    new BuffParams(caster.Stats, caster.Id),
                    target.Buffs,
                    Util.TypeToObject<Buff>(dummy.GetType()))
            };
        }

        protected sealed override string CreateDescriptionHelper() {
            return CreateBuffDescription(this.TargetType, dummy);
        }

        /// <summary>
        /// Used to create a descriptive buff tooltip.
        /// </summary>
        /// <param name="buff">Buff to create a description for.</param>
        /// <returns>Descriptive buff tooltip</returns>
        public static string CreateBuffDescription(TargetType target, Buff buff) {
            return string.Format(
                "{0} is affected by <color=cyan>{1}</color>.\n{2}\nLasts {3} turns.",
                target.Name,
                buff.Name,
                buff.Description,
                buff.DurationText
                );
        }
    }
}