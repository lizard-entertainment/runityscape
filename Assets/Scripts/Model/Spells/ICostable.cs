using Scripts.Model.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Model.Interfaces {

    /// <summary>
    /// This class can be used as the cost to a spell!
    /// I use this to ensure that both items and stat-resources can be used as a cost!
    /// </summary>
    public interface ICostable {

        string GetName();

        bool CanAfford(int amount, Character characterToCheck);

        void DeductCostFromCharacter(int amount, Character unitToDeductFrom);
    }
}