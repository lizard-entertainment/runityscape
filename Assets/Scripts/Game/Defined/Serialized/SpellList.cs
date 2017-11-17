using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Model.Stats;
using Scripts.View.Portraits;
using Scripts.Model.Spells;
using Scripts.Game.Defined.SFXs;
using Scripts.Model.Characters;
using Scripts.Game.Defined.Spells;
using Scripts.Model.Buffs;
using Scripts.Model.Pages;
using Scripts.View.Effects;
using Scripts.Game.Serialized.Brains;
using UnityEngine;
using Scripts.Presenter;
using Scripts.Game.Defined.Characters;
using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Unserialized.Spells;
using Scripts.Game.Defined.Unserialized.Buffs;
using System.Linq;

namespace Scripts.Game.Defined.Serialized.Spells {

    public class Attack : BasicSpellbook {
        public const float PERCENT = 0.01f;
        public const float HIT_VARIANCE = .25f;
        public const float HIT_STRENGTH_RATIO = 0.5f;
        public const float CRITICAL_STRENGTH_RATIO = 1f;
        public const float CRITICAL_VARIANCE = .125f;
        public const float BASE_ACCURACY = .90f;
        public const float BASE_CRITICAL_RATE = 0f;
        public const int SKILL_ON_HIT = 1;
        public const int SKILL_ON_CRIT = 2;

        public Attack() : base("Attack", Util.GetSprite("fist"), TargetType.ONE_FOE, SpellType.OFFENSE) {
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("A basic attack with the weapons or fists.");
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] { SFX.DoMeleeEffect(caster, target, 0.5f, "Slash_0") };
        }

        protected override bool IsHit(Character caster, Character target) {
            return Util.IsChance(BASE_ACCURACY + StatUtil.GetDifference(StatType.AGILITY, caster.Stats, target.Stats) * PERCENT);
        }

        protected override bool IsCritical(Character caster, Character target) {
            return Util.IsChance(BASE_CRITICAL_RATE + StatUtil.GetDifference(StatType.AGILITY, caster.Stats, target.Stats) * PERCENT);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            int totalCasterStrength = caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.STRENGTH);
            int damage = -Util.Random(Mathf.CeilToInt(totalCasterStrength * HIT_STRENGTH_RATIO), HIT_VARIANCE);

