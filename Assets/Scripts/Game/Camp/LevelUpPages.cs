using Scripts.Game.Defined.Serialized.Statistics;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.Stats;
using Scripts.Model.TextBoxes;
using System.Collections.Generic;

namespace Scripts.Game.Pages {

    /// <summary>
    /// Pages for performing level ups and viewing stats on party members.
    /// </summary>
    /// <seealso cref="Scripts.Model.Pages.PageGroup" />
    public class LevelUpPages : PageGroup {

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelUpPages"/> class.
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="party">The party.</param>
        public LevelUpPages(Page previous, IEnumerable<Character> party) : base(new Page("Party")) {
            SetupRoot(previous, party);
        }

        private void SetupRoot(Page previous, IEnumerable<Character> party) {
            Page p = Get(ROOT_INDEX);
            p.AddCharacters(Side.LEFT, party);
            p.Icon = Util.GetSprite("person");

            List<IButtonable> buttons = new List<IButtonable>();
            buttons.Add(PageUtil.GenerateBack(previous));
            foreach (Character partyMember in party) {
                buttons.Add(GetSpecificCharacterStats(partyMember, p));
            }
            p.Actions = buttons;
        }

        /// <summary>
        /// Gets a specific character's stats.
        /// </summary>
        /// <param name="characterToViewStatsOf">The c.</param>
        /// <param name="previous">The previous.</param>
        /// <returns></returns>
        private Page GetSpecificCharacterStats(Character characterToViewStatsOf, Page previous) {
            Page p = new Page(string.Format("{0}", characterToViewStatsOf.Look.DisplayName));
            p.AddCharacters(Side.LEFT, characterToViewStatsOf);
            p.Icon = characterToViewStatsOf.Look.Sprite;
            p.OnEnter = () => {
                List<IButtonable> actions = new List<IButtonable>();
                actions.Add(PageUtil.GenerateBack(previous));
                actions.Add(GetSendPlayerToPointAllocationPageProcess(characterToViewStatsOf, p));
                if (Util.IS_DEBUG) {
                    actions.Add(ExpHack(characterToViewStatsOf));
                }
                DisplayStats(p, characterToViewStatsOf);
                p.AddText(characterToViewStatsOf.Spells.SpellTextsDisplay);
                LevelUpPage(characterToViewStatsOf, p);
                p.Actions = actions.ToArray();
            };
            return p;
        }

        /// <summary>
        /// Displays the stats.
        /// </summary>
        /// <param name="current">The p.</param>
        /// <param name="characterToDisplayStatsOf">The c.</param>
        private void DisplayStats(Page current, Character characterToDisplayStatsOf) {
            int level = characterToDisplayStatsOf.Stats.Level;
            int diffToLevelUp = characterToDisplayStatsOf.Stats.GetStatCount(Stats.Get.MAX, StatType.EXPERIENCE) - characterToDisplayStatsOf.Stats.GetStatCount(Stats.Get.MOD, StatType.EXPERIENCE);
            int statPoints = characterToDisplayStatsOf.Stats.UnassignedStatPoints;
            current.AddText(characterToDisplayStatsOf.Stats.LongAttributeDistribution);
            current.AddText(string.Format("{0} Experience until level up.", diffToLevelUp));
            current.AddText(string.Format("{0} Stat Point(s) to allocate.", statPoints));
        }

        /// <summary>
        /// Hacks experience. Usable in debug mode.
        /// </summary>
        /// <param name="characterToAddExperiencePointsTo">The character to add experience points to.</param>
        /// <returns></returns>
        private Process ExpHack(Character characterToAddExperiencePointsTo) {
            return new Process(
                "Exp HACK!",
                () => characterToAddExperiencePointsTo.Stats.AddToStat(StatType.EXPERIENCE, Stats.Set.MOD_UNBOUND, 1000)
                );
        }

        /// <summary>
        /// Gets the process that sends the user to the allocation page for a particular character.
        /// </summary>
        /// <param name="characterToAllocatePointsTo">Character to allocate points to.</param>
        /// <param name="previous">The previous page.</param>
        /// <returns></returns>
        private Process GetSendPlayerToPointAllocationPageProcess(Character characterToAllocatePointsTo, Page previous) {
            return new Process(
                    string.Format("Allocate ({0})",
                        characterToAllocatePointsTo.Stats.UnassignedStatPoints),
                    "Allocate stat points.",
                    () => LevelUpPage(characterToAllocatePointsTo, previous).Invoke()
                    );
        }

        /// <summary>
        /// Levels up page.
        /// </summary>
        /// <param name="characterToDoAllocationsFor">The character to do allocations for.</param>
        /// <param name="previous">The previous page.</param>
        /// <returns></returns>
        private Page LevelUpPage(Character characterToDoAllocationsFor, Page previous) {
            Page p = new Page(string.Format("{0}'s stat allocation", characterToDoAllocationsFor.Look.DisplayName));
            p.Body = "Choose a stat to increase. You must rest afterwards to gain the benefit of the stat.";
            p.AddCharacters(Side.LEFT, characterToDoAllocationsFor);
            p.OnEnter = () => {
                LetUserSelectAStatToIncrease(previous, p, characterToDoAllocationsFor);
            };
            return p;
        }

        /// <summary>
        /// Lets the user select a stat to increase.
        /// </summary>
        /// <param name="previous">The previous page.</param>
        /// <param name="current">The current page.</param>
        /// <param name="characterWhoIsBeingAllocated">The c.</param>
        private void LetUserSelectAStatToIncrease(Page previous, Page current, Character characterWhoIsBeingAllocated) {
            List<IButtonable> actions = new List<IButtonable>();
            actions.Add(PageUtil.GenerateBack(previous));

            foreach (StatType st in StatType.ASSIGNABLES) {
                if (characterWhoIsBeingAllocated.Stats.HasStat(st)) {
                    actions.Add(
                        AddStatProcess(current, characterWhoIsBeingAllocated.Stats, st)
                        );
                }
            }

            if (characterWhoIsBeingAllocated.Stats.UnassignedStatPoints > 0) {
                current.AddText(string.Format("Select a stat to increase.\nPoints remaining: {0}.", characterWhoIsBeingAllocated.Stats.UnassignedStatPoints));
            } else {
                current.AddText(string.Format("{0} has no stat points to spend.", characterWhoIsBeingAllocated.Look.DisplayName));
            }
            current.Actions = actions.ToArray();
        }

        /// <summary>
        /// Gets a process that adds to a particular stat.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="statsToAddTo">The stats to add to.</param>
        /// <param name="statTypeToAddTo">The stat type to add to.</param>
        /// <returns></returns>
        private Process AddStatProcess(Page current, Stats statsToAddTo, StatType statTypeToAddTo) {
            return new Process(
                statTypeToAddTo.ColoredName,
                statTypeToAddTo.Sprite,
                string.Format("Add {0} point(s) in\n{1}\n{2}",
                    statTypeToAddTo.StatPointIncreaseAmount,
                    statTypeToAddTo.ColoredName,
                    statTypeToAddTo.Description),
                () => {
                    statsToAddTo.UnassignedStatPoints--;
                    statsToAddTo.AddToStat(statTypeToAddTo, Stats.Set.MAX, statTypeToAddTo.StatPointIncreaseAmount);
                    current.AddText(string.Format("Maximum {0} was increased to {1}.\nPoints remaining: {2}.", statTypeToAddTo.Name, statsToAddTo.GetStatCount(Stats.Get.MAX, statTypeToAddTo), statsToAddTo.UnassignedStatPoints));
                },
                () => statsToAddTo.UnassignedStatPoints > 0
                );
        }
    }
}