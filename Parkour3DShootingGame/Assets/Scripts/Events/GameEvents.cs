using System;
using UniRx;
using UnityEngine;

namespace Parkour3DShooting.Events
{
    /// <summary>
    /// ダメージ発生時に通知する対象とダメージ量です。
    /// </summary>
    public readonly struct DamageEvent
    {
        /// <summary>
        /// ダメージイベントの値を初期化します。
        /// </summary>
        public DamageEvent(GameObject target, int amount)
        {
            Target = target;
            Amount = amount;
        }

        /// <summary>ダメージを受けた対象です。</summary>
        public GameObject Target { get; }
        /// <summary>発生したダメージ量です。</summary>
        public int Amount { get; }
    }

    /// <summary>
    /// HPが変化したときに通知する対象、現在HP、最大HPです。
    /// </summary>
    public readonly struct HealthChangedEvent
    {
        /// <summary>
        /// HP変更イベントの値を初期化します。
        /// </summary>
        public HealthChangedEvent(GameObject target, int current, int max)
        {
            Target = target;
            Current = current;
            Max = max;
        }

        /// <summary>HPが変化した対象です。</summary>
        public GameObject Target { get; }
        /// <summary>現在HPです。</summary>
        public int Current { get; }
        /// <summary>最大HPです。</summary>
        public int Max { get; }
    }

    /// <summary>
    /// グレイズ加点時に通知する発生位置とスコアです。
    /// </summary>
    public readonly struct GrazeEvent
    {
        /// <summary>
        /// グレイズイベントの値を初期化します。
        /// </summary>
        public GrazeEvent(Vector3 position, int score)
        {
            Position = position;
            Score = score;
        }

        /// <summary>グレイズが発生したワールド位置です。</summary>
        public Vector3 Position { get; }
        /// <summary>加算するスコアです。</summary>
        public int Score { get; }
    }

    /// <summary>
    /// 速度低下が発生したときに通知する対象と効果時間です。
    /// </summary>
    public readonly struct SpeedDownEvent
    {
        /// <summary>
        /// 速度低下イベントの値を初期化します。
        /// </summary>
        public SpeedDownEvent(GameObject target, float duration)
        {
            Target = target;
            Duration = duration;
        }

        /// <summary>速度低下の対象です。</summary>
        public GameObject Target { get; }
        /// <summary>速度低下の秒数です。</summary>
        public float Duration { get; }
    }

    /// <summary>
    /// ゲーム内イベントをUniRxのObservableとして集約するハブです。
    /// </summary>
    public static class GameEvents
    {
        /// <summary>ステージ開始通知のSubjectです。</summary>
        private static readonly Subject<Unit> StageStartSubject = new Subject<Unit>();
        /// <summary>ボス出現通知のSubjectです。</summary>
        private static readonly Subject<Unit> BossAppearSubject = new Subject<Unit>();
        /// <summary>ボス撃破通知のSubjectです。</summary>
        private static readonly Subject<Unit> BossDeadSubject = new Subject<Unit>();
        /// <summary>ステージクリア通知のSubjectです。</summary>
        private static readonly Subject<Unit> StageClearSubject = new Subject<Unit>();
        /// <summary>ゲームオーバー通知のSubjectです。</summary>
        private static readonly Subject<Unit> GameOverSubject = new Subject<Unit>();
        /// <summary>ダメージ通知のSubjectです。</summary>
        private static readonly Subject<DamageEvent> DamageSubject = new Subject<DamageEvent>();
        /// <summary>HP変更通知のSubjectです。</summary>
        private static readonly Subject<HealthChangedEvent> HealthChangedSubject = new Subject<HealthChangedEvent>();
        /// <summary>速度低下通知のSubjectです。</summary>
        private static readonly Subject<SpeedDownEvent> SpeedDownSubject = new Subject<SpeedDownEvent>();
        /// <summary>グレイズ加点通知のSubjectです。</summary>
        private static readonly Subject<GrazeEvent> GrazeSubject = new Subject<GrazeEvent>();

        /// <summary>ステージ開始を購読するObservableです。</summary>
        public static IObservable<Unit> StageStart => StageStartSubject;
        /// <summary>ボス出現を購読するObservableです。</summary>
        public static IObservable<Unit> BossAppear => BossAppearSubject;
        /// <summary>ボス撃破を購読するObservableです。</summary>
        public static IObservable<Unit> BossDead => BossDeadSubject;
        /// <summary>ステージクリアを購読するObservableです。</summary>
        public static IObservable<Unit> StageClear => StageClearSubject;
        /// <summary>ゲームオーバーを購読するObservableです。</summary>
        public static IObservable<Unit> GameOver => GameOverSubject;
        /// <summary>ダメージ発生を購読するObservableです。</summary>
        public static IObservable<DamageEvent> Damage => DamageSubject;
        /// <summary>HP変更を購読するObservableです。</summary>
        public static IObservable<HealthChangedEvent> HealthChanged => HealthChangedSubject;
        /// <summary>速度低下を購読するObservableです。</summary>
        public static IObservable<SpeedDownEvent> SpeedDown => SpeedDownSubject;
        /// <summary>グレイズ加点を購読するObservableです。</summary>
        public static IObservable<GrazeEvent> Graze => GrazeSubject;

        /// <summary>ステージ開始を通知します。</summary>
        public static void RaiseStageStart() => StageStartSubject.OnNext(Unit.Default);
        /// <summary>ボス出現を通知します。</summary>
        public static void RaiseBossAppear() => BossAppearSubject.OnNext(Unit.Default);
        /// <summary>ボス撃破を通知します。</summary>
        public static void RaiseBossDead() => BossDeadSubject.OnNext(Unit.Default);
        /// <summary>ステージクリアを通知します。</summary>
        public static void RaiseStageClear() => StageClearSubject.OnNext(Unit.Default);
        /// <summary>ゲームオーバーを通知します。</summary>
        public static void RaiseGameOver() => GameOverSubject.OnNext(Unit.Default);
        /// <summary>指定対象へのダメージ発生を通知します。</summary>
        public static void RaiseDamage(GameObject target, int amount) => DamageSubject.OnNext(new DamageEvent(target, amount));
        /// <summary>指定対象のHP変更を通知します。</summary>
        public static void RaiseHealthChanged(GameObject target, int current, int max) => HealthChangedSubject.OnNext(new HealthChangedEvent(target, current, max));
        /// <summary>指定対象への速度低下を通知します。</summary>
        public static void RaiseSpeedDown(GameObject target, float duration) => SpeedDownSubject.OnNext(new SpeedDownEvent(target, duration));
        /// <summary>グレイズ加点を通知します。</summary>
        public static void RaiseGraze(Vector3 position, int score) => GrazeSubject.OnNext(new GrazeEvent(position, score));
    }
}
