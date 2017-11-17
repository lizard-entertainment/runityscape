using Scripts.Game.Defined.Characters;
using Scripts.Model.Acts;
using Scripts.Model.Characters;
using Scripts.Model.Pages;
using System.Linq;

namespace Scripts.Game.Stages {

    // SceneStages go here to avoid gunking up AreaList.
    public static class SceneList {

        public static SceneStage Example(Party party) {
            Page page = new Page("Example Location");
            page.AddCharacters(Side.LEFT, party); // Party members on the left
            return new SceneStage(page, "Stage Name",
                    new TextAct("Hello"),
                    new TextAct(party.Default, Side.LEFT, "I am saying something.")
                );
        }

        public static SceneStage JavaCrypt1(Party party) {
            Character friend = party.GetCharacter(c => c.HasFlag(Flag.PARTNER));
            string friendName = friend.Look.DisplayName;
            
            Page page = new Page("The Java Crypt");
            page.AddCharacters(Side.LEFT, party); // Party members on the left
            return new SceneStage(page, "Stanic Beginings",
                    new TextAct("It's well past midnight at the Java Crypt"),
                    new TextAct(party.Default, Side.LEFT, "Gathering freshly deceased souls for the dark lord is a lot of work..."),
                    new TextAct(party.Default, Side.LEFT, "It's worth it though. Anything to grow my capitalistic power."),
                    new TextAct(friend, Side.RIGHT, "Uh, dude, what're you doing out here? It's like 1am?"),
                    new TextAct("Your friend " + friendName + " looks around nervously"),
                    new TextAct(friend, Side.RIGHT, "This place is spooky!"),
                    new TextAct(party.Default, Side.LEFT, "I'm glad you made it out here " + friendName + ", I need you to help me gather some souls."),
                    new TextAct(party.Default, Side.LEFT, "I need them to gain material wealth.")
                );
        }

        public static SceneStage JavaCrypt2(Party party) {
            Character friend = party.GetCharacter(c => c.HasFlag(Flag.PARTNER));
            string friendName = friend.Look.DisplayName;

            Page page = new Page("The Java Crypt");
            page.AddCharacters(Side.LEFT, party); // Party members on the left
            return new SceneStage(page, "Stanic Reveal",
                    new TextAct(friend, Side.RIGHT, "We've mushed a lot of these things... Did you ever even tell me what you're using them for?"),
                    new TextAct(party.Default, Side.LEFT, "Oh, nothing really"),
                    new TextAct(friendName + " looks at you, unconvinced"),
                    new TextAct(party.Default, Side.LEFT, "I mean, if you must know"),
                    new TextAct(friendName + " keeps staring at you"),
                    new TextAct(party.Default, Side.LEFT, "Ugh, fine, I'm using them for a stanic summoning ritual to obtain material wealth"),
                    new TextAct(friend, Side.RIGHT, "That sounds kinda silly and little bourgeoisie but I guess I can't leave you now.")

                );
        }

        public static SceneStage JavaCrypt3(Party party) {
            Character friend = party.GetCharacter(c => c.HasFlag(Flag.PARTNER));
            string friendName = friend.Look.DisplayName;
            Character demonWhoseNameEscapesMe = RuinsNPCs.Replicant();
            Character mom1 = RuinsNPCs.Villager();
            Character mom2 = RuinsNPCs.Knight();
            Character mom3 = RuinsNPCs.BigKnight();
            Character mom4 = RuinsNPCs.Healer();
            Character mom5 = RuinsNPCs.Wizard();
            Character mom6 = RuinsNPCs.Illusionist();
            Character mom7 = RuinsNPCs.Villager();
            Character mom8 = RuinsNPCs.Wizard();
            Character mom9 = RuinsNPCs.Illusionist();
            Character mom10 = RuinsNPCs.Villager();


            Page page = new Page("The Java Crypt");
            page.AddCharacters(Side.LEFT, party); // Party members on the left
            return new SceneStage(page, "Stanic Reversal",
                    new TextAct(party.Default, Side.LEFT, "We sure showed that big scary demon! It's too bad it got away before I could harvest its soul..."),
                    new TextAct(friend, Side.LEFT, "Yeah... I hope it doesn't have friends..."),
                    new TextAct("At that very moment..."),
                    new TextAct(demonWhoseNameEscapesMe, Side.RIGHT, "You thought I was a lame friendless looooser! But just you wait! I told my mom what you said to me"),
                    new TextAct(mom1, Side.RIGHT, ":("),
                    new TextAct(mom2, Side.RIGHT, ":(:("),
                    new TextAct(mom3, Side.RIGHT, ":(:(:("),
                    new TextAct(mom4, Side.RIGHT, ":(:(:(:(:("),
                    new TextAct(mom5, Side.RIGHT, ":(:(:(:(:(:("),
                    new TextAct(mom6, Side.RIGHT, ":(:(:(:(:(:(:("),
                    new TextAct(mom7, Side.RIGHT, ":(:(:(:(:(:(:(:("),
                    new TextAct(mom8, Side.RIGHT, ":(:(:(:(:(:(:(:(:("),
                    new TextAct(mom9, Side.RIGHT, ":(:(:(:(:(:(:(:(:(:("),
                    new TextAct(mom10, Side.RIGHT, ":(:(:(:(:(:(:(:(:(:(:("),

                    new TextAct(friend, Side.LEFT, "Run for it!"),
                    new TextAct("The two of you flee the onrushing hoard, barely making it out through the city's sewer system, and into the Objective Sea")

                );
        }