            return new SpellEffect[] {
                    new AddToModStat(target.Stats, StatType.HEALTH, damage),
                    new AddToModStat(caster.Stats, StatType.SKILL, 1)
                };
        }

        protected override IList<SpellEffect> GetCriticalEffects(Page page, Character caster, Character target) {
            int totalCasterStrength = caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.STRENGTH);
            int damage = -Util.Random(totalCasterStrength, CRITICAL_VARIANCE);
            return new SpellEffect[] {
                    new AddToModStat(target.Stats, StatType.HEALTH, damage),
                    new AddToModStat(caster.Stats, StatType.SKILL, 2)
                };
        }
    }

    public class Wait : BasicSpellbook {

        public Wait() : base("Wait", Util.GetSprite("hourglass"), TargetType.SELF, SpellType.MERCY) {
            this.flags.Remove(Model.Spells.Flag.CASTER_REQUIRES_SPELL);
        }

        protected override string CreateDescriptionHelper() {
            return "Literally do nothing.";
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[0];
        }
    }

    public class MagicMissile : BasicSpellbook {
        private const float INTELLECT_RATIO = 1f;

        public MagicMissile() : base("Magic Missile", Util.GetSprite("water-bolt"), TargetType.ONE_FOE, SpellType.OFFENSE) {
            AddCost(StatType.MANA, 10);
        }

        protected override string CreateDescriptionHelper() {
            return "Flings a magical projectile at a single foe.";
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, -(int)(caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.INTELLECT) * INTELLECT_RATIO))
            };
        }
    }

    public class MassCheck : BuffAdder<SuperCheck> {

        public MassCheck() : base(TargetType.ALL_FOE, SpellType.OFFENSE, "Clairvoyance", PriorityType.NORMAL) {
            this.isBuffUnique = true;
            AddCost(StatType.MANA, 10);
        }
    }

    public class SelfHeal : BasicSpellbook {
        private const int MISSING_HEALTH_HEAL_AMOUNT = 50;
        private const int SKILL_COST = 3;

        public SelfHeal() : base("Meditate", Util.GetSprite("prayer"), TargetType.SELF, SpellType.DEFENSE) {
            AddCost(StatType.SKILL, SKILL_COST);
            flags.Add(Model.Spells.Flag.USABLE_OUT_OF_COMBAT);
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("Restore {0}% of your missing {1}.", MISSING_HEALTH_HEAL_AMOUNT, StatType.HEALTH);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, (int)(target.Stats.GetMissingStatCount(StatType.HEALTH) * MISSING_HEALTH_HEAL_AMOUNT.ConvertToPercent()))
            };
        }
    }

    public class QuickAttack : BasicSpellbook {
        public const int INTELLECT_TO_DAMAGE = 1;

        public QuickAttack() : base("Lightning Strike", Util.GetSprite("power-lightning"), TargetType.ONE_FOE, SpellType.OFFENSE, PriorityType.HIGH) {
            AddCost(StatType.SKILL, 2);
        }

        protected override string CreateDescriptionHelper() {
            return "A faster-than-usual attack that can never miss.";
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] { SFX.DoMeleeEffect(caster, target, 0.2f, "Slash_0") };
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(
                    target.Stats,
                    StatType.HEALTH,
                    -caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.INTELLECT) * INTELLECT_TO_DAMAGE)
            };
        }
    }

    /// <summary>
    /// Can't use BuffAdder because this also adds check text to the textboxes
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.BasicSpellbook" />
    public class Check : BasicSpellbook {
        private static readonly BasicChecked DUMMY = new BasicChecked();

        public Check() : base("Check", Util.GetSprite("magnifying-glass"), TargetType.ANY, SpellType.BOOST) {
            AddCost(StatType.MANA, 1);
        }

        protected override string CreateDescriptionHelper() {
            return BuffAdder<BasicChecked>.CreateBuffDescription(this.TargetType, DUMMY);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new PostText(CheckText(target)),
                new DispelBuff<BasicChecked>(target.Buffs),
                new AddBuff(new BuffParams(caster.Stats, caster.Id), target.Buffs, new BasicChecked())
            };
        }

        protected override bool IsMeetCastRequirements(Character caster, Character target) {
            return !target.Buffs.HasBuff<BasicChecked>();
        }

        private static string CheckText(Character target) {
            return string.Format(
                "{0}\n{1}",
                target.Look.DisplayName,
                target.Stats.ShortAttributeDistribution
                );
        }
    }

    public class Purge : BasicSpellbook {

        public Purge() : base("Purge", Util.GetSprite("beams-aura"), TargetType.ANY, SpellType.DEFENSE) {
            AddCost(StatType.MANA, 10);
            flags.Add(Model.Spells.Flag.USABLE_OUT_OF_COMBAT);
        }

        protected override string CreateDescriptionHelper() {
            return "Target is dispelled of most buffs.";
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new DispelAllBuffs(target.Buffs)
            };
        }
    }

    public class InflictPoison : BuffAdder<Poison> {

        public InflictPoison() : base(TargetType.ONE_FOE, SpellType.OFFENSE, "Infect", PriorityType.NORMAL) {
            AddCost(StatType.MANA, 1);
            this.isBuffUnique = true;
        }
    }

    public class CrushingBlow : BasicSpellbook {
        private const float INTELLECT_TO_DAMAGE_RATIO = 3f;
        private const int SKILL_COST = 3;

        public CrushingBlow() : base("Crushing Blow", Util.GetSprite("fist"), TargetType.ONE_FOE, SpellType.OFFENSE, PriorityType.LOW) {
            AddCost(StatType.SKILL, SKILL_COST);
            this.TurnsToCharge = 1;
        }

        protected override string CreateDescriptionHelper() {
            return "A powerful blow that takes time to charge.";
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                    new AddToModStat(
                        target.Stats,
                        StatType.HEALTH,
                        (int)(
                        caster.Stats.GetStatCount(
                            Stats.Get.TOTAL,
                            StatType.INTELLECT) * -INTELLECT_TO_DAMAGE_RATIO))
                };
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] {
                SFX.DoMeleeEffect(caster, target, 0.5f, "Boom_6", true)
            };
        }
    }

    public class Revive : BasicSpellbook {
        public const int MANA_COST = 50;
        private const int REVIVAL_HEALTH_PERCENT = 20;

        public Revive() : base("Revive", Util.GetSprite("beams-aura"), TargetType.ONE_ALLY, SpellType.BOOST, PriorityType.LOW, true) {
            AddCost(StatType.MANA, MANA_COST);
            flags.Add(Model.Spells.Flag.USABLE_OUT_OF_COMBAT);
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("Restores {0}% of a target's missing {1}. Can be used on fallen allies.", REVIVAL_HEALTH_PERCENT, StatType.HEALTH.ColoredName);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new RestoreMissingStatPercent(target.Stats, StatType.HEALTH, REVIVAL_HEALTH_PERCENT)
            };
        }
    }

    public class Inspire : BasicSpellbook {
        private const int MISSING_MANA_RESTORATION_PERCENT = 50;
        private const int SKILL_COST = 1;

        public Inspire() : base("Inspire", Util.GetSprite("fire"), TargetType.ONE_ALLY, SpellType.BOOST) {
            AddCost(StatType.SKILL, SKILL_COST);
            flags.Add(Model.Spells.Flag.USABLE_OUT_OF_COMBAT);
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("Inspire an ally, restoring {0}% of their missing {1}.", MISSING_MANA_RESTORATION_PERCENT, StatType.MANA.ColoredName);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new RestoreMissingStatPercent(target.Stats, StatType.MANA, MISSING_MANA_RESTORATION_PERCENT)
            };
        }

        protected override bool IsMeetCastRequirements(Character caster, Character target) {
            return target.Stats.HasStat(StatType.MANA);
        }
    }

    public class PlayerHeal : BasicSpellbook {
        private const int INTELLECT_TO_HEALTH = 1;
        private const int CRITICAL_INTELLECT_TO_HEALTH = 2;
        private const int CRITICAL_HEALTH_PERCENT = 10;

        public PlayerHeal() : base("Heal", Util.GetSprite("health-normal"), TargetType.ONE_ALLY, SpellType.BOOST) {
            AddCost(StatType.MANA, 20);
            flags.Add(Model.Spells.Flag.USABLE_OUT_OF_COMBAT);
        }

        protected override string CreateDescriptionHelper() {
            return string.Format(
                "Restore {0} to a single ally. Has an increased effect on targets below {1}% {0}.",
                StatType.HEALTH.ColoredName,
                CRITICAL_HEALTH_PERCENT);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.INTELLECT) * INTELLECT_TO_HEALTH)
            };
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] { SFX.PlaySound("healspell1") };
        }

        protected override bool IsCritical(Character caster, Character target) {
            float currentHealth = target.Stats.GetStatCount(Stats.Get.MOD, StatType.HEALTH);
            float maxHealth = target.Stats.GetStatCount(Stats.Get.MAX, StatType.HEALTH);

            return (currentHealth / maxHealth) <= (CRITICAL_HEALTH_PERCENT / 100f);
        }

        protected override IList<SpellEffect> GetCriticalEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.INTELLECT) * CRITICAL_INTELLECT_TO_HEALTH)
            };
        }
    }

    public class SetupDefend : BuffAdder<Defend> {

        public SetupDefend() : base(TargetType.SELF, SpellType.DEFENSE, "Defend", PriorityType.HIGHEST) {
            this.isBuffUnique = true;
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] { SFX.PlaySound("Ping_0") };
        }
    }

    public class Multistrike : BasicSpellbook {

        public Multistrike() : base("Multistrike", Util.GetSprite("sword-array"), TargetType.ALL_FOE, SpellType.OFFENSE, PriorityType.LOW) {
            AddCost(StatType.SKILL, 3);
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("A delayed strike that hits all enemies.");
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            int damage = -caster.Stats.GetStatCount(Stats.Get.MOD, StatType.INTELLECT);
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, damage),
            };
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] {
                SFX.DoMeleeEffect(caster, target, 0.2f, "Slash_0")
            };
        }
    }

    // DEBUG SPELL
    public class HalfLife : BasicSpellbook {

        public HalfLife() : base("HalfLife", Util.GetSprite("gladius"), TargetType.ONE_FOE, SpellType.OFFENSE) {
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("DEBUG: Enemy takes half of current health in damage + 1.");
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, -target.Stats.GetStatCount(Stats.Get.MOD, StatType.HEALTH) - 1)
            };
        }
    }

    // DEBUG SPELL
    public class InstaKill : BasicSpellbook {

        public InstaKill() : base("InstaKill", Util.GetSprite("fist"), TargetType.ONE_FOE, SpellType.OFFENSE) {
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("Kill.");
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, -1337)
            };
        }
    }
}

