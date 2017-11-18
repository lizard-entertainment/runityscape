using Scripts.Game.Areas;
using Scripts.Game.Defined.Characters;
using Scripts.Game.Defined.SFXs;
using Scripts.Game.Dungeons;
using Scripts.Game.Pages;
using Scripts.Game.Serialized;
using Scripts.Game.Stages;
using Scripts.Model.Acts;
using Scripts.Model.Characters;
using Scripts.Model.Pages;
using Scripts.Model.TextBoxes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Areas {

    public static class AreaList {

        /// <summary>
        /// Flags = World flags for current save
        /// Party = current party
        /// Page = Camp reference
        /// Page = DungeonPages reference
        /// Area = Area to return
        /// </summary>
        public static readonly ReadOnlyDictionary<AreaType, Func<Flags, Party, Page, Page, Area>> ALL_AREAS
            = new ReadOnlyDictionary<AreaType, Func<Flags, Party, Page, Page, Area>>(
                new Dictionary<AreaType, Func<Flags, Party, Page, Page, Area>>() {
                    { AreaType.RUINS, (flags, party, camp, dungeonPages) => CreateRuins(flags, party, camp, dungeonPages) },
                    { AreaType.SEA_WORLD, (flags, party, camp, dungeonPages) => SeaWorld(flags, party, camp, dungeonPages) },
                    { AreaType.LAB, (flags, party, camp, dungeonPages) => EvilLabs(flags, party, camp, dungeonPages) }
        });

        public static readonly ReadOnlyDictionary<AreaType, Music> AREA_MUSIC
            = new ReadOnlyDictionary<AreaType, Music>(
                new Dictionary<AreaType, Music>() {
                            { AreaType.RUINS, Music.RUINS },
                            { AreaType.SEA_WORLD, Music.WATER },
                            { AreaType.LAB, Music.LAB }
        });

        public static readonly ReadOnlyDictionary<AreaType, Sprite> AREA_SPRITES
            = new ReadOnlyDictionary<AreaType, Sprite>(
                new Dictionary<AreaType, Sprite>() {
                    { AreaType.RUINS, Util.GetSprite("skull-crack") },
                    { AreaType.SEA_WORLD, Util.GetSprite("at-sea") },
                    { AreaType.LAB, Util.GetSprite("cube") }
                });

        private static Area CreateRuins(Flags flags, Party party, Page camp, Page quests) {
            return new Area(
                AreaType.RUINS,
                    new Stage[] {
                        SceneList.RuinsIntro(party, "Welcome to the Crypt"),
                        new BattleStage(
                            "Start of Adventure",
                            () => new Encounter[] {
                                new Encounter(RuinsNPCs.Villager()),
                                new Encounter(RuinsNPCs.Villager(), RuinsNPCs.Villager())
                            }),
                        new BattleStage(
                            "Stronger Monsters",
                            () => new Encounter[] {
                                new Encounter(RuinsNPCs.Villager(), RuinsNPCs.Villager()),
                                new Encounter(RuinsNPCs.Villager(), RuinsNPCs.Knight())
                            }),
                        new BattleStage(
                            "Restoration",
                            () => new Encounter[] {
                                new Encounter(RuinsNPCs.Healer(), RuinsNPCs.Healer()),
                                new Encounter(RuinsNPCs.Healer(), RuinsNPCs.Knight())
                            }),
                        SceneList.RuinsMidboss(party, "A Bigger Foe"),
                        new BattleStage(
                            "Bigger Monsters",
                            () => new Encounter[] {
                                new Encounter(Music.BOSS, RuinsNPCs.Healer(), RuinsNPCs.BigKnight(), RuinsNPCs.Healer())
                            }),
                        new BattleStage(
                            "Ancient Magicks",
                            () => new Encounter[] {
                                new Encounter(RuinsNPCs.Wizard()),
                                new Encounter(RuinsNPCs.Wizard(), RuinsNPCs.Wizard())
                            }),
                        new BattleStage(
                            "Wizards' Tower",
                            () => new Encounter[] {
                                new Encounter(RuinsNPCs.Wizard(), RuinsNPCs.Wizard()),
                                new Encounter(
                                    RuinsNPCs.Wizard(),
                                    RuinsNPCs.Wizard(),
                                    RuinsNPCs.Healer(),
                                    RuinsNPCs.Healer()),
                                new Encounter(RuinsNPCs.Illusionist())
                            }),
                        new BattleStage(
                            "Premonition",
                            () => new Encounter[] {
                                new Encounter(RuinsNPCs.Villager()),
                                new Encounter(RuinsNPCs.BigKnight(), RuinsNPCs.BigKnight(), RuinsNPCs.Wizard(), RuinsNPCs.Wizard())
                            }),
                        SceneList.RuinsBoss(party, "Descent into Madness"),
                        new BattleStage(
                            "The Replicant",
                            () => new Encounter[] {
                                new Encounter(Music.CREEPY, RuinsNPCs.Healer(), RuinsNPCs.Replicant(), RuinsNPCs.Healer())
                            }),
                        SceneList.RuinsOutro(party, "To the Ocean!")
                    },
                    new PageGroup[] { RuinsNPCs.RuinsShop(camp, flags, party), RuinsNPCs.RuinsTrainer(camp, party), RuinsNPCs.RuinsMaster(camp, party) }
                );
        }

        private static Area SeaWorld(Flags flags, Party party, Page camp, Page quests) {
            return new Area(
                    AreaType.SEA_WORLD,
                    new Stage[] {
                        SceneList.SeaIntro(party, "A Watery Grave"),
                        new BattleStage(
                            "Welcome to the Ocean",
                            () => new Encounter[] {
                                new Encounter(OceanNPCs.Shark())
                            }),
                        new BattleStage(
                            "Sinister Singers",
                            () => new Encounter[] {
                                new Encounter(OceanNPCs.Siren()),
                                new Encounter(OceanNPCs.Siren(), OceanNPCs.Shark())
                            }),
                        new BattleStage(
                            "Insaniquarium",
                            () => new Encounter[] {
                                new Encounter(OceanNPCs.Shark(), OceanNPCs.Siren()),
                                new Encounter(OceanNPCs.Shark(), OceanNPCs.Siren(), OceanNPCs.Shark(), OceanNPCs.Siren())
                            }),
                        SceneList.SeaMidboss(party, "Horrors from the Deep"),
                        new BattleStage(
                            "The Kraken",
                            () => new Encounter [] {
                                new Encounter(Music.BOSS, OceanNPCs.Kraken())
                            }),
                        new BattleStage(
                            "Heart of the Swarm",
                            () => new Encounter[] {
                                new Encounter(OceanNPCs.Swarm(), OceanNPCs.Swarm()),
                                new Encounter(OceanNPCs.Swarm(), OceanNPCs.Swarm(), OceanNPCs.Swarm(), OceanNPCs.Swarm()),
                                new Encounter(OceanNPCs.Swarm(), OceanNPCs.Swarm(), OceanNPCs.Swarm(), OceanNPCs.Swarm(), OceanNPCs.Swarm()),
                            }),
                        new BattleStage(
                            "Nearing the End",
                            () => new Encounter[] {
                                new Encounter(OceanNPCs.Shuck()),
                                new Encounter(OceanNPCs.Elemental()),
                            }),
                        new BattleStage(
                            "Final Trench",
                            () => new Encounter[] {
                                new Encounter(OceanNPCs.Elemental(), OceanNPCs.Shuck()),
                                new Encounter(OceanNPCs.Elemental(), OceanNPCs.Elemental(), OceanNPCs.Shuck()),
                                new Encounter(OceanNPCs.Elemental(), OceanNPCs.Siren(), OceanNPCs.Shark(), OceanNPCs.Shuck()),
                                new Encounter(OceanNPCs.Swarm(), OceanNPCs.Swarm(), OceanNPCs.Swarm(), OceanNPCs.Elemental(), OceanNPCs.Siren(), OceanNPCs.Shark(), OceanNPCs.Shuck())
                            }),
                        SceneList.SeaBoss(party, "The Captain"),
                        new BattleStage(
                            "VS " + OceanNPCs.SharkPirate().Look.DisplayName,
                            () => new Encounter[] {
                                new Encounter(Music.BOSS, OceanNPCs.SharkPirate())
                            }),
                        SceneList.SeaOutro(party, "The Labs")
                    },
                    new PageGroup[] { OceanNPCs.OceanShop(camp, flags, party), OceanNPCs.OceanTrainer(camp, party), OceanNPCs.OceanMaster(camp, party) }
                );
        }

        private static Area EvilLabs(Flags flags, Party party, Page camp, Page quests) {
            return new Area(AreaType.LAB,
                new Stage[] {
                    new BattleStage(
                        "Adventure's End",
                        () => new Encounter[] {
                            new Encounter(LabNPCs.Ruins.Villager(), LabNPCs.Ruins.Villager(), LabNPCs.Ruins.Villager()),
                            new Encounter(LabNPCs.Ruins.Enforcer()),
                            new Encounter(LabNPCs.Ruins.Enforcer(), LabNPCs.Ruins.Enforcer(), LabNPCs.Ruins.Enforcer()),
                            new Encounter(LabNPCs.Ruins.BigKnightA())
                        }),
                    new BattleStage(
                        "Maleficent Magicks",
                        () => new Encounter[] {
                            new Encounter(LabNPCs.Ruins.Cleric(), LabNPCs.Ruins.Enforcer()),
                            new Encounter(LabNPCs.Ruins.Cleric(), LabNPCs.Ruins.Enforcer(), LabNPCs.Ruins.Cleric(), LabNPCs.Ruins.Enforcer()),
                            new Encounter(LabNPCs.Ruins.Mage()),
                            new Encounter(LabNPCs.Ruins.Darkener(), LabNPCs.Ruins.Darkener()),
                            new Encounter(LabNPCs.Ruins.Cleric(), LabNPCs.Ruins.Mage(), LabNPCs.Ruins.Mage(), LabNPCs.Ruins.Cleric()),
                            new Encounter(LabNPCs.Ruins.BigKnightB())
                        }),
                    new BattleStage(
                        "Determination",
                        () => new Encounter[] {
                            new Encounter(Music.BOSS, LabNPCs.Ruins.BigKnightA(), LabNPCs.Ruins.BigKnightB())
                        }),
                    new BattleStage(
                        "Sharp Sea",
                        () => new Encounter[] {
                            new Encounter(LabNPCs.Ocean.Shark(), LabNPCs.Ocean.Shark()),
                            new Encounter(LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm()),
                            new Encounter(LabNPCs.Ocean.Shark(), LabNPCs.Ocean.Shark(), LabNPCs.Ocean.Shark(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm())
                        }),
                    new BattleStage(
                        "Sirens' Trench",
                        () => new Encounter[] {
                            new Encounter(LabNPCs.Ocean.Siren(), LabNPCs.Ocean.Siren()),
                            new Encounter(LabNPCs.Ocean.DreadSinger()),
                            new Encounter(LabNPCs.Ocean.Elemental()),
                            new Encounter(LabNPCs.Ocean.Siren(), LabNPCs.Ocean.Siren(), LabNPCs.Ocean.DreadSinger(), LabNPCs.Ocean.Elemental())
                        }),
                    new BattleStage(
                        "Affinity",
                        () => new Encounter[] {
                            new Encounter(LabNPCs.Ocean.Tentacle(), LabNPCs.Ocean.Tentacle()),
                            new Encounter(Music.BOSS, LabNPCs.Ocean.Kraken())
                        }),
                    new BattleStage(
                        "Premonition II",
                        () => new Encounter[] {
                            new Encounter(LabNPCs.Ruins.Villager(), LabNPCs.Ruins.Villager(), LabNPCs.Ruins.Villager()),
                            new Encounter(LabNPCs.Ruins.Enforcer(), LabNPCs.Ruins.Enforcer(), LabNPCs.Ruins.Enforcer()),
                            new Encounter(LabNPCs.Ruins.Cleric(), LabNPCs.Ruins.Enforcer(), LabNPCs.Ruins.Cleric(), LabNPCs.Ruins.Enforcer()),
                            new Encounter(LabNPCs.Ruins.Cleric(), LabNPCs.Ruins.Mage(), LabNPCs.Ruins.Mage(), LabNPCs.Ruins.Cleric()),
                            new Encounter(LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm()),
                            new Encounter(LabNPCs.Ocean.Shark(), LabNPCs.Ocean.Shark(), LabNPCs.Ocean.Shark(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm(), LabNPCs.Ocean.Swarm()),
                            new Encounter(LabNPCs.Ocean.Siren(), LabNPCs.Ocean.Siren(), LabNPCs.Ocean.DreadSinger(), LabNPCs.Ocean.Elemental())
                        }),
                    SceneList.LabBoss(party, "Duality"),
                    new BattleStage(
                        "System's Exit",
                        () => new Encounter[] {
                            new Encounter(Music.FINAL_STAGE, LabNPCs.Final.HeroClone()),
                            new Encounter(Music.FINAL_STAGE, LabNPCs.Final.PartnerClone()),
                            new Encounter(Music.FINAL_BOSS, LabNPCs.Final.HeroClone(), LabNPCs.Final.PartnerClone())
                        }),
                    SceneList.Ending(party, flags, "An Ending")
                },
                new PageGroup[] { LabNPCs.Trainer(camp, party) }
                );
        }
    }
}