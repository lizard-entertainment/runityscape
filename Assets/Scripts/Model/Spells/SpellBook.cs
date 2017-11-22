using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Stats;
using Scripts.View.Portraits;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.SaveLoad;
using System.Collections.ObjectModel;
using System.Linq;
using Scripts.Model.Pages;
using Scripts.Game.Defined.SFXs;
using Scripts.Presenter;
using Scripts.Model.Tooltips;
using System;

namespace Scripts.Model.Spells {

    /// <summary>
    /// Creates spells.
    /// </summary>
    /// <seealso cref="Scripts.Model.Interfaces.ISpellable" />
    /// <seealso cref="Scripts.Model.SaveLoad.ISaveable{Scripts.Model.SaveLoad.SaveObjects.SpellBookSave}" />
    public abstract class SpellBook : ISpellable, ISaveable<SpellBookSave> {

        /// <summary>
        /// The icon
        /// </summary>
        public readonly Sprite Icon;

        /// <summary>
        /// The name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The priority
        /// </summary>
        public readonly PriorityType Priority;

        /// <summary>
        /// The spell type
        /// </summary>
        public readonly SpellType SpellType;

        /// <summary>
        /// The target type
        /// </summary>
        public readonly TargetType TargetType;

        /// <summary>
        /// The verb
        /// </summary>
        public readonly string Verb;

        /// <summary>
        /// The flags
        /// </summary>
        protected readonly HashSet<Flag> flags;

        /// <summary>
        /// The costs
        /// </summary>
        private readonly IDictionary<ICostable, int> costs;

        private ReadOnlyDictionary<ICostable, int> roCosts;

        /// <summary>
        /// Number of turns it takes to charge this spell.
        /// </summary>
        private int turnsToCharge;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellBook"/> class.
        /// </summary>
        /// <param name="spellName">Name of the spell.</param>
        /// <param name="sprite">The sprite.</param>
        /// <param name="target">The target.</param>
        /// <param name="spell">The spell.</param>
        /// <param name="castTime">The cast time.</param>
        /// <param name="cooldown">The cooldown.</param>
        /// <param name="verb">The verb.</param>
        public SpellBook(string spellName, Sprite sprite, TargetType target, SpellType spell, string verb) : this(spellName, sprite, target, spell, 0, verb) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellBook"/> class.
        /// </summary>
        /// <param name="spellName">Name of the spell.</param>
        /// <param name="sprite">The sprite.</param>
        /// <param name="target">The target.</param>
        /// <param name="spell">The spell.</param>
        /// <param name="castTime">The cast time.</param>
        /// <param name="cooldown">The cooldown.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="verb">The verb.</param>
        public SpellBook(string spellName, Sprite sprite, TargetType target, SpellType spell, PriorityType priority, string verb) {
            this.Name = spellName;
            this.Icon = sprite;
            this.costs = new Dictionary<ICostable, int>();
            this.TargetType = target;
            this.SpellType = spell;
            this.flags = new HashSet<Flag>() { Flag.CASTER_REQUIRES_SPELL };
            this.Verb = verb;
            this.Priority = priority;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellBook"/> class.
        /// </summary>
        /// <param name="spellName">Name of the spell.</param>
        /// <param name="spriteLoc">The sprite loc.</param>
        /// <param name="target">The target.</param>
        /// <param name="spell">The spell.</param>
        /// <param name="castTime">The cast time.</param>
        /// <param name="cooldown">The cooldown.</param>
        public SpellBook(string spellName, string spriteLoc, TargetType target, SpellType spell, string verb)
            : this(spellName, Util.GetSprite(spriteLoc), target, spell, verb) { }

        /// <summary>
        /// Gets the costs.
        /// </summary>
        /// <value>
        /// The costs.
        /// </value>
        public IDictionary<ICostable, int> Costs {
            get {
                return roCosts ?? (roCosts = new ReadOnlyDictionary<ICostable, int>(costs));
            }
        }

        public string NameAndDescription {
            get {
                return string.Format(
                    "{0} ({1}) - {2}",
                        Name,
                        GetCommaSeparatedCosts(),
                        CreateDescriptionHelper()
                    );
            }
        }

        public int TurnsToCharge {
            get {
                return turnsToCharge;
            }
            protected set {
                Util.Assert(turnsToCharge >= 0, "Turns to charge must be nonnegative.");
                this.turnsToCharge = value;
            }
        }

        /// <summary>
        /// Gets the textbox tooltip.
        /// </summary>
        /// <value>
        /// The textbox tooltip.
        /// </value>
        public virtual TooltipBundle TextboxTooltip {
            get {
                return new TooltipBundle(
                    this.Icon,
                    this.Name,
                    string.Format("{0}{1}{2}{3}",
                        TurnsToCharge == 0 ? string.Empty : string.Format("<color=yellow>{0}</color> turn windup\n", TurnsToCharge),
                        Priority == 0 ? string.Empty : string.Format("{0} priority\n", Priority.GetDescription()),
                        Costs.Count == 0 ? string.Empty : string.Format("Costs {0}\n", GetCommaSeparatedCosts()),
                        CreateDescriptionHelper()
                        )
                    );
            }
        }

        /// <summary>
        /// Builds the spell.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public Spell BuildSpell(Page page, Character caster, ICollection<Character> targets) {
            Util.Assert(
                IsCastable(page, caster, targets),
                string.Format(
                "Attempted to cast {0} without requirements fulfilled. Resources={1}, OtherRequirements={2}.",
                this.Name,
                CasterHasResources(caster),
                targets.Any(target => IsMeetOtherCastRequirements(page, caster, target))
                ));

            // Consume resources
            foreach (KeyValuePair<ICostable, int> pair in Costs) {
                caster.ConsumeCosts(pair.Key, pair.Value);
            }

            Spell spellToReturn = null;
            if (TargetType.TargetCount == TargetCount.SINGLE_TARGET) {
                spellToReturn = ForceSpell(page, caster, targets.First()); // Single target version
            } else if (TargetType.TargetCount == TargetCount.MULTIPLE_TARGETS) {
                spellToReturn = ForceSpell(page, caster); // Multi target version
            }
            return spellToReturn;
        }

