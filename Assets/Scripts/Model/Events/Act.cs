using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.TextBoxes;
using Scripts.Presenter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Scripts.Model.Spells;
using Scripts.Game.Defined.SFXs;
using Scripts.View.TextInput;

namespace Scripts.Model.Acts {

    /// <summary>
    /// Utility class to setup in game scenes, like two characters talking to each other.
    /// </summary>
    public static class ActUtil {

        /// <summary>
        /// Symbol representing that the text after it should be in a standard textbox.
        /// </summary>
        private const char TEXTBOX_SYMBOL = 't';

        /// <summary>
        /// Symbol representing that the text after it should be in an avatarbox
        /// </summary>
        private const char AVATARBOX_SYMBOL = 'a';

        /// <summary>
        /// Regex string that detects the tag and any text after it UNTIL the next tag.
        /// </summary>
        private static readonly string SYMBOL_TAG_DETECTION = string.Format("((<({0}|{1})>)(?:(?!(<({0}|{1})>)).)*)", TEXTBOX_SYMBOL, AVATARBOX_SYMBOL); //"((<.>)(?:(?!(<.>)).)*)";

        /// <summary>
        /// Position of the identifier character. The t in <t> is index 1.
        /// </summary>
        private const int IDENTIFIER_POSITION = 1;

        /// <summary>
        /// Length of a tag. <t> is 3 characters.
        /// </summary>
        private const int TAG_LENGTH = 3;

        /// <summary>
        /// Creates a scene when called.
        /// </summary>
        /// <param name="acts">Acts to perform in order.</param>
        public static void SetupScene(params Act[] acts) {
            SetupScene(acts, new Action[0]);
        }

        /// <summary>
        /// Overloaded variant.
        /// </summary>
        /// <param name="acts">Acts to perform in order.</param>
        /// <param name="postActions">Actions to call AFTER the scene is finished. Flag setting, etc.</param>
        public static void SetupScene(Act[] acts, params Action[] postActions) {
            Main.Instance.StartCoroutine(SetupSceneRoutine(acts, postActions));
        }

        /// <summary>
        /// Converts a string into dialogue and emotes.
        /// </summary>
        /// <param name="current">Page that the speaker is currently on.</param>
        /// <param name="speaker">Who is doing the emoting and dialogue</param>
        /// <param name="big">Large string filled with tags and text. <t>Hello world<a>Hi Bob"/></param>
        /// <param name="postActions">Actions to call AFTER the scene is finished</param>
        /// <returns></returns>
        public static Act[] LongTalk(Page current, Character speaker, string big, params Act[] postActions) {
            List<Act> events = new List<Act>();
            IList<TextBox> boxes = SplitBigDialogue(current, speaker, big);

            for (int i = 0; i < boxes.Count; i++) {
                events.Add(new TextAct(boxes[i]));
            }

            events.AddRange(postActions);

            return events.ToArray();
        }

        /// <summary>
        /// IEnumerator return variant.
        /// </summary>
        /// <param name="acts">Acts to perform in order.</param>
        /// <returns>A coroutine.</returns>
        public static IEnumerator SetupSceneRoutine(Act[] acts) {
            yield return SetupSceneRoutine(acts, new Action[0]);
        }

        /// <summary>
        /// IEnumerator return variant.
        /// </summary>
        /// <param name="acts">Acts to perform in order</param>
        /// <param name="postActions">Actions to do after the acts are done playing.</param>
        /// <returns></returns>
        public static IEnumerator SetupSceneRoutine(Act[] acts, Action[] postActions) {
            SubGrid grid = new SubGrid(string.Empty);
            Main.Instance.PagePresenter.Override = grid;

            bool isSkipAll = false;
            int lastIndex = 0;

            // Go through all acts
            for (int i = 0; i < acts.Length && !isSkipAll; i++) {
                lastIndex = i;
                bool isStepped = false;
                Act act = acts[i];

                // Only allow skip option if act is skippable
                if (act.IsSkippable) {
                    grid.List = new IButtonable[] {
                        new Process("Skip", string.Empty, () => act.SkipAct())
                    };
                }

                // Wait for act to finish
                yield return act.Play();

                // If not the last act, allow for stepping to the next one.
                grid.List = new IButtonable[] {
                        new Process("Step", string.Empty, () => isStepped = true),
                        new Process("Skip All", string.Empty, () => isSkipAll = true)
                    };
                yield return new WaitUntil(() => isStepped || isSkipAll || !act.IsSkippable);
            }

            // Skip all acts possible
            if (isSkipAll) {
                for (int i = lastIndex + 1; i < acts.Length; i++) {
                    Act act = acts[i];
                    act.SkipAct();
                    yield return act.Play();
                }
            }

            foreach (Action action in postActions) {
                action();
            }

            // If grid we're using is still in use, get rid of it. This is to handle nested Scenes.
            if (Main.Instance.PagePresenter.Override == grid) {
                Main.Instance.PagePresenter.Override = null;
            }
        }

