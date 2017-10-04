using Scripts.Model.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scripts.Model.Interfaces;
using Scripts.Model.Characters;

namespace Scripts.Model.Items {

    /// <summary>
    /// Items that are usable.
    /// </summary>
    /// <seealso cref="Scripts.Model.Items.Item" />
    public abstract class UseableItem : Item {

        /// <summary>
        /// Initializes a new instance of the <see cref="UseableItem"/> class.
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        /// <param name="basePrice">The base price.</param>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public UseableItem(Sprite sprite, int basePrice, TargetType target, string name, string description)
            : base(sprite, basePrice, target, name, description) {
            flags.Add(Flag.OCCUPIES_SPACE);
        }
    }
}