using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Processes;
using Scripts.Model.TextBoxes;
using Scripts.View.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Pages {

    /// <summary>
    /// A page is the basic "scene" of this game.
    /// </summary>
    /// <seealso cref="Scripts.Model.Interfaces.IButtonable" />
    public class Page : IButtonable, IPage {

        /// <summary>
        /// The change page function
        /// </summary>
        public static Action<Page> ChangePageFunc;

        /// <summary>
        /// The update grid UI function
        /// </summary>
        public static Action<Page> UpdateGridUI;

        /// <summary>
        /// The function that allows for typing text.
        /// </summary>
        public static Func<TextBox, PooledBehaviour> TypeText;

        /// <summary>
        /// The left side
        /// </summary>
        private readonly HashList<Character> left;

        /// <summary>
        /// The location
        /// </summary>
        public string Location;

        /// <summary>
        /// The music
        /// </summary>
        public string Music;

        /// <summary>
        /// The right side
        /// </summary>
        private readonly HashList<Character> right;

        /// <summary>
        /// The initial text on enter
        /// </summary>
        public string Body;

        /// <summary>
        /// The icon
        /// </summary>
        public Sprite Icon;

        /// <summary>
        /// The on enter
        /// </summary>
        public Action OnEnter;

        /// <summary>
        /// The condition
        /// </summary>
        public Func<bool> Condition;

        /// <summary>
        /// The has input field
        /// </summary>
        public bool HasInputField;

        /// <summary>
        /// The input
        /// </summary>
        public string Input;

        /// <summary>
        /// The tooltip
        /// </summary>
        private string tooltip;

        /// <summary>
        /// The grid
        /// </summary>
        private IButtonable[] grid;

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public Page(string location) {
            this.Location = location;
            this.grid = new IButtonable[SubGrid.DEFAULT_BUTTON_COUNT];
            this.left = new HashList<Character>(new IdNumberEqualityComparer<Character>());
            this.right = new HashList<Character>(new IdNumberEqualityComparer<Character>());
            this.OnEnter = () => { };
            this.Input = string.Empty;
            this.Condition = () => true;
            this.tooltip = string.Empty;
        }

        /// <summary>
        /// Gets or sets the actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public IList<IButtonable> Actions {
            set {
                IButtonable[] set = null;
                if (value.Count < SubGrid.DEFAULT_BUTTON_COUNT) {
                    set = new IButtonable[SubGrid.DEFAULT_BUTTON_COUNT];
                    Array.Copy(value.ToArray(), set, value.Count);
                } else {
                    set = value.ToArray();
                }
                this.grid = set;
                UpdateGridUI.Invoke(this);
            }

            get {
                return grid;
            }
        }

        /// <summary>
        /// Gets the left.
        /// </summary>
        /// <value>
        /// The left.
        /// </value>
        public ICollection<Character> Left {
            get {
                return left;
            }
        }

        /// <summary>
        /// Gets the right.
        /// </summary>
        /// <value>
        /// The right.
        /// </value>
        public ICollection<Character> Right {
            get {
                return right;
            }
        }

        /// <summary>
        /// Gets the button text.
        /// </summary>
        /// <value>
        /// The button text.
        /// </value>
        public string ButtonText {
            get {
                return Util.ColorString(Location, IsInvokable);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is invokable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is invokable; otherwise, <c>false</c>.
        /// </value>
        public bool IsInvokable {
            get {
                return Condition();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is visible on disable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is visible on disable; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisibleOnDisable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the tooltip text.
        /// </summary>
        /// <value>
        /// The tooltip text.
        /// </value>
        public string TooltipText {
            get {
                return tooltip;
            }
        }

        /// <summary>
        /// Gets the sprite.
        /// </summary>
        /// <value>
        /// The sprite.
        /// </value>
        public Sprite Sprite {
            get {
                return Icon;
            }
        }

        public virtual bool IsInBattle {
            get {
                return false;
            }
        }

        /// <summary>
        /// Sets the tooltip.
        /// </summary>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public Page SetTooltip(string tooltip) {
            this.tooltip = tooltip;
            return this;
        }

        /// <summary>
        /// Adds the characters.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <param name="chars">The chars.</param>
        public void AddCharacters(Side side, params Character[] chars) {
            ICollection<Character> mySet = (side == Side.LEFT) ? Left : Right;
            foreach (Character c in chars) {
                mySet.Add(c);
            }
        }

        /// <summary>
        /// Adds the characters.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <param name="chars">The chars.</param>
        public void AddCharacters(Side side, IEnumerable<Character> chars) {
            ICollection<Character> mySet = (side == Side.LEFT) ? Left : Right;
            foreach (Character c in chars) {
                mySet.Add(c);
            }
        }

        /// <summary>
        /// Adds the text.
        /// </summary>
        /// <param name="texts">The texts.</param>
        public void AddText(params string[] texts) {
            foreach (string s in texts) {
                AddText(s);
            }
        }

        /// <summary>
        /// Clears the action grid.
        /// </summary>
        public void ClearActionGrid() {
            Actions = new IButtonable[SubGrid.DEFAULT_BUTTON_COUNT];
        }

        /// <summary>
        /// Gets all characters.
        /// </summary>
        /// <param name="characterToExclude">Character to exclude from the list..</param>
        /// <returns>All characters in the page</returns>
        public List<Character> GetAll(Character characterToExclude = null) {
            List<Character> allChars = new List<Character>();
            allChars.AddRange(GetCharacters(Side.LEFT));
            allChars.AddRange(GetCharacters(Side.RIGHT));
            return characterToExclude == null ? allChars : allChars.Where(chara => chara != characterToExclude).ToList();
        }

        /// <summary>
        /// Shuffles the specified side.
        /// </summary>
        /// <param name="side">The side.</param>
        public void Shuffle(Side side) {
            if (side == Side.LEFT) {
                left.Shuffle();
            } else {
                right.Shuffle();
            }
        }

        /// <summary>
        /// Gets the allies.
        /// </summary>
        /// <param name="characterWithAllies">The c.</param>
        /// <returns></returns>
        public ICollection<Character> GetAllies(Character characterWithAllies) {
            return GetCharacters(GetSide(characterWithAllies));
        }

        /// <summary>
        /// Gets the foes.
        /// </summary>
        /// <param name="characterWithFoes">The character with foes.</param>
        /// <returns></returns>
        public ICollection<Character> GetFoes(Character characterWithFoes) {
            ICollection<Character> coll = null;
            if (GetSide(characterWithFoes) == Side.LEFT) {
                coll = GetCharacters(Side.RIGHT);
            } else {
                coll = GetCharacters(Side.LEFT);
            }
            return coll;
        }

        /// <summary>
        /// Gets the characters.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <returns></returns>
        public ICollection<Character> GetCharacters(Side side) {
            return side == Side.LEFT ? Left : Right;
        }

        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public void Tick() {
            IList<Character> all = GetAll();
            RepeatedCharacterCheck(all);
            UpdateCharacters(Left);
            UpdateCharacters(Right);
        }

        /// <summary>
        /// Updates the characters.
        /// </summary>
        /// <param name="characters">The characters.</param>
        private void UpdateCharacters(IEnumerable<Character> characters) {
            foreach (Character c in characters) {
                c.Update();
            }
        }

        /// <summary>
        /// Invokes this instance.
        /// </summary>
        public void Invoke() {
            ChangePageFunc.Invoke(this);
        }

        /// <summary>
        /// Gets the side.
        /// </summary>
        /// <param name="characterToGetSideOf">The character to get side of.</param>
        /// <returns></returns>
        public Side GetSide(Character characterToGetSideOf) {
            if (Left.Contains(characterToGetSideOf)) {
                return Side.LEFT;
            } else if (Right.Contains(characterToGetSideOf)) {
                return Side.RIGHT;
            } else {
                throw new UnityException(string.Format("Character {0} not found in either side.", characterToGetSideOf.Look.DisplayName));
            }
        }

        /// <summary>
        /// Adds the text.
        /// </summary>
        /// <param name="speaker">The c.</param>
        /// <param name="dialogue">The s.</param>
        /// <returns></returns>
        public PooledBehaviour AddText(Character speaker, string dialogue) {
            return TypeText(new AvatarBox(GetSide(speaker), speaker.Look.Sprite, speaker.Look.TextColor, dialogue));
        }

        /// <summary>
        /// Adds the text.
        /// </summary>
        /// <param name="text">The text to add.</param>
        /// <returns></returns>
        public PooledBehaviour AddText(string text) {
            return TypeText(new TextBox(text));
        }

        /// <summary>
        /// Adds the text.
        /// </summary>
        /// <param name="textbox">The textbox.</param>
        /// <returns></returns>
        public PooledBehaviour AddText(TextBox textbox) {
            return TypeText(textbox);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() {
            return 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            Page item = obj as Page;
            if (item == null) {
                return false;
            }
            return item.Location.Equals(this.Location);
        }

        /// <summary>
        /// Check if repeated characters exist on a side, if so, append a number as the suffix
        /// For example: Steve A, Steve B
        /// </summary>
        /// <param name="characters">Characters to check for repeats</param>
        private void RepeatedCharacterCheck(IList<Character> characters) {
            Dictionary<string, IList<Character>> repeatedCharacters = new Dictionary<string, IList<Character>>();
            foreach (Character c in characters) {
                if (!repeatedCharacters.ContainsKey(c.Look.Name)) {
                    List<Character> characterList = new List<Character>();
                    characterList.Add(c);
                    repeatedCharacters.Add(c.Look.Name, characterList);
                } else {
                    repeatedCharacters[c.Look.Name].Add(c);
                }
            }

            foreach (KeyValuePair<string, IList<Character>> cPair in repeatedCharacters) {
                if (cPair.Value.Count > 1) {
                    int index = 0;
                    foreach (Character c in cPair.Value) {
                        if (characters.Contains(c)) {
                            c.Look.Suffix = string.Format("{0}", index++);
                        }
                    }
                } else {
                    cPair.Value[0].Look.Suffix = "";
                }
            }
        }
    }
}