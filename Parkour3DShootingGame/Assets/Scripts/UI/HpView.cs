using Parkour3DShooting.Events;
using Parkour3DShooting.Player;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Parkour3DShooting.UI
{
    public sealed class HpView : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Text hpText;

        private void Awake()
        {
            GameEvents.HealthChanged
                .Where(e => player != null && e.Target == player.gameObject)
                .Subscribe(e => SetHp(e.Current, e.Max))
                .AddTo(this);
        }

        public void SetHp(int current, int max)
        {
            if (hpText != null)
            {
                hpText.text = $"HP {current}/{max}";
            }
        }

        public void Configure(PlayerController targetPlayer, Text text)
        {
            player = targetPlayer;
            hpText = text;
        }
    }
}
