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
using Scripts.Model.Pages;

namespace Scripts.Game.Defined.Characters {

    public static class LabNPCs {

        public static Trainer Trainer(Page previous, Party party) {
            return new Trainer(
                    previous,
                    party,
                    Ruins.Villager(),
                    new PurchasedSpell(1000, new Revive()),
                    new PurchasedSpell(1000, new Inspire()),
                    new PurchasedSpell(2000, new MagicMissile()),
                    new PurchasedSpell(2000, new SelfHeal())
                );
        }

        public static Shop Shop(Page previous, Flags flags, Party party) {
            return new Shop(
                previous,
                "Gift Shop",
                flags,
                party,
                .7f,
                1,
                Ruins.Enforcer()
                )
                .AddBuys(
                new HealingPotion(),
                new LifeGem(),
                new ManaPotion(),
                new ManaGem(),
                new FinalCasterArmor(),
                new FinalCasterOffhand(),
                new FinalCasterTrinket(),
                new FinalStaff(),
                new FinalMeleeArmor(),
                new FinalMeleeOffhand(),
                new FinalMeleeTrinket(),
                new FinalSword()
                )
                .AddTalks(new Talk("Why is everything so expensive?", "<a>The intent is to provide buyers with a sense of pride and accomplishment for purchasing different items."));
        }

        public static InventoryMaster LabMaster(Page previous, Party party) {
            return new InventoryMaster(
                previous,
                party,
                Ruins.Darkener(),
                8,
                10,
                5000
                );
        }

        public static class Ruins {

            public static Character Villager() {
                return CharacterUtil.StandardEnemy(
                    new Stats(20, 8, 5, 1, 50),
                    new Look("Spectre",
                             "villager lab",
                             "A villager who fights for the collective.",
                             Breed.SPIRIT),
                    new Attacker()
                    )
                    .AddMoney(50);
            }

            public static Character Enforcer() {
                return CharacterUtil.StandardEnemy(
                    new Stats(12, 12, 8, 5, 75),
                    new Look("Enforcer",
                             "knight lab",
                             "A knight who trained in judo throws. Not because they left their sword at home or anything.",
                             Breed.SPIRIT),
                    new LabKnight()
                    )
                    .AddStats(new Skill())
                    .AddSpells(new CrushingBlow(), new SetupDefend())
                    .AddMoney(50);
            }

            public static Character Darkener() {
                return CharacterUtil.StandardEnemy(
                    new Stats(12, 5, 20, 15, 50),
                    new Look("Darkener", "illusionist lab", "Still sleeps with a night light.", Breed.SPIRIT),
                    new Illusionist()
                    )
                    .AddSpells(new Blackout())
                    .AddMoney(50);
            }

            public static Character BigKnightA() {
                return BigKnight("Perse", "big-knight-a");
            }

            public static Character BigKnightB() {
                return BigKnight("Verance", "big-knight-b");
            }

            public static Character Mage() {
                return CharacterUtil.StandardEnemy(
                        new Stats(12, 4, 20, 20, 80),
                        new Look("Warlock",
                                 "wizard lab",
                                 "Conjures catnip for stray cats on their days off.",
                                 Breed.SPIRIT),
                        new Warlock()
                    ).AddSpells(new Inferno())
                    .AddBuff(new UnholyInsight())
                    .AddMoney(75);
            }

            public static Character Cleric() {
                return CharacterUtil.StandardEnemy(
                        new Stats(12, 4, 20, 20, 40),
                        new Look("Cleric",
                                 "white-mage lab",
                                 "Secretly really likes Satan (not to be confused with seitan).",
                                 Breed.SPIRIT),
                        new Cleric()
                    ).AddSpells(new SetupDefend(), new PlayerHeal())
                    .AddBuff(new UnholyInsight())
                    .AddMoney(75);
            }

            private static Character BigKnight(string name, string sprite) {
                return CharacterUtil.StandardEnemy(
                    new Stats(15, 15, 10, 10, 120),
                    new Look(
                        name,
                        sprite,
                        "One of a pair of knights filled with DETERMINATION.",
                        Breed.SPIRIT
                        ),
                    new LabBigKnight()
                    ).AddFlags(Flag.PERSISTS_AFTER_DEFEAT)
                    .AddBuff(new StandardCountdown())
                    .AddSpells(new UnholyRevival(), new CrushingBlow(), new SetupCounter())
                    .AddStats(new Skill())
                    .AddMoney(100);
            }
        }

        public static class Ocean {

