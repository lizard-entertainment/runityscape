using Scripts.Game.Defined.Serialized.Brains;
using Scripts.Game.Defined.SFXs;
using Scripts.Game.Defined.Unserialized.Spells;
using Scripts.Model.Acts;
using Scripts.Model.Buffs;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Items;
using Scripts.Model.Processes;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using Scripts.Model.TextBoxes;
using Scripts.Model.Tooltips;
using Scripts.Presenter;
using Scripts.View.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Pages {

    /// <summary>
    /// This page represents a battle between two sides.
    /// </summary>
    public class Battle : Page {
        private const int MINIMUM_EXP_PER_BATTLE = 1;

        private const string EXPERIENCE_GAIN = "{0} gains {1} {2}.";

        private const string BATTLE_START = "The battle begins.";

        private const string BUFF_AFFECT = "<color=yellow>{0}</color> is affected by <color=cyan>{1}</color>.";

        private const string BUFF_FADE = "<color=yellow>{0}</color>'s <color=cyan>{1}</color> fades.";

        private const int CAPACITY_FLAGGED_ITEM_LOOT_AMOUNT = 1;

        private const string CHARACTER_DEATH = "<color=yellow>{0}</color> has been <color=red>defeated</color>.";

        private const string DEFEAT = "Defeat!";

        private const string LOOT_MESSAGE = "{0}({1}) was added to the inventory.";

        private const string PLAYER_QUESTION = "What will <color=yellow>{0}</color> do?";

        private const string ROUND_START = "Turn {0}";

        private const string VICTORY = "Victory!";

        private Page defeat;

        private HashSet<Character> leftGraveyard;

        private HashSet<Character> rightGraveyard;

        private int turnCount;

        private Page victory;

        private IDictionary<Item, int> loot;

        private int experienceGiven;

        private bool wasExperienceGiven;

        private bool isUseTransition;

        private IList<ISpellable> temporarySpells;

        private IDictionary<Character, Spell> charactersChargingSpells;

        private Side victoriousSide;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defeat">Page to go to on defeat</param>
        /// <param name="victory">Page to go to on victory</param>
        /// <param name="music">Music to be played</param>
        /// <param name="location">Location of the battle</param>
        /// <param name="left">Characters on the left in battle</param>
        /// <param name="right">Characters on the right in battle</param>
        public Battle(Page defeat, Page victory, Music music, string location, IEnumerable<Character> left, IEnumerable<Character> right, bool isUseTransition = false, bool isFleeable = true) : base(location) {
            this.wasExperienceGiven = false;
            this.loot = new Dictionary<Item, int>();
            this.leftGraveyard = new HashSet<Character>(new IdentityEqualityComparer<Character>());
            this.rightGraveyard = new HashSet<Character>(new IdentityEqualityComparer<Character>());
            this.defeat = defeat;
            this.victory = victory;
            this.Music = music.GetDescription();
            this.isUseTransition = isUseTransition;
            turnCount = 0;
            temporarySpells = new List<ISpellable>();
            foreach (Character c in left) {
                Left.Add(c);
            }
            foreach (Character c in right) {
                Right.Add(c);
            }
            if (isFleeable) {
                SetupTempSpells();
            }
            this.charactersChargingSpells = new Dictionary<Character, Spell>(new IdNumberEqualityComparer<Character>());
            OnEnter += () => {
                if (!isFleeable) {
                    AddText("Can't flee this battle!");
                }
                if (!IsResolved) {
                    Presenter.Main.Instance.StartCoroutine(startBattle());
                } else {
                    PostBattle(PlayerStatus);
                }
            };
        }

        /// <summary>
        /// After resolution this will return the EXP given out.
        /// </summary>
        public int ExperienceGiven {
            get {
                return this.experienceGiven;
            }
        }

        /// <summary>
        /// Turns elapsed in this battle.
        /// </summary>
        public int TurnCount {
            get {
                return turnCount;
            }
        }

        public override bool IsInBattle {
            get {
                return !IsResolved;
            }
        }

        /// <summary>
        /// Adds in the Flee spell, which requires
        /// a page to be constructed.
        /// </summary>
        private void SetupTempSpells() {
            AddTempSpell(new Flee(defeat, () => {
                Main.Instance.StopAllCoroutines();
                AddText("Fled from battle.");
            }
            ));
        }

        private void AddTempSpell(SpellBook temporary) {
            Util.Assert(!temporary.HasFlag(Spells.Flag.CASTER_REQUIRES_SPELL), "Has flag: Caster req spell.");
            temporarySpells.Add(temporary);
        }

        private enum PlayerPartyStatus {
            ALIVE,
            DEAD,
            NOT_FOUND
        }

        private bool IsResolved {
            get {
                return Left.All(c => c.Stats.State == State.DEAD) || Right.All(c => c.Stats.State == State.DEAD);
            }
        }

        public static void DoPageTransition(Page destination) {
            Main.Instance.Title.Play(destination.Sprite, string.Format("{0}", destination.Location.Replace(" - ", "\n")), () => destination.Invoke());
            Main.Instance.Sound.PlaySound("Steps_0");
        }

        private PlayerPartyStatus PlayerStatus {
            get {
                // Left is player party
                if (Left.Any(c => c.HasFlag(Characters.Flag.PLAYER))) {
                    return Left.Any(c => c.Stats.State == State.ALIVE) ? PlayerPartyStatus.ALIVE : PlayerPartyStatus.DEAD;

                    // right is player party
                } else if (Right.Any(c => c.HasFlag(Characters.Flag.PLAYER))) {
                    return Right.Any(c => c.Stats.State == State.ALIVE) ? PlayerPartyStatus.ALIVE : PlayerPartyStatus.DEAD;

                    //Player party not found
                } else {
                    return PlayerPartyStatus.NOT_FOUND;
                }
            }
        }

        private ICollection<Character> AllDefeated {
            get {
                List<Character> defeated = new List<Character>();
                if (VictoriousSide == Side.LEFT) {
                    defeated.AddRange(Right);
                    defeated.AddRange(rightGraveyard);
                } else {
                    defeated.AddRange(Left);
                    defeated.AddRange(leftGraveyard);
                }
                return defeated;
            }
        }

        private ICollection<Character> VictoriousParty {
            get {
                if (VictoriousSide == Side.LEFT) {
                    return Left;
                } else {
                    return Right;
                }
            }
        }

        private Side VictoriousSide {
            get {
                return victoriousSide;
            }
        }

        public bool IsChargingSpell(Character character) {
            return charactersChargingSpells.ContainsKey(character);
        }

        public bool IsFoeWillUnleashSpellNextTurnOnTarget<T>(Character target) where T : SpellBook {
            return
                GetFoes(target).Any(foe =>
                    IsChargingSpell(foe)
                    && charactersChargingSpells[foe].IsTarget(target)
                    && charactersChargingSpells[foe].IsSpellCharged
                    && charactersChargingSpells[foe].SpellBook is T);
        }

        private static void AddEquipmentToLoot(IDictionary<Item, int> loot, Equipment equipment) {
            List<EquippableItem> equipmentItems = ((IEnumerable<EquippableItem>)equipment).ToList();
            foreach (EquippableItem item in equipmentItems) {
                AddItemToDictionary(loot, item, 1);
                equipment.RemoveEquip(new Inventory(), item.Type); // Dummy inventory to remove equipment
            }
        }

        private static void AddInventoryToLoot(IDictionary<Item, int> loot, Inventory inventory) {
            List<Item> inventoryItems = ((IEnumerable<Item>)inventory).ToList();
            foreach (Item item in inventoryItems) {
                AddItemToDictionary(loot, item, inventory.GetCount(item));
                inventory.Remove(item, inventory.GetCount(item));
            }
        }

        private static void AddItemsToLoot(IDictionary<Item, int> loot, IEnumerable<Character> defeatedSide) {
            foreach (Character c in defeatedSide) {
                if (c.HasFlag(Characters.Flag.DROPS_ITEMS)) {
                    AddInventoryToLoot(loot, c.Inventory);
                    AddEquipmentToLoot(loot, c.Equipment);
                }
            }
        }

        private static void AddItemToDictionary(IDictionary<Item, int> loot, Item item, int count) {
            if (!loot.ContainsKey(item)) {
                loot.Add(item, 0);
            }
            loot[item] += count;
        }

        private static Process GetItemStackProcess(Page current, SubGrid previousGrid, SubGrid lootGrid, IDictionary<Item, int> loot, Item item, Inventory victorInventory) {
            return new Process(
                    string.Format("{0}({1})", item.Name, loot[item]),
                    item.Icon,
                    string.Format("Loot this item.\n{0}\n{1}",
                    Util.ColorString(string.Format("Stack of {0}", loot[item]), Color.grey),
                    item.Description),
                    () => {
                        int lootAmount = GetOneClickLootAmount(loot, item);
                        loot[item] -= lootAmount;
                        if (loot[item] <= 0) {
                            loot.Remove(item);
                        }
                        victorInventory.Add(item, lootAmount);
                        current.AddText(string.Format(LOOT_MESSAGE, item.Name, lootAmount, victorInventory.Fraction));
                        lootGrid.Invoke();
                    },
                    () => loot.ContainsKey(item) && loot[item] > 0 && victorInventory.IsAddable(item, GetOneClickLootAmount(loot, item)
                ));
        }

        private static SubGrid GetLootGrid(Page current, SubGrid previous, IDictionary<Item, int> loot, ICollection<Character> victoriousSide) {
            SubGrid lootGrid = new SubGrid("Loot");
            lootGrid.Tooltip = "Pick up dropped items.";
            lootGrid.Icon = Util.GetSprite("swap-bag");
            lootGrid.Condition = () => (loot.Count > 0);
            lootGrid.OnEnter = () => {
                lootGrid.List.Clear();
                lootGrid.List.Add(PageUtil.GenerateBack(previous));
                foreach (Item item in loot.Keys) {
                    if (loot[item] > 0) {
                        lootGrid.List.Add(GetItemStackProcess(current, previous, lootGrid, loot, item, victoriousSide.FirstOrDefault().Inventory));
                    }
                }
            };
            return lootGrid;
        }

        private static int GetOneClickLootAmount(IDictionary<Item, int> loot, Item item) {
            int lootAmount = 0;
            if (item.HasFlag(Items.Flag.OCCUPIES_SPACE)) {
                lootAmount = CAPACITY_FLAGGED_ITEM_LOOT_AMOUNT;
            } else {
                lootAmount = loot[item];
            }
            return lootAmount;
        }

        private bool CharacterCanCast(Character c) {
            return c.Stats.State == State.ALIVE;
        }

        private IEnumerator CheckForDeath() {
            foreach (Character c in GetAll()) {
                HashSet<Character> graveyard = null;

                bool isLeftGrave = false;

                if (Left.Contains(c)) {
                    graveyard = leftGraveyard;
                    isLeftGrave = true;
                } else {
                    graveyard = rightGraveyard;
                    isLeftGrave = false;
                }
                bool characterIsInGraveyard = graveyard.Contains(c);

                // Display death effect
                if (c.Stats.State == State.DEAD && !characterIsInGraveyard) {
                    IList<Buff> dispellableOnDeath = c.Buffs.Where(b => b.IsDispellable).ToList();
                    foreach (Buff removableBuff in dispellableOnDeath) {
                        c.Buffs.RemoveBuff(RemovalType.DISPEL, removableBuff);
                    }
                    Main.Instance.Sound.PlaySound("synthetic_explosion_1");
                    yield return SFX.DoDeathEffect(c, 1f);
                    AddText(string.Format(CHARACTER_DEATH, c.Look.Name));

                    graveyard.Add(c);
                    if (!c.HasFlag(Characters.Flag.PERSISTS_AFTER_DEFEAT)) {
                        Left.Remove(c);
                        Right.Remove(c);
                    }
                }

                // Bring them back! (for revivals)
                if (c.Stats.State == State.ALIVE && characterIsInGraveyard) {
                    graveyard.Remove(c);
                    if (isLeftGrave) {
                        Left.Add(c);
                    } else {
                        Right.Add(c);
                    }
                }
            }
        }

        private IEnumerator DetermineCharacterActions(IList<Character> chars, IList<Spell> plays, HashSet<Character> playerActionSet) {
            ICollection<PooledBehaviour> declarations = new List<PooledBehaviour>();
            for (int i = 0; i < chars.Count; i++) {
                Character c = chars[i];

                if (c.Stats.State == State.ALIVE && !charactersChargingSpells.ContainsKey(c)) {
                    PooledBehaviour textbox = null;

                    // Only show when player controlled is asked to make a move
                    bool brainIsPlayer = (c.Brain is Player);

                    // Helps show user which character is doing stuff
                    if (brainIsPlayer) {
                        textbox = AddText(string.Format(PLAYER_QUESTION, c.Look.DisplayName));
                    }

                    // Wait until this is true until we ask the next character what action they want to perform
                    bool isActionTaken = false;

                    // Pass to the Brain a func that lets that lets them pass us what action they want to take
                    c.Brain.PreActionSetup(
                        temporarySpells,
                        (p) => InBattlePlayHandler(playerActionSet, plays, c, ref isActionTaken, p));

                    // Let brain decide now
                    c.Brain.DetermineAction();

                    // Wait until they choose a move. (This so the player can wait as long as they want)
                    yield return new WaitWhile(() => !isActionTaken);
                    ClearActionGrid();

                    if (textbox != null) {
                        ObjectPoolManager.Instance.Return(textbox); // Remove "What will X do?" textbox, reduce clutter
                    }

                    // Contains check again because they can do a charge move this turn
                    if (brainIsPlayer && !charactersChargingSpells.ContainsKey(c)) {
                        Spell playerMadePlay = plays[plays.Count - 1];
                        declarations.Add(AddText(
                            playerMadePlay.DeclareText
                            )); // "X will do Y" helper textbox
                    }
                }
            }
            // Remove all "X will do Y" texts
            foreach (PooledBehaviour pb in declarations) {
                ObjectPoolManager.Instance.Return(pb);
            }
        }

        private void InBattlePlayHandler(
            HashSet<Character> playerActionSet,
            IList<Spell> plays,
            Character characterAddingPlay,
            ref bool isActionTaken,
            Spell playToAdd) {
            if (!playerActionSet.Contains(characterAddingPlay)) {
                if (playToAdd.IsSpellCharged) {
                    plays.Add(playToAdd);
                } else {
                    charactersChargingSpells.Add(characterAddingPlay, playToAdd);
                }
                playerActionSet.Add(characterAddingPlay);
                isActionTaken = true;
            } else {
                Util.Assert(false, string.Format("{0}'s brain adds more than one Spell objects in its DetermineAction().", characterAddingPlay.Look.DisplayName));
            }
        }

        private void EndRound() {
            turnCount++;
        }

        private IButtonable GetLootButton(SubGrid previous) {
            ICollection<Character> graveyardToLoot = null;
            ICollection<Character> sideToLoot = null;
            if (VictoriousSide == Side.RIGHT) {
                graveyardToLoot = leftGraveyard;
                sideToLoot = Left;
            } else {
                graveyardToLoot = rightGraveyard;
                sideToLoot = Right;
            }
            AddItemsToLoot(loot, graveyardToLoot);
            AddItemsToLoot(loot, sideToLoot);
            return GetLootGrid(this, previous, loot, VictoriousParty);
        }

        private IEnumerator GoThroughBuffs(IList<Character> chars) {
            //End of round buff interactions
            foreach (Character myC in chars) {
                Character c = myC;
                Characters.Buffs buffs = c.Buffs;
                IList<Buff> timedOut = new List<Buff>();
                foreach (Buff myB in buffs) {
                    Buff b = myB;
                    if (myB.HasEndOfTurn) {
                        yield return new WaitForSeconds(0.02f);
                        AddText(new TextBox(
                            string.Format(BUFF_AFFECT, c.Look.DisplayName, b.Name),
                            new TooltipBundle(
                                b.Sprite,
                                b.Name,
                                b.Description
                                )
                            ));
                    }
                    b.OnEndOfTurn(c.Stats);
                    if (b.IsTimedOut) {
                        timedOut.Add(b);
                    }
                }
                foreach (Buff myB in timedOut) {
                    yield return new WaitForSeconds(0.02f);
                    Buff b = myB;
                    AddText(new TextBox(
                        string.Format(BUFF_FADE, c.Look.DisplayName, b.Name),
                        new TooltipBundle(
                            b.Sprite,
                            b.Name,
                            b.Description
                            )
                        ));
                    buffs.RemoveBuff(RemovalType.TIMED_OUT, b);
                }
            }
        }

        private IEnumerator UpdateChargingSpells(List<Spell> spellsToBeCast) {
            // Remove dead characters from charging dict
            foreach (Character battler in GetAll()) {
                if (battler.Stats.State == State.DEAD) {
                    charactersChargingSpells.Remove(battler);
                }
            }

            // Get all spells that are finished charging
            List<Spell> spellsThatFinishedCharging = new List<Spell>();
            foreach (KeyValuePair<Character, Spell> pair in charactersChargingSpells) {
                if (pair.Value.IsSpellCharged) {
                    spellsThatFinishedCharging.Add(pair.Value);
                } else {
                    yield return AddText(pair.Value.ChargingText);
                    pair.Value.DecrementTurnsToCharge();
                }
            }

            // Remove finished spells from the dict and Add finished spells to spells to be cast
            foreach (Spell spellThatFinishedCharging in spellsThatFinishedCharging) {
                charactersChargingSpells.Remove(spellThatFinishedCharging.Caster);
                spellsToBeCast.Add(spellThatFinishedCharging);
            }
        }

        private IEnumerator PerformActions(List<Spell> spellsToBeCast) {
            // Shuffle first, then do a stable sort to make speed ties random
            spellsToBeCast.Shuffle();
            spellsToBeCast = spellsToBeCast.OrderBy(p => p).ToList();
            for (int i = 0; i < spellsToBeCast.Count; i++) {
                Spell spell = spellsToBeCast[i];
                if (spell.Caster.Stats.State == State.ALIVE) {
                    yield return new WaitForSeconds(0.10f);
                    yield return spell.Play(this, true);
                }
            }
        }

        private void PostBattle(PlayerPartyStatus status) {
            SubGrid postBattle = new SubGrid("Main");
            postBattle.OnEnter = () => {
                postBattle.List.Clear();

                Page pageToContinueTo = null;

                if (status == PlayerPartyStatus.ALIVE || status == PlayerPartyStatus.NOT_FOUND) {
                    if (status == PlayerPartyStatus.ALIVE) {
                        postBattle.List.Add(GetLootButton(postBattle));
                        if (!wasExperienceGiven) {
                            wasExperienceGiven = true;
                            GiveExperienceToVictors();
                        }
                    }

                    postBattle.List.Add(PageUtil.GenerateGroupSpellBooks(this, postBattle, VictoriousParty));
                    postBattle.List.Add(PageUtil.GenerateGroupItemsGrid(this, postBattle, VictoriousParty, PageUtil.GetOutOfBattlePlayableHandler(this)));
                    postBattle.List.Add(PageUtil.GenerateGroupEquipmentGrid(postBattle, this, VictoriousParty, PageUtil.GetOutOfBattlePlayableHandler(this)));
                    pageToContinueTo = victory;
                } else {
                    pageToContinueTo = defeat;
                }
                postBattle.List.Add(
                    new Process(
                        "Continue",
                        Util.GetSprite("walking-boot"),
                        string.Format("Continue to {0}.",
                        pageToContinueTo.ButtonText), () => GoToPage(pageToContinueTo)));
                Actions = postBattle.List;
            };
            postBattle.Invoke();
        }

        /// <summary>
        /// Goes to page.
        /// </summary>
        /// <param name="page">The page.</param>
        private void GoToPage(Page page) {
            if (isUseTransition) {
                DoPageTransition(page);
            } else {
                page.Invoke();
            }
        }

        private void GiveExperienceToVictors() {
            foreach (Character victor in VictoriousParty) {
                int rawExperience = 0;
                foreach (Character defeated in AllDefeated) {
                    if (defeated.HasFlag(Characters.Flag.GIVES_EXPERIENCE)) {
                        rawExperience += CalculateExperience(victor.Stats, defeated.Stats);
                    }
                }
                int calculatedExperience = Mathf.Max(MINIMUM_EXP_PER_BATTLE, rawExperience);
                experienceGiven += calculatedExperience;
                victor.Stats.AddToStat(StatType.EXPERIENCE, Characters.Stats.Set.MOD_UNBOUND, Mathf.Max(MINIMUM_EXP_PER_BATTLE, calculatedExperience));
                this.AddText(string.Format(EXPERIENCE_GAIN, Util.ColorString(victor.Look.DisplayName, Color.yellow), Util.ColorString(calculatedExperience.ToString(), Color.yellow), StatType.EXPERIENCE.Name));
            }
        }

        /// <summary>
        /// Exp earned = Defeated.Level
        /// </summary>
        /// <param name="earner">Whoever is earning the experience</param>
        /// <param name="defeated">Defeated character to do calculation for</param>
        /// <returns>Calculated experience earner should recieve</returns>
        private int CalculateExperience(Characters.Stats earner, Characters.Stats defeated) {
            return defeated.Level;
        }

        private IEnumerator startBattle() {
            AddText(BATTLE_START);
            while (!IsResolved) {
                yield return startRound();
                yield return new WaitForSeconds(0.10f);
            }

            string message = string.Empty;
            PlayerPartyStatus result = PlayerStatus;
            if (result == PlayerPartyStatus.ALIVE) {
                message = VICTORY;
            } else if (result == PlayerPartyStatus.DEAD) {
                message = DEFEAT;
            } else { // Didn't find party
                message = "...";
            }

            victoriousSide = Left.Any(c => c.Stats.State == State.ALIVE)
                ? Side.LEFT
                : Side.RIGHT;

            AddText(message);
            PostBattle(result);
        }

        private IEnumerator startRound() {
            AddText(string.Format(Util.ColorString(ROUND_START, Color.grey), turnCount));
            List<Spell> plays = new List<Spell>();
            List<Character> allBattlers = GetAll();
            HashSet<Character> playerActionSet = new HashSet<Character>(new IdNumberEqualityComparer<Character>());

            yield return StartOfRound(allBattlers);
            yield return DetermineCharacterActions(allBattlers, plays, playerActionSet);
            yield return UpdateChargingSpells(plays);
            yield return PerformActions(plays);
            yield return GoThroughBuffs(allBattlers);
            yield return CheckForDeath();
            EndRound();
        }

        private IEnumerator StartOfRound(IEnumerable<Character> battlers) {
            foreach (Character c in battlers) {
                c.Brain.StartOfRoundSetup(this, c);
                yield return CharacterDialogue(this, c, c.Brain.StartOfRoundDialogue());
            }
        }

        public static IEnumerator CharacterDialogue(Page current, Character speaker, string content) {
            if (!string.IsNullOrEmpty(content)) {
                yield return ActUtil.SetupSceneRoutine(new Act[] { new TextAct(new AvatarBox(current.GetSide(speaker), speaker.Look.Sprite, speaker.Look.TextColor, content)) });
            }
            yield break;
        }
    }
}