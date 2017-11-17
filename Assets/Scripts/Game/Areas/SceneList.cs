using Scripts.Model.Acts;
using Scripts.Model.Characters;
using Scripts.Model.Pages;

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
    }
}