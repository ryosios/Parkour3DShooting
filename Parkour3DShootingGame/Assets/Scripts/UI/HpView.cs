using Parkour3DShooting.Events;
using Parkour3DShooting.Player;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Parkour3DShooting.UI
{
    /// <summary>
    /// プレイヤーHPの変更イベントを受け取り、画面上のHP表示を更新します。
    /// </summary>
    public sealed class HpView : MonoBehaviour
    {
        /// <summary>HP表示対象のプレイヤーです。</summary>
        [SerializeField] private PlayerController player;
        /// <summary>HP文字列を表示するTextです。</summary>
        [SerializeField] private Text hpText;

        /// <summary>
        /// 対象プレイヤーのHP変更イベントだけを購読します。
        /// </summary>
        private void Awake()
        {
            GameEvents.HealthChanged
                .Where(e => player != null && e.Target == player.gameObject)
                .Subscribe(e => SetHp(e.Current, e.Max))
                .AddTo(this);
        }

        /// <summary>
        /// 現在HPと最大HPをUI文字列へ反映します。
        /// </summary>
        public void SetHp(int current, int max)
        {
            if (hpText != null)
            {
                hpText.text = $"HP {current}/{max}";
            }
        }

        /// <summary>
        /// シーン生成時に対象プレイヤーとText参照を設定します。
        /// </summary>
        public void Configure(PlayerController targetPlayer, Text text)
        {
            player = targetPlayer;
            hpText = text;
        }
    }
}
