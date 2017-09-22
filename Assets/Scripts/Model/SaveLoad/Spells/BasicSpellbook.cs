using UnityEngine;

namespace Scripts.Model.Spells {

    /// <summary>
    /// A simple spellbook abstraction.
    /// Most spellbooks can't target dead people and don't have cooldowns.
    /// So this implementation pre-overides these methods.
    /// </summary>
    public abstract class BasicSpellbook : SpellBook {

        /// <summary>
        /// Normal priority constructor
        /// </summary>
        /// <param name="spellName">Name of the spell</param>
        /// <param name="sprite">Sprite of the spell</param>
        /// <param name="target">What kind of targets this spell can be used on</param>
        /// <param name="spell">What type of spell is this</param>
        public BasicSpellbook(string spellName, Sprite sprite, TargetType target, SpellType spell) : base(spellName, sprite, target, spell, 0, 0, "Perform") { }

        /// <summary>
        /// Priority constructor
        /// </summary>
        /// <param name="spellName">Name of the spell</param>
        /// <param name="sprite">Sprite of the spell</param>
        /// <param name="target">What kind of targets this spell can be used on</param>
        /// <param name="spell">What type of spell is this</param>
        /// <param name="priority">Spell's priority</param>
        public BasicSpellbook(string spellName, Sprite sprite, TargetType target, SpellType spell, PriorityType priority) : base(spellName, sprite, target, spell, 0, 0, priority, "Perform") { }

        protected sealed override bool IsMeetOtherCastRequirements(SpellParams caster, SpellParams target) {
            return caster.Stats.State == Characters.State.ALIVE && target.Stats.State == Characters.State.ALIVE && IsMeetCastRequirements(caster, target);
        }

        protected virtual bool IsMeetCastRequirements(SpellParams caster, SpellParams target) {
            return true;
        }
    }

}