/// <summary>
/// Spells that only enemies can have don't need to be saved.
/// </summary>
namespace Scripts.Game.Defined.Unserialized.Spells {

    public class SelfDestruct : BasicSpellbook {

        public SelfDestruct() : base("Self Destruct", Util.GetSprite("skull-crack"), TargetType.SELF, SpellType.OFFENSE) {
        }

        protected override string CreateDescriptionHelper() {
            return "End your existence.";
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, -target.Stats.GetStatCount(Stats.Get.TOTAL, StatType.HEALTH))
            };
        }
    }

    public class SetupCounter : BuffAdder<Counter> {

        public SetupCounter() : base(TargetType.SELF, SpellType.DEFENSE, "Counter", PriorityType.LOW) {
            AddCost(StatType.SKILL, 2);
            this.isBuffUnique = true;
        }
    }

    public class UnholyRevival : BasicSpellbook {
        private const int REVIVED_HEALTH_AMOUNT = 50;

        public UnholyRevival() : base("Determination", Util.GetSprite("skull-crack"), TargetType.ONE_ALLY, SpellType.BOOST, PriorityType.LOWEST, true) {
            this.TurnsToCharge = 2;
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("Revive a fallen ally to {0}% {1}.", REVIVED_HEALTH_AMOUNT, StatType.HEALTH);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, (int) (target.Stats.GetStatCount(Stats.Get.MAX, StatType.HEALTH) * REVIVED_HEALTH_AMOUNT.ConvertToPercent()))
            };
        }

        protected override bool IsMeetCastRequirements(Character caster, Character target) {
            return target.Stats.State == State.DEAD; // Prevent targeting if alive
        }
    }

    public class EnemyHeal : BasicSpellbook {
        private const int BASE_HEALING_AMOUNT = 10;

        public EnemyHeal() : base("Heal", Util.GetSprite("health-normal"), TargetType.ONE_ALLY, SpellType.BOOST, PriorityType.HIGH) {
        }

        protected override string CreateDescriptionHelper() {
            return "Restore a little health.";
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, BASE_HEALING_AMOUNT + caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.INTELLECT))
            };
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] { SFX.PlaySound("healspell1")
                };
        }
    }

    public class RevealTrueForm : BasicSpellbook {

        public RevealTrueForm() : base("True Form", Util.GetSprite("paranoia"), TargetType.SELF, SpellType.BOOST) {
        }

        protected override string CreateDescriptionHelper() {
            return "Reveal your true form.";
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] {
                    SFX.DoSteamEffect(caster),
                    SFX.PlaySound("Creepy"),
                    SFX.PlaySound("i_see_you_voice"),
                    SFX.LoopMusic("enchanted tiki 86")
                };
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new ChangeLookEffect(caster, RuinsNPCs.ReplicantLook()),
            };
        }
    }

    public class ReflectiveClone : BasicSpellbook {
        private const int NUMBER_OF_CLONES = 4;

        public ReflectiveClone() : base("Hallucination", Util.GetSprite("fox-head"), TargetType.SELF, SpellType.BOOST, PriorityType.LOWEST) {
        }

        protected override string CreateDescriptionHelper() {
            return "Creates clones of the caster. Clones do no damage, and die when the caster is attacked. Clones reflect all damage taken.";
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] {
                SFX.Wait(HitsplatView.TOTAL_DURATION),
                SFX.PlaySound("synthetic_explosion_1")
            };
        }

        protected override IList<SpellEffect> GetHitEffects(Page current, Character caster, Character target) {
            Func<Character> cloneFunc =
                () => {
                    Character c = new Character(
                        new Stats(caster.Stats.Level, 0, caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.AGILITY), 1, 1),
                        new Look(caster.Look.Name, caster.Look.SpriteLoc, caster.Look.Tooltip, caster.Look.Breed, caster.Look.TextColor),
                        new ReplicantClone());
                    c.AddFlag(Model.Characters.Flag.IS_CLONE);
                    c.Buffs.AddBuff(new ReflectAttack(), c);
                    c.Buffs.AddBuff(new SpiritLink(), caster);
                    return c;
                };

            return new SpellEffect[] {
                new CloneEffect(NUMBER_OF_CLONES, current.GetSide(caster), cloneFunc, current),
                new ShuffleEffect(current, current.GetSide(caster))
            };
        }
    }

    public class Blackout : BuffAdder<BlackedOut> {

        public Blackout() : base(TargetType.ONE_FOE, SpellType.OFFENSE, "Blackout", PriorityType.HIGH) {
            this.isBuffUnique = true;
        }
    }

    public class Ignite : BuffAdder<TempIgnited> {

        public Ignite() : base(TargetType.ONE_FOE, SpellType.OFFENSE, "Ignite", PriorityType.NORMAL) {
            AddCost(StatType.MANA, 10);
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] {
                SFX.PlaySound("synthetic_explosion_1"),
                SFX.DoSteamEffect(target, Color.red)
            };
        }
    }

    public class Inferno : BasicSpellbook {
        private static readonly Buff DUMMY = new PermanantIgnited();

        public Inferno() : base("Inferno", Util.GetSprite("fire"), TargetType.ONE_FOE, SpellType.OFFENSE) {
            AddCost(StatType.MANA, 10);
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("Blaze a foe, dealing additional damage for each stack of {0} on them.\n{1}",
                AbstractIgnited.NAME,
                BuffAdder<PermanantIgnited>.CreateBuffDescription(TargetType.ONE_FOE, DUMMY));
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] {
                SFX.PlaySound("synthetic_explosion_1"),
                SFX.DoSteamEffect(target, Color.magenta)
            };
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, -target.Buffs.GetBuffCount<AbstractIgnited>()),
                new AddBuff(new BuffParams(caster.Stats, caster.Id), target.Buffs, new PermanantIgnited())
            };
        }
    }

    public class SingStrengthSong : SingSirenSong<StrengthSirenSong> {

        public SingStrengthSong() : base() {
        }
    }

    public class SingAgilitySong : SingSirenSong<AgilitySirenSong> {

        public SingAgilitySong() : base() {
        }
    }

    public class SingIntellectSong : SingSirenSong<IntellectSirenSong> {

        public SingIntellectSong() : base() {
        }
    }

    public class SingVitalitySong : SingSirenSong<VitalitySirenSong> {

        public SingVitalitySong() : base() {
        }
    }

    public abstract class AbstractSpawnTentacles : BasicSpellbook {
        private readonly Func<Character> tentacleSpawnFunction;

        public AbstractSpawnTentacles(string spellName, Func<Character> tentacleToSpawn)
            : base(spellName,
                  Util.GetSprite("at-sea"),
                  TargetType.SELF,
                  SpellType.BOOST,
                  PriorityType.LOWEST) {
            this.tentacleSpawnFunction = tentacleToSpawn;
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("Spawns Tentacles to defend the caster. Number increases with missing health.");
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            float targetHealthPercentage = ((float)target.Stats.GetStatCount(Stats.Get.MOD, StatType.HEALTH)) / target.Stats.GetStatCount(Stats.Get.MAX, StatType.HEALTH);
            int tentaclesToSummon = 0;

            // TODO fix this terrible code, use a dictionary please
            if (targetHealthPercentage > .66) {
                tentaclesToSummon = 2;
            } else if (targetHealthPercentage > .33) {
                tentaclesToSummon = 3;
            } else {
                tentaclesToSummon = 4;
            }

            Func<Character> summonTentacleFunc = () => {
                Character tentacle = OceanNPCs.Tentacle();
                Interceptor interceptor = new Interceptor();
                interceptor.Caster = new BuffParams(target.Stats, target.Id);
                tentacle.Buffs.AddBuff(interceptor);
                return tentacle;
            };
            return new SpellEffect[] {
                new SummonEffect(page.GetSide(target), page, summonTentacleFunc, tentaclesToSummon)
            };
        }
    }

    public class SpawnTentacles : AbstractSpawnTentacles {

        public SpawnTentacles() : base("Tentacle Sprout", () => OceanNPCs.Tentacle()) {
        }
    }

    public class SpawnLashers : AbstractSpawnTentacles {

        public SpawnLashers() : base("Lasher Eruption", () => LabNPCs.Ocean.Tentacle()) {
        }
    }

    public abstract class SingSirenSong<T> : BuffAdder<T> where T : Buff {

        public SingSirenSong() : base(TargetType.ALL_FOE, SpellType.OFFENSE, Util.GetSprite("sonic-shout")) {
        }
    }

    public class Flee : BasicSpellbook {
        private Page destination;
        private Action stopBattle;

        public Flee(Page destination, Action stopBattle) : base("Flee", Util.GetSprite("walking-boot"), TargetType.SELF, SpellType.DEFENSE) {
            this.destination = destination;
            this.stopBattle = stopBattle;
            this.flags.Remove(Model.Spells.Flag.CASTER_REQUIRES_SPELL);
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("Escape back to {0}.", destination.Location);
        }

        protected override IList<SpellEffect> GetHitEffects(Page current, Character caster, Character target) {
            return new SpellEffect[] { new GoToPage(destination, stopBattle) };
        }
    }

    public class WaterboltSingle : WaterboltAbstract {

        public WaterboltSingle() : base(TargetType.ONE_FOE, "Scald") {
        }
    }

    public class WaterboltMulti : WaterboltAbstract {
        private const int MANA_COST = 25;

        public WaterboltMulti() : base(TargetType.ALL_FOE, "Multi Scald") {
            AddCost(StatType.MANA, MANA_COST);
        }
    }

    public abstract class WaterboltAbstract : BasicSpellbook {
        private static readonly Buff IGNITE_BUFF = new TempIgnited();

        private readonly string description;

        public WaterboltAbstract(TargetType targetType, string name) : base(name, Util.GetSprite("water-bolt"), targetType, SpellType.OFFENSE) {
            this.TurnsToCharge = 3;
            this.description = "A blast of boiling water. If target is not Guarding, inflicts <color=cyan<{0}</color>\n{1}";
        }

        protected override string CreateDescriptionHelper() {
            return string.Format(
                description,
                IGNITE_BUFF.Name,
                IGNITE_BUFF.Description);
        }

        protected override bool IsCritical(Character caster, Character target) {
            return !target.Buffs.HasBuff<Defend>();
        }

        protected override IList<SpellEffect> GetCriticalEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                DamageEffect(caster, target),
                new AddBuff(new BuffParams(caster.Stats, caster.Id), target.Buffs, new TempIgnited())
            };
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                DamageEffect(caster, target)
            };
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] {
                SFX.DoSteamEffect(target, Color.cyan)
            };
        }

        private SpellEffect DamageEffect(Character caster, Character target) {
            return new AddToModStat(target.Stats, StatType.HEALTH, -caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.INTELLECT));
        }
    }

    public class CastDelayedDeath : BuffAdder<DelayedDeath> {

        public CastDelayedDeath() : base(TargetType.ONE_FOE, SpellType.OFFENSE, PriorityType.NORMAL) {
            this.isBuffUnique = true;
        }
    }

    public class CastDelayedEternalDeath : BuffAdder<DelayedHyperDeath> {

        public CastDelayedEternalDeath() : base(TargetType.ONE_FOE, SpellType.OFFENSE, PriorityType.NORMAL) {
            this.isBuffUnique = true;
        }
    }

    public class SummonSeaCreatures : BasicSpellbook {
        private int NUMBER_OF_SUMMON_PICKS = 2;

        private static readonly IDictionary<Func<Character>, int> POSSIBLE_SUMMONS = new Dictionary<Func<Character>, int> {
            { () => OceanNPCs.Shuck(), 1 },
            { () => OceanNPCs.Elemental(), 1 },
            { () => OceanNPCs.Shark(), 2 },
            { () => OceanNPCs.Siren(), 1 },
            { () => OceanNPCs.Swarm(), 3 }
        };

        public SummonSeaCreatures() : base("Ocean's Assembly", Util.GetSprite("angler-fish"), TargetType.SELF, SpellType.BOOST) {
        }

        protected override string CreateDescriptionHelper() {
            return "Random creatures of the sea come to the caster's aid.";
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return POSSIBLE_SUMMONS
                .RandomValues()
                .Take(NUMBER_OF_SUMMON_PICKS)
                .Select(pick => new SummonEffect(page.GetSide(target), page, pick, POSSIBLE_SUMMONS[pick]))
                .ToArray();
        }
    }

    public class GrantImmunity : BuffAdder<Immunity> {

        public GrantImmunity() : base(TargetType.ONE_ALLY, SpellType.DEFENSE, PriorityType.NORMAL) {
        }
    }

    public class OneShotKill : BasicSpellbook {
        private const int HIGH_DAMAGE = 1337;

        public OneShotKill() : base("Death Strike", Util.GetSprite("skull-crack"), TargetType.ONE_FOE, SpellType.OFFENSE) {
            this.TurnsToCharge = 1;
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("A killing blow that misses if the target has {0} up.", Defend.DEFEND_NAME);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, -HIGH_DAMAGE)
            };
        }

        protected override bool IsHit(Character caster, Character target) {
            return target.Buffs.HasBuff<Defend>();
        }
    }

    public class GiveEvasion : BuffAdder<Evasion> {

        public GiveEvasion() : base(TargetType.SELF, SpellType.BOOST, PriorityType.LOW) {
        }
    }

    public class NullifyHealing : BuffAdder<AntiHeal> {

        public NullifyHealing() : base(TargetType.ONE_FOE, SpellType.OFFENSE, PriorityType.NORMAL) {
        }
    }

    public class SuperBurn : BuffAdder<SuperIgnited> {

        public SuperBurn() : base(TargetType.ONE_FOE, SpellType.OFFENSE, PriorityType.NORMAL) {
        }
    }
}