        /// <summary>
        /// Use regex to split up a long string into an Emotes
        /// and Talks cutscene by a specific character
        /// </summary>
        /// <param name="current">Current page speaker is on.</param>
        /// <param name="speaker">Speaker of the big string</param>
        /// <param name="big">Large tagged string from which textboxes are generated from.</param>
        /// <returns>IList with textboxes whose contents are from the big string</returns>
        private static IList<TextBox> SplitBigDialogue(Page current, Character speaker, string big) {
            List<string> matches = new List<string>();
            foreach (Match m in Regex.Matches(big, SYMBOL_TAG_DETECTION)) {
                matches.Add(m.Value);
            }
            IList<TextBox> boxes = new List<TextBox>();

            foreach (string match in matches) {
                char identifier = match[IDENTIFIER_POSITION];
                string text = string.Format(match.Substring(TAG_LENGTH), speaker.Look.DisplayName);

                TextBox box = null;
                if (identifier.Equals(TEXTBOX_SYMBOL)) {
                    box = new TextBox(text);
                } else if (identifier.Equals(AVATARBOX_SYMBOL)) {
                    box = new AvatarBox(current.GetSide(speaker), speaker.Look.Sprite, speaker.Look.TextColor, text);
                } else {
                    throw new UnityException("Unknown tag identifier: " + identifier);
                }
                boxes.Add(box);
            }
            return boxes;
        }
    }

    /// <summary>
    /// Events that can happen in a scene.
    /// </summary>
    public abstract class Act {

        /// <summary>
        /// Determines whether the user can skip through the event.
        /// </summary>
        public readonly bool IsSkippable;

        /// <summary>
        /// True when the event has ended. Automatically set in play.
        /// </summary>
        private bool hasEnded;

        /// <summary>
        /// True if this event is skipped.
        /// </summary>
        private bool isSkip;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isSkippable">If true, this event can be skipped.</param>
        public Act(bool isSkippable = true) {
            this.IsSkippable = isSkippable;
        }

        /// <summary>
        /// Goes through the Act.
        /// </summary>
        /// <returns>A coroutine.</returns>
        public IEnumerator Play() {
            if (isSkip) {
                Skip();
            }
            Coroutine co = Main.Instance.StartCoroutine(PlayTemplate());
            bool wasSkipped = false;
            while (!hasEnded && !wasSkipped) {
                if (isSkip) {
                    wasSkipped = true;
                    Skip();
                }
                yield return null;
            }
        }

        /// <summary>
        /// Skips the act.
        /// </summary>
        public void SkipAct() {
            this.isSkip = true;
        }

        /// <summary>
        /// Particular events to perform when the act is skipped.
        /// </summary>
        protected abstract void Skip();

        /// <summary>
        /// Template method that performs the act.
        /// </summary>
        /// <returns>Coroutine</returns>
        protected abstract IEnumerator PlayHelper();

