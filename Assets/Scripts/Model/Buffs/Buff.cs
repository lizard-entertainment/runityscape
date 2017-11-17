using Scripts.Model.Interfaces;
using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Scripts.Model.Tooltips;
using Scripts.Model.TextBoxes;
using Scripts.Model.Pages;
using Scripts.Model.Characters;

namespace Scripts.Model.Buffs {

    /// <summary>
    /// Parameters needed for buff initialization.
    /// </summary>
    public struct BuffParams {
        public readonly Characters.Stats Caster;
        public readonly int CasterId;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuffParams"/> struct.
        /// </summary>
        /// <param name="caster">The caster's stats.</param>
        /// <param name="casterId">A unique id number associated with a character.</param>
        public BuffParams(Characters.Stats caster, int casterId) {
            this.Caster = caster;
            this.CasterId = casterId;
        }

        public BuffParams(Character caster) : this(caster.Stats, caster.Id) {
        }
    }

    /// <summary>
    /// Buffs are... buffs. They stick on a player for some time.
    /// They have various effects. For easy serialization only the caster's
    /// stats are used.
    /// </summary>
    public abstract class Buff : IComparable<Buff>, ISaveable<BuffSave>, IIdNumberable, IEnumerable<KeyValuePair<StatType, int>> {
        private const string BUFF_REACT_TEXT = "<color=yellow>{0}</color>'s <color=cyan>{1}</color> activates <color=yellow>{2}</color>'s <color=cyan>{3}</color>!";

        /// <summary>
        /// Buff description
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// If true, buff can be dispelled.
        /// Otherwise, only timing out will remove the buff.
        /// </summary>
        public readonly bool IsDispellable;

        /// <summary>
        /// Name of the buff.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Sprite representing the buff
        /// </summary>
        public readonly Sprite Sprite;

        private static int idCount;

        /// <summary>
        /// ID used to differentiate buffs.
        /// </summary>
        private int buffId;

        /// <summary>
        /// Buff's caster
        /// </summary>
        private Characters.Stats caster;

        /// <summary>
        /// ID of the caster who created this buff.
        /// </summary>
        private int casterId;

        /// <summary>
        /// If true, caster has been initialized.
        /// </summary>
        private bool isCasterSet;

        /// <summary>
        /// If false, buff will last forever.
        /// </summary>
        private bool isDefinite;

        /// <summary>
        /// The stat bonuses to have on a character while
        /// this buff is on them.
        /// </summary>
        private IDictionary<StatType, int> statBonuses;

        /// <summary>
        /// Turns until the buff is removed.
        /// </summary>
        private int turnsRemaining;

        /// <summary>
        /// Infinite duration buff constructor
        /// </summary>
        /// <param name="sprite"><see cref="Sprite"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="description"><see cref="Description"/></param>
        /// <param name="isDispellable"><see cref="IsDispellable"/></param>
        public Buff(Sprite sprite, string name, string description, bool isDispellable) {
            this.Sprite = sprite;
            this.Name = name;
            this.Description = description;
            this.IsDispellable = isDispellable;
            this.buffId = idCount++;
            this.statBonuses = new Dictionary<StatType, int>();
        }

        /// <summary>
        /// Finite duration buff constructor
        /// </summary>
        /// <param name="duration"><see cref="turnsRemaining"/></param>
        /// <param name="sprite"><see cref="Sprite"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="description"><see cref="Description"/></param>
        /// <param name="isDispellable"><see cref="IsDispellable"/></param>
        public Buff(int duration, Sprite sprite, string name, string description, bool isDispellable) : this(sprite, name, description, isDispellable) {
            Util.Assert(duration > 0, "Duration must be positive.");
            this.turnsRemaining = duration;
            this.isDefinite = true;
        }

        /// <summary>
        /// Stats of the buff's caster
        /// </summary>
        public Characters.Stats BuffCaster {
            get {
                return caster;
            }
        }

        public TooltipBundle Tooltip {
            get {
                return new TooltipBundle(
                    this.Sprite,
                    this.Name,
                    this.DetailedDescription);
            }
        }

