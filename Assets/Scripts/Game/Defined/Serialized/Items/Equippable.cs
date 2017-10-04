using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Model.Buffs;
using Scripts.Model.Items;
using Scripts.Model.Stats;

namespace Scripts.Game.Defined.Serialized.Items {

    public class PoisonArmor : EquippableItem {

        public PoisonArmor() : base(EquipType.ARMOR, 10, "Poisoned Armor", "This doesn't look safe.") {
            AddFlatStatBonus(StatType.VITALITY, 3);
            AddFlatStatBonus(StatType.AGILITY, -1);
        }

        public override Buff CreateBuff() {
            return new Poison();
        }
    }

    public class Shield : EquippableItem {

        public Shield() : base(Util.GetSprite("round-shield"), EquipType.OFFHAND, 0, "Basic Shield ", "A basic wooden shield.") {
            AddFlatStatBonus(StatType.AGILITY, -5);
            AddFlatStatBonus(StatType.VITALITY, 5);
        }

        public override Buff CreateBuff() {
            return new DamageResist();
        }
    }

    public class FishHook : EquippableItem {

        public FishHook() : base(EquipType.WEAPON, 50, "Fish Hook", "A used fish hook.") {
            AddFlatStatBonus(StatType.STRENGTH, 5);
            AddFlatStatBonus(StatType.AGILITY, 1);
            AddFlatStatBonus(StatType.VITALITY, -1);
        }

        public override Buff CreateBuff() {
            return new FishShook();
        }
    }
}