using Parkour3DShooting.Core;
using Parkour3DShooting.Events;
using Parkour3DShooting.Player;
using UnityEngine;

namespace Parkour3DShooting.Boss
{
    public sealed class BossController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private int maxHp = 80;
        [SerializeField] private float targetDistance = 38f;
        [SerializeField] private float followLerp = 3f;
        [SerializeField] private float hoverAmplitude = 1.5f;
        [SerializeField] private float hoverFrequency = 1.7f;
        [SerializeField] private float shotInterval = 0.75f;
        [SerializeField] private int shotDamage = 1;
        [SerializeField] private Transform shotOrigin;
        [SerializeField] private Projectile projectilePrefab;

        private int hp;
        private float shotTimer;
        private bool isDead;

        public int CurrentHp => hp;
        public int MaxHp => maxHp;

        private void Start()
        {
            ResetBoss();
        }

        private void Update()
        {
            if (isDead || player == null)
            {
                return;
            }

            Move();
            ShootAtPlayer();
        }

        public void ResetBoss()
        {
            hp = maxHp;
            isDead = false;
            shotTimer = shotInterval;
            GameEvents.RaiseHealthChanged(gameObject, hp, maxHp);
        }

        public void ApplyDamage(int amount)
        {
            if (isDead || amount <= 0)
            {
                return;
            }

            hp = Mathf.Max(0, hp - amount);
            GameEvents.RaiseDamage(gameObject, amount);
            GameEvents.RaiseHealthChanged(gameObject, hp, maxHp);

            if (hp <= 0)
            {
                isDead = true;
                GameEvents.RaiseBossDead();
            }
        }

        private void Move()
        {
            Vector3 targetPosition = player.position + Vector3.forward * targetDistance + Vector3.up * (4f + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude);
            transform.position = Vector3.Lerp(transform.position, targetPosition, followLerp * Time.deltaTime);
            transform.LookAt(player.position + Vector3.up * 1.5f);
        }

        private void ShootAtPlayer()
        {
            if (projectilePrefab == null)
            {
                return;
            }

            shotTimer -= Time.deltaTime;
            if (shotTimer > 0f)
            {
                return;
            }

            shotTimer = shotInterval;
            Vector3 origin = shotOrigin != null ? shotOrigin.position : transform.position;
            Vector3 direction = (player.position + Vector3.up - origin).normalized;
            Projectile projectile = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction, Vector3.up));
            projectile.Launch(direction, ProjectileOwner.Boss, shotDamage);
        }

        public void Configure(Transform playerTransform, Projectile prefab, Transform origin)
        {
            player = playerTransform;
            projectilePrefab = prefab;
            shotOrigin = origin;
        }
    }
}