        public string DetailedDescription {
            get {
                return string.Format(
                    "<color=yellow>{0}</color>{1}",
                    IsDispellable ? "Dispellable\n" : string.Empty,
                    Description
                    );
            }
        }

        public string StringTooltip {
            get {
                return string.Format(
                    "<color=cyan>{0}</color>\n<color=yellow>{1}</color>{2}",
                    this.Name,
                    IsDispellable ? "Dispellable\n" : string.Empty,
                    Description
                    );
            }
        }

        /// <summary>
        /// Caster of the buff
        /// </summary>
        public BuffParams Caster {
            set {
                Util.Assert(!isCasterSet, "Attempted to set caster twice.");
                this.caster = value.Caster;
                this.casterId = value.CasterId;
                isCasterSet = true;
            }
        }

        /// <summary>
        /// Character ID associated with buff's caster
        /// </summary>
        public int CasterId {
            get {
                return casterId;
            }
        }

        /// <summary>
        /// Text describing the buff's duration.
        /// </summary>
        public string DurationText {
            get {
                string s = string.Empty;
                if (isDefinite) {
                    s = turnsRemaining.ToString();
                } else {
                    s = "inf";
                }
                return s;
            }
        }

        /// <summary>
        /// Determines if the buff has any end-of-turn effects.
        /// </summary>
        public bool HasEndOfTurn {
            get {
                return OnEndOfTurnHelper(new Characters.Stats()).Count > 0;
            }
        }

        /// <summary>
        /// Unique identifier int for buff
        /// </summary>
        public int Id {
            get {
                return buffId;
            }
        }

        /// <summary>
        /// If true, buff needs to be removed.
        /// </summary>
        public bool IsTimedOut {
            get {
                return isDefinite && turnsRemaining <= 0;
            }
        }

        /// <summary>
        /// Turns until the buff expires
        /// </summary>
        public int TurnsRemaining {
            get {
                return turnsRemaining;
            }
        }

        /// <summary>
        /// Compare by turns remaining.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Duration difference.</returns>
        public int CompareTo(Buff other) {
            return this.turnsRemaining - other.turnsRemaining;
        }

        /// <summary>
        /// Type, duration, and caster equality.
        /// </summary>
        /// <param name="obj">Buff to check</param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            var item = obj as Buff;

            if (item == null) {
                return false;
            }

            return this.GetType().Equals(item.GetType())
                && this.turnsRemaining.Equals(item.turnsRemaining)
                && this.caster.Equals(item.caster);
        }

        public IEnumerator<KeyValuePair<StatType, int>> GetEnumerator() {
            return statBonuses.GetEnumerator();
        }

