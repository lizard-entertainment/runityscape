using Scripts.Game.Areas;
using Scripts.Game.Dungeons;
using Scripts.Game.Serialized;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using System.Collections.Generic;

namespace Scripts.Game.Pages {

    /// <summary>
    /// Pages showing off the bonus areas of a particular aea.
    /// </summary>
    /// <seealso cref="Scripts.Model.Pages.PageGroup" />
    public class PlacePages : Model.Pages.PageGroup {
        private readonly Page previous;
        private readonly Flags flags;
        private readonly Party party;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlacePages"/> class.
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="party">The party.</param>
        public PlacePages(Page previous, Flags flags, Party party) : base(new Page("Places")) {
            this.previous = previous;
            this.flags = flags;
            this.party = party;

            SetupRoot();
        }

        private void SetupRoot() {
            Page p = Root;
            p.Condition = PageUtil.GetVisitProcessCondition(flags, party);
            p.Icon = Util.GetSprite("castle-ruins");
            p.Body = "Where would you like to go?";
            p.AddCharacters(Side.LEFT, party);
            var buttons = new List<IButtonable>();
            buttons.Add(PageUtil.GenerateBack(previous));

            foreach (PageGroup pg in GetCurrentArea(flags.CurrentArea).Places) {
                buttons.Add(GetPlaceProcess(pg));
            }

            p.Actions = buttons;
        }

        /// <summary>
        /// Gets the place process.
        /// </summary>
        /// <param name="pg">The pg.</param>
        /// <returns></returns>
        private Process GetPlaceProcess(Model.Pages.PageGroup pg) {
            return new Process(
                    pg.Root.Location,
                    pg.Root.Sprite,
                    pg.Root.TooltipText,
                    () => {
                        pg.Root.Invoke();
                        flags.ShouldAdvanceTimeInCamp = true;
                    }
                );
        }

        /// <summary>
        /// Gets the current area.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private Area GetCurrentArea(AreaType type) {
            return AreaList.ALL_AREAS[type](flags, party, previous, Root);
        }
    }
}