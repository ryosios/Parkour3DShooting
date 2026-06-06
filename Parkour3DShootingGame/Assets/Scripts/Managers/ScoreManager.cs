using Parkour3DShooting.Events;
using UniRx;
using UnityEngine;

namespace Parkour3DShooting.Managers
{
    public sealed class ScoreManager : MonoBehaviour
    {
        [SerializeField] private int bossDefeatBonus = 3000;
        [SerializeField] private int stageClearBonus = 1000;
        [SerializeField] private int timeBonusBase = 3000;

        private readonly ReactiveProperty<int> score = new ReactiveProperty<int>();
        private float stageStartTime;

        public IReadOnlyReactiveProperty<int> Score => score;
        public int CurrentScore => score.Value;

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

        public void AddScore(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            score.Value += amount;
        }

        private int CalculateTimeBonus()
        {
            float elapsed = Mathf.Max(0f, Time.time - stageStartTime);
            return Mathf.Max(0, Mathf.RoundToInt(timeBonusBase - elapsed * 25f)) + stageClearBonus;
        }
    }
}
