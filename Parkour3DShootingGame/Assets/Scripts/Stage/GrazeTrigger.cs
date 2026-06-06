using Parkour3DShooting.Events;
using Parkour3DShooting.Player;
using UnityEngine;

namespace Parkour3DShooting.Stage
{
    [RequireComponent(typeof(Collider))]
    public sealed class GrazeTrigger : MonoBehaviour
    {
        [SerializeField] private int score = 100;
        [SerializeField] private bool consumeOnce = true;

        private bool consumed;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (consumed || !other.TryGetComponent(out PlayerController _))
            {
                return;
            }

            consumed = consumeOnce;
            GameEvents.RaiseGraze(transform.position, score);
        }
    }
}
