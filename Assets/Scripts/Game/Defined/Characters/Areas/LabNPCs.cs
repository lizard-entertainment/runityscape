using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Serialized.Items;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Serialized.Statistics;
using Scripts.Game.Defined.Unserialized.Buffs;
using Scripts.Game.Defined.Unserialized.Spells;
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
                    Ruins.Cultist(),
                    new PurchasedSpell(200, new Revive()),
                    new PurchasedSpell(200, new Inspire()),
                    new PurchasedSpell(500, new MagicMissile()),
                    new PurchasedSpell(500, new SelfHeal())
                );
        }

        public static class Ruins {

            public static Character Cultist() {
                return CharacterUtil.StandardEnemy(
                    new Stats(10, 8, 5, 1, 25),
                    new Look("Spectre",
                             "villager",
                             "A villager who fights for the collective.",
                             Breed.SPIRIT),
                    new Attacker()
                    );
            }

            public static Character Enforcer() {
                return CharacterUtil.StandardEnemy(
                    new Stats(12, 12, 8, 5, 80),
                    new Look("Enforcer",
                             "knight",
                             "A knight who trained in judo throws. Not because they left their sword at home or anything.",
                             Breed.SPIRIT),
                    new LabKnight()
                    )
                    .AddSpells(new CrushingBlow());
            }

            public static Character Darkener() {
                return CharacterUtil.StandardEnemy(
                    new Stats(12, 5, 20, 15, 50),
                    new Look("Darkener", "illusionist", "Still sleeps with a night light.", Breed.SPIRIT),
                    new Illusionist()
                    )
                    .AddSpells(new Blackout());
            }

            public static Character BigKnightA() {
                return BigKnight("Perse");
            }

            public static Character BigKnightB() {
                return BigKnight("Verance");
            }

            public static Character Mage() {
                return CharacterUtil.StandardEnemy(
                        new Stats(12, 4, 20, 20, 40),
                        new Look("Warlock",
                                 "wizard",
                                 "Conjues catnip for stray cats on their days off.",
                                 Breed.SPIRIT),
                        new Warlock()
                    ).AddSpells(new Inferno())
                    .AddBuff(new UnholyInsight());
            }

            public static Character Cleric() {
                return CharacterUtil.StandardEnemy(
                        new Stats(12, 4, 20, 20, 40),
                        new Look("Cleric",
                                 "white-mage",
                                 "Secretly really likes satan.",
                                 Breed.SPIRIT),
                        new Cleric()
                    ).AddSpells(new SetupDefend(), new PlayerHeal())
                    .AddBuff(new UnholyInsight());
            }

            private static Character BigKnight(string name) {
                return CharacterUtil.StandardEnemy(
                    new Stats(15, 15, 10, 10, 120),
                    new Look(
                        name,
                        "big-knight",
                        "One of a pair of knights filled with DETERMINATION",
                        Breed.SPIRIT
                        ),
                    new LabBigKnight()
                    ).AddFlags(Flag.PERSISTS_AFTER_DEFEAT)
                    .AddBuff(new StandardCountdown())
                    .AddSpells(new UnholyRevival());
            }
        }

        public static class Ocean {

            public static Character Shark() {
                return CharacterUtil.StandardEnemy(
                    new Stats(5, 5, 6, 8, 60),
                    new Look(
                        "Razor Shark",
                        "shark",
                        "Shark who needs lotion.",
                        Breed.FISH
                        ),
                    new Attacker())
                    .AddBuff(new RougherSkin())
                    .AddItem(new Money(), Util.RandomRange(50, 100));
            }

            public static Character Siren() {
                return CharacterUtil.StandardEnemy(
                        new Stats(6, 4, 10, 10, 40),
                        new Look(
                            "Enthraller",
                            "siren",
                            "Sings a mean tune.",
                            Breed.FISH
                        ),
                        new Siren()
                    ).AddSpells(Game.Serialized.Brains.Siren.DEBUFF_LIST);
            }

            public static Character Tentacle() {
                Character c = CharacterUtil.StandardEnemy(
                        new Stats(7, 3, 25, 1, 20),
                        new Look(
                            "Lasher",
                            "tentacle",
                            "Hugging instrument belonging to a Leviathan.",
                            Breed.FISH
                            ),
                        new Attacker()
                    );
                if (Util.IsChance(.50)) {
                    c.AddBuff(new OnlyAffectedByHero());
                } else {
                    c.AddBuff(new OnlyAffectedByPartner());
                }
                return c;
            }

            public static Character Kraken() {
                return CharacterUtil.StandardEnemy(
                        new Stats(8, 10, 10, 20, 200),
                        new Look(
                            "Leviathan",
                            "kraken",
                            "It's lonely; It really just wants a hug.",
                            Breed.FISH
                            ),
                        new Kraken()
                    )
                    .AddSpells(new SpawnLashers())
                    .AddSpells(new CrushingBlow())
                    .AddBuff(new StandardCountdown())
                    .AddStats(new Skill());
            }

            public static Character Elemental() {
                return CharacterUtil.StandardEnemy(
                    new Stats(9, 5, 20, 15, 20),
                    new Look(
                        "Undine",
                        "villager",
                        "A heroic sea elemental.",
                        Breed.FISH
                        ),
                    new Elemental())
                    .AddStats(new Mana())
                    .AddSpells(new WaterboltSingle(), new WaterboltMulti())
                    .AddBuff(new UnholyInsight());
            }

            public static Character DreadSinger() {
                return CharacterUtil.StandardEnemy(
                        new Stats(10, 5, 20, 20, 25),
                        new Look(
                            "Sea Witch",
                            "siren",
                            "Its tune is actually pretty catchy.",
                            Breed.FISH
                            ),
                        new DreadSinger())
                        .AddSpells(new NullifyHealing())
                        .AddSpells(new CastDelayedEternalDeath())
                        .AddItem(new Cleansing(), 1);
            }

            public static Character Swarm() {
                return CharacterUtil.StandardEnemy(
                    new Stats(2, 1, 5, 2, 15),
                    new Look(
                        "Swarm",
                        "angler-fish",
                        "Just trying to get an eduation.",
                        Breed.FISH
                        ),
                    new Swarm())
                    .AddItem(new Money(), Util.RandomRange(5, 10));
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
                    new Stats(15, 15, 30, 30, 30),
                    new Look("Anomaly H", "player", "An anomaly in a familiar form.", Breed.PROGRAMMER),
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
                    new Stats(15, 20, 30, 20, 35),
                    new Look("Anomaly P", "partner", "An anomaly in a familiar form.", Breed.HUMAN),
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