using Parkour3DShooting.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace Parkour3DShooting.UI
{
    public sealed class ResultView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text titleText;
        [SerializeField] private Text scoreText;
        [SerializeField] private UIAnimationBase openAnimation;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

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

        public void ShowClear(int score)
        {
            Show("STAGE CLEAR", score);
        }

        public void ShowGameOver(int score)
        {
            Show("GAME OVER", score);
        }

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

        public void Configure(CanvasGroup group, Text title, Text score, UIAnimationBase animation)
        {
            canvasGroup = group;
            titleText = title;
            scoreText = score;
            openAnimation = animation;
        }
    }
}
