using System;
using Scripts.Model.Interfaces;
using UnityEngine;
using System.Collections.Generic;

namespace Scripts.Model.Processes {

    /// <summary>
    /// A grid is an ActionGrid, independant of any page.
    /// </summary>
    /// <seealso cref="Scripts.Model.Interfaces.IButtonable" />
    public class SubGrid : IButtonable {
        public const int DEFAULT_BUTTON_COUNT = 16;

        public static Action<IList<IButtonable>> ChangeGridFunc {
            set {
                changeGridFunc = value;
            }
        }

        private static Action<IList<IButtonable>> changeGridFunc;

        /// <summary>
        /// The name on the grid button
        /// </summary>
        public string Name;

        /// <summary>
        /// The tooltip on the grid button
        /// </summary>
        public string Tooltip;

        /// <summary>
        /// The icon seen on the grid button
        /// </summary>
        public Sprite Icon;

        /// <summary>
        /// The condition for the grid to be usable.
        /// </summary>
        public Func<bool> Condition;

        /// <summary>
        /// Called on entering the grid
        /// </summary>
        public Action OnEnter;

        /// <summary>
        /// The list of buttons on the grid.
        /// </summary>
        public IList<IButtonable> List;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubGrid"/> class.
        /// </summary>
        /// <param name="name">The name of the grid.</param>
        public SubGrid(string name) {
            this.Name = name;
            this.Tooltip = "";
            this.Icon = null;
            this.Condition = () => true;
            OnEnter = () => { };
            this.List = new List<IButtonable>();
        }

        public string ButtonText {
            get {
                return Util.ColorString(Name, IsInvokable);
            }
        }

        public bool IsInvokable {
            get {
                return Condition.Invoke();
            }
        }

        public Sprite Sprite {
            get {
                return Icon;
            }
        }

        public string TooltipText {
            get {
                return Tooltip;
            }
        }

        public bool IsVisibleOnDisable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Sets the condition needed for the grid to be enterable.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public SubGrid SetCondition(Func<bool> condition) {
            this.Condition = condition;
            return this;
        }

        /// <summary>
        /// Changes the current grid to this one's buttons.
        /// </summary>
        public void Invoke() {
            this.OnEnter();
            changeGridFunc.Invoke(List);
        }
    }
}