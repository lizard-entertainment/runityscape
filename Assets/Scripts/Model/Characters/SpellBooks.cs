using Scripts.Model.Interfaces;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Model.Stats;
using Scripts.Model.SaveLoad;
using System;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Model.Pages;

namespace Scripts.Model.Characters {

    /// <summary>
    /// Character's spells
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.Interfaces.ISpellable}" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.Spells.SpellBook}" />
    /// <seealso cref="Scripts.Model.SaveLoad.ISaveable{Scripts.Model.SaveLoad.SaveObjects.CharacterSpellBooksSave}" />
    public class SpellBooks : IEnumerable<ISpellable>, IEnumerable<SpellBook>, ISaveable<CharacterSpellBooksSave> {

        /// <summary>
        /// The default spells
        /// </summary>
        private static readonly SpellBook[] DEFAULT_SPELLS = new SpellBook[] { new Attack(), new Wait() };

        /// <summary>
        /// The set
        /// </summary>
        private readonly HashSet<SpellBook> set;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellBooks"/> class.
        /// </summary>
        public SpellBooks() {
            set = new HashSet<SpellBook>();
            foreach (SpellBook sb in DEFAULT_SPELLS) {
                set.Add(sb);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellBooks"/> class.
        /// </summary>
        /// <param name="initialSpells">The initial spells.</param>
        public SpellBooks(params SpellBook[] initialSpells) : this() {
            foreach (SpellBook sb in initialSpells) {
                set.Add(sb);
            }
        }

        public string SpellTextsDisplay {
            get {
                List<string> spells = new List<string>();
                foreach (SpellBook sb in set) {
                    spells.Add(sb.Name);
                }
                return "Spells:\n" + string.Join(", ", spells.ToArray());
            }
        }

        /// <summary>
        /// Gets the highest skill cost.
        /// </summary>
        /// <value>
        /// The highest skill cost.
        /// </value>
        public int HighestSkillCost {
            get {
                int highest = 0;
                if (set.Any(s => s.Costs.ContainsKey(StatType.SKILL))) {
                    highest = set.Select(s => s.GetCost(StatType.SKILL)).DefaultIfEmpty().Max();
                }
                return highest;
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count {
            get {
                return set.Count;
            }
        }

        /// <summary>
        /// Adds the spell book.
        /// </summary>
        /// <param name="s">The s.</param>
        public void AddSpellBook(SpellBook s) {
            Util.Assert(s.HasFlag(Spells.Flag.CASTER_REQUIRES_SPELL), "Uncastable spell.");
            set.Add(s);
        }

        /// <summary>
        /// Determines whether [has spell book] [the specified s].
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>
        ///   <c>true</c> if [has spell book] [the specified s]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasSpellBook(SpellBook s) {
            return set.Contains(s);
        }

        public bool HasResourcesToCast(Characters.Stats spellBooksOwner, SpellBook sb) {
            // Can't use hasResources because Skill is = highest cost skill spell
            // Since character without spell starts with 0 Skill, they won't be able to learn anything
            return sb.Costs.Keys
                .Where(cost => cost is StatType)
                .Cast<StatType>()
                .All(statType => spellBooksOwner.HasStat(statType));
        }

        public bool CanLearnSpellBook(Characters.Stats spellBooksOwner, SpellBook sb) {
            return !HasSpellBook(sb) && HasResourcesToCast(spellBooksOwner, sb);
        }

        /// <summary>
        /// Creates the spell. Single target version.
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public Spell CreateSpell(Page page, SpellBook spell, Character caster, Character target) {
            Util.Assert(spell.TargetType.TargetCount == TargetCount.SINGLE_TARGET);
            return spell.BuildSpell(page, caster, new Character[] { target });
        }

        /// <summary>
        /// Creates the spell. Multi-target version.
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public Spell CreateSpell(Page page, SpellBook spell, Character caster) {
            Util.Assert(spell.TargetType.TargetCount == TargetCount.MULTIPLE_TARGETS);
            return spell.BuildSpell(page, caster, spell.TargetType.GetTargets(caster, page));
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<ISpellable> IEnumerable<ISpellable>.GetEnumerator() {
            return set.Cast<ISpellable>().GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return set.GetEnumerator();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            var item = obj as SpellBooks;

            if (item == null) {
                return false;
            }

            return this.set.SetEquals(item.set);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() {
            return 0;
        }

        /// <summary>
        /// Gets the save object. A save object contains the neccessary
        /// information to initialize a clean class to its saved state.
        /// A save object is also serializable.
        /// </summary>
        /// <returns></returns>
        public CharacterSpellBooksSave GetSaveObject() {
            List<SpellBookSave> books = new List<SpellBookSave>();
            IEnumerable<SpellBook> enumer = this;
            foreach (SpellBook s in enumer) {
                books.Add(new SpellBookSave(s.GetType()));
            }
            return new CharacterSpellBooksSave(books);
        }

        /// <summary>
        /// Initializes from save object.
        /// </summary>
        /// <param name="saveObject">The save object.</param>
        public void InitFromSaveObject(CharacterSpellBooksSave saveObject) {
            foreach (SpellBookSave save in saveObject.Books) {
                SpellBook sb = save.CreateObjectFromID();
                set.Add(sb);
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<SpellBook> IEnumerable<SpellBook>.GetEnumerator() {
            return set.GetEnumerator();
        }
    }
}