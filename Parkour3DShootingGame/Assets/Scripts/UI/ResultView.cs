using Parkour3DShooting.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace Parkour3DShooting.UI
{
    /// <summary>
    /// ステージクリアとゲームオーバーのリザルト表示を制御します。
    /// </summary>
    public sealed class ResultView : MonoBehaviour
    {
        /// <summary>表示、非表示、入力ブロックをまとめて制御するCanvasGroupです。</summary>
        [SerializeField] private CanvasGroup canvasGroup;
        /// <summary>リザルトタイトルを表示するTextです。</summary>
        [SerializeField] private Text titleText;
        /// <summary>最終スコアを表示するTextです。</summary>
        [SerializeField] private Text scoreText;
        /// <summary>リザルト表示時に再生するUIアニメーションです。</summary>
        [SerializeField] private UIAnimationBase openAnimation;

        /// <summary>
        /// CanvasGroup参照が未設定なら自身から取得します。
        /// </summary>
        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// リザルトUIを即座に非表示にします。
        /// </summary>
        public void HideImmediate()
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// ステージクリア表示を開きます。
        /// </summary>
        public void ShowClear(int score)
        {
            Show("STAGE CLEAR", score);
        }

        /// <summary>
        /// ゲームオーバー表示を開きます。
        /// </summary>
        public void ShowGameOver(int score)
        {
            Show("GAME OVER", score);
        }

        /// <summary>
        /// タイトルとスコアを反映し、必要なら表示アニメーションを再生します。
        /// </summary>
        private void Show(string title, int score)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (scoreText != null)
            {
                scoreText.text = $"SCORE {score:000000}";
            }

            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            if (openAnimation != null)
            {
                openAnimation.Play();
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// シーン生成時にUI参照をまとめて設定します。
        /// </summary>
        public void Configure(CanvasGroup group, Text title, Text score, UIAnimationBase animation)
        {
            canvasGroup = group;
            titleText = title;
            scoreText = score;
            openAnimation = animation;
        }
    }
}
