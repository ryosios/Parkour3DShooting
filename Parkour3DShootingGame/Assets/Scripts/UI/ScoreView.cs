using Parkour3DShooting.Managers;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Parkour3DShooting.UI
{
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private Text scoreText;

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

        public void SetScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE {score:000000}";
            }
        }

        public void Configure(ScoreManager manager, Text text)
        {
            scoreManager = manager;
            scoreText = text;
        }
    }
}
