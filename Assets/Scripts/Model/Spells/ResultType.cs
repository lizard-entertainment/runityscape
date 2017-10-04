using System.Collections.Generic;

namespace Scripts.Model.Spells {

    /// <summary>
    /// Various types of results associated with a spell
    /// </summary>
    public sealed class ResultType {

        /// <summary>
        /// The text
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultType"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        private ResultType(string text) {
            this.Text = text;
        }

        /// <summary>
        /// The hit
        /// </summary>
        public static readonly ResultType HIT = new ResultType("");

        /// <summary>
        /// The critical
        /// </summary>
        public static readonly ResultType CRITICAL = new ResultType("\nIt's a critical!");

        /// <summary>
        /// The miss
        /// </summary>
        public static readonly ResultType MISS = new ResultType("\nBut it misses!");

        /// <summary>
        /// The failed
        /// </summary>
        public static readonly ResultType FAILED = new ResultType("\nBut it fails!");

        /// <summary>
        /// The successful results
        /// </summary>
        public static readonly HashSet<ResultType> SUCCESSFUL_RESULTS = new HashSet<ResultType>() { HIT, CRITICAL };

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            return obj == this;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() {
            return Text.GetHashCode();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is successful type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is successful type; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccessfulType {
            get {
                return SUCCESSFUL_RESULTS.Contains(this);
            }
        }
    }
}