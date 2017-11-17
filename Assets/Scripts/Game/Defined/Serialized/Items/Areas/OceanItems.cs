using System;
using System.Collections.Generic;
using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Model.Buffs;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using Scripts.Game.Defined.Spells;
using Scripts.Game.Defined.Unserialized.Items;
using Scripts.Game.Defined.Unserialized.Spells;
using Scripts.Game.Defined.Unserialized.Buffs;
using Scripts.Game.Defined.Characters;
using Scripts.Model.Pages;
using System.Linq;

namespace Scripts.Game.Defined.Serialized.Items {
    // Drops

    public class SharkSkin : EquippableItem {

        public SharkSkin() : base("spiked-armor", EquipType.ARMOR, 50, "Shark Skin", "Rough skin dropped by a shark.") {
            AddFlatStatBonus(StatType.AGILITY, 5);
            AddFlatStatBonus(StatType.VITALITY, 2);
        }

        public override PermanentBuff CreateBuff() {
            return new RoughSkin();
        }
    }

    public class FishingPole : EquippableItem {

        public FishingPole() : base("fishing-pole", EquipType.WEAPON, 20, "Fishing Pole", "Too bad there's no Fishing skill in this game.") {
            AddFlatStatBonus(StatType.STRENGTH, 1);
            AddFlatStatBonus(StatType.AGILITY, -1);
        }
    }

    public class Dynamite : ConsumableItem {
        private const int DAMAGE = 5;

        public Dynamite() : base(
            "dynamite",
            20,
            TargetType.ALL_FOE,
            "Dynamite",
            string.Format("Used for illegal blast fishing. Deals {0} damage to all foes.", DAMAGE)) {
            Util.Log("removing usable");
            this.flags.Remove(Model.Items.Flag.USABLE_OUT_OF_COMBAT);
        }

