using Scripts.Game.Defined.Characters;
using Scripts.Game.Serialized;
using Scripts.Model.Acts;
using Scripts.Model.Characters;
using Scripts.Model.Pages;
using Scripts.Model.TextBoxes;
using UnityEngine;

namespace Scripts.Game.Pages {

    /// <summary>
    /// Pages used in the game introduction.
    /// </summary>
    /// <seealso cref="Scripts.Model.Pages.PageGroup" />
    public class IntroPages : PageGroup {
        private static readonly Sprite hero = CharacterList.Hero(string.Empty).Look.Sprite;

        private static readonly Sprite partner = CharacterList.Partner(string.Empty).Look.Sprite;

        public IntroPages(string name) : base(new Page("Unknown")) {
            SetupIntro(name);
        }

        private void GoToCamp(Character you, Character partner) {
            Party party = new Party();
            Flags flags = new Flags();
            party.AddMember(you);
            party.AddMember(partner);
            Camp camp = new Camp(party, flags);
            camp.Root.Invoke();
        }

        private TextAct PartnerVoice(string message) {
            return new TextAct(new AvatarBox(Side.RIGHT, partner, Color.white, message));
        }

        private void SetupIntro(string name) {
            Page page = Root;
            Character you = CharacterList.Hero(name);
            Character partner = CharacterList.Partner("???");

            page.AddCharacters(Side.LEFT, you);

            page.OnEnter = () => {
                ActUtil.SetupScene(
                         YourVoice("(Where am I?)"),
                         YourVoice("(Is this a dream...?)"),
                         YourVoice("..."),
                         YourVoice("(Last thing I remember, I was working on that VGDev game...)"),
                         new ActionAct(() => page.AddCharacters(Side.RIGHT, partner)),
                         PartnerVoice(string.Format("{0}! There is no time to waste! The twin demons must be destroyed!", name)),
                         YourVoice("(Their sprite... That's the main character for the game I'm working on!)"),
                         YourVoice("(What name did I give them again?)"),
                         new InputAct("What is their name?", (s) =>
                                ActUtil.SetupScene(
                                    new ActionAct(() => partner.Look.Name = s),
                                    YourVoice(string.Format("{0}! I don't think you understand! I'm not supposed to be here!", s)),
                                    PartnerVoice("An anomaly surely caused by those foul demons! Let us make haste and carve out a pathway to them!"),
                                    YourVoice("(Are they talking about the final boss? Maybe if I can escape this system if we beat the game...)"),
                                    YourVoice("(I'll play along for now.)"),
                                    YourVoice(string.Format("Lead the way, brave knight {0}.", s)),
                                    PartnerVoice("Let us approach our camp of operations."),
                                    new ActionAct(() => GoToCamp(you, partner))
                                )
                        )
                    );
            };
        }

        private TextAct YourVoice(string message) {
            return new TextAct(new AvatarBox(Side.LEFT, hero, Color.white, message));
        }
    }
}