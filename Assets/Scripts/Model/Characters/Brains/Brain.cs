using Scripts.Game.Defined.Unserialized.Spells;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.Spells;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Model.Characters {

    /// <summary>
    /// Artificial intelligence class. Determines what actions
    /// an NPC will take in a battle.
    /// </summary>
    public abstract class Brain : ISaveable<BrainSave> {
        public SpellBooks Spells;

        // Refreshed every turn
        protected ICollection<Character> allies;

        protected ICollection<Character> foes;

        protected Character brainOwner;
        protected Battle currentBattle;
        protected IEnumerable<ISpellable> temporarySpells;
        protected Action<Spell> spellHandler;

        public Brain() {
        }

        public void StartOfRoundSetup(Battle currentBattle, Character brainOwner) {
            this.brainOwner = brainOwner;
            this.currentBattle = currentBattle;
            allies = currentBattle.GetAllies(brainOwner).ToArray();
            foes = currentBattle.GetFoes(brainOwner).ToArray();
        }

        /// <summary>
        /// Called before actions are decided every turn.
        /// </summary>
        /// <param name="temporarySpells">Spells that are only usable in battle. Player needs a reference so they can see and use their buttons.</param>
        /// <param name="playHandler">Call this on a play to use that as the owner's play during a battle.</param>
        public void PreActionSetup(IEnumerable<ISpellable> temporarySpells, Action<Spell> spellHandler) {
            this.temporarySpells = temporarySpells;
            this.spellHandler = spellHandler;
        }

        /// <summary>
        /// Determines the action of the owner during a turn.
        /// </summary>
        public abstract void DetermineAction();

        /// <summary>
        /// Reaction to a spell targeting this character.
        /// </summary>
        /// <param name="spellTargetingUs"></param>
        /// <returns>String that is converted into an avatarbox.</returns>
        public virtual string ReactToSpell(SingleSpell spellTargetingUs) {
            return string.Empty;
        }

        /// <summary>
        /// Dialgue said at the beginning of every round.
        /// </summary>
        /// <returns>String converted into an avatarbox.</returns>
        public virtual string StartOfRoundDialogue() {
            return string.Empty;
        }

        /// <summary>
        /// Check if types match.
        /// </summary>
        /// <param name="obj">Brain to check</param>
        /// <returns>True if types match.</returns>
        public override bool Equals(object obj) {
            var item = obj as Brain;

            if (item == null) {
                return false;
            }

            return this.GetType().Equals(item.GetType());
        }

        /// <summary>
        /// No hashable content
        /// </summary>
        /// <returns>0</returns>
        public override int GetHashCode() {
            return 0;
        }

        /// <summary>
        /// Brain is saved only by its type.
        /// </summary>
        /// <returns>Serializable brain</returns>
        public BrainSave GetSaveObject() {
            return new BrainSave(GetType());
        }

        /// <summary>
        /// Nothing to save but the type, nothing to init.
        /// </summary>
        /// <param name="saveObject">Serializable brain object</param>
        public void InitFromSaveObject(BrainSave saveObject) {
            // Nothing
        }
    }
}