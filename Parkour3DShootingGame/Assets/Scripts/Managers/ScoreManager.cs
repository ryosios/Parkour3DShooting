using Parkour3DShooting.Events;
using UniRx;
using UnityEngine;

namespace Parkour3DShooting.Managers
{
    /// <summary>
    /// グレイズ、ボス撃破、ステージクリア時間からスコアを集計します。
    /// </summary>
    public sealed class ScoreManager : MonoBehaviour
    {
        /// <summary>ボス撃破時に加算する固定ボーナスです。</summary>
        [SerializeField] private int bossDefeatBonus = 3000;
        /// <summary>ステージクリア時に加算する固定ボーナスです。</summary>
        [SerializeField] private int stageClearBonus = 1000;
        /// <summary>タイムボーナス計算の基準点です。</summary>
        [SerializeField] private int timeBonusBase = 3000;

        /// <summary>現在スコアを通知するReactivePropertyです。</summary>
        private readonly ReactiveProperty<int> score = new ReactiveProperty<int>();
        /// <summary>ステージ開始時刻です。</summary>
        private float stageStartTime;

        public IReadOnlyReactiveProperty<int> Score => score;
        public int CurrentScore => score.Value;

        /// <summary>
        /// スコア関連イベントを購読し、ステージ開始時にスコアをリセットします。
        /// </summary>
        private void Awake()
        {
            GameEvents.StageStart.Subscribe(_ =>
            {
                stageStartTime = Time.time;
                score.Value = 0;
            }).AddTo(this);

            GameEvents.Graze.Subscribe(e => AddScore(e.Score)).AddTo(this);
            GameEvents.BossDead.Subscribe(_ => AddScore(bossDefeatBonus)).AddTo(this);
            GameEvents.StageClear.Subscribe(_ => AddScore(CalculateTimeBonus())).AddTo(this);
        }

        /// <summary>
        /// 正の値だけを現在スコアへ加算します。
        /// </summary>
        public void AddScore(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            score.Value += amount;
        }

        /// <summary>
        /// ステージ開始からの経過時間に応じたクリアボーナスを計算します。
        /// </summary>
        private int CalculateTimeBonus()
        {
            float elapsed = Mathf.Max(0f, Time.time - stageStartTime);
            return Mathf.Max(0, Mathf.RoundToInt(timeBonusBase - elapsed * 25f)) + stageClearBonus;
        }
    }
}
