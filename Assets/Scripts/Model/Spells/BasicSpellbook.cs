using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using UnityEngine;

namespace Scripts.Model.Spells {

    /// <summary>
    /// A simple spellbook abstraction.
    /// Most spellbooks can't target dead people
    /// So this implementation pre-overides these methods.
    /// </summary>
    public abstract class BasicSpellbook : SpellBook {
        private readonly bool canTargetDead;

        /// <summary>
        /// Normal priority constructor
        /// </summary>
        /// <param name="spellName">Name of the spell</param>
        /// <param name="sprite">Sprite of the spell</param>
        /// <param name="target">What kind of targets this spell can be used on</param>
        /// <param name="spell">What type of spell is this</param>
        public BasicSpellbook(string spellName, Sprite sprite, TargetType target, SpellType spell) : this(spellName, sprite, target, spell, PriorityType.NORMAL, false) { }

        /// <summary>
        /// Priority constructor
        /// </summary>
        /// <param name="spellName">Name of the spell</param>
        /// <param name="sprite">Sprite of the spell</param>
        /// <param name="target">What kind of targets this spell can be used on</param>
        /// <param name="spell">What type of spell is this</param>
        /// <param name="priority">Spell's priority</param>
        public BasicSpellbook(string spellName, Sprite sprite, TargetType target, SpellType spell, PriorityType priority) : this(spellName, sprite, target, spell, priority, false) { }

        /// <summary>
        /// Priority constructor
        /// </summary>
        /// <param name="spellName">Name of the spell</param>
        /// <param name="sprite">Sprite of the spell</param>
        /// <param name="target">What kind of targets this spell can be used on</param>
        /// <param name="spell">What type of spell is this</param>
        /// <param name="priority">Spell's priority</param>
        public BasicSpellbook(string spellName, Sprite sprite, TargetType target, SpellType spell, PriorityType priority, bool canTargetDead) : base(spellName, sprite, target, spell, priority, "Perform") {
            this.canTargetDead = canTargetDead;
        }

        protected sealed override bool IsMeetOtherCastRequirements(IPage current, Character caster, Character target) {
            return (canTargetDead || target.Stats.State == Characters.State.ALIVE) && IsMeetCastRequirements(caster, target);
        }

        protected virtual bool IsMeetCastRequirements(Character caster, Character target) {
            return true;
        }
    }
}