            public static Character Shark() {
                return CharacterUtil.StandardEnemy(
                    new Stats(16, 10, 15, 20, 60),
                    new Look(
                        "Razor Shark",
                        "shark lab",
                        "Shark who needs lotion.",
                        Breed.FISH
                        ),
                    new Attacker())
                    .AddBuff(new RougherSkin())
                    .AddItem(new Money(), Util.RandomRange(50, 100))
                    .AddMoney(150);
            }

            public static Character Siren() {
                return CharacterUtil.StandardEnemy(
                        new Stats(16, 10, 20, 30, 40),
                        new Look(
                            "Enthraller",
                            "siren lab",
                            "Sings a mean tune.",
                            Breed.FISH
                        ),
                        new Siren()
                    ).AddSpells(Game.Serialized.Brains.Siren.DEBUFF_LIST)
                    .AddMoney(150);
            }

            public static Character Tentacle() {
                Character c = CharacterUtil.StandardEnemy(
                        new Stats(12, 3, 25, 1, 20),
                        new Look(
                            "Lasher",
                            "tentacle lab",
                            "Hugging instrument belonging to a Leviathan.",
                            Breed.FISH
                            ),
                        new Attacker()
                    )
                    .AddItem(new OctopusLeg());
                if (Util.IsChance(.50)) {
                    c.AddBuff(new OnlyAffectedByHero());
                } else {
                    c.AddBuff(new OnlyAffectedByPartner());
                }
                return c;
            }

            public static Character Kraken() {
                return CharacterUtil.StandardEnemy(
                        new Stats(20, 20, 20, 20, 200),
                        new Look(
                            "Leviathan",
                            "kraken lab",
                            "It's lonely; It really just wants a hug.",
                            Breed.FISH
                            ),
                        new Kraken()
                    )
                    .AddSpells(new SpawnLashers(), new CrushingBlow())
                    .AddBuff(new StandardCountdown())
                    .AddStats(new Skill())
                    .AddMoney(200);
            }

            public static Character Elemental() {
                return CharacterUtil.StandardEnemy(
                    new Stats(18, 15, 25, 25, 60),
                    new Look(
                        "Hellemental",
                        "elemental lab",
                        "A heroic sea elemental.",
                        Breed.FISH
                        ),
                    new Elemental())
                    .AddStats(new Mana())
                    .AddSpells(new WaterboltSingle(), new WaterboltMulti())
                    .AddBuff(new UnholyInsight())
                    .AddMoney(150);
            }

            public static Character DreadSinger() {
                return CharacterUtil.StandardEnemy(
                        new Stats(18, 10, 20, 25, 55),
                        new Look(
                            "Hellhound",
                            "shuck lab",
                            "Its tune is actually pretty catchy.",
                            Breed.BEAST
                            ),
                        new DreadSinger())
                        .AddSpells(new NullifyHealing())
                        .AddSpells(new CastDelayedEternalDeath())
                        .AddItem(new Cleansing(), 1)
                        .AddMoney(150);
            }

            public static Character Swarm() {
                return CharacterUtil.StandardEnemy(
                    new Stats(13, 6, 66, 6, 25),
                    new Look(
                        "Myriad",
                        "swarm lab",
                        "Just trying to get an eduation.",
                        Breed.FISH
                        ),
                    new Swarm())
                    .AddMoney(20);
            }
        }

        public static class Final {

            private static Character PlayerClone(Stats stats, Look look, Brain brain) {
                return CharacterUtil.StandardEnemy(stats, look, brain)
                    .RemoveFlags(Flag.DROPS_ITEMS, Flag.GIVES_EXPERIENCE)
                    .AddFlags(Flag.PERSISTS_AFTER_DEFEAT);
            }

            public static Character HeroClone() {
                return PlayerClone(
                    new Stats(15, 5, 20, 20, 150),
                    new Look("Memory H", "player evil", "A corrupted memory in a familiar form.", Breed.PROGRAMMER),
                    new Hero()
                    )
                    .AddStats(new Mana())
                    .AddSpells(
                        new Revive(),
                        new PlayerHeal(),
                        new SetupDefend(),
                        new MagicMissile())
                    .AddEquip(new EvilCloneTrinket(), 1)
                    .AddEquip(new EvilCloneArmor(), 1);
            }

            public static Character PartnerClone() {
                return PlayerClone(
                    new Stats(15, 10, 20, 10, 150),
                    new Look("Memory P", "partner evil", "A corrupted memory in a familiar form.", Breed.HUMAN),
                    new Partner()
                    )
                    .AddStats(new Skill())
                    .AddSpells(
                        new QuickAttack(),
                        new SelfHeal(),
                        new Inspire(),
                        new SetupDefend(),
                        new CrushingBlow(),
                        new Multistrike())
                    .AddEquip(new EvilFriendTrinket(), 1);
            }
        }
    }
}