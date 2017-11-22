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

namespace Scripts.Game.Defined.Characters {

    public static class OceanNPCs {

        public static Shop OceanShop(Page previous, Flags flags, Party party) {
            return new Shop(
                previous,
                "Fishy Market",
                flags,
                party,
                0.6f,
                1f,
                Shark())
                .AddBuys(
                    new GrayWater(),
                    new Rocktail(),
                    new Dynamite(),
                    new Cleansing(),
                    new SharkBait(),
                    new ShellArmor(),
                    new FishHook()
                );
        }

        public static Trainer OceanTrainer(Page previous, Party party) {
            return new Trainer(
                previous,
                party,
                Siren(),
                    new PurchasedSpell(50, new Purge()),
                    new PurchasedSpell(50, new CrushingBlow()),
                    new PurchasedSpell(100, new MassCheck()),
                    new PurchasedSpell(100, new Multistrike())
                );
        }

        public static InventoryMaster OceanMaster(Page previous, Party party) {
            return new InventoryMaster(
                previous,
                party,
                Siren(),
                6,
                8,
                200
                );
        }

        public static Character Shark() {
            return CharacterUtil.StandardEnemy(
                new Stats(5, 10, 6, 8, 35),
                new Look(
                    "Shark",
                    "shark",
                    "Hatless shark.",
                    Breed.FISH
                    ),
                new SharkAttacker())
                .AddBuff(new RoughSkin())
                .AddItem(new CrackedSharkTooth(), .25f)
                .AddItem(new SharkBlood(), .25f)
                .AddItem(new SharkTooth(), .02f)
                .AddItem(new SharkFin(), .02f)
                .AddItem(new SharkSkin(), .02f)
                .AddMoney(10);
        }

        public static Character Siren() {
            return CharacterUtil.StandardEnemy(
                    new Stats(6, 6, 12, 10, 20),
                    new Look(
                        "Siren",
                        "siren",
                        "Has the power to sing people to death.",
                        Breed.FISH
                    ),
                    new Siren()
                )
                .AddSpells(Game.Serialized.Brains.Siren.DEBUFF_LIST)
                .AddItem(new BlackWater(), 75f)
                .AddItem(new GrayWater(), .25f)
                .AddItem(new CrackedOrb(), .25f)
                .AddItem(new Cleansing(), .50f)
                .AddItem(new SirenNote(), .10f)
                .AddMoney(10);
        }

        public static Character Tentacle() {
            return CharacterUtil.StandardEnemy(
                    new Stats(7, 6, 5, 1, 5),
                    new Look(
                        "Tentacle",
                        "tentacle",
                        "Tentacle belonging to a creature of the deep.",
                        Breed.FISH
                        ),
                    new Attacker()
                )
                .AddItem(new OctopusLeg());
        }

        public static Character Kraken() {
            return CharacterUtil.StandardEnemy(
                    new Stats(8, 15, 16, 10, 120),
                    new Look(
                        "Krackle",
                        "kraken",
                        "You gotta be squidding me!",
                        Breed.FISH
                        ),
                    new Kraken()
                )
                .AddSpells(new SpawnTentacles())
                .AddSpells(new CrushingBlow())
                .AddStats(new Skill())
                .AddItem(
                    new Item[] {
                        new VitalityTrinket(),
                        new AgilityTrinket(),
                        new IntellectTrinket(),
                        new StrengthTrinket()
                    }.ChooseRandom()
                )
                .AddMoney(20);
        }

        public static Character Swarm() {
            return CharacterUtil.StandardEnemy(
                new Stats(9, 5, 50, 2, 5),
                new Look(
                    "Swarm",
                    "swarm",
                    "Questionable member of the sea that travels in schools.",
                    Breed.FISH
                    ),
                new Swarm())
                .AddBuff(new RoughSkin())
                .AddSpells(new EnemyHeal())
                .AddMoney(2);
        }

        public static Character Elemental() {
            return CharacterUtil.StandardEnemy(
                new Stats(9, 1, 30, 15, 50),
                new Look(
                    "Elemental",
                    "elemental",
                    "Sea elemental.",
                    Breed.FISH
                    ),
                new Elemental()
                )
                .AddStats(new Mana())
                .AddSpells(new WaterboltSingle(),
                    new WaterboltMulti())
                .AddItem(new Cleansing(), Util.IsChance(.50f))
                .AddItem(new PureWater(), .20f)
                .AddMoney(50);
        }

        public static Character Shuck() {
            return CharacterUtil.StandardEnemy(
                    new Stats(9, 5, 20, 20, 30),
                    new Look(
                        "Shuck",
                        "shuck",
                        "Cursed coastal canine.",
                        Breed.FISH
                        ),
                    new DreadSinger())
                    .AddSpells(new CastDelayedDeath())
                    .AddItem(new GrayWater(), .50f)
                    .AddItem(new Cleansing(), 1)
                    .AddItem(new WaterOrb(), .05f)
                    .AddEquip(new Spear(), .10f)
                    .AddMoney(20);
        }

        public static Character SharkBaitDecoy() {
            return CharacterUtil.StandardEnemy(
                new Stats(1, 0, 0, 0, 1),
                new Look(
                    "Shark Bait",
                    "villager",
                    "No villagers were harmed in the making of this product.",
                    Breed.SHARK_BAIT
                    ),
                new DoNothing());
        }

        public static Character SharkPirate() {
            return CharacterUtil.StandardEnemy(
                new Stats(10, 10, 5, 18, 150),
                new Look(
                    "Cap'n Selach",
                    "shark-pirate",
                    "Fierce captain of shark crew.",
                    Breed.FISH
                    ),
                new SharkPirate())
                .AddBuff(new RougherSkin())
                .AddMoney(30)
                .AddSpells(
                    new SummonSeaCreatures(),
                    new OneShotKill(),
                    new CastDelayedEternalDeath(),
                    new GiveEvasion(),
                    new SetupCounter(),
                    new ReflectiveClone())
                .AddItem(new SharkFin())
                .AddItem(new SharkBlood())
                .AddItem(new ToothNecklace(), .25f)
                .AddItem(new SharkTooth(), .25f)
                .AddItem(new Item[] {
                    new Trident(),
                    new Hammer(),
                    new ScaledArmor()
                    }.ChooseRandom()
                );
        }
    }
}