using Scripts.Game.Areas;
using Scripts.Game.Defined.Characters;
using Scripts.Game.Serialized;
using Scripts.Model.Acts;
using Scripts.Model.Characters;
using Scripts.Model.Pages;
using System.Linq;

namespace Scripts.Game.Stages {

    // SceneStages go here to avoid gunking up AreaList.
    public static class SceneList {

        public static SceneStage RuinsIntro(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Page convo = Conversation(party, AreaType.RUINS.GetDescription());
            return new SceneStage(convo, name,
                    new TextAct(hero, Side.LEFT, "A crypt? Are the demons in here?"),
                    new TextAct(partner, Side.LEFT, "In this crypt lairs a horrifying enemy that corrupts the spirits of those fallen."),
                    new TextAct(partner, Side.LEFT, "We will burn a path to the deepest part of this crypt, and destroy this monstrosity!")
                );
        }

        public static SceneStage RuinsMidboss(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Character knight = RuinsNPCs.BigKnight();
            Page convo = Conversation(party, AreaType.RUINS.GetDescription(), knight);
            return new SceneStage(convo, name,
                    new TextAct(partner, Side.LEFT, string.Format("Steel yourself, {0}!", hero)),
                    new TextAct(partner, Side.LEFT, string.Format("A powerful enemy lies just before us, the {0}!", knight)),
                    new TextAct(hero, Side.LEFT, string.Format("(Really, I named them {0}?)", knight)),
                    new TextAct(partner, Side.LEFT, string.Format("Keep your wits about you, {0}!", hero)),
                    new TextAct(partner, Side.LEFT, string.Format("<color=yellow>Strike only when the moment allows for it!</color>"))
                );
        }

        public static SceneStage RuinsBoss(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Page convo = Conversation(party, AreaType.RUINS.GetDescription());
            return new SceneStage(convo, name,
                    new TextAct(partner, Side.LEFT, "There's no telling what will be in the final chamber."),
                    new TextAct(partner, Side.LEFT, "This foul thing must be stopped!")
                );
        }

        public static SceneStage RuinsOutro(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Page convo = Conversation(party, AreaType.RUINS.GetDescription());
            return new SceneStage(convo, name,
                    new TextAct(hero, Side.LEFT, "(That wasn't the final boss?)"),
                    new TextAct(partner, Side.LEFT, string.Format("We have fought well, {0}! Those spirits can now rest. But our adventure is only beginning! To the {1}!",
                    hero,
                    AreaType.SEA_WORLD.GetDescription())),
                    new TextAct(hero, Side.LEFT, "We have a boat, right?"),
                    new TextAct("(Use the World button at Camp to change worlds.)")
                );
        }

        public static SceneStage SeaIntro(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Page convo = Conversation(party, AreaType.SEA_WORLD.GetDescription());
            return new SceneStage(convo, name,
                    new TextAct(partner, Side.LEFT, "There is no doubt that monsters will rise from the deep to attack us on our journey."),
                    new TextAct(partner, Side.LEFT, "Luckily, some shopkeepers have set up shop on this boat as well."),
                    new TextAct(partner, Side.LEFT, "They will be of great use to us.")
                );
        }

        public static SceneStage SeaMidboss(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Character kraken = OceanNPCs.Kraken();
            Page convo = Conversation(party, AreaType.SEA_WORLD.GetDescription(), kraken);
            return new SceneStage(convo, name,
                    new TextAct(partner, Side.LEFT, string.Format("A {0}! Those nasty tentacles will <color=yellow>intercept</color> our attacks!", kraken)),
                    new TextAct(partner, Side.LEFT, string.Format("That {0} doesn't look like a pushover either. If we fail to <color=cyan>setup our defenses</color>, there will be no victory for us!", kraken))
                );
        }

