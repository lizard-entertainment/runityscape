using Scripts.Game.Defined.Characters;
using Scripts.Game.Defined.Serialized.Statistics;
using Scripts.Game.Dungeons;
using Scripts.Game.Serialized;
using Scripts.Game.Undefined.Characters;
using Scripts.Model.Acts;
using Scripts.Model.Buffs;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using Scripts.Model.TextBoxes;
using Scripts.Presenter;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Game.Pages {

    /// <summary>
    /// The hub of the game, from which all other parts can be visited.
    /// </summary>
    public class Camp : Model.Pages.PageGroup {

        /// <summary>
        /// Missing percentage to restore when resting
        /// </summary>
        private const float MISSING_REST_RESTORE_PERCENTAGE = .2f;

        private Flags flags;
        private Party party;

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="party">Party for this particular game.</param>
        /// <param name="flags">Flags for this particular game.</param>
        public Camp(Party party, Flags flags) : base(new Page("Campsite")) {
            this.party = party;
            this.flags = flags;
            SetupCamp();
        }

        /// <summary>
        /// Setup on enter events.
        /// </summary>
        private void SetupCamp() {
            Page root = Get(ROOT_INDEX);
            root.OnEnter = () => {
                root.Location = flags.CurrentArea.GetDescription();
                root.Icon = Areas.AreaList.AREA_SPRITES[flags.CurrentArea];

                // If this isn't first Resting will advance to wrong time
                if (flags.ShouldAdvanceTimeInCamp) {
                    AdvanceTime(root);
                    flags.ShouldAdvanceTimeInCamp = false;
                }

                foreach (Character partyMember in party.Collection) {
                    if (partyMember.Stats.HasUnassignedStatPoints) {
                        root.AddText(
                            string.Format(
                                "<color=cyan>{0}</color> has unallocated stat points. Points can be allocated in the <color=yellow>Party</color> page.",
                                partyMember.Look.DisplayName));
                    }
                }

                Model.Pages.PageGroup dungeonSelectionPage = new StagePages(root, party, flags);

                root.AddCharacters(Side.LEFT, party.Collection);
                root.Actions = new IButtonable[] {
                    SubPageWrapper(dungeonSelectionPage, "Visit the stages for this World. When all stages are completed, the next World is unlocked."),
                    SubPageWrapper(new PlacePages(root, flags, party), "Visit a place in this World. Places are unique to a world and can offer you a place to spend your wealth."),
                    SubPageWrapper(new WorldPages(root, flags, party), "Return to a previously unlocked World. Worlds consist of unique Stages and Places."),
                    SubPageWrapper(new LevelUpPages(Root, party), "View party member stats and distribute stat points."),
                    PageUtil.GenerateGroupSpellBooks(root, root, party.Collection),
                    SubPageWrapper(new InventoryPages(root, party), "View the party's shared inventory and use items."),
                    SubPageWrapper(new EquipmentPages(root, party), "View and manage the equipment of your party members."),
                    RestProcess(root),
                    SubPageWrapper(new SavePages(root, party, flags), "Save and exit the game.")
                };

                PostTime(root);
            };
        }

        private Process SubPageWrapper(PageGroup pg, string description) {
            return new Process(
                    pg.ButtonText,
                    pg.Sprite,
                    description,
                    () => pg.Invoke(),
                    () => pg.IsInvokable
                );
        }

        /// <summary>
        /// Posts the time onto the textholder.
        /// </summary>
        /// <param name="current"></param>
        private void PostTime(Page current) {
            current.AddText(string.Format("{0} of day {1}.", flags.Time.GetDescription(), flags.DayCount));
            if (flags.Time == TimeOfDay.NIGHT) {
                current.AddText("It is too dark to leave camp.");
            }
        }

        /// <summary>
        /// Creates the process for resting.
        /// </summary>
        /// <param name="current">Current page</param>
        /// <returns>A rest process.</returns>
        private Process RestProcess(Page current) {
            TimeOfDay[] times = Util.EnumAsArray<TimeOfDay>();
            int currentIndex = (int)flags.Time;
            int newIndex = (currentIndex + 1) % times.Length;
            bool isLastTime = (currentIndex == (times.Length - 1));
            return new Process(
                isLastTime ? "Sleep" : "Rest",
                isLastTime ? Util.GetSprite("bed") : Util.GetSprite("health-normal"),
                isLastTime ? string.Format("Sleep to the next day ({0}).\nFully restores most stats and removes most status conditions.", flags.DayCount + 1)
                    : string.Format("Take a short break, advancing the time of day to {0}.\nSomewhat restores most stats.", times[newIndex].GetDescription()),
                () => {
                    foreach (Character c in party) {
                        c.Stats.RestoreResourcesByMissingPercentage(isLastTime ? 1 : MISSING_REST_RESTORE_PERCENTAGE);
                        if (isLastTime) {
                            c.Buffs.DispelAllBuffs();
                        }
                    }
                    if (isLastTime) {
                        flags.DayCount %= int.MaxValue;
                        flags.DayCount++;
                    }
                    flags.Time = times[newIndex];
                    current.AddText(string.Format("The party {0}s.", isLastTime ? "sleep" : "rest"));
                    current.OnEnter();
                }
                );
        }

        /// <summary>
        /// Makes time go forward in camp. From visiting places.
        /// </summary>
        /// <param name="current">The current page.</param>
        private void AdvanceTime(Page current) {
            TimeOfDay[] times = Util.EnumAsArray<TimeOfDay>();
            int currentIndex = (int)flags.Time;
            int newIndex = (currentIndex + 1) % times.Length;
            flags.Time = times[newIndex];
            current.AddText("Some time has passed.");
        }
    }
}