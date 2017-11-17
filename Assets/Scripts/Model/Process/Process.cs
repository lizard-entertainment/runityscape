using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using System;
using UnityEngine;

namespace Scripts.Model.Processes {

    /// <summary>
    /// Processes are used to construct buttons.
    /// </summary>
    /// <seealso cref="Scripts.Model.Interfaces.IButtonable" />
    public class Process : IButtonable {
        private Action action;
        private Sprite sprite;
        private string description;
        private string name;
        private Func<bool> condition;
        private bool isVisibleOnDisable;
        private string disabledDescription;

        /// <summary>
        /// Descriptionless constructor
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <param name="condition">The condition.</param>
        public Process(string name,
               Action action,
               Func<bool> condition = null) : this(name, null, null, action, condition) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Process" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sprite">The sprite.</param>
        /// <param name="action">The action.</param>
        /// <param name="condition">The condition.</param>
        public Process(string name,
                        Sprite sprite,
                        Action action,
                        Func<bool> condition = null) : this(name, sprite, null, action, condition) { }

        /// <summary>
        /// Dummy Process for indicating something
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public Process(string name, string description) : this(name, description, () => { }) { }

        /// <summary>
        /// Dummy Process for indicating something
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sprite">The sprite.</param>
        /// <param name="description">The description.</param>
        public Process(string name, Sprite sprite, string description) : this(name, sprite, description, () => { }) { }

        /// <summary>
        /// Spriteless constructor
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="action">The action.</param>
        /// <param name="condition">The condition.</param>
        public Process(string name,
                       string description,
                       Action action,
                       Func<bool> condition = null) : this(name, null, description, action, condition) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Process" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sprite">The sprite.</param>
        /// <param name="description">The description.</param>
        /// <param name="action">The action.</param>
        /// <param name="condition">The condition.</param>
        public Process(string name,
                       Sprite sprite,
                       string description,
                       Action action,
                       Func<bool> condition) {
            this.name = name;
            this.sprite = sprite;
            this.description = description;
            this.action = action;
            this.condition = condition ?? (() => true);
            this.isVisibleOnDisable = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Process"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sprite">The sprite.</param>
        /// <param name="description">The description.</param>
        /// <param name="action">The action.</param>
        public Process(string name, Sprite sprite, string description, Action action) : this(name, sprite, description, action, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Process"/> class.
        /// </summary>
        public Process() {
            this.name = "";
            this.description = "";
            this.action = (() => { });
            this.condition = (() => { return false; });
            this.isVisibleOnDisable = true;
        }

        /// <summary>
        /// Gets the button text.
        /// </summary>
        /// <value>
        /// The button text.
        /// </value>
        public string ButtonText {
            get {
                return
                    Util.ColorString(name, IsInvokable ? Color.white : Color.red);
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
                return isInvokable();
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
                string message = string.Empty;
                if (!condition() && !string.IsNullOrEmpty(disabledDescription)) {
                    message = disabledDescription;
                } else {
                    message = description;
                }
                return message;
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
                return sprite;
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
                return isVisibleOnDisable;
            }
        }

        /// <summary>
        /// Invokes this instance.
        /// </summary>
        public virtual void Invoke() {
            if (IsInvokable) {
                action.Invoke();
            }
        }

        /// <summary>
        /// Sets the visible on disable.
        /// </summary>
        /// <param name="isVisibleOnDisable">if set to <c>true</c> [is visible on disable].</param>
        /// <returns></returns>
        public Process SetVisibleOnDisable(bool isVisibleOnDisable) {
            this.isVisibleOnDisable = isVisibleOnDisable;
            return this;
        }

        /// <summary>
        /// Sets the condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public Process SetCondition(Func<bool> condition) {
            Util.Assert(condition != null, "Condition cannot be null");
            this.condition = condition;
            return this;
        }

        public Process AddDisabledText(string message) {
            this.disabledDescription = message;
            return this;
        }

        /// <summary>
        /// Gets the back process.
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <returns></returns>
        public static Process GetBackProcess(Page previous) {
            return new Process(
                "Back",
                Util.GetSprite("plain-arrow"),
                "Return to the previous page.",
                () => previous.Invoke()
                );
        }

        /// <summary>
        /// Determines whether this instance is invokable.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is invokable; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool isInvokable() {
            return condition.Invoke();
        }
    }
}