        /// <summary>
        /// Casters the has resource.
        /// </summary>
        /// <param name="stat">The stat.</param>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        public bool CasterHasResource(ICostable cost, int amount, Character caster) {
            return caster.CanAffordCost(cost, amount);
        }

        /// <summary>
        /// Casters the has resources.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        public bool CasterHasResources(Character caster) {
            foreach (KeyValuePair<ICostable, int> cost in Costs) {
                if (!caster.CanAffordCost(cost.Key, cost.Value)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates the description.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        public string CreateDescription(Character caster) {
            return string.Format("{0}{1}{2}{3}{4}",
                CasterHasResources(caster) ? string.Empty : Util.ColorString("Insufficient resource.\n", Color.red),
                TurnsToCharge == 0 ? string.Empty : string.Format("<color=yellow>{0}</color> turn charge\n", TurnsToCharge),
                Priority == 0 ? string.Empty : string.Format("{0} priority\n", Priority.GetDescription()),
                Costs.Count == 0 ? string.Empty : string.Format("Costs {0}\n", GetCommaSeparatedCosts(caster)),
                CreateDescriptionHelper()
                );
        }

        /// <summary>
        /// Creates the target description.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public string CreateTargetDescription(string targetName) {
            return string.Format("{0} {1} on {2}.", this.Verb, this.Name, targetName);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            var item = obj as SpellBook;

            if (item == null) {
                return false;
            }

            return this.GetType().Equals(item.GetType());
        }

        /// <summary>
        /// Gets the cost.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public int GetCost(StatType type) {
            int cost = 0;
            if (Costs.ContainsKey(type)) {
                cost = Costs[type];
            }
            return cost;
        }

        /// <summary>
        /// Gets the name of the detailed.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        public virtual string GetDetailedName(Character caster) {
            return Util.ColorString(string.Format("{0}", Name), CasterHasResources(caster));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() {
            return GetType().GetHashCode();
        }

        /// <summary>
        /// Gets the save object. A save object contains the neccessary
        /// information to initialize a clean class to its saved state.
        /// A save object is also serializable.
        /// </summary>
        /// <returns></returns>
        public SpellBookSave GetSaveObject() {
            return new SpellBookSave(GetType());
        }

        /// <summary>
        /// Gets the spell book.
        /// </summary>
        /// <returns></returns>
        SpellBook ISpellable.GetSpellBook() {
            return this;
        }

        /// <summary>
        /// Determines whether the specified f has flag.
        /// </summary>
        /// <param name="f">The f.</param>
        /// <returns>
        ///   <c>true</c> if the specified f has flag; otherwise, <c>false</c>.
        /// </returns>
        public bool HasFlag(Flag f) {
            return flags.Contains(f);
        }

        /// <summary>
        /// Initializes from save object.
        /// </summary>
        /// <param name="saveObject">The save object.</param>
        public void InitFromSaveObject(SpellBookSave saveObject) {
            // Spellbook doesn't need anything restored!
        }