        public override IList<SpellEffect> GetEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, -DAMAGE)
            };
        }
    }

    public class ShellArmor : EquippableItem {

        public ShellArmor() : base("turtle-shell", EquipType.OFFHAND, 500, "Shell Armor", "Shell from an endangered species.") {
            AddFlatStatBonus(StatType.VITALITY, 10);
        }

        public override PermanentBuff CreateBuff() {
            return new DamageResist();
        }
    }

    public class SharkTooth : EquippableItem {

        public SharkTooth() : base("curvy-knife", EquipType.WEAPON, 50, "Shark Tooth", "A sharp tooth from a shark. Looks like a knife for some reason.") {
            AddFlatStatBonus(StatType.AGILITY, 4);
            AddFlatStatBonus(StatType.STRENGTH, 2);
        }
    }

    public class CrackedSharkTooth : EquippableItem {

        public CrackedSharkTooth() : base("plain-dagger", EquipType.WEAPON, 25, "Cracked Tooth", "A cracked tooth from a shark. Looks like a dagger for some reason.") {
            AddFlatStatBonus(StatType.AGILITY, 1);
            AddFlatStatBonus(StatType.STRENGTH, 2);
        }
    }

    public class CrackedOrb : EquippableItem {

        public CrackedOrb() : base("unstable-orb", EquipType.OFFHAND, 25, "Cracked Orb", "The orb is cracked and is as dry as a desert.") {
            AddFlatStatBonus(StatType.INTELLECT, 3);
        }
    }

    public class WaterOrb : EquippableItem {

        public WaterOrb() : base("at-sea", EquipType.OFFHAND, 50, "Orb of Flowing Water", "Water is seeping out of it.") {
            AddFlatStatBonus(StatType.INTELLECT, 5);
        }

        public override PermanentBuff CreateBuff() {
            return new RegenerateMana();
        }
    }

    public class SharkBlood : ConsumableItem {
        private int HEALING_RANGE = 20;

        public SharkBlood() : base("water-drop", 10, TargetType.ONE_ALLY, "Shark Blood", "Blood from a questionable source. Can heal or harm the consumer.") {
        }

        public override IList<SpellEffect> GetEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, Util.RandomRange(-HEALING_RANGE, HEALING_RANGE))
            };
        }
    }

    public class ToothNecklace : EquippableItem {

        public ToothNecklace() : base("saber-tooth", EquipType.TRINKET, 40, "Sharktooth Necklace", "Pretend you're a surfer.") {
            AddFlatStatBonus(StatType.AGILITY, 5);
            AddFlatStatBonus(StatType.STRENGTH, 5);
        }
    }

    public class CrackedToothNecklace : EquippableItem {

        public CrackedToothNecklace() : base("saber-tooth", EquipType.TRINKET, 10, "Cracked Necklace", "It's missing some teeth.") {
            AddFlatStatBonus(StatType.AGILITY, 2);
            AddFlatStatBonus(StatType.STRENGTH, 2);
        }
    }

    public class OctopusLeg : HealingItem {

        public OctopusLeg() : base("Octopus Leg", "suckered-tentacle", "Leg from an octopus or similar creature.", 10, 8) {
        }
    }

    public class SirenNote : ConsumableItem {

        private static readonly Func<Buff>[] POSSIBLE_EFFECTS = new Func<Buff>[] {
            () => new StrengthSirenSong(),
            () => new AgilitySirenSong(),
            () => new IntellectSirenSong(),
            () => new VitalitySirenSong()
        };

        public SirenNote() : base("g-clef", 50, TargetType.ALL_FOE, "Frozen Note", "Applies a random Siren song to all enemies.") {
            flags.Remove(Model.Items.Flag.USABLE_OUT_OF_COMBAT);
        }

        public override IList<SpellEffect> GetEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddBuff(new BuffParams(caster), target.Buffs, POSSIBLE_EFFECTS.ChooseRandom().Invoke())
            };
        }
    }

    public class BlackWater : ManaItem {
        private const int MANA_RESTORE = 5;

        public BlackWater() : base("Blackwater", "boiling-bubbles", "Water mixed with tar from a questionable source.", 10, MANA_RESTORE) {
        }
    }

    public class GrayWater : ManaItem {
        private const int MANA_RESTORE = 10;

        public GrayWater() : base("Graywater", "boiling-bubbles", "Tar-free, but still gross.", 20, MANA_RESTORE) {
        }
    }

    public class PureWater : ManaItem {
        private const int MANA_RESTORE = 50;

        public PureWater() : base("Pure Water", "potion-ball", "Pure water. A rarity in these parts.", 100, MANA_RESTORE) {
        }
    }

    public class VitalityTrinket : SingleStatTrinket {

        public VitalityTrinket() : base(StatType.VITALITY, 50, 4, "Life") {
        }
    }

    public class AgilityTrinket : SingleStatTrinket {

        public AgilityTrinket() : base(StatType.AGILITY, 50, 4, "Swiftness") {
        }
    }

    public class IntellectTrinket : SingleStatTrinket {

        public IntellectTrinket() : base(StatType.INTELLECT, 50, 2, "Smarts") {
        }
    }

    public class StrengthTrinket : SingleStatTrinket {

        public StrengthTrinket() : base(StatType.STRENGTH, 50, 2, "Force") {
        }
    }

    public class SharkFin : HealingItem {
        private const int HEALING_AMOUNT = 25;

        public SharkFin() : base("Shark Fin", "shark-fin", "An expensive and illegal fin from a shark.", 200, HEALING_AMOUNT) {
        }
    }

    public class Spear : EquippableItem {

        public Spear() : base("ice-spear", EquipType.WEAPON, 80, "Spear of Just Ice", "The ice refuses to melt.") {
            AddFlatStatBonus(StatType.STRENGTH, 5);
            AddFlatStatBonus(StatType.AGILITY, 5);
            AddFlatStatBonus(StatType.VITALITY, 5);
        }
    }

    public class Trident : EquippableItem {

        public Trident() : base("harpoon-trident", EquipType.WEAPON, 100, "Salach's Trident", "Used by rulers of underground places.") {
            AddFlatStatBonus(StatType.STRENGTH, 5);
            AddFlatStatBonus(StatType.INTELLECT, 10);
        }
    }

    public class Hammer : EquippableItem {

        public Hammer() : base("thor-hammer", EquipType.WEAPON, 100, "Salach's Mighty Hammer", "A powerful, heavy hammer.") {
            AddFlatStatBonus(StatType.STRENGTH, 15);
            AddFlatStatBonus(StatType.AGILITY, -20);
        }
    }

    public class ScaledArmor : EquippableItem {

        public ScaledArmor() : base("layered-armor", EquipType.ARMOR, 100, "Salach's Armor", "Made from Salach's own skin. Gross.") {
            AddFlatStatBonus(StatType.VITALITY, 15);
            AddFlatStatBonus(StatType.AGILITY, 5);
        }

        public override PermanentBuff CreateBuff() {
            return new RougherSkin();
        }
    }

    // Shop

    public class SharkBait : ConsumableItem {

        public SharkBait() : base(
            "food-chain",
            20,
            TargetType.SELF,
            "Shark Bait", "Creates a fragile shark attractant. <color=red>Fishy Market is not responsible for wasteful uses of this item outside of combat.</color>") {
            this.flags.Remove(Model.Items.Flag.USABLE_OUT_OF_COMBAT);
        }

        public override IList<SpellEffect> GetEffects(Page page, Character caster, Character target) {
            target = caster;
            Func<Character> summonDecoyFunc = () => {
                Character sharkBait = OceanNPCs.SharkBaitDecoy();
                return sharkBait;
            };
            return new SpellEffect[] {
                new SummonEffect(page.GetSide(target), page, summonDecoyFunc, 1)
            };
        }
    }

    public class FishHook : EquippableItem {

        public FishHook() : base("fishing-hook", EquipType.WEAPON, 500, "Fish Hook", "A used fish hook.") {
            AddFlatStatBonus(StatType.STRENGTH, 10);
            AddFlatStatBonus(StatType.AGILITY, 10);
            AddFlatStatBonus(StatType.VITALITY, -1);
        }

        public override PermanentBuff CreateBuff() {
            return new FishShook();
        }
    }

    public class Rocktail : HealingItem {
        private const int HEALING_AMOUNT = 15;

        public Rocktail() : base("Rocktail", "angler-fish", "A fish made of living rock. Somehow edible.", 25, HEALING_AMOUNT, false) {
        }
    }

    public class Cleansing : ConsumableItem {
        private const int DAMAGE = 5;

        public Cleansing() : base(
            "hospital-cross",
            20,
            TargetType.ANY,
            "Cleansing",
            string.Format("Caster sacrifices {0} {1} to remove all dispellable buffs from a target.", DAMAGE, StatType.HEALTH)) {
        }

        public override IList<SpellEffect> GetEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new DispelAllBuffs(target.Buffs),
                new AddToModStat(caster.Stats, StatType.HEALTH, -DAMAGE)
            };
        }

        protected override bool IsMeetOtherRequirements(Character caster, Character target) {
            return target.Buffs.Count(b => b.IsDispellable) > 0;
        }
    }
}