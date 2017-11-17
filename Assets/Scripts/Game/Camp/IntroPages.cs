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
            page.AddCharacters(Side.RIGHT, partner);

            page.OnEnter = () => {
                ActUtil.SetupScene(
                         YourVoice("(Where am I)"),
                         PartnerVoice(string.Format("My name is...")),
                         new InputAct("What is their name?", (s) =>
                                ActUtil.SetupScene(
                                    PartnerVoice("My name is " + s),
                                    new ActionAct(() => partner.Look.Name = s),
                                    new ActionAct(() => GoToCamp(you, partner)
                                    )
                                )
                        )
                    );
            };
        }

        private TextAct UnknownVoice(string message) {
            return new TextAct(new AvatarBox(Side.RIGHT, Util.GetSprite("question-mark"), Color.grey, message));
        }

        private TextAct YourVoice(string message) {
            return new TextAct(new AvatarBox(Side.LEFT, hero, Color.white, message));
        }
    }
}