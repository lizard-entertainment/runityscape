using Scripts.Game.Defined.Serialized.Items;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.Spells;
using System;
using System.Collections.Generic;

namespace Scripts.Game.Shopkeeper {

    public class PurchasedSpell : IComparable<PurchasedSpell> {
        public readonly int Price;
        public readonly Func<SpellBook> Book;

        public PurchasedSpell(int price, SpellBook book) {
            Util.Assert(price > 0, "Price must be positive.");
            this.Price = price;
            this.Book = () => Util.TypeToObject<SpellBook>(book.GetType());
        }

        int IComparable<PurchasedSpell>.CompareTo(PurchasedSpell other) {
            return this.Price.CompareTo(other.Price);
        }
    }

    public class Trainer : PageGroup {

        public Trainer(Page previous, Party party, Character trainerPerson, params PurchasedSpell[] spells) : base(new Page("Spell Trainer")) {
            Root.SetTooltip("Spell Trainers can teach party members new abilities... for a fee, of course.");
            Root.Icon = Util.GetSprite("spell-book");
            SubGrid main = new SubGrid("Main");
            main.List.Add(PageUtil.GenerateBack(previous));
            Array.Sort(spells);
            foreach (PurchasedSpell spell in spells) {
                main.List.Add(
                    GetSpellPurchaseProcess(spell, party, main)
                    );
            }
            Root.AddCharacters(Side.LEFT, party);
            Root.AddCharacters(Side.RIGHT, trainerPerson);
            Root.Actions = main.List;
            Root.OnEnter = () => {
                Root.AddText(string.Format(party.Shared.WealthText));
            };
        }

        private Process GetSpellPurchaseProcess(PurchasedSpell spell, Party party, SubGrid previous) {
            SpellBook book = spell.Book();
            return new Process(
                    string.Format("{0} - {1}", book.Name, spell.Price),
                    book.Icon,
                    string.Format("Purchase {0} for {1} {2}s\n<color=cyan>{0}</color>\n{3}", book.Name, spell.Price, Money.NAME, book.TextboxTooltip.Text),
                    () => {
                        SubGrid grid = new SubGrid("Choice");
                        grid.List.Add(PageUtil.GenerateBack(previous));
                        foreach (Character partyMember in party) {
                            grid.List.Add(GetSpellTeachChoiceProcess(previous, book, spell.Price, partyMember));
                        }
                        grid.Invoke();
                    },
                    () => Util.IS_DEBUG || (party.Shared.GetCount(new Money()) >= spell.Price)
                );
        }

        private Process GetSpellTeachChoiceProcess(SubGrid main, SpellBook bookToLearn, int price, Character partyMember) {
            return new Process(
                partyMember.Look.DisplayName,
                partyMember.Look.Sprite,
                string.Format("{0} will learn {1}.", partyMember.Look.DisplayName, bookToLearn.Name),
                () => {
                    partyMember.Spells.AddSpellBook(bookToLearn);
                    if (!Util.IS_DEBUG) {
                        partyMember.Inventory.Remove(new Money(), price);
                    }
                    Root.AddText(
                        string.Format("<color=cyan>{0}</color> learned <color=yellow>{1}</color>.\n{2}",
                        partyMember.Look.DisplayName,
                        bookToLearn.Name,
                        partyMember.Inventory.WealthText
                        ));
                    main.Invoke();
                },
                () => partyMember.Spells.CanLearnSpellBook(partyMember.Stats, bookToLearn)
                );
        }
    }
}