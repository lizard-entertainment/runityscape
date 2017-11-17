using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Model.Spells {

    /// <summary>
    /// Whomst a Spell can target.
    /// </summary>
    public sealed class TargetType {
        private const string MULTI_TARGET_ICON = "minions";

        public readonly string Name;
        public readonly bool IsTargetEnemies;
        public readonly TargetCount TargetCount;
        private Func<Character, Page, ICollection<Character>> getFunc;

        private TargetType(string name, TargetCount targetCount, Func<Character, Page, ICollection<Character>> getFunc) {
            this.Name = name;
            this.TargetCount = targetCount;
            this.getFunc = getFunc;
        }

        public static TargetType SELF = new TargetType("Self", TargetCount.SINGLE_TARGET, (c, p) => new Character[] { c });
        public static TargetType ONE_ALLY = new TargetType("One ally", TargetCount.SINGLE_TARGET, (c, p) => p.GetAllies(c));

        public static TargetType ALL_ALLY = new TargetType("All allies", TargetCount.MULTIPLE_TARGETS, (c, p) => p.GetAllies(c).ToArray());
        public static TargetType ONE_FOE = new TargetType("One foe", TargetCount.SINGLE_TARGET, (c, p) => p.GetFoes(c));

        public static TargetType ALL_FOE = new TargetType("All foes", TargetCount.MULTIPLE_TARGETS, (c, p) => p.GetFoes(c).ToArray());
        public static TargetType ANY = new TargetType("Any", TargetCount.SINGLE_TARGET, (c, p) => p.GetAll());

        public static TargetType ALL = new TargetType("All", TargetCount.MULTIPLE_TARGETS, (c, p) => p.GetAll().ToArray());
        public static TargetType NONE = new TargetType("None", TargetCount.NONE, (c, p) => new Character[0]);

        public ICollection<Character> GetTargets(Character caster, Page current) {
            return getFunc.Invoke(caster, current);
        }

        public Process[] GetTargetProcesses(Page current, ISpellable spellable, Character caster, Action<Spell> spellHandler) {
            List<Process> processes = new List<Process>();
            SpellBook spellbook = spellable.GetSpellBook();
            if (this.TargetCount == TargetCount.SINGLE_TARGET) {
                foreach (Character target in GetTargets(caster, current)) {
                    processes.Add(GetTargetProcess(current, spellbook, caster, target, spellHandler));
                }
            } else if (this.TargetCount == TargetCount.MULTIPLE_TARGETS) {
                processes.Add(GetMultiTargetProcess(current, spellable, caster, spellHandler));
            }
            return processes.ToArray();
        }

        private Process GetMultiTargetProcess(Page current, ISpellable spellable, Character caster, Action<Spell> spellHandler) {
            SpellBook spellbook = spellable.GetSpellBook();
            return new Process(
                spellbook.TargetType.Name,
                Util.GetSprite(MULTI_TARGET_ICON),
                () => spellHandler(caster.Spells.CreateSpell(current, spellbook, caster)),
                () => spellbook.IsCastable(current, caster, spellbook.TargetType.GetTargets(caster, current))
                );
        }

        private Process GetTargetProcess(Page current, SpellBook spell, Character caster, Character target, Action<Spell> spellHandler) {
            Character[] targets = new Character[] { target };
            return new Process(
                target.Look.DisplayName,
                target.Look.Sprite,
                spell.CreateTargetDescription(target.Look.DisplayName),
                () => spellHandler(caster.Spells.CreateSpell(current, spell, caster, target)),
                () => spell.IsCastable(current, caster, targets)
                );
        }
    }
}

public enum TargetCount {
    MULTIPLE_TARGETS,
    SINGLE_TARGET,
    NONE
}