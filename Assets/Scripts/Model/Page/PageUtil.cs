using Scripts.Game.Serialized;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Items;
using Scripts.Model.Processes;
using Scripts.Model.Spells;
using Scripts.Presenter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Pages {

    public static class PageUtil {
        public static readonly Sprite EQUIPMENT = Util.GetSprite("battle-gear");
        public static readonly Sprite INVENTORY = Util.GetSprite("knapsack");
        public static readonly Sprite SPELLBOOK = Util.GetSprite("spell-book");
        public static readonly Sprite ACTION = Util.GetSprite("talk");

        public static Func<bool> GetVisitProcessCondition(Flags flags, Party party) {
            return () => flags.Time != TimeOfDay.NIGHT && party.Any(c => c.Stats.State == State.ALIVE);
        }

        public static void GetFirstPlaceVisitMessage(PageGroup place) {
            place.Root.AddText(string.Format("{0} was discovered.\nThis place can be visited again in the <color=yellow>Places</color> menu back at camp.", place.Root.Location));
        }

        /// <summary>
        /// Creates a Grid asking if the user wants to perform a task.
        /// </summary>
        /// <param name="current">Page grid is on</param>
        /// <param name="previous">Previous buttonable</param>
        /// <param name="confirm">Process serving as the confirm button</param>
        /// <param name="name">Name of this grid</param>
        /// <param name="tooltip">Tooltip of this grid</param>
        /// <param name="confirmationQuestion">Question to ask when grid is entered.</param>
        /// <returns></returns>
        public static SubGrid GetConfirmationGrid(Page current, IButtonable previous, Process confirm, string name, string tooltip, string confirmationQuestion) {
            SubGrid g = new SubGrid(name);
            g.Tooltip = tooltip;
            g.OnEnter += () => {
                current.AddText(confirmationQuestion);
            };
            g.List.Add(PageUtil.GenerateBack(previous));
            g.List.Add(confirm);
            return g;
        }

        /// <summary>
        /// Gets a confirmation page asking if the user is sure they want to do a particular action.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="previous">The previous.</param>
        /// <param name="confirm">The confirm.</param>
        /// <param name="name">The name.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <param name="confirmationQuestion">The confirmation question.</param>
        /// <returns></returns>
        public static Page GetConfirmationPage(Page current, IButtonable previous, Process confirm, string name, string tooltip, string confirmationQuestion) {
            Page p = new Page(name);
            p.Body = confirmationQuestion;

            p.Actions = new IButtonable[] {
                GenerateBack(previous),
                confirm
            };
            return p;
        }

        /// <summary>
        /// Create a Back Process, which is used
        /// to leave a subgrid.
        /// </summary>
        /// <param name="previous"></param>
        /// <returns></returns>
        public static Process GenerateBack(IButtonable previous) {
            return new Process(
                "Back",
                Util.GetSprite("plain-arrow"),
                string.Format("Go back to {0}.", previous.ButtonText),
                () => previous.Invoke()
                );
        }

        /// <summary>
        /// Gets the out of battle playable handler.
        /// Allows me to use the same Item usage logic in battle
        /// as out of battle, allowing for
        /// CODE REUSE!
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public static Action<Spell> GetOutOfBattlePlayableHandler(Page page) {
            return (ip) => {
                Main.Instance.StartCoroutine(PerformInOrder(page, ip, () => { }));
            };
        }

        /// <summary>
        /// Create a subgrid that can be left to a main grid.
        /// </summary>
        /// <param name="previous">Content to go back to</param>
        /// <param name="icon">Icon on the Button leading to this submenu</param>
        /// <param name="name">Name on the button leading to this submenu</param>
        /// <param name="tooltip">Description on the button leading to this submenu</param>
        /// <returns></returns>
        private static SubGrid GenerateBackableGrid(IButtonable previous, Sprite icon, string name, string tooltip) {
            SubGrid grid = new SubGrid(name);
            grid.Icon = icon;
            grid.Tooltip = tooltip;
            List<IButtonable> buttons = new List<IButtonable>();
            buttons.Add(GenerateBack(previous));
            grid.List = buttons;
            return grid;
        }

        public static SubGrid GenerateGroupItemsGrid(
            Page current,
            IButtonable previous,
            IEnumerable<Character> party,
            Action<Spell> addPlay
            ) {
            SubGrid grid = GenerateBackableGrid(previous, INVENTORY, party.FirstOrDefault().Inventory.DetailedName, party.FirstOrDefault().Inventory.DetailedDescription);

            foreach (Character partyMember in party) {
                if (partyMember.Stats.State == State.ALIVE) {
                    // TODO Exclude items that cannot be used out of combat
                    grid.List.Add(
                        GenerateItemsGrid(
                            current,
                            grid,
                            partyMember,
                            addPlay,
                            partyMember.Look.Sprite,
                            partyMember.Look.DisplayName,
                            string.Format("{0} will be the item's caster.",
                            partyMember.Look.DisplayName)));
                } else {
                    grid.List.Add(
                        GetDeadCharacterProcess(partyMember)
                    );
                }
            }
            return grid;
        }

        private static Process GetDeadCharacterProcess(Character partyMember) {
            return new Process(
                        Util.ColorString(partyMember.Look.DisplayName, false),
                        partyMember.Look.Sprite,
                        "This unit is dead and is unable to be a caster.");
        }

        /// <summary>
        /// Generates a grid with inventory items
        /// to use
        /// </summary>
        /// <param name="current">Page the owner is on</param>
        /// <param name="previous">Main grid</param>
        /// <param name="owner">Owner of the inventory</param>
        /// <param name="addPlay">IPlayable handler</param>
        /// <returns></returns>
        public static SubGrid GenerateItemsGrid(
                    Page current,
                    IButtonable previous,
                    Character owner,
                    Action<Spell> addPlay,
                    Sprite sprite,
                    string name,
                    string description
                    ) {
            return GenerateSpellableGrid(
                current,
                previous,
                owner,
                null,
                owner.Inventory,
                addPlay,
                sprite,
                name,
                description
                );
        }

        /// <summary>
        /// Generate a Grid containing all SpellBooks
        /// in Characters.SpellBooks
        /// </summary>
        /// <param name="current">Page the Owner is on</param>
        /// <param name="previous">Previous IButtonable you want this Grid to go back to</param>
        /// <param name="owner">Owner of the SpellBooks</param>
        /// <param name="excluded">SpellBook to exclude, typically 'Attack'</param>
        /// <param name="spellCollection">SpellBooks owned by Owner</param>
        /// <param name="addPlay">Chosen action handler</param>
        /// <returns></returns>
        public static SubGrid GenerateSpellBooks(
            Page current,
            IButtonable previous,
            Character owner,
            SpellBook excluded,
            Action<Spell> addPlay
            ) {
            return GenerateSpellableGrid(
                current,
                previous,
                owner,
                excluded,
                ((IEnumerable<ISpellable>)owner.Spells).Where(s => s.GetSpellBook().Costs.Count > 0),
                addPlay,
                SPELLBOOK,
                "Spells",
                "Cast a spell.");
        }

        public static SubGrid GenerateGroupSpellBooks(
            Page current,
            IButtonable previous,
            ICollection<Character> party
            ) {
            SubGrid grid = new SubGrid("Spells");
            grid.Tooltip = "Cast spells out of combat.";
            grid.Icon = SPELLBOOK;
            grid.List.Add(PageUtil.GenerateBack(previous));
            foreach (Character member in party) {
                IButtonable itemToAdd = null;
                if (member.Stats.State == State.ALIVE) {
                    itemToAdd = GenerateSpellableGrid(
                        current,
                        previous,
                        member,
                        null,
                        ((IEnumerable<ISpellable>)member.Spells)
                            .Where(b => b.GetSpellBook().HasFlag(Spells.Flag.USABLE_OUT_OF_COMBAT)
                                    && !b.GetSpellBook().TargetType.IsTargetEnemies),
                        GetOutOfBattlePlayableHandler(current),
                        member.Look.Sprite,
                        member.Look.DisplayName,
                        "This unit will be casting spells."
                        );
                } else {
                    itemToAdd = GetDeadCharacterProcess(member);
                }
                grid.List.Add(itemToAdd);
            }
            return grid;
        }

        /// <summary>
        /// Actions are spells without a cost.
        /// </summary>
        /// <param name="current">The current page.</param>
        /// <param name="previous">The previous page.</param>
        /// <param name="owner">The owner of the action.</param>
        /// <param name="excluded">The excluded spell.</param>
        /// <param name="concat">The spells to add alongside the spellbook's.</param>
        /// <param name="playHandler">The play handler.</param>
        /// <returns></returns>
        public static SubGrid GenerateActions(
            Page current,
            IButtonable previous,
            Character owner,
            SpellBook excluded,
            IEnumerable<ISpellable> concat,
            Action<Spell> playHandler
            ) {
            List<SpellBook> spellsThatHaveACost = new List<SpellBook>();

            return GenerateSpellableGrid(
                current,
                previous,
                owner,
                excluded,
                ((IEnumerable<ISpellable>)owner.Spells).Where(s => s.GetSpellBook().Costs.Count <= 0).ToList().Concat(concat),
                playHandler,
                ACTION,
                "Action",
                "Perform an action.");
        }

        /// <summary>
        /// Creates a subGrid containing all possible targets
        /// of spellable
        /// </summary>
        /// <param name="current">Page that owner is on</param>
        /// <param name="previous">supergrid containing a list of possible ISpellables to use</param>
        /// <param name="owner">User of the Spellable</param>
        /// <param name="spellable">Spellable to use on target</param>
        /// <param name="spellHandler">Spell handler</param>
        /// <returns></returns>
        public static SubGrid GenerateTargets(Page current, IButtonable previous, Character owner, ISpellable spellable, Action<Spell> spellHandler) {
            return GenerateTargets(current, previous, owner, spellable, spellable.GetSpellBook().Icon, spellHandler);
        }

        /// <summary>
        /// Creates a subGrid containing all possible targets
        /// of spellable
        /// </summary>
        /// <param name="current">Page that owner is on</param>
        /// <param name="previous">supergrid containing a list of possible ISpellables to use</param>
        /// <param name="caster">User of the Spellable</param>
        /// <param name="spellable">Spellable to use on target</param>
        /// <param name="spellHandler">Spell handler</param>
        /// <returns></returns>
        public static SubGrid GenerateTargets(
            Page current,
            IButtonable previous,
            Character caster,
            ISpellable spellable,
            Sprite sprite,
            Action<Spell> spellHandler) {
            SpellBook sb = spellable.GetSpellBook();
            ICollection<Character> targets = sb.TargetType.GetTargets(caster, current);
            SubGrid grid = GenerateBackableGrid(previous, sb.Icon, sb.Name, sb.CreateDescription(caster));

            grid.Icon = sprite;

            ICollection<Process> targetProcesses = spellable.GetSpellBook().TargetType.GetTargetProcesses(current, spellable, caster, spellHandler);
            foreach (Process targetProcess in targetProcesses) {
                grid.List.Add(targetProcess
                    .AddDisabledText("This unit cannot be targeted because the castable's requirements have not been met."));
            }

            Item item = spellable as Item;
            if (item != null && item.HasFlag(Items.Flag.OCCUPIES_SPACE)) {
                SpellBook tossItem = new TossItem(item, caster.Inventory);
                Util.Log(tossItem.Name + tossItem.HasFlag(Spells.Flag.USABLE_OUT_OF_COMBAT));
                Util.Log("isInBattle? " + current.IsInBattle);
                grid.List.Add(
                    new Process(
                        string.Format("Toss"),
                        string.Format("Throw away {0}.", item.Name),
                        () => {
                            spellHandler(
                                caster.Spells.CreateSpell(current, tossItem, caster, caster)
                                );
                        },
                        () => tossItem.IsCastable(current, caster, new Character[] { caster })
                        ));
            }
            return grid;
        }

        /// <summary>
        /// Creates a subgrid containing user's equipment
        /// </summary>
        /// <param name="previous">supergrid</param>
        /// <param name="owner">Owner of the equipment</param>
        /// <param name="spellHandler">Playable handler</param>
        /// <returns></returns>
        public static SubGrid GenerateEquipmentGrid(Page current, IButtonable previous, Character owner, Action<Spell> spellHandler, Sprite sprite, string name) {
            SubGrid grid = GenerateBackableGrid(
                previous,
                sprite,
                name,
                string.Format("Manage {0}'s equipment.", owner.Look.DisplayName));

            grid.OnEnter = () => {
                grid.List.Clear();
                grid.List.Add(GenerateBack(previous));
                foreach (EquipType myET in EquipType.AllTypes) {
                    EquipType et = myET;
                    IButtonable ib = null;
                    Equipment eq = owner.Equipment;
                    if (owner.Equipment.Contains(et)) {
                        CastUnequipItem unequip = new CastUnequipItem(owner.Inventory, owner.Equipment, eq.PeekItem(et));
                        ib = GetUnequipProcess(current, unequip, owner, grid, spellHandler);
                    } else {
                        ib = GetEquipGrid(current, owner, et, owner.Inventory, grid, spellHandler);
                    }
                    grid.List.Add(ib);
                }
            };
            return grid;
        }

        public static SubGrid GenerateGroupEquipmentGrid(IButtonable previous, Page current, ICollection<Character> party, Action<Spell> spellHandler) {
            SubGrid grid = new SubGrid("Equipment");
            grid.Icon = EQUIPMENT;
            grid.List.Add(GenerateBack(previous));

            foreach (Character partyMember in party) {
                if (partyMember.Stats.State == State.ALIVE) {
                    grid.List.Add(GenerateEquipmentGrid(current, grid, partyMember, spellHandler, partyMember.Look.Sprite, partyMember.Look.DisplayName));
                } else {
                    grid.List.Add(
                        GetDeadCharacterProcess(partyMember)
                    );
                }
            }

            return grid;
        }

        private static SubGrid GenerateSpellableGrid(
                    Page current,
                    IButtonable previous,
                    Character owner,
                    SpellBook excluded,
                    IEnumerable<ISpellable> spellCollection,
                    Action<Spell> addPlay,
                    Sprite sprite,
                    string name,
                    string description) {
            SubGrid grid = GenerateBackableGrid(previous, sprite, name, description);

            grid.OnEnter = () => {
                grid.List.Clear();
                grid.List.Add(GenerateBack(previous));
                foreach (ISpellable myS in spellCollection) {
                    ISpellable s = myS;
                    if (!s.Equals(excluded)) {
                        grid.List.Add(GenerateSpellProcess(current, grid, owner, s, addPlay));
                    }
                }
            };

            return grid;
        }

        private static Process GenerateSpellProcess(Page current, IButtonable previous, Character owner, ISpellable spellable, Action<Spell> spellHandler) {
            SpellBook sb = spellable.GetSpellBook();
            return new Process(
                sb.GetDetailedName(owner),
                sb.Icon,
                sb.CreateDescription(owner),
                () => {
                    if (sb.IsMeetPreTargetRequirements(owner)) {
                        GenerateTargets(current, previous, owner, spellable, spellHandler).Invoke();
                    }
                });
        }

        private static SubGrid GetEquipGrid(
            Page current,
            Character owner,
            EquipType et,
            Inventory inv,
            IButtonable previous,
            Action<Spell> spellHandler) {
            SubGrid grid = GenerateBackableGrid(
                previous,
                et.Sprite,
                Util.ColorString(et.Name, Color.grey),
                string.Format("Equip an item in the <color=yellow>{0}</color> slot.", et.Name)
                );
            foreach (EquippableItem ei in inv as IEnumerable<EquippableItem>) {
                if (ei.Type.Equals(et)) {
                    grid.List.Add(ei.GetSelfTargetProcess(previous, current, owner, spellHandler));
                }
            }
            return grid;
        }

        private static Process GetUnequipProcess(Page current, CastUnequipItem unequipSpell, Character owner, IButtonable previous, Action<Spell> spellHandler) {
            return new Process(unequipSpell.Name, unequipSpell.Icon, unequipSpell.CreateDescription(owner),
                        () => {
                            spellHandler(owner.Spells.CreateSpell(current, unequipSpell, owner, owner));
                            previous.Invoke();
                        },
                        () => unequipSpell.IsCastable(current, owner, new Character[] { owner })
                        );
        }

        /// <summary>
        /// There's a slight delay to Play()'s effects occurring, so we wait for
        /// Play() to be completely finished, and then perform our action.
        ///
        /// If we don't do this, then item counts won't be updated since it'll go in this order:
        /// Play coroutine started
        /// Item buttons get setup
        /// Used item is decremented in count by 1
        /// </summary>
        /// <param name="playgroup">PlayGroup</param>
        /// <param name="postAction">Action to perform after PlayGroup is finished.</param>
        /// <returns></returns>
        private static IEnumerator PerformInOrder(Page page, Spell spell, Action postAction) {
            yield return spell.Play(page, true);
            postAction.Invoke();
        }
    }
}