        /// <summary>
        /// Automatically sets hasEnded to be true when the act finishes playing.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator PlayTemplate() {
            yield return PlayHelper();
            hasEnded = true;
        }
    }

    /// <summary>
    /// Create a textbox.
    /// </summary>
    public class TextAct : Act {
        private TextBox box;

        /// <summary>
        /// Accepts all kinds of textboxes constructor.
        /// </summary>
        /// <param name="box">Textbox to use.</param>
        public TextAct(TextBox box) {
            this.box = box;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextAct"/> class.
        /// </summary>
        /// <param name="avatar">The avatar.</param>
        /// <param name="side">The side.</param>
        /// <param name="message">The message.</param>
        public TextAct(IAvatarable avatar, Side side, string message) : this(new AvatarBox(avatar, side, message)) { }

        /// <summary>
        /// Vanilla textbox constructor.
        /// </summary>
        /// <param name="message">Message to enclose in a textbox.</param>
        public TextAct(string message) : this(new TextBox(message)) {
        }

        /// <summary>
        /// Wait for the text to finish typing.
        /// </summary>
        /// <returns>Coroutine.</returns>
        protected override IEnumerator PlayHelper() {
            Page.TypeText(box);
            while (!box.IsDone) {
                yield return null;
            }
        }

        /// <summary>
        /// Skips the text in the box.
        /// </summary>
        protected override void Skip() {
            box.Skip();
        }
    }

    /// <summary>
    /// Change the current page.
    /// </summary>
    public class PageChangeAct : Act {
        private Page destination;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="destination">Which page to go to</param>
        public PageChangeAct(Page destination) : base(false) {
            this.destination = destination;
        }

        /// <summary>
        /// Change the page.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator PlayHelper() {
            destination.Invoke();
            yield break;
        }

        /// <summary>
        /// Skipping cannot affect this.
        /// </summary>
        protected override void Skip() {
        }
    }

    public class CoroutineAct : Act {
        private IEnumerator routine;

        public CoroutineAct(IEnumerator routine) : base(false) {
            this.routine = routine;
        }

        protected override IEnumerator PlayHelper() {
            yield return routine;
        }

        protected override void Skip() {
            this.routine = SFX.Wait(0);
        }
    }

    /// <summary>
    /// Calls a function.
    /// </summary>
    public class ActionAct : Act {
        private Action action;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">Action to play when it is this act.</param>
        public ActionAct(Action action) : base(false) {
            this.action = action;
        }

        /// <summary>
        /// Invoke and you're done.
        /// </summary>
        /// <returns>Coroutine.</returns>
        protected override IEnumerator PlayHelper() {
            action.Invoke();
            yield break;
        }

        /// <summary>
        /// Unskippable
        /// </summary>
        protected override void Skip() {
        }
    }

    /// <summary>
    /// Opens an input window for text to be typed in.
    /// If you want to use the input, you must use the callback function.
    /// Where the returned string will be the inputted value.
    /// </summary>
    public class InputAct : Act {

        /// <summary>
        /// Minimum number of characters allowed in input before allowing confirmation.
        /// </summary>
        private const int MINIMUM_CHAR_COUNT = 3;

        /// <summary>
        /// Called after the input is confirmed. The returned value is the user inputted value.
        /// </summary>
        private Action<string> callback;

        /// <summary>
        /// Text question that describes what the user should input.
        /// </summary>
        private string text;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text"><see cref="text"/></param>
        /// <param name="callback"><see cref="callback"/></param>
        public InputAct(string text, Action<string> callback) : base(false) {
            this.text = text;
            this.callback = callback;
        }

        /// <summary>
        /// Unskippable.
        /// </summary>
        protected override void Skip() {
        }

        /// <summary>
        /// Enable input box, wait for confirmation, then hide.
        /// </summary>
        /// <returns>Coroutine.</returns>
        protected override IEnumerator PlayHelper() {
            InputView input = Main.Instance.Input;

            input.gameObject.SetActive(true);
            input.Request = text;
            bool isInputConfirmed = false;
            SubGrid inputGrid = new SubGrid("input");
            SubGrid oldGrid = Main.Instance.PagePresenter.Override;

            Main.Instance.PagePresenter.Override = inputGrid;

            inputGrid.List.Add(new Process(
                "Confirm",
                () => isInputConfirmed = true,
                () => input.Input.Length >= MINIMUM_CHAR_COUNT
                ));
            yield return new WaitUntil(() => isInputConfirmed);
            input.gameObject.SetActive(false);
            Main.Instance.PagePresenter.Override = oldGrid;
            callback(input.Input);
        }
    }

    /// <summary>
    /// Fades in and out the boss transition window.
    /// </summary>
    public class BossTransitionAct : Act {

        /// <summary>
        /// What page to go to after the act finishes
        /// </summary>
        private Page destination;

        /// <summary>
        /// Sprite to use on the window
        /// </summary>
        private Sprite sprite;

        /// <summary>
        /// Text to put on the window
        /// </summary>
        private string text;

        /// <summary>
        ///
        /// </summary>
        /// <param name="destination"><see cref="destination"/></param>
        /// <param name="boss">Character look to use as text and icon.</param>
        public BossTransitionAct(Page destination, Characters.Look boss) : base(false) {
            this.destination = destination;
            this.sprite = boss.Sprite;
            this.text = boss.Name;
        }

        /// <summary>
        /// Wait for transition to end, then change page.
        /// </summary>
        /// <returns>Coroutine.</returns>
        protected override IEnumerator PlayHelper() {
            yield return null;
            destination.Invoke();
        }

        /// <summary>
        /// Instantly get rid of the window.
        /// </summary>
        protected override void Skip() {
            Main.Instance.Title.Cancel();
        }
    }
}