﻿using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.View.ActionGrid;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Model.World.Pages {

    /// <summary>
    /// A page where the user can loot the spoils from a battle.
    /// </summary>
    public class LootPage : ReadPage {
        private Page back;

        private Inventory inventory;

        private List<Item> loot;

        public LootPage(BattlePage back, Party looters, IList<Character> defeated) : base(looters, "", "", "", "Looting", null, null) {
            this.back = back;
            this.inventory = looters.Inventory;
            this.loot = ExtractLoot(defeated);
            OnTickAction += () => {
                this.ClearActionGrid();
                CreateTooltip();
                DisplayLoot();
                ActionGrid[ActionGridView.TOTAL_BUTTON_COUNT - 1] = back;
            };
        }

        public bool HasKeyItems {
            get {
                return loot.Any(i => i.IsKeyItem);
            }
        }

        public bool HasLoot {
            get {
                return loot.Count > 0;
            }
        }

        private void CreateTooltip() {
            Tooltip = inventory.SizeText;
            if (inventory.IsFull) {
                Tooltip += "\n<color=red>Your inventory is full.</color>";
            }
        }

        private void DisplayLoot() {
            for (int i = 0; i < ActionGridView.TOTAL_BUTTON_COUNT - 1 && i < loot.Count; i++) {
                ActionGrid[i] = LootProcess(loot[i]);
            }
        }

        private List<Item> ExtractLoot(IList<Character> defeated) {
            List<Item> loot = new List<Item>();

            // Get inventories and equips
            foreach (Character _c in defeated) {
                Character c = _c;
                foreach (Item i in c.Inventory) {
                    loot.Add(i);
                }
                foreach (EquippableItem _e in c.Equipment) {
                    EquippableItem e = _e;
                    if (e != null) {
                        loot.Add(e);
                    }
                }
            }

            return loot;
        }

        private Process LootProcess(Item i) {
            return new Process(i.Name, string.Format("Loot {0}.\n{1}", i.Name, i.Description), () => {
                inventory.Add(i);
                loot.Remove(i);
            }
                ,
                () => !inventory.IsFull
            );
        }
    }
}