        /// <summary>
        /// Determines whether the specified caster is castable.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if the specified caster is castable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCastable(IPage page, Character caster, ICollection<Character> targets) {
            return
                CasterHasResources(caster)
                && IsCastableIgnoreResources(page, caster, targets);
        }

        public bool IsCastableIgnoreResources(IPage page, Character caster, ICollection<Character> targets) {
            return
                IsNumberOfTargetsValid(targets.Count)
                && targets.Any(t => IsCastableIgnoreResources(page, caster, t));
        }

        /// <summary>
        /// Determines whether the number of targets is valid given the TargetType's target count.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <returns>
        ///   <c>true</c> if [is number of targets valid] [the specified targets]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsNumberOfTargetsValid(int count) {
            if (count > 1) {
                return (this.TargetType.TargetCount == TargetCount.MULTIPLE_TARGETS);
            }
            return (count == 1);
        }

        /// <summary>
        /// Determines whether [is meet pre target requirements] [the specified caster].
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns>
        ///   <c>true</c> if [is meet pre target requirements] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMeetPreTargetRequirements(Character caster) {
            return CasterHasResources(caster)
                && IsMeetOtherPreTargetRequirements();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() {
            return this.Name;
        }

        /// <summary>
        /// Adds the cost.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="cost">The cost.</param>
        protected void AddCost(ICostable cost, int amount) {
            Util.Assert(amount > 0, "Amount must be positive.");
            costs.Add(cost, amount);
        }

        /// <summary>
        /// Creates the description helper.
        /// </summary>
        /// <returns></returns>
        protected abstract string CreateDescriptionHelper();

        /// <summary>
        /// Gets the critical effects.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected virtual IList<SpellEffect> GetCriticalEffects(Page page, Character caster, Character target) {
            return new SpellEffect[0];
        }

        /// <summary>
        /// Gets the hit effects.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected abstract IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target);

        /// <summary>
        /// Gets the hit SFX.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected virtual IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[0];
        }

        /// <summary>
        /// Gets the miss effects.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected virtual IList<SpellEffect> GetMissEffects(Page page, Character caster, Character target) {
            return new SpellEffect[0];
        }

        /// <summary>
        /// Gets the miss SFX.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected virtual IList<IEnumerator> GetMissSFX(Character caster, Character target) {
            return new IEnumerator[0];
        }

        /// <summary>
        /// Determines whether the specified caster is critical.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if the specified caster is critical; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsCritical(Character caster, Character target) {
            return false;
        }

        /// <summary>
        /// Determines whether the specified caster is hit.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if the specified caster is hit; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsHit(Character caster, Character target) {
            return true;
        }

        /// <summary>
        /// Determines whether [is meet other cast requirements] [the specified caster].
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if [is meet other cast requirements] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsMeetOtherCastRequirements(IPage current, Character caster, Character target) {
            return true;
        }

        /// <summary>
        /// Determines whether [is meet other pre target requirements].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is meet other pre target requirements]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsMeetOtherPreTargetRequirements() {
            return true;
        }

        /// <summary>
        /// Forces the spell. Multi-target version.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="caster">The caster.</param>
        /// <param name="targets">The targets.</param>
        /// <returns></returns>
        private MultiSpell ForceSpell(Page current, Character caster) {
            return new MultiSpell(
                    this,
                    caster,
                    current,
                    (currentPage, casterUnit, target) => ForceSpell(currentPage, casterUnit, target)
                );
        }

        /// <summary>
        /// Creates spell without resource consumption. Single-target version.
        /// </summary>
        /// <param name="current">The current page.</param>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>SingleSpell</returns>
        private SingleSpell ForceSpell(Page current, Character caster, Character target) {
            Result res = new Result();
            if (IsHit(caster, target)) {
                res.AddSFX(GetHitSFX(caster, target));
                if (IsCritical(caster, target)) {
                    res.Type = ResultType.CRITICAL;
                    res.AddEffects(GetCriticalEffects(current, caster, target));
                } else {
                    res.Type = ResultType.HIT;
                    res.AddEffects(GetHitEffects(current, caster, target));
                }
            } else {
                res.AddSFX(GetMissSFX(caster, target));
                res.AddSFX(SFX.DoHitSplat(target, new SplatDetails(Color.grey, "MISS!")));
                res.Type = ResultType.MISS;
                res.AddEffects(GetMissEffects(current, caster, target));
            }

            return new SingleSpell(this, res, caster, target, current);
        }

        /// <summary>
        /// Gets the comma separated costs.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        private string GetCommaSeparatedCosts(Character caster = null) {
            string[] arr = new string[Costs.Count];

            int index = 0;
            foreach (KeyValuePair<ICostable, int> pair in Costs) {
                arr[index++] = string.Format("{0} {1}",
                    Util.ColorString(pair.Value.ToString(), caster == null || CasterHasResource(pair.Key, pair.Value, caster)),
                    pair.Key.GetName());
            }
            return string.Join(", ", arr);
        }

        /// <summary>
        /// Determines whether [is castable ignore resources] [the specified caster].
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if [is castable ignore resources] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCastableIgnoreResources(IPage page, Character caster, Character target) {
            return
                caster.Stats.State == State.ALIVE
                && (page.IsInBattle || HasFlag(Flag.USABLE_OUT_OF_COMBAT))
                && IsMeetOtherCastRequirements(page, caster, target)
                && (caster.Spells.HasSpellBook(this) || !flags.Contains(Flag.CASTER_REQUIRES_SPELL));
        }
    }
}