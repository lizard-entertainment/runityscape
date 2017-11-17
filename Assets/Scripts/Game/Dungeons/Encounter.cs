using Scripts.Model.Acts;
using Scripts.Model.Characters;
using Scripts.Model.Pages;

namespace Scripts.Game.Dungeons {

    public struct Encounter {
        public readonly Character[] Enemies;
        public Music Music;

        /// <summary>
        /// Initializes a new instance of the <see cref="Encounter"/> struct.
        /// </summary>
        /// <param name="music">The music to play during this battle.</param>
        /// <param name="enemies">The enemies in this battle.</param>
        public Encounter(Music music, params Character[] enemies) {
            this.Enemies = enemies;
            this.Music = music;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Encounter"/> struct.
        /// Defaults to Music.NORMAL.
        /// </summary>
        /// <param name="enemies">The enemies in this battle.</param>
        public Encounter(params Character[] enemies) : this(Music.NORMAL, enemies) { }
    }
}