        public void ReactToSpell(Page page, SingleSpell spellToReactTo, Character buffHolder) {
            if (this.IsReact(spellToReactTo, buffHolder.Stats)) {
                page.AddText(new TextBox(
                    string.Format(BUFF_REACT_TEXT,
                        spellToReactTo.CasterName,
                        spellToReactTo.SpellBook.Name,
                        buffHolder.Look.DisplayName,
                        this.Name
                        ),
                    this.Tooltip));
                this.React(spellToReactTo, buffHolder.Stats);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return statBonuses.GetEnumerator();
        }

        /// <summary>
        /// Name and description.
        /// </summary>
        /// <returns>Hash</returns>
        public override int GetHashCode() {
            return Name.GetHashCode() ^ Description.GetHashCode();
        }

        /// <summary>
        /// Get serializable buff object.
        /// </summary>
        /// <returns>Serializable buff object.</returns>
        public BuffSave GetSaveObject() {
            return new BuffSave(turnsRemaining, caster.GetSaveObject(), casterId, GetType());
        }

        /// <summary>
        /// Initialize duration.
        /// </summary>
        /// <param name="saveObject">Serializable buff object.</param>
        public void InitFromSaveObject(BuffSave saveObject) {
            this.turnsRemaining = saveObject.TurnsRemaining;
            // Setup caster and id in party!
        }

        /// <summary>
        /// If true, buff will react to the spells being cast in battle. This can be any spell.
        /// The one who has the buff doesn't have to be targeted.
        /// </summary>
        /// <param name="incomingSpell">Spell cast on character</param>
        /// <param name="ownerOfThisBuff">Stats of the character who is the target of the spell</param>
        /// <returns>True if the buff will react.</returns>
        public virtual bool IsReact(SingleSpell incomingSpell, Characters.Stats ownerOfThisBuff) {
            return false;
        }

        /// <summary>
        /// Effects that occur when the buff is added to a character.
        /// </summary>
        /// <param name="owner"></param>
        public void OnApply(Characters.Stats owner) {
            PerformSpellEffects(OnApplyHelper(owner));
        }

        /// <summary>
        /// Cast when the buff is dispelled.
        /// </summary>
        /// <param name="ownerOfThisBuff">Stats of the character who is the target of the spell</param>
        public void OnDispell(Characters.Stats ownerOfThisBuff) {
            PerformSpellEffects(OnDispellHelper(ownerOfThisBuff));
        }

        /// <summary>
        /// Effects that occur at the end of a turn.
        /// </summary>
        /// <param name="owner"></param>
        public void OnEndOfTurn(Characters.Stats owner) {
            // TODO use an overridable condition instead
            if (owner.State == Characters.State.ALIVE) {
                PerformSpellEffects(OnEndOfTurnHelper(owner));
            }
            if (isDefinite) {
                turnsRemaining--;
            }
        }

        /// <summary>
        /// Cast when the buff expires.
        /// </summary>
        /// <param name="ownerOfThisBuff">Stats of the character who is the target of the spell</param>
        public void OnTimeOut(Characters.Stats ownerOfThisBuff) {
            PerformSpellEffects(OnTimeOutHelper(ownerOfThisBuff));
        }

        /// <summary>
        /// Respond to a spell cast in battle.
        /// </summary>
        /// <param name="incomingSpell">Spell cast on character</param>
        /// <param name="ownerOfThisBuff">Stats of the character who is the target of the spell</param>
        private void React(SingleSpell incomingSpell, Characters.Stats ownerOfThisBuff) {
            ReactHelper(incomingSpell, ownerOfThisBuff);
        }

        /// <summary>
        /// Adds the multiplicative stat bonus.
        /// If you want Strength to be increased by +50%,
        /// you would put 50 for amount.
        /// </summary>
        /// <param name="st">The stat type.</param>
        /// <param name="amount">The amount.</param>
        protected void AddMultiplicativeStatBonus(StatType st, int amount) {
            Util.Assert(StatType.ASSIGNABLES.Contains(st), "Stat is not assignable.");
            Util.Assert(!statBonuses.ContainsKey(st), "Stat already included.");
            Util.Assert(amount != 0, "Amount must be non-zero.");
            statBonuses[st] = amount;
        }

        /// <see cref="OnApply(Characters.Stats)"/>
        protected virtual IList<SpellEffect> OnApplyHelper(Characters.Stats ownerOfThisBuff) {
            return new SpellEffect[0];
        }

        /// <see cref="OnDispell(Characters.Stats)"/>
        protected virtual IList<SpellEffect> OnDispellHelper(Characters.Stats ownerOfThisBuff) {
            return OnTimeOutHelper(ownerOfThisBuff);
        }

        /// <see cref="OnEndOfTurn(Characters.Stats)"/>
        protected virtual IList<SpellEffect> OnEndOfTurnHelper(Characters.Stats ownerOfThisBuff) {
            return new SpellEffect[0];
        }

        /// <see cref="OnTimeOut(Characters.Stats)"/>
        protected virtual IList<SpellEffect> OnTimeOutHelper(Characters.Stats ownerOfThisBuff) {
            return new SpellEffect[0];
        }

        /// <see cref="React(Spell, Characters.Stats)"/>
        protected virtual void ReactHelper(SingleSpell spellCast, Characters.Stats ownerOfThisBuff) {
        }

        /// <summary>
        /// Performs spell effects in order.
        /// </summary>
        /// <param name="list">List of spell effects.</param>
        private void PerformSpellEffects(IList<SpellEffect> list) {
            foreach (SpellEffect mySE in list) {
                SpellEffect se = mySE;
                se.CauseEffect();
            }
        }
    }
}