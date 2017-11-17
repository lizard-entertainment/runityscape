using System.Collections.Generic;
using Scripts.Game.Serialized;
using Scripts.Model.Characters;
using Scripts.Model.Pages;
using Scripts.Game.Areas;
using Scripts.Model.Acts;
using System.Linq;

namespace Scripts.Game.Stages {

    public class SceneStage : Stage {
        private Page page;
        private Act[] acts;

        public SceneStage(Page page, string stageName, params Act[] acts) : base(stageName) {
            this.acts = acts;
            this.page = page;
        }

        public override Page GetPage(int stageIndex, int areaTotalStageCount, Flags flags, IEnumerable<Character> party, Page camp, Page quests, AreaType type) {
            page.OnEnter = () =>
                ActUtil.SetupScene(
                    acts,
                    () => OnStageClear(areaTotalStageCount, type, flags, stageIndex), // Do this first incase if a scene is the last stage area
                    () => {
                        // Probably a better solution than this? TODO
                        if (!acts.Any(a => a is PageChangeAct)) {
                            camp.Invoke();
                        }
                    });
            return page;
        }
    }
}