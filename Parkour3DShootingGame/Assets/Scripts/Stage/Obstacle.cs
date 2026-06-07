using Parkour3DShooting.Player;
using UnityEngine;

namespace Parkour3DShooting.Stage
{
    /// <summary>
    /// プレイヤーが触れると一定時間だけ移動速度を下げる障害物です。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class Obstacle : MonoBehaviour
    {
        /// <summary>プレイヤーを減速させる秒数です。</summary>
        [SerializeField] private float speedDownDuration = 1.2f;

        /// <summary>
        /// プレイヤーが触れたら減速効果を付与します。
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                player.ApplySpeedDown(speedDownDuration);
            }
        }
    }
}
