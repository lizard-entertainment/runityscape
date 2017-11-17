using Scripts.Game.Defined.Serialized.Items;
using Scripts.Game.Serialized;
using Scripts.Model.Acts;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Items;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Game.Shopkeeper {

    /// <summary>
    /// Shops sell things!
    /// </summary>
    /// <seealso cref="Scripts.Model.Pages.PageGroup" />
    public class Shop : PageGroup {
        private static readonly Money MONEY = new Money();

        private Flags flags;
        private Party party;
        private float sellPriceMultiplier;
        private float buyPriceMultiplier;
        private ICollection<Talk> talks;
        private ICollection<Buy> buys;
        private Character shopkeeper;
        private Page previous;

        /// <summary>
        /// Initializes a new instance of the <see cref="Shop"/> class.
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="name">The name.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="party">The party.</param>
        /// <param name="sellPriceMultiplier">The sell price modifier.</param>
        /// <param name="buyPriceMultiplier">The buy price modifier.</param>
        /// <param name="shopkeeper">The shopkeeper.</param>
        public Shop(Page previous, string name, Flags flags, Party party, float sellPriceMultiplier, float buyPriceMultiplier, Character shopkeeper) : base(new Page(name)) {
            Root.SetTooltip("Items can be purchased and sold at shops.");
            Root.Icon = Util.GetSprite("battle-gear");
            this.previous = previous;
            this.flags = flags;
            this.party = party;
            this.sellPriceMultiplier = sellPriceMultiplier;
            this.buyPriceMultiplier = buyPriceMultiplier;
            this.shopkeeper = shopkeeper;
            this.talks = new List<Talk>();
            this.buys = new List<Buy>();
            Root.AddCharacters(Side.LEFT, party.Collection);
            Root.AddCharacters(Side.RIGHT, shopkeeper);
            AddOnEnter(() => Root.AddText(party.Default.Inventory.WealthText));
            SetupMenus();
        }

        /// <summary>
        /// Adds an on enter event
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public Shop AddOnEnter(Action action) {
            Root.OnEnter += action;
            return this;
        }

        /// <summary>
        /// Adds a purchasable item
        /// </summary>
        /// <param name="buys">The buys.</param>
        /// <returns></returns>
        public Shop AddBuys(params Buy[] buys) {
            foreach (Buy buy in buys) {
                this.buys.Add(buy);
            }
            return this;
        }

        public Shop AddBuys(params Item[] items) {
            foreach (Item item in items) {
                this.buys.Add(new Buy(item));
            }
            return this;
        }

        /// <summary>
        /// Adds a talk button.
        /// </summary>
        /// <param name="talks">The talks.</param>
        /// <returns></returns>
        public Shop AddTalks(params Talk[] talks) {
            foreach (Talk talk in talks) {
                this.talks.Add(talk);
            }
            return this;
        }

        /// <summary>
        /// Determines whether the party can buy the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if this instance can buy the specified item; otherwise, <c>false</c>.
        /// </returns>
        private bool CanBuy(Item item) {
            return (party.Shared.HasItem(MONEY, GetFullBuyPrice(item)) || Util.IS_DEBUG) && party.Shared.IsAddable(item);
        }

        /// <summary>
        /// Gets the full sell price.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private int GetFullSellPrice(Item item) {
            return Mathf.Max((int)(item.BasePrice * sellPriceMultiplier), 1);
        }

        /// <summary>
        /// Adds/removes to/from party's money.
        /// </summary>
        /// <param name="amount">The amount.</param>
        private void AddToMoney(int amount) {
            if (amount > 0) {
                party.Shared.Add(MONEY, amount);
            } else if (amount < 0) {
                party.Shared.Remove(MONEY, -amount);
            }
        }

        /// <summary>
        /// Gets the full buy price.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private int GetFullBuyPrice(Item item) {
            return Mathf.Max((int)(item.BasePrice * buyPriceMultiplier), 1);
        }

        /// <summary>
        /// Determines whether this instance can sell the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if this instance can sell the specified item; otherwise, <c>false</c>.
        /// </returns>
        private bool CanSell(Item item) {
            return party.Shared.HasItem(item) && item.BasePrice > 0;
        }

        /// <summary>
        /// Gets the party money count.
        /// </summary>
        /// <returns></returns>
        private int GetPartyMoneyCount() {
            return party.Shared.GetCount(MONEY);
        }

        private void SetupMenus() {
            Page p = Get(ROOT_INDEX);
            SubGrid main = new SubGrid("Shop Menu");
            main.OnEnter = () => {
                main.List.Clear();
                //main.List.Add(SetupTalkMenu(main)); TODO : not in use for the demo
                main.List.Add(SetupSellMenu(main));
                main.List.Add(SetupBuyMenu(main));
                main.List.Add(null);
                main.List.Add(PageUtil.GenerateGroupItemsGrid(p, main, party, PageUtil.GetOutOfBattlePlayableHandler(p)));
                main.List.Add(PageUtil.GenerateGroupEquipmentGrid(main, p, party.Collection, PageUtil.GetOutOfBattlePlayableHandler(p)));
                main.List.Add(null);
                main.List.Add(PageUtil.GenerateBack(previous));
            };
            p.OnEnter += () => {
                main.Invoke();
            };
        }

        private void PostMoneyAmount() {
            Get(ROOT_INDEX).AddText(string.Format("{0}s: {0}", MONEY.Name, party.Shared.GetCount(MONEY)));
        }

        private SubGrid SetupShopMenu<T>(
            IButtonable previous,
            string name,
            string spriteLoc,
            string tooltip,
            IEnumerable<T> items,
            Func<T, Action, Process> conversion,
            Func<T, int> sorter = null) {
            SubGrid grid = new SubGrid(name);
            grid.Icon = Util.GetSprite(spriteLoc);
            grid.Tooltip = tooltip;
            IEnumerable<T> itemsList = items.ToArray();
            if (sorter != null) {
                itemsList = itemsList.OrderBy(sorter);
            }
            grid.OnEnter = () => {
                grid.List.Clear();
                grid.List.Add(PageUtil.GenerateBack(previous));
                foreach (T item in itemsList) {
                    grid.List.Add(conversion(item, () => grid.Invoke()));
                };
            };
            return grid;
        }

        private SubGrid SetupBuyMenu(IButtonable previous) {
            return SetupShopMenu<Buy>(previous, "Buy", "buy-card", string.Format("Buy items with {0}s.", MONEY.Name), buys, (b, a) => GetBuyProcess(b, a), b => b.BasePrice);
        }

        private Process GetBuyProcess(Buy buy, Action postBuy) {
            return new Process(
                string.Format("{0}-{1}", buy.Name, GetFullBuyPrice(buy.GetItem())),
                buy.Sprite,
                string.Format("{0}\n\nBuy this item for {1} {2}(s).", buy.Description, GetFullBuyPrice(buy.GetItem()), MONEY.Name),
                () => {
                    Item item = buy.GetItem();

                    if (!Util.IS_DEBUG) {
                        AddToMoney(-GetFullBuyPrice(item));
                    }
                    party.Shared.Add(item);
                    Root.AddText(string.Format("Purchased {0} for {1} {2}(s).\n\nInventory: ({3})\n{2}s now: {4}",
                        item.Name,
                        GetFullBuyPrice(item).Color(Color.red),
                        MONEY.Name,
                        party.Shared.Fraction,
                        GetPartyMoneyCount()));
                    postBuy();
                },
                () => CanBuy(buy.GetItem())
                );
        }

        private SubGrid SetupSellMenu(IButtonable previous) {
            return SetupShopMenu<Item>(previous, "Sell", "sell-card", string.Format("Sell items for {0}s.", MONEY.Name), party.Shared, (i, a) => GetSellProcess(i, a));
        }

        private Process GetSellProcess(Item item, Action postSell) {
            return new Process(
                string.Format("{0}",
                    party.Shared.CountedItemName(item),
                    !CanSell(item) ?
                    string.Empty :
                    string.Format("-{0}", GetFullSellPrice(item))),
                item.Icon,
                !CanSell(item) ?
                "This item cannot be sold." :
                string.Format("{0}\nSell this item for {1} {2}(s).", item.Description, GetFullSellPrice(item), MONEY.Name),
                () => {
                    AddToMoney(GetFullSellPrice(item));
                    party.Shared.Remove(item);
                    Root.AddText(string.Format("Sold {0} for {1} {2}(s).\n\nInventory: ({3})\n{2}s now: {4}",
                        item.Name,
                        GetFullSellPrice(item).Color(Color.green),
                        MONEY.Name,
                        party.Shared.Fraction,
                        GetPartyMoneyCount()));
                    postSell();
                },
                () => CanSell(item)
                );
        }

        private SubGrid SetupTalkMenu(IButtonable previous) {
            return SetupShopMenu<Talk>(previous, "Talk", "talk", "Talk to the shopkeeper about something.", talks, (t, a) => GetTalkProcess(t, a));
        }

        private Process GetTalkProcess(Talk talk, Action postTalk) {
            return new Process(
                talk.Name,
                Util.GetSprite("talk"),
                string.Format("Talk about this topic.", talk.Name),
                () => {
                    ActUtil.SetupScene(
                        ActUtil.LongTalk(
                            Get(ROOT_INDEX),
                            shopkeeper,
                            talk.Big
                        ),
                            () => talk.SetFlags(flags),
                            () => postTalk()
                        );
                },
                () => talk.Condition(flags)
                ).SetVisibleOnDisable(false);
        }
    }
}