        public static SceneStage ObjectiveSea1(Party party) {
            Character friend = party.GetCharacter(c => c.HasFlag(Flag.PARTNER));
            string friendName = friend.Look.DisplayName;

            Page page = new Page("The Objective Sea");
            page.AddCharacters(Side.LEFT, party); // Party members on the left
            return new SceneStage(page, "Watery Stanism",
                    new TextAct("The two of you have fled into the depths of the Objective Sea, and find yourselves among the vicious predators there."),
                    new TextAct(party.Default, Side.LEFT, "Wow! It sure smells fishy here! Nyuck nyuck nyuck!"),
                    new TextAct(friend, Side.RIGHT, "Now isn't the time for kracken bad jokes! We're surrounded by hungry fish, and way out of our element!"),
                    new TextAct(party.Default, Side.LEFT, "Don't worry, I got a lot of souls from the crypt!"),
                                        new TextAct(party.Default, Side.LEFT, "We just need to collect a few more, and offer them to the dark lord!"),
                    new TextAct(party.Default, Side.LEFT, "Let's get out there and do some murder! OwO")
                );
        }

        public static SceneStage ObjectiveSea2(Party party) {
            Character friend = party.GetCharacter(c => c.HasFlag(Flag.PARTNER));
            string friendName = friend.Look.DisplayName;
            Character mom1 = OceanNPCs.SharkPirate();

            Page page = new Page("The Objective Sea");
            page.AddCharacters(Side.LEFT, party); // Party members on the left
            return new SceneStage(page, "Stanism hostage crisis",
                    new TextAct("You have the shark pirate defeated, you're prepared to strike the final blow, when:"),
                    new TextAct(mom1, Side.RIGHT, "Wait! I know we both just tried really hard to commit murder against our respective bodily vessels, but hear me out"),
                    new TextAct(party.Default, Side.LEFT, "Fool! I would never listen to the desperate ramblings of a bourgeoisie scum!"),
                    new TextAct(mom1, Side.RIGHT, "No no, I hate the rich just as much as you! I can help you purify the world of them!"),
                    new TextAct(mom1, Side.RIGHT, "There's a secret bioweapon being developed in a government facility."),
                    new TextAct(mom1, Side.RIGHT, "It's a terrible contagion that spreads through gold-plated cutlery"),
                    new TextAct(mom1, Side.RIGHT, "The prolitarieat would be completely unnaffected, but you could wipe out the owning class overnight!"),
                    new TextAct(party.Default, Side.LEFT, "A tempting offer... Perhaps I will spare your life in exchange..."),
                    new TextAct(friend, Side.LEFT, "Let's get down to buisysnessness!"),
                                        new TextAct(friend, Side.LEFT, "To defeat"),
                                                            new TextAct(friend, Side.LEFT, "The bourgeoisie"),
                    new TextAct("The two of you skip off merrily into the sunset, off to find a bioweapon and engage in class warfare.")
                );
        }

        public static SceneStage Labs1(Party party) {
            Character friend = party.GetCharacter(c => c.HasFlag(Flag.PARTNER));
            string friendName = friend.Look.DisplayName;

            Page page = new Page("The Mat Labs");
            page.AddCharacters(Side.LEFT, party); // Party members on the left
            return new SceneStage(page, "Mutated Stanism",
                    new TextAct("You arrive at the high security super secret government research facility."),
                    new TextAct(party.Default, Side.LEFT, "Excellent! Plenty of guards to my soul sacrifice collection."),
                    new TextAct(friend, Side.LEFT, "Let's get to work! Time to liberate the proletariat! The streets will run pink with the ketchup of the bourgeoisie")
                );
        }

        public static SceneStage Labs2(Party party) {
            Character friend = party.GetCharacter(c => c.HasFlag(Flag.PARTNER));
            string friendName = friend.Look.DisplayName;

            Page page = new Page("The Mat Labs");
            page.AddCharacters(Side.LEFT, party); // Party members on the left
            return new SceneStage(page, "Stanist clones?",
                    new TextAct(party.Default, Side.LEFT, "Ahead... In the darkness... Doesn't that look like... You?."),
                    new TextAct(friend, Side.LEFT, "No, it's you! The bourgeoisie must've extracted the miniscule individualistic desire within you and used it as last-ditch effort to stall their erradication")
                );
        }

        public static SceneStage Labs3(Party party) {
            Character friend = party.GetCharacter(c => c.HasFlag(Flag.PARTNER));
            string friendName = friend.Look.DisplayName;

            Page page = new Page("The Mat Labs");
            page.AddCharacters(Side.LEFT, party); // Party members on the left
            return new SceneStage(page, "Gold plated Stanism",
                new TextAct("You strike down your evil clone, composed of pure capitalism"),
                    new TextAct(party.Default, Side.LEFT, "I feel lighter... Almost as though the sins of capitalism have been lifted from my soul"),
                    new TextAct(friend, Side.LEFT, "I feel it too! I'm finally free from the iron boots of consumerism!"),
                    new TextAct(party.Default, Side.LEFT, "Come, comrade, let us bathe in the kethup of our foes, and proclaim the rebirth of the collectivist society!"),
                    new TextAct("And all the bourgeoisie lost their ketchup. THE END")
                );
        }
    }
}