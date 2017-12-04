using Script.View.Tooltip;
using Scripts.Game.Defined.Serialized.Brains;
using Scripts.Game.Defined.SFXs;
using Scripts.Model.Acts;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.TextBoxes;
using Scripts.View.ActionGrid;
using Scripts.View.ObjectPool;
using Scripts.View.Other;
using Scripts.View.Portraits;
using Scripts.View.Sounds;
using Scripts.View.TextBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Presenter {

    /// <summary>
    /// Connection between things on the screen
    /// and the page model.
    /// </summary>
    public class PagePresenter {
        private Page page;
        private SubGrid overrideGrid;

        private TextBoxHolderView textBoxHolder;
        private ActionGridView actionGrid;
        private PortraitHolderView left;
        private PortraitHolderView right;
        private HeaderView header;
        private SoundView sound;

        private Func<string> GetInputFunc;

        private IList<CharacterPresenter> characterPresenters;

        private InputBoxView inputBox;

        private IDictionary<Character, CharacterPresenter> presenters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagePresenter"/> class.
        /// </summary>
        /// <param name="initial">The initial.</param>
        /// <param name="textBoxHolder">The text box holder.</param>
        /// <param name="actionGrid">The action grid.</param>
        /// <param name="left">The left portraits.</param>
        /// <param name="right">The right portraits.</param>
        /// <param name="header">The header.</param>
        /// <param name="sound">The sound.</param>
        public PagePresenter(Page initial, TextBoxHolderView textBoxHolder, ActionGridView actionGrid, PortraitHolderView left, PortraitHolderView right, HeaderView header, SoundView sound) {
            this.textBoxHolder = textBoxHolder;
            this.actionGrid = actionGrid;
            this.left = left;
            this.right = right;
            this.header = header;
            this.sound = sound;
            this.characterPresenters = new List<CharacterPresenter>();
            InitializeFunctions();
            this.presenters = new Dictionary<Character, CharacterPresenter>(new IdNumberEqualityComparer<Character>());
            this.Page = initial;
        }

        /// <summary>
        /// Gets or sets the override grid.
        /// Used for making sure the user can't
        /// do anything during events while
        /// also ensuring that the buttons remain.
        /// </summary>
        /// <value>
        /// The override.
        /// </value>
        public SubGrid Override {
            set {
                actionGrid.ClearAll();
                this.overrideGrid = value;
            }
            get {
                return overrideGrid;
            }
        }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        /// <value>
        /// The page.
        /// </value>
        public Page Page {
            set {
                SetPage(value);
            }

            private get {
                return page;
            }
        }

        /// <summary>
        /// Initializes the functions.
        /// </summary>
        private void InitializeFunctions() {
            SubGrid.ChangeGridFunc = (a => {
                Page.Actions = a;
            }
            );
            Page.ChangePageFunc = (p => {
                Page = p;
            });
            Page.UpdateGridUI = p => {
                if (p == Page) {
                    actionGrid.ClearAll();
                    actionGrid.SetButtonAttributes(p.Actions);
                }
            };
            Page.TypeText = (t) => {
                return AddTextBox(t);
            };
        }

        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public void Tick() {
            page.Input = GetInputFunc.Invoke();
            Page.Tick();
            header.Location = Page.Location;
            if (overrideGrid == null) {
                actionGrid.SetButtonAttributes(page.Actions);
            } else {
                actionGrid.SetButtonAttributes(overrideGrid.List);
            }
            SetCharacterPresenters(Page.Left.ToArray(), left);
            SetCharacterPresenters(Page.Right.ToArray(), right);
        }

        /// <summary>
        /// Sets the page.
        /// </summary>
        /// <param name="page">The page.</param>
        private void SetPage(Page page) {
            presenters.Clear();

            // Music decision
            if (string.IsNullOrEmpty(page.Music)) {
                sound.StopAllSounds();
            } else if (string.Equals(page.Music, Page.Music)) {
                //Don't change music if both pages use same
            } else {
                sound.StopAllSounds();
                sound.LoopMusic(page.Music);
            }

            this.page = page;

            // Return all textboxes used
            textBoxHolder.ReturnChildren();

            // Stop any cutscenes
            Main.Instance.StopCoroutine("Timeline");

            if (!string.IsNullOrEmpty(page.Body)) {
                AddTextBox(new TextBox(page.Body));
            }

            actionGrid.IsHotKeysEnabled = !page.HasInputField;
            SetInputField(page.HasInputField);

            actionGrid.ClearAll();
            page.OnEnter.Invoke();

            Tick();
        }

        /// <summary>
        /// Sets the input field.
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        private void SetInputField(bool isEnabled) {
            if (isEnabled) {
                InputBoxView ibv = textBoxHolder.AddInputBox();
                ibv.Reset();
                ibv.SetFocus();
                GetInputFunc = () => ibv.Input;
            } else {
                GetInputFunc = () => string.Empty;
            }
        }

        /// <summary>
        /// Adds the text box.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public PooledBehaviour AddTextBox(TextBox t) {
            textBoxHolder.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
            return textBoxHolder.AddTextBox(t);
        }

        private void SetCharacterPresenters(IList<Character> characters, PortraitHolderView portraitHolder) {
            AddCharactersToPortraitHolder(characters, portraitHolder);
            foreach (Character c in characters) {
                CharacterPresenter presenter = new CharacterPresenter(c, portraitHolder.GetPortrait(c.Id));
                presenter.Tick();
                if (!presenters.ContainsKey(c)) {
                    presenters.Add(c, null);
                }
                presenters[c] = presenter;
            }
        }

        private void AddCharactersToPortraitHolder(IList<Character> characters, PortraitHolderView portraitHolder) {
            int revealContributionFromFlags = Convert.ToInt32(characters.Any(c => c.Brain is Player)); // Being in same party as player gives +1 to revealing
            portraitHolder.AddContents(
                characters
                .Select(
                    c => new PortraitHolderView.PortraitContent(
                        id: c.Id,
                        name: c.Look.DisplayName,
                        sprite: c.Look.Sprite,
                        tip: string.Format("Level {0} {1}\n<color=grey>{2}</color>",
                            c.Stats.Level,
                            c.Look.Breed.GetDescription(),
                            c.Look.Tooltip),
                        buffs: c.Buffs
                            .Select(
                            b => new BuffHolderView.BuffContent(
                                id: b.Id,
                                color: Color.white,
                                description: b.DetailedDescription,
                                duration: b.DurationText,
                                name: b.Name,
                                sprite: b.Sprite
                            )).ToArray(),
                        resources: c
                            .Stats
                            .Resources
                            .Select(
                            r => new ResourceHolderView.ResourceContent(
                                id: r.Type.GetHashCode(),
                                barText: r.Mod.ToString(),
                                numerator: r.Mod,
                                denominator: r.Max,
                                fillColor: r.Type.Color,
                                negativeColor: r.Type.NegativeColor,
                                sprite: r.Type.Sprite,
                                typeDescription: r.Type.Description,
                                title: r.Type.Name
                            )).ToList(),
                        isRevealed: IsRevealedCalculation(c.Stats, revealContributionFromFlags)
                    )
                ).ToArray()
            );
        }

        private bool IsRevealedCalculation(Stats stats, int contributionFromFlags) {
            return Util.IS_DEBUG || (stats.ResourceVisibility + contributionFromFlags) > 0;
        }
    }
}