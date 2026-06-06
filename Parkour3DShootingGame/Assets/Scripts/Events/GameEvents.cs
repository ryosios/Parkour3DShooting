using System;
using UniRx;
using UnityEngine;

namespace Parkour3DShooting.Events
{
    public readonly struct DamageEvent
    {
        public DamageEvent(GameObject target, int amount)
        {
            Target = target;
            Amount = amount;
        }

        public GameObject Target { get; }
        public int Amount { get; }
    }

    public readonly struct HealthChangedEvent
    {
        public HealthChangedEvent(GameObject target, int current, int max)
        {
            Target = target;
            Current = current;
            Max = max;
        }

        public GameObject Target { get; }
        public int Current { get; }
        public int Max { get; }
    }

    public readonly struct GrazeEvent
    {
        public GrazeEvent(Vector3 position, int score)
        {
            Position = position;
            Score = score;
        }

        public Vector3 Position { get; }
        public int Score { get; }
    }

    public readonly struct SpeedDownEvent
    {
        public SpeedDownEvent(GameObject target, float duration)
        {
            Target = target;
            Duration = duration;
        }

        public GameObject Target { get; }
        public float Duration { get; }
    }

    public static class GameEvents
    {
        private static readonly Subject<Unit> StageStartSubject = new Subject<Unit>();
        private static readonly Subject<Unit> BossAppearSubject = new Subject<Unit>();
        private static readonly Subject<Unit> BossDeadSubject = new Subject<Unit>();
        private static readonly Subject<Unit> StageClearSubject = new Subject<Unit>();
        private static readonly Subject<Unit> GameOverSubject = new Subject<Unit>();
        private static readonly Subject<DamageEvent> DamageSubject = new Subject<DamageEvent>();
        private static readonly Subject<HealthChangedEvent> HealthChangedSubject = new Subject<HealthChangedEvent>();
        private static readonly Subject<SpeedDownEvent> SpeedDownSubject = new Subject<SpeedDownEvent>();
        private static readonly Subject<GrazeEvent> GrazeSubject = new Subject<GrazeEvent>();

        public static IObservable<Unit> StageStart => StageStartSubject;
        public static IObservable<Unit> BossAppear => BossAppearSubject;
        public static IObservable<Unit> BossDead => BossDeadSubject;
        public static IObservable<Unit> StageClear => StageClearSubject;
        public static IObservable<Unit> GameOver => GameOverSubject;
        public static IObservable<DamageEvent> Damage => DamageSubject;
        public static IObservable<HealthChangedEvent> HealthChanged => HealthChangedSubject;
        public static IObservable<SpeedDownEvent> SpeedDown => SpeedDownSubject;
        public static IObservable<GrazeEvent> Graze => GrazeSubject;

        public static void RaiseStageStart() => StageStartSubject.OnNext(Unit.Default);
        public static void RaiseBossAppear() => BossAppearSubject.OnNext(Unit.Default);
        public static void RaiseBossDead() => BossDeadSubject.OnNext(Unit.Default);
        public static void RaiseStageClear() => StageClearSubject.OnNext(Unit.Default);
        public static void RaiseGameOver() => GameOverSubject.OnNext(Unit.Default);
        public static void RaiseDamage(GameObject target, int amount) => DamageSubject.OnNext(new DamageEvent(target, amount));
        public static void RaiseHealthChanged(GameObject target, int current, int max) => HealthChangedSubject.OnNext(new HealthChangedEvent(target, current, max));
        public static void RaiseSpeedDown(GameObject target, float duration) => SpeedDownSubject.OnNext(new SpeedDownEvent(target, duration));
        public static void RaiseGraze(Vector3 position, int score) => GrazeSubject.OnNext(new GrazeEvent(position, score));
    }
}
