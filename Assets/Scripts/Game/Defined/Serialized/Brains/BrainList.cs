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
using Scripts.Game.Defined.Unserialized.Spells;
using Scripts.Game.Defined.Serialized.Buffs;

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

namespace Scripts.Game.Defined.Unserialized.Brains {

    public class DummyBrain : BasicBrain {
        private static readonly Wait WAIT = new Wait();
        private static readonly Attack ATTACK = new Attack();
        private static readonly Check CHECK = new Check();
        private static readonly SelfDestruct SUICIDE = new SelfDestruct();

        private Status state;

        public override string StartOfRoundDialogue() {
            string dialog = string.Empty;
            if (foes.Any(c => !CHECK.CasterHasResources(c))) {
                dialog = string.Format("You... ran out of {0} to cast {1}? Really?", StatType.MANA, CHECK);
                state = Status.SUICIDE;
            }
            switch (state) {
                case Status.ATTACK:
                    dialog = "Hit me with an Attack!";
                    break;

                case Status.SPELL:
                    if (brainOwner.Buffs.HasBuff<BasicChecked>()) {
                        dialog = "Nice job!";
                        state = Status.TOOLTIPS;
                    } else {
                        dialog = string.Format("Go into Spell and use {0}!", CHECK);
                    }
                    break;

                case Status.TOOLTIPS:
                    dialog = string.Format("If an enemy uses a spell that you don't know, just hover over the textbox to learn about it! I will now self-destruct.");
                    state = Status.SUICIDE;
                    break;
            }
            return dialog;
        }

        public override string ReactToSpell(SingleSpell spellTargetingUs) {
            string dialog = string.Empty;
            switch (state) {
                case Status.ATTACK:
                    if (spellTargetingUs.SpellBook is Attack) {
                        dialog = string.Format("Nice job! Now for a Spell. I want to see you {0} me!", CHECK);
                        state = Status.SPELL;
                    } else {
                        dialog = string.Format("That was not a {0}! Try again!", ATTACK);
                    }
                    break;

                case Status.SPELL:
                    if (spellTargetingUs.SpellBook is Check) {
                        dialog = string.Format("Nice work! Now see that textbox you created? Hover over it to learn more about the ability!", ATTACK);
                        state = Status.TOOLTIPS;
                    } else {
                        dialog = string.Format("That was not a {0}! Try again!", CHECK);
                    }
                    break;

                case Status.SUICIDE:
                    break;
            }

            return dialog;
        }

        protected override Spell GetSpell() {
            if (state != Status.SUICIDE) {
                return CastOnRandom(WAIT);
            } else {
                return CastOnRandom(SUICIDE);
            }
        }

        private bool IsSpellCastByFoe(Spell spellToCheck) {
            return foes.Contains(spellToCheck.Caster);
        }

        private enum Status {
            ATTACK,
            SPELL,
            TOOLTIPS,
            SUICIDE
        }
    }
}