using Parkour3DShooting.Managers;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Parkour3DShooting.UI
{
    /// <summary>
    /// ScoreManagerのスコア変更を受け取り、画面上のスコア表示を更新します。
    /// </summary>
    public sealed class ScoreView : MonoBehaviour
    {
        /// <summary>購読対象のスコア管理です。</summary>
        [SerializeField] private ScoreManager scoreManager;
        /// <summary>スコア文字列を表示するTextです。</summary>
        [SerializeField] private Text scoreText;

        /// <summary>
        /// ScoreManagerを取得し、スコア変更を購読します。
        /// </summary>
        private void Start()
        {
            if (scoreManager == null)
            {
                scoreManager = FindFirstObjectByType<ScoreManager>();
            }

            if (scoreManager != null)
            {
                scoreManager.Score.Subscribe(SetScore).AddTo(this);
            }
        }

        /// <summary>
        /// スコアを6桁表記でUI文字列へ反映します。
        /// </summary>
        public void SetScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE {score:000000}";
            }
        }

        /// <summary>
        /// シーン生成時にScoreManagerとText参照を設定します。
        /// </summary>
        public void Configure(ScoreManager manager, Text text)
        {
            scoreManager = manager;
            scoreText = text;
        }
    }
}
