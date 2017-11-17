using Scripts.Game.Areas;
using Scripts.Game.Serialized;
using Scripts.Model.Acts;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Pages {

    /// <summary>
    /// Area selection page.
    /// </summary>
    /// <seealso cref="Scripts.Model.Pages.PageGroup" />
    public class WorldPages : Model.Pages.PageGroup {

        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPages"/> class.
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="party">The party.</param>
        public WorldPages(Page previous, Flags flags, Party party) : base(new Page("Worlds")) {
            Root.Body = "Which world would you like to go to?";
            Root.Icon = Util.GetSprite("journey");
            Root.Condition = PageUtil.GetVisitProcessCondition(flags, party);
            IList<IButtonable> buttons = new List<IButtonable>();
            buttons.Add(PageUtil.GenerateBack(previous));

            Root.AddCharacters(Side.LEFT, party.Collection);

            foreach (AreaType type in AreaList.ALL_AREAS.Keys) {
                if (flags.IsAreaUnlocked(type) && flags.CurrentArea != type) {
                    buttons.Add(GetAreaChangeProcess(type, flags, party, previous));
                }
            }

            Root.Actions = buttons;
        }

        /// <summary>
        /// Gets the area change process.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="party">The party.</param>
        /// <param name="previous">The previous.</param>
        /// <returns></returns>
        private Process GetAreaChangeProcess(AreaType type, Flags flags, Party party, Page previous) {
            return new Process(
                    type.GetDescription(),
                    AreaList.AREA_SPRITES[type],
                    "Move to this area.",
                    () => {
                        flags.ShouldAdvanceTimeInCamp = true;
                        flags.CurrentArea = type;

                        // Manually update camp details
                        previous.Icon = AreaList.AREA_SPRITES[type]; // Won't update soon enough
                        previous.Location = type.GetDescription();

                        Battle.DoPageTransition(previous);
                        previous.AddText(string.Format("Moved to {0}.", type.GetDescription()));
                    }
                );
        }
    }
}