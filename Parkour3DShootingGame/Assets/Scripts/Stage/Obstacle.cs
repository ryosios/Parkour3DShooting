using Parkour3DShooting.Player;
using UnityEngine;

namespace Parkour3DShooting.Stage
{
    [RequireComponent(typeof(Collider))]
    public sealed class Obstacle : MonoBehaviour
    {
        [SerializeField] private float speedDownDuration = 1.2f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                player.ApplySpeedDown(speedDownDuration);
            }
        }
    }
}
