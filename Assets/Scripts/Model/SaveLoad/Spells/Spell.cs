using Scripts.Model.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scripts.Presenter;

namespace Scripts.Model.Spells {

    /// <summary>
    /// Spells result from a spellbook being used on a target by a caster
    /// </summary>
    /// <seealso cref="IPlayable" />
    /// <seealso cref="System.IComparable{Scripts.Model.Spells.Spell}" />
    public class Spell : IPlayable, IComparable<Spell> {
        /// <summary>
        /// The spell text
        /// </summary>
        public const string SPELL_TEXT = "<color=yellow>{0}</color> {1} <color=cyan>{2}</color>{3}.{4}";
        /// <summary>
        /// The book
        /// </summary>
        public readonly SpellBook Book;
        /// <summary>
        /// The result
        /// </summary>
        public readonly Result Result;
        /// <summary>
        /// The caster
        /// </summary>
        public readonly SpellParams Caster;
        /// <summary>
        /// The target
        /// </summary>
        public readonly SpellParams Target;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spell"/> class.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="result">The result.</param>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        public Spell(SpellBook book, Result result, SpellParams caster, SpellParams target) {
            this.Book = book;
            this.Result = result;
            this.Caster = caster;
            this.Target = target;
        }

        /// <summary>
        /// Gets the spell declare text.
        /// </summary>
        /// <value>
        /// The spell declare text.
        /// </value>
        public string SpellDeclareText {
            get {
                return string.Format("<color=yellow>{0}</color> will {1} <color=cyan>{2}</color>{3}.",
                  Caster.Look.DisplayName,
                  Book.Verb.ToLower(),
                  Book.Name,
                  Caster.Equals(Target) ? string.Empty : string.Format(" on <color=yellow>{0}</color>", Target.Look.DisplayName)
              );
            }
        }

        /// <summary>
        /// Gets the spell text.
        /// </summary>
        /// <value>
        /// The spell text.
        /// </value>
        public string SpellText {
            get {
                return GetCastMessage(Caster, Target, this.Book, this.Result.Type);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is playable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is playable; otherwise, <c>false</c>.
        /// </value>
        public bool IsPlayable {
            get {
                return Book.IsCastableIgnoreResources(Caster, Target); // Resources already used up
            }
        }

        /// <summary>
        /// Gets the spell associated with the action.
        /// </summary>
        /// <value>
        /// My spell.
        /// </value>
        public Spell MySpell {
            get {
                return this;
            }
        }

        /// <summary>
        /// Gets the text associated with the action.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text {
            get {
                return SpellText;
            }
        }

        /// <summary>
        /// Gets the cast message.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <param name="book">The book.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static string GetCastMessage(SpellParams caster, SpellParams target, SpellBook book, ResultType result) {
            return string.Format("<color=yellow>{0}</color> {1} <color=cyan>{2}</color>{3}.{4}",
              caster.Look.DisplayName,
              book.Verb.ToLower() + 's',
              book.Name,
              caster.Equals(target) ? string.Empty : string.Format(" on <color=yellow>{0}</color>", target.Look.DisplayName),
              result.Text
          );
        }

        /// <summary>
        /// Skips this instance.
        /// </summary>
        /// <returns>
        /// Coroutine.
        /// </returns>
        public IEnumerator Skip() {
            foreach (SpellEffect se in Result.Effects) {
                se.CauseEffect();
            }
            yield break;
        }

        /// <summary>
        /// Plays this instance.
        /// </summary>
        /// <returns>
        /// Coroutine
        /// </returns>
        public IEnumerator Play() {
            return Cast();
        }

        /// <summary>
        /// Compares to another spell to determine which occurs first.
        /// </summary>
        /// <param name="other">The other spell.</param>
        /// <returns></returns>
        public int CompareTo(Spell other) {
            int diff = other.Book.Priority - this.Book.Priority; // 1 - (-1) = 2
            if (diff == 0) {
                return this.Caster.Stats.CompareTo(other.Caster.Stats);
            } else {
                return diff;
            }
        }

        /// <summary>
        /// Casts this instance.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Cast() {
            if (IsPlayable) {
                IList<IEnumerator> sfx = Result.SFX;
                for (int i = 0; i < sfx.Count; i++) {
                    yield return sfx[i];
                }
                foreach (SpellEffect mySE in Result.Effects) {
                    SpellEffect se = mySE;
                    se.CauseEffect();
                }
            } else {
                // Unplayable due to cast requirements becoming failed AFTER target selection due to a spellcast before this one
                Result.Type = ResultType.FAILED;
            }
        }

        /// <summary>
        /// Compares to another playable by spells.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(IPlayable other) {
            return this.CompareTo(other.MySpell);
        }
    }
}