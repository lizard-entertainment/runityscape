using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.Spells;
using UnityEngine;
using Scripts.Model.Characters;
using Scripts.Game.Defined.Spells;
using Scripts.Model.Items;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Model.Stats;
using System.Collections;
using Scripts.Model.Acts;

namespace Scripts.Game.Defined.Serialized.Brains {

    /// <summary>
    /// Player character's brain.
    /// </summary>
    public class Player : Brain {
        public static readonly Attack ATTACK = new Attack();

        /// <summary>
        /// Wait for an action to be chosen.
        /// </summary>
        public override void DetermineAction() {
            SubGrid main = new SubGrid("Main");

            main.List = new IButtonable[] {
                PageUtil.GenerateTargets(currentBattle, main, brainOwner, ATTACK, GetAttackSprite(brainOwner.Equipment), spellHandler),
                null,
                null,
                null,
                PageUtil.GenerateActions(currentBattle, main, brainOwner, ATTACK, temporarySpells, spellHandler),
                PageUtil.GenerateSpellBooks(currentBattle, main, brainOwner, ATTACK, spellHandler),
                PageUtil.GenerateItemsGrid(currentBattle, main, brainOwner, spellHandler, PageUtil.INVENTORY, brainOwner.Inventory.DetailedName, brainOwner.Inventory.DetailedDescription),
                PageUtil.GenerateEquipmentGrid(currentBattle, main, brainOwner, spellHandler, PageUtil.EQUIPMENT, "Equipment"),
            };
            currentBattle.Actions = main.List;
        }

        /// <summary>
        /// Attack's button sprite depends on your currently equipped weapon.
        /// </summary>
        /// <param name="equipmentOfOwner">Reference to owner's equipment.</param>
        /// <returns></returns>
        private Sprite GetAttackSprite(Equipment equipmentOfOwner) {
            if (equipmentOfOwner.Contains(EquipType.WEAPON)) {
                return equipmentOfOwner.PeekItem(EquipType.WEAPON).Icon;
            } else {
                return Util.GetSprite("fist");
            }
        }
    }

    public class DebugAI : Brain {
        public static readonly Attack ATTACK = new Attack();

        public override void DetermineAction() {
            //addPlay(Spells.CreateSpell(Attack, Owner, foes.PickRandom()));
        }
    }
}