using Parkour3DShooting.Boss;
using Parkour3DShooting.Core;
using UnityEngine;

namespace Parkour3DShooting.Player
{
    [RequireComponent(typeof(Collider))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 45f;
        [SerializeField] private float lifetime = 4f;

        private ProjectileOwner owner;
        private int damage = 1;
        private Vector3 direction = Vector3.forward;
        private float lifeTimer;

        private void Awake()
        {
            Collider projectileCollider = GetComponent<Collider>();
            projectileCollider.isTrigger = true;
        }

        private void OnEnable()
        {
            lifeTimer = lifetime;
        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        public void Launch(Vector3 launchDirection, ProjectileOwner projectileOwner, int projectileDamage)
        {
            direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector3.forward;
            owner = projectileOwner;
            damage = Mathf.Max(1, projectileDamage);
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (owner == ProjectileOwner.Player && other.TryGetComponent(out BossController boss))
            {
                boss.ApplyDamage(damage);
                Destroy(gameObject);
                return;
            }

            if (owner == ProjectileOwner.Boss && other.TryGetComponent(out PlayerController player))
            {
                player.ApplyDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
