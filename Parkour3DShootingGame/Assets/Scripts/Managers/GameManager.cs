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
    /// <summary>
    /// ゲーム全体の状態遷移と開始、クリア、ゲームオーバー表示を管理します。
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        /// <summary>ステージ開始時に初期化するプレイヤーです。</summary>
        [SerializeField] private PlayerController player;
        /// <summary>ステージ開始時に初期化し、撃破を監視するボスです。</summary>
        [SerializeField] private BossController boss;
        /// <summary>クリアやゲームオーバーを表示するリザルトUIです。</summary>
        [SerializeField] private ResultView resultView;
        /// <summary>リザルト表示時に参照するスコア管理です。</summary>
        [SerializeField] private ScoreManager scoreManager;
        /// <summary>開始時に自動でステージを始めるかどうかです。</summary>
        [SerializeField] private bool autoStart = true;

        /// <summary>現在のゲーム状態を通知するReactivePropertyです。</summary>
        private readonly ReactiveProperty<GameState> state = new ReactiveProperty<GameState>(GameState.Title);

        public IReadOnlyReactiveProperty<GameState> State => state;

        /// <summary>
        /// ボス撃破とゲームオーバーイベントを購読します。
        /// </summary>
        private void Awake()
        {
            GameEvents.BossDead.Subscribe(_ => ClearStageAsync().Forget()).AddTo(this);
            GameEvents.GameOver.Subscribe(_ => OpenGameOver()).AddTo(this);
        }

        /// <summary>
        /// 自動開始が有効ならステージ開始処理を実行します。
        /// </summary>
        private void Start()
        {
            if (autoStart)
            {
                StartStageAsync().Forget();
            }
        }

        /// <summary>
        /// プレイヤーとボスを初期化し、ステージからボス戦へ遷移します。
        /// </summary>
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

        /// <summary>
        /// ステージクリア状態へ遷移し、少し待ってリザルトを表示します。
        /// </summary>
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

        /// <summary>
        /// ゲームオーバー状態へ遷移し、現在スコアを表示します。
        /// </summary>
        private void OpenGameOver()
        {
            if (state.Value == GameState.Result)
            {
                return;
            }

            state.Value = GameState.GameOver;
            resultView?.ShowGameOver(scoreManager != null ? scoreManager.CurrentScore : 0);
        }

        /// <summary>
        /// シーン生成時に管理対象の参照をまとめて設定します。
        /// </summary>
        public void Configure(PlayerController playerController, BossController bossController, ResultView view, ScoreManager manager)
        {
            player = playerController;
            boss = bossController;
            resultView = view;
            scoreManager = manager;
        }
    }
}
