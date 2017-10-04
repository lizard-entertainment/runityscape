using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Serialized.Items;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Serialized.Statistics;
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
                .AddTalks(new Talk("Shield", "<a>A fine wooden shield, complete with a steel band around the rim."))
                .AddTalks(new Talk("Test", "<a>A sturdy fish hook, best for fighting fish."))
                .AddBuys(
                    new Apple(),
                    new IdentifyScroll(),
                    new RevivalSeed(),
                    new CrushingBlowTome(),
                    new HealTome(),
                    new DefendTome(),
                    new RegenArmor()
                    )
                .AddBuys(new Buy(new Inventory1x6())
                .AddPitch("The 6Pack series of backpack expanders can increase your inventory size up to 6! A perfect option for the packrat in your life."));
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
                .AddItem(new Money(), Util.Range(0, 3));
        }

        public static Character Knight() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 1, 2, 2, 5),
                new Look(
                    "Knight",
                    "knight",
                    "A knight who didn't make it. May be armed.",
                    Breed.SPIRIT
                    ),
                new Attacker())
                .AddItem(new Item[] { new BrokenSword(), new GhostArmor() }.ChooseRandom(), Util.IsChance(.50f));
        }

        public static Character BigKnight() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 10, 2, 2, 15),
                new Look(
                    "Big Knight",
                    "big-knight",
                    "It's a big guy.",
                    Breed.SPIRIT
                    ),
                new BigKnight())
                .AddStats(new Skill())
                .AddSpells(new SetupCounter());
        }

        public static Character Wizard() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 1, 1, 2, 3),
                new Look(
                    "Wizard",
                    "wizard",
                    "Can dish it out but cannot take it.",
                    Breed.SPIRIT
                    ),
                new Wizard())
                .AddStats(new Mana())
                .AddSpells(new Ignite())
                .AddBuff(new Insight());
        }

        public static Character Healer() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 1, 5, 5, 2),
                new Look(
                    "Healer",
                    "white-mage",
                    "Healer in life. Healer in death.",
                    Breed.SPIRIT
                    ),
                new Healer())
                .AddItem(new Money(), Util.RandomRange(5, 15))
                .AddSpells(new EnemyHeal());
        }

        public static Character Illusionist() {
            return CharacterUtil.StandardEnemy(
                new Stats(3, 4, 10, 8, 20),
                new Look(
                    "Illusionist",
                    "illusionist",
                    "A wicked master of illusions.",
                    Breed.SPIRIT
                    ),
                new Illusionist())
                .AddSpells(new Blackout());
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
                new Stats(5, 5, 5, 10, 30),
                ReplicantDisguisedLook(),
                new Replicant()
                )
            .AddFlags(Model.Characters.Flag.PERSISTS_AFTER_DEFEAT)
            .AddSpells(new ReflectiveClone(), new RevealTrueForm());
        }
    }
}