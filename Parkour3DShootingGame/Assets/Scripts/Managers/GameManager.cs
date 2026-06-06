using Cysharp.Threading.Tasks;
using Parkour3DShooting.Boss;
using Parkour3DShooting.Core;
using Parkour3DShooting.Events;
using Parkour3DShooting.Player;
using Parkour3DShooting.UI;
using UniRx;
using UnityEngine;

namespace Parkour3DShooting.Managers
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private BossController boss;
        [SerializeField] private ResultView resultView;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private bool autoStart = true;

        private readonly ReactiveProperty<GameState> state = new ReactiveProperty<GameState>(GameState.Title);

        public IReadOnlyReactiveProperty<GameState> State => state;

        private void Awake()
        {
            GameEvents.BossDead.Subscribe(_ => ClearStageAsync().Forget()).AddTo(this);
            GameEvents.GameOver.Subscribe(_ => OpenGameOver()).AddTo(this);
        }

        private void Start()
        {
            if (autoStart)
            {
                StartStageAsync().Forget();
            }
        }

        public async UniTask StartStageAsync()
        {
            state.Value = GameState.Stage;
            resultView?.HideImmediate();
            player?.ResetPlayer();
            boss?.ResetBoss();

            GameEvents.RaiseStageStart();
            await UniTask.Delay(500, cancellationToken: this.GetCancellationTokenOnDestroy());

            state.Value = GameState.BossBattle;
            GameEvents.RaiseBossAppear();
        }

        private async UniTask ClearStageAsync()
        {
            if (state.Value == GameState.Result)
            {
                return;
            }

            state.Value = GameState.Result;
            GameEvents.RaiseStageClear();
            await UniTask.Delay(500, cancellationToken: this.GetCancellationTokenOnDestroy());
            resultView?.ShowClear(scoreManager != null ? scoreManager.CurrentScore : 0);
        }

        private void OpenGameOver()
        {
            if (state.Value == GameState.Result)
            {
                return;
            }

            state.Value = GameState.GameOver;
            resultView?.ShowGameOver(scoreManager != null ? scoreManager.CurrentScore : 0);
        }

        public void Configure(PlayerController playerController, BossController bossController, ResultView view, ScoreManager manager)
        {
            player = playerController;
            boss = bossController;
            resultView = view;
            scoreManager = manager;
        }
    }
}
