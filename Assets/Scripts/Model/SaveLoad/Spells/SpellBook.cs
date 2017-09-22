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
    /// SpellParams hold information needed for the spellbook.
    /// </summary>
    public struct SpellParams {
        /// <summary>
        /// The character identifier
        /// </summary>
        public readonly int CharacterId;
        /// <summary>
        /// The character
        /// </summary>
        public readonly Character Character;
        /// <summary>
        /// The look
        /// </summary>
        public readonly Characters.Look Look;
        /// <summary>
        /// The stats
        /// </summary>
        public readonly Characters.Stats Stats;
        /// <summary>
        /// The spells
        /// </summary>
        public readonly Characters.SpellBooks Spells;
        /// <summary>
        /// The buffs
        /// </summary>
        public readonly Characters.Buffs Buffs;
        /// <summary>
        /// The inventory
        /// </summary>
        public readonly Characters.Inventory Inventory;
        /// <summary>
        /// The equipment
        /// </summary>
        public readonly Characters.Equipment Equipment;
        /// <summary>
        /// The page
        /// </summary>
        public readonly Page Page;
        /// <summary>
        /// Gets the portrait.
        /// </summary>
        /// <value>
        /// The portrait.
        /// </value>
        public PortraitView Portrait {
            get {
                return Character.Presenter.PortraitView;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellParams"/> struct.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="currentPage">The current page.</param>
        public SpellParams(Characters.Character c, Page currentPage) {
            this.CharacterId = c.Id;
            this.Character = c;
            this.Look = c.Look;
            this.Stats = c.Stats;
            this.Spells = c.Spells;
            this.Buffs = c.Buffs;
            this.Inventory = c.Inventory;
            this.Equipment = c.Equipment;
            this.Page = currentPage;
        }
    }

    /// <summary>
    /// Creates spells.
    /// </summary>
    /// <seealso cref="Scripts.Model.Interfaces.ISpellable" />
    /// <seealso cref="Scripts.Model.SaveLoad.ISaveable{Scripts.Model.SaveLoad.SaveObjects.SpellBookSave}" />
    public abstract class SpellBook : ISpellable, ISaveable<SpellBookSave> {

        /// <summary>
        /// The name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The icon
        /// </summary>
        public readonly Sprite Icon;
        /// <summary>
        /// The target type
        /// </summary>
        public readonly TargetType TargetType;
        /// <summary>
        /// The spell type
        /// </summary>
        public readonly SpellType SpellType;
        /// <summary>
        /// The priority
        /// </summary>
        public readonly PriorityType Priority;

        /// <summary>
        /// The cast time
        /// </summary>
        public readonly int CastTime;
        /// <summary>
        /// The cooldown
        /// </summary>
        public readonly int Cooldown;
        /// <summary>
        /// The is silenced
        /// </summary>
        public readonly bool IsSilenced;
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
        private readonly IDictionary<StatType, int> costs;

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
        public SpellBook(string spellName, Sprite sprite, TargetType target, SpellType spell, int castTime, int cooldown, string verb) : this(spellName, sprite, target, spell, castTime, cooldown, 0, verb) { }

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
        public SpellBook(string spellName, Sprite sprite, TargetType target, SpellType spell, int castTime, int cooldown, PriorityType priority, string verb) {
            this.Name = spellName;
            this.Icon = sprite;
            this.costs = new Dictionary<StatType, int>();
            this.TargetType = target;
            this.SpellType = spell;
            this.CastTime = castTime;
            this.Cooldown = cooldown;
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
        public SpellBook(string spellName, string spriteLoc, TargetType target, SpellType spell, int castTime, int cooldown)
            : this(spellName, Util.GetSprite(spriteLoc), target, spell, castTime, cooldown, "use") { }

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

            return this.GetType().Equals(item.GetType())
                && this.IsSilenced.Equals(item.IsSilenced);
        }

        /// <summary>
        /// Gets the costs.
        /// </summary>
        /// <value>
        /// The costs.
        /// </value>
        public IDictionary<StatType, int> Costs {
            get {
                return new ReadOnlyDictionary<StatType, int>(costs);
            }
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
                    string.Format("{0}{1}{2}",
                        Priority == 0 ? string.Empty : string.Format("{0} priority\n", Priority.GetDescription()),
                        Costs.Count == 0 ? string.Empty : string.Format("Costs {0}\n\n", GetCommaSeparatedCosts()),
                        CreateDescriptionHelper()
                        )
                    );
            }
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
        /// Creates the description.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        public string CreateDescription(SpellParams caster) {
            return string.Format("{0}{1}{2}{3}",
                CasterHasResources(caster.Stats) ? string.Empty : Util.ColorString("Insufficient resource.\n", Color.red),
                Priority == 0 ? string.Empty : string.Format("{0} priority", Priority.GetDescription()),
                Costs.Count == 0 ? string.Empty : string.Format("\nCosts {0}\n\n", GetCommaSeparatedCosts(caster.Stats)),
                CreateDescriptionHelper()
                );
        }

        /// <summary>
        /// Creates the target description.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public string CreateTargetDescription(SpellParams caster, SpellParams target) {
            return string.Format("{0} {1} on {2}.", this.Verb, this.Name, target.Look.DisplayName);
        }

        /// <summary>
        /// Creates the description helper.
        /// </summary>
        /// <returns></returns>
        protected abstract string CreateDescriptionHelper();

        /// <summary>
        /// Determines whether [is meet pre target requirements] [the specified caster].
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns>
        ///   <c>true</c> if [is meet pre target requirements] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMeetPreTargetRequirements(Characters.Stats caster) {
            return CasterHasResources(caster) && IsMeetOtherPreTargetRequirements();
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
        /// Casters the has resource.
        /// </summary>
        /// <param name="stat">The stat.</param>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        public bool CasterHasResource(StatType stat, Characters.Stats caster) {
            return caster.GetStatCount(Characters.Stats.Get.MOD, stat) >= Costs[stat];
        }

        /// <summary>
        /// Casters the has resources.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        public bool CasterHasResources(Characters.Stats caster) {
            foreach (KeyValuePair<StatType, int> stat in Costs) {
                if (caster.GetStatCount(Characters.Stats.Get.MOD, stat.Key) < stat.Value) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified caster is castable.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if the specified caster is castable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCastable(SpellParams caster, SpellParams target) {
            return CasterHasResources(caster.Stats) && IsCastableIgnoreResources(caster, target);
        }

        /// <summary>
        /// Determines whether [is castable ignore resources] [the specified caster].
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if [is castable ignore resources] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCastableIgnoreResources(SpellParams caster, SpellParams target) {
            return !IsSilenced && IsMeetOtherCastRequirements(caster, target) && (caster.Spells.HasSpellBook(this) || !flags.Contains(Flag.CASTER_REQUIRES_SPELL));
        }

        /// <summary>
        /// Builds the spell.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public Spell BuildSpell(SpellParams caster, SpellParams target) {
            Util.Assert(
                IsCastable(caster, target),
                string.Format(
                "Attempted to cast spell without requirements fulfilled. Resources={0}, OtherRequirements={1}."
                , CasterHasResources(caster.Stats),
                IsMeetOtherCastRequirements(caster, target)
                ));

            // Consume resources
            foreach (KeyValuePair<StatType, int> pair in Costs) {
                caster.Stats.AddToStat(pair.Key, Characters.Stats.Set.MOD, -pair.Value);
            }

            return ForceSpell(caster, target);
        }

        /// <summary>
        /// Forces the spell.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public Spell ForceSpell(SpellParams caster, SpellParams target) {
            Result res = new Result();
            if (IsHit(caster, target)) {
                res.AddSFX(GetHitSFX(caster.Portrait, target.Portrait));
                if (IsCritical(caster, target)) {
                    res.Type = ResultType.CRITICAL;
                    res.AddEffects(GetCriticalEffects(caster, target));
                } else {
                    res.Type = ResultType.HIT;
                    res.AddEffects(GetHitEffects(caster, target));
                }
            } else {
                res.AddSFX(GetMissSFX(caster.Portrait, target.Portrait));
                res.AddSFX(SFX.DoHitSplat(target.Portrait.EffectsHolder, new SplatDetails(Color.grey, "MISS!")));
                res.Type = ResultType.MISS;
                res.AddEffects(GetMissEffects(caster, target));
            }

            return new Spell(this, res, caster, target);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() {
            return base.ToString();
        }

        /// <summary>
        /// Determines whether [is meet other cast requirements] [the specified caster].
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if [is meet other cast requirements] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsMeetOtherCastRequirements(SpellParams caster, SpellParams target) {
            return true;
        }

        /// <summary>
        /// Determines whether the specified caster is hit.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if the specified caster is hit; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsHit(SpellParams caster, SpellParams target) {
            return true;
        }

        /// <summary>
        /// Gets the hit effects.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected abstract IList<SpellEffect> GetHitEffects(SpellParams caster, SpellParams target);

        /// <summary>
        /// Determines whether the specified caster is critical.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if the specified caster is critical; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsCritical(SpellParams caster, SpellParams target) {
            return false;
        }

        /// <summary>
        /// Gets the critical effects.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected virtual IList<SpellEffect> GetCriticalEffects(SpellParams caster, SpellParams target) {
            return new SpellEffect[0];
        }

        /// <summary>
        /// Gets the miss effects.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected virtual IList<SpellEffect> GetMissEffects(SpellParams caster, SpellParams target) {
            return new SpellEffect[0];
        }

        /// <summary>
        /// Gets the hit SFX.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected virtual IList<IEnumerator> GetHitSFX(PortraitView caster, PortraitView target) {
            return new IEnumerator[0];
        }

        /// <summary>
        /// Adds the cost.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="cost">The cost.</param>
        protected void AddCost(StatType type, int cost) {
            Util.Assert(cost > 0, "Cost must be positive.");
            costs.Add(type, cost);
        }

        /// <summary>
        /// Gets the miss SFX.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        protected virtual IList<IEnumerator> GetMissSFX(PortraitView caster, PortraitView target) {
            return new IEnumerator[0];
        }

        /// <summary>
        /// Gets the comma separated costs.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        private string GetCommaSeparatedCosts(Characters.Stats caster = null) {
            string[] arr = new string[Costs.Count];

            int index = 0;
            foreach (KeyValuePair<StatType, int> pair in Costs) {
                arr[index++] = string.Format("{0} {1}",
                    Util.ColorString(pair.Value.ToString(), caster != null && CasterHasResource(pair.Key, caster)),
                    Util.ColorString(pair.Key.Name, pair.Key.Color));
            }
            return string.Join(", ", arr);
        }

        /// <summary>
        /// Gets the spell book.
        /// </summary>
        /// <returns></returns>
        SpellBook ISpellable.GetSpellBook() {
            return this;
        }

        /// <summary>
        /// Gets the name of the detailed.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        public virtual string GetDetailedName(SpellParams caster) {
            return Util.ColorString(string.Format("{0}", Name), CasterHasResources(caster.Stats));
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
        /// Initializes from save object.
        /// </summary>
        /// <param name="saveObject">The save object.</param>
        public void InitFromSaveObject(SpellBookSave saveObject) {
            // Spellbook doesn't need anything restored!
        }
    }
}