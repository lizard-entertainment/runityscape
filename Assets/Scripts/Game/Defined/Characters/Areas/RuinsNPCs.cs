using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Serialized.Items;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Serialized.Statistics;
using Scripts.Game.Defined.Unserialized.Buffs;
using Scripts.Game.Defined.Unserialized.Spells;
using Scripts.Game.Serialized;
using Scripts.Game.Serialized.Brains;
using Scripts.Game.Shopkeeper;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Pages;
using UnityEngine;

namespace Scripts.Game.Defined.Characters {

    public static class RuinsNPCs {

        public static Shop RuinsShop(Page previous, Flags flags, Party party) {
            return new Shop(
                previous,
                "Desecrated Shop",
                flags,
                party,
                0.5f,
                1f,
                Villager())
                .AddBuys(
                    new Apple(),
                    new IdentifyScroll(),
                    new RevivalSeed(),
                    new RegenArmor()
                    );
        }

        public static Trainer RuinsTrainer(Page previous, Party party) {
            return new Trainer(
                previous,
                party,
                Villager(),
                new PurchasedSpell(30, new SetupDefend()),
                new PurchasedSpell(50, new PlayerHeal())
                );
        }

        public static InventoryMaster RuinsMaster(Page previous, Party party) {
            return new InventoryMaster(
                    previous,
                    party,
                    Villager(),
                    Inventory.INITIAL_CAPACITY,
                    6,
                    100
                );
        }

        public static Character Villager() {
            return CharacterUtil.StandardEnemy(
                new Stats(2, 1, 1, 1, 2),
                new Look(
                    "Ghost",
                    "villager",
                    "A villager who didn't make it.",
                    Breed.SPIRIT
                    ),
                new Attacker())
                .AddItem(new WornDagger(), .20f)
                .AddItem(new RealKnife(), .05f)
                .AddMoney(3);
        }

        public static Character Knight() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 2, 2, 2, 10),
                new Look(
                    "Knight",
                    "knight",
                    "A knight who didn't make it. May be armed.",
                    Breed.SPIRIT
                    ),
                new Attacker())
                .AddEquip(new BrokenSword(), .20f)
                .AddEquip(new GhostArmor(), .20f)
                .AddItem(new SilverBoot(), .05f)
                .AddMoney(5);
        }

        public static Character BigKnight() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 3, 2, 2, 25),
                new Look(
                    "Big Knight",
                    "big-knight",
                    "It's a big guy.",
                    Breed.SPIRIT
                    ),
                new BigKnight())
                .AddStats(new Skill())
                .AddSpells(new SetupCounter())
                .AddEquip(new GhostArmor())
                .AddEquip(new BrokenSword())
                .AddItem(new BigArmor(), .20f)
                .AddItem(new BigSword(), .20f)
                .AddItem(
                Util.ChooseRandom<Item>(
                    new MinorAgilityTrinket(),
                    new MinorIntellectTrinket(),
                    new MinorStrengthTrinket(),
                    new MinorVitalityTrinket()))
                .AddMoney(10);
        }

        public static Character Wizard() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 1, 1, 2, 18),
                new Look(
                    "Wizard",
                    "wizard",
                    "Can dish it out but cannot take it.",
                    Breed.SPIRIT
                    ),
                new Wizard())
                .AddStats(new Mana())
                .AddSpells(new Ignite())
                .AddBuff(new Insight())
                .AddEquip(new Wand(), .20f)
                .AddItem(new IdentifyScroll(), .20f)
                .AddItem(new SpiritDust(), .50f)
                .AddItem(new SpiritOrb(), .05f)
                .AddMoney(10);
        }

        public static Character Healer() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 1, 5, 5, 3),
                new Look(
                    "Healer",
                    "white-mage",
                    "Healer in life. Healer in death.",
                    Breed.SPIRIT
                    ),
                new Healer())
                .AddSpells(new EnemyHeal())
                .AddEquip(new Wand(), .25f)
                .AddItem(new Apple(), .25f)
                .AddItem(new SpiritDust(), .50f)
                .AddItem(new SpiritOrb(), .10f)
                .AddItem(new RevivalSeed(), .01f)
                .AddItem(new UsedBandage(), .50f)
                .AddItem(new CleanBandage(), .20f)
                .AddMoney(10);
        }

        public static Character Illusionist() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 4, 10, 8, 40),
                new Look(
                    "Illusionist",
                    "illusionist",
                    "A wicked master of illusions.",
                    Breed.SPIRIT
                    ),
                new Illusionist())
                .AddSpells(new Blackout())
                .AddEquip(new BetterWand())
                .AddItem(new SpiritDust())
                .AddItem(new IdentifyScroll())
                .AddMoney(15);
        }

        public static Look ReplicantLook() {
            return new Look(
                    "Xird'neth",
                    "replicant",
                    "Its form is incomprehensible.",
                    Breed.UNKNOWN,
                    Color.magenta
                    );
        }

        private static Look ReplicantDisguisedLook() {
            return new Look(
                "Irdne",
                "villager",
                "An innocent villager.",
                Breed.SPIRIT,
                Color.magenta
                );
        }

        public static Character Replicant() {
            return CharacterUtil.StandardEnemy(
                new Stats(5, 5, 5, 10, 100),
                ReplicantDisguisedLook(),
                new Replicant()
                )
            .AddFlags(Model.Characters.Flag.PERSISTS_AFTER_DEFEAT)
            .AddSpells(new ReflectiveClone(), new RevealTrueForm())
            .AddItem(new Item[] { new MadnessStaff(), new HorrorEmblem() }.ChooseRandom())
            .AddMoney(20);
        }
    }
}