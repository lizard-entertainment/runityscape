using System;
using System.Collections.Generic;
using Scripts.Model.Characters;
using Scripts.Model.Stats;
using Scripts.Model.Buffs;
using Scripts.Model.Items;
using Scripts.Model.Spells;
using Scripts.Model.Pages;
using Scripts.Game.Defined.SFXs;
using Scripts.Presenter;

namespace Scripts.Game.Defined.Spells {

    /// <summary>
    /// Adds to a stat's mod value.
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class AddToModStat : SpellEffect {

        /// <summary>
        /// The affected
        /// </summary>
        private readonly StatType affected;

        /// <summary>
        /// The target
        /// </summary>
        private readonly Stats target;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddToModStat"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="affected">The affected.</param>
        /// <param name="value">The value.</param>
        public AddToModStat(Stats target, StatType affected, int value) : base(value) {
            this.affected = affected;
            this.target = target;
        }

        /// <summary>
        /// Gets the affected stat.
        /// </summary>
        /// <value>
        /// The affected stat.
        /// </value>
        public StatType AffectedStat {
            get {
                return affected;
            }
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public override void CauseEffect() {
            target.AddToStat(affected, Stats.Set.MOD, Value);
        }
    }

    /// <summary>
    /// Add a buff
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class AddBuff : SpellEffect {

        /// <summary>
        /// The buff
        /// </summary>
        private readonly Buff buff;

        /// <summary>
        /// The target
        /// </summary>
        private readonly Buffs target;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddBuff"/> class.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <param name="buff">The buff.</param>
        public AddBuff(BuffParams caster, Buffs target, Buff buff) : base(1) {
            this.target = target;
            this.buff = buff;
            this.buff.Caster = caster;
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public override void CauseEffect() {
            target.AddBuff(buff);
        }
    }

    /// <summary>
    /// Params needed to equip an item
    /// </summary>
    public struct EquipParams {

        /// <summary>
        /// The caster
        /// </summary>
        public readonly Inventory Caster;

        /// <summary>
        /// The target
        /// </summary>
        public readonly Equipment Target;

        /// <summary>
        /// The item
        /// </summary>
        public readonly EquippableItem Item;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipParams"/> struct.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <param name="item">The item.</param>
        public EquipParams(Inventory caster, Equipment target, EquippableItem item) {
            this.Caster = caster;
            this.Target = target;
            this.Item = item;
        }
    }

    public class LearnSpellEffect : SpellEffect {
        private SpellBook spellToLearn;
        private SpellBooks learner;

        public LearnSpellEffect(SpellBooks learner, SpellBook spellToLearn) : base(1) {
            this.spellToLearn = spellToLearn;
            this.learner = learner;
        }

        public override void CauseEffect() {
            learner.AddSpellBook(spellToLearn);
        }
    }

    /// <summary>
    /// Equips an item
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class EquipItemEffect : SpellEffect {

        /// <summary>
        /// The ep
        /// </summary>
        private EquipParams ep;

        /// <summary>
        /// The bp
        /// </summary>
        private BuffParams bp;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipItemEffect"/> class.
        /// </summary>
        /// <param name="ep">The ep.</param>
        /// <param name="bp">The bp.</param>
        public EquipItemEffect(EquipParams ep, BuffParams bp) : base(1) {
            this.ep = ep;
            this.bp = bp;
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public override void CauseEffect() {
            ep.Target.AddEquip(ep.Caster, bp, ep.Item);
        }
    }

    /// <summary>
    /// Unequips an item
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class UnequipItemEffect : SpellEffect {

        /// <summary>
        /// The ep
        /// </summary>
        private EquipParams ep;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnequipItemEffect"/> class.
        /// </summary>
        /// <param name="ep">The ep.</param>
        public UnequipItemEffect(EquipParams ep) : base(1) {
            this.ep = ep;
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public override void CauseEffect() {
            ep.Target.RemoveEquip(ep.Caster, ep.Item.Type);
        }
    }

    public class ChangeLookEffect : SpellEffect {
        private Character caster;
        private Look targetLook;

        public ChangeLookEffect(Character caster, Look target) : base(1) {
            this.caster = caster;
            this.targetLook = target;
        }

        public override void CauseEffect() {
            caster.Look = targetLook;
        }
    }

    /// <summary>
    /// Consumes an item
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class ConsumeItemEffect : SpellEffect {

        /// <summary>
        /// The caster
        /// </summary>
        private Inventory caster;

        /// <summary>
        /// The item
        /// </summary>
        private Item item;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumeItemEffect"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="caster">The caster.</param>
        public ConsumeItemEffect(Item item, Inventory caster) : base(1) {
            this.caster = caster;
            this.item = item;
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public override void CauseEffect() {
            caster.Remove(item, Value);
        }
    }

    /// <summary>
    /// Makes research more or less visible.
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class AddToResourceVisibility : SpellEffect {

        /// <summary>
        /// The target
        /// </summary>
        private Stats target;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddToResourceVisibility"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        public AddToResourceVisibility(Stats target, int value) : base(value) {
            this.target = target;
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public override void CauseEffect() {
            if (Value > 0) {
                target.IncreaseResourceVisibility();
            } else if (Value < 0) {
                target.DecreaseResourceVisibility();
            }
        }
    }

    /// <summary>
    /// Posts text to the page.
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class PostText : SpellEffect {

        /// <summary>
        /// The message
        /// </summary>
        private string message;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostText"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PostText(string message) : base(1) {
            this.message = message;
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public override void CauseEffect() {
            Page.TypeText(new Model.TextBoxes.TextBox(message));
        }
    }

    /// <summary>
    /// Changes the page
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class GoToPage : SpellEffect {

        /// <summary>
        /// The destination
        /// </summary>
        private Page destination;

        /// <summary>
        /// The stop battle
        /// </summary>
        private Action stopBattle;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoToPage"/> class.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="stopBattle">The stop battle.</param>
        public GoToPage(Page destination, Action stopBattle) : base(1) {
            this.destination = destination;
            this.stopBattle = stopBattle;
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public override void CauseEffect() {
            Page.ChangePageFunc(destination);
            stopBattle.Invoke();
        }
    }

    /// <summary>
    /// Produces clones
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class CloneEffect : SpellEffect {

        /// <summary>
        /// To be cloned
        /// </summary>
        private Func<Character> toBeCloned;

        /// <summary>
        /// The side
        /// </summary>
        private Side side;

        /// <summary>
        /// The current page
        /// </summary>
        private Page currentPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloneEffect"/> class.
        /// </summary>
        /// <param name="numberOfClones">The number of clones.</param>
        /// <param name="side">The side.</param>
        /// <param name="toBeCloned">To be cloned.</param>
        /// <param name="page">The page.</param>
        public CloneEffect(int numberOfClones, Side side, Func<Character> toBeCloned, Page page) : base(numberOfClones) {
            this.toBeCloned = toBeCloned;
            this.side = side;
            this.currentPage = page;
        }

        /// <summary>
        /// Causes the effect. Kill existing clones. Make new ones.
        /// </summary>
        public override void CauseEffect() {
            List<Character> clonesToBeKilled = new List<Character>();
            foreach (Character possibleClone in currentPage.GetCharacters(side)) {
                if (possibleClone.HasFlag(Model.Characters.Flag.IS_CLONE)) {
                    clonesToBeKilled.Add(possibleClone);
                }
            }
            foreach (Character cloneToBeRemoved in clonesToBeKilled) {
                cloneToBeRemoved.Stats.SetToStat(StatType.HEALTH, Stats.Set.MOD, 0);
            }
            for (int i = 0; i < Value; i++) {
                Character clone = toBeCloned();
                clone.AddFlag(Model.Characters.Flag.IS_CLONE);
                currentPage.AddCharacters(side, clone);
            }
        }
    }

    /// <summary>
    /// Increases inventory capacity.
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class IncreaseInventoryCapacityEffect : SpellEffect {
        private Inventory target;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncreaseInventoryCapacityEffect"/> class.
        /// </summary>
        /// <param name="target">The target inventory to increases.</param>
        /// <param name="amountToIncreaseInventoryBy">The amount to increase inventory by.</param>
        public IncreaseInventoryCapacityEffect(Inventory target, int amountToIncreaseInventoryBy)
            : base(amountToIncreaseInventoryBy) {
            this.target = target;
        }

        public override void CauseEffect() {
            target.Capacity += Value;
        }
    }

    /// <summary>
    /// Shuffles a side.
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellEffect" />
    public class ShuffleEffect : SpellEffect {

        /// <summary>
        /// The page
        /// </summary>
        private Page page;

        /// <summary>
        /// The side
        /// </summary>
        private Side side;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShuffleEffect"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="side">The side.</param>
        public ShuffleEffect(Page page, Side side) : base(1) {
            this.page = page;
            this.side = side;
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public override void CauseEffect() {
            foreach (Character toBeShuffled in page.GetCharacters(side)) {
                Main.Instance.StartCoroutine(SFX.DoSteamEffect(toBeShuffled));
            }
            page.Shuffle(side);
        }
    }
}