        public static SceneStage SeaBoss(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Character pirate = OceanNPCs.SharkPirate();
            Page convo = Conversation(party, AreaType.SEA_WORLD.GetDescription(), pirate);
            return new SceneStage(convo, name,
                    new TextAct(partner, Side.LEFT, string.Format("{0}... Wanted dead in seven sea systems for disrupting the natural order.", pirate)),
                    new TextAct(partner, Side.LEFT, string.Format("{0}! It'll take <color=yellow>everything we've got</color> to defeat this sinister shark!", hero))
                );
        }

        public static SceneStage SeaOutro(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Page convo = Conversation(party, AreaType.SEA_WORLD.GetDescription());
            return new SceneStage(convo, name,
                    new TextAct(hero, Side.LEFT, string.Format("{0}... Is it over?", partner)),
                    new TextAct(partner, Side.LEFT, string.Format("We approach the end, my friend!")),
                    new TextAct(partner, Side.LEFT, string.Format("The Monster Augmentation Technologies Labs... that is where we will meet our final foe.")),
                    new TextAct(partner, Side.LEFT, string.Format("They will not go easily, but neither will we.")),
                    new TextAct(partner, Side.LEFT, string.Format("Onward to the next World!"))
                );
        }

        public static SceneStage LabBoss(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Character evilHero = LabNPCs.Final.HeroClone();
            Character evilPartner = LabNPCs.Final.PartnerClone();
            Page convo = Conversation(party, AreaType.LAB.GetDescription(), evilHero, evilPartner);
            return new SceneStage(convo, name,
                    new TextAct(evilHero, Side.LEFT, string.Format("//STARTING SYSTEM DIAGNOSTIC... THREAT DETECTED!")),
                    new TextAct(evilPartner, Side.LEFT, string.Format("//TWO VARIABLES, \"{0}\" && \"{1}\" ARE ALREADY DECLARED IN THIS SCOPE.", hero, partner)),
                    new TextAct(evilHero, Side.LEFT, string.Format("//REQUESTING SYSTEM CALLBACK... SOLUTION RECIEVED!")),
                    new TextAct(evilHero, Side.LEFT, string.Format("//BEGIN PROCESS \"AUGMENT\" ON TERMINATION ACTOR.")),
                    new TextAct(evilPartner, Side.LEFT, string.Format("//BEGIN PROCESS \"TERMINATE\"."))
                );
        }

        public static SceneStage LabOutro(Party party, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Page convo = Conversation(party, AreaType.LAB.GetDescription());
            return new SceneStage(convo, name,
                    new TextAct(partner, Side.LEFT, string.Format("The world is saved!")),
                    new TextAct(hero, Side.LEFT, string.Format("(I guess this is the end...)"))
                );
        }

        public static SceneStage Ending(Party party, Flags flags, string name) {
            Character hero = GetHero(party);
            Character partner = GetPartner(party);
            Page convo = Conversation(party, AreaType.LAB.GetDescription());
            return new SceneStage(convo, name,
                    new TextAct(string.Format("With the help of {0}, {1} was able to exit the system in {2} days at {3}.",
                        partner,
                        hero,
                        flags.DayCount,
                        flags.Time.GetDescription())),
                    new TextAct(partner, Side.LEFT, string.Format("Catch you on the replay, {0}!", hero))
                );
        }

        private static Page Conversation(Party party, string location, params Character[] rights) {
            Page page = new Page(location);
            page.AddCharacters(Side.LEFT, GetHero(party));
            page.AddCharacters(Side.LEFT, GetPartner(party));
            foreach (Character right in rights) {
                page.AddCharacters(Side.RIGHT, right);
            }
            return page;
        }

        private static Character GetFlagged(Party party, Flag flag) {
            return party.Where(c => c.HasFlag(flag)).First();
        }

        private static Character GetHero(Party party) {
            return GetFlagged(party, Flag.HERO);
        }

        private static Character GetPartner(Party party) {
            return GetFlagged(party, Flag.PARTNER);
        }
    }
}