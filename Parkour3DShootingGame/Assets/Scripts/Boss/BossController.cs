using Parkour3DShooting.Core;
using Parkour3DShooting.Events;
using Parkour3DShooting.Player;
using UnityEngine;

namespace Parkour3DShooting.Boss
{
    /// <summary>
    /// ボスの追従移動、HP管理、プレイヤーへの射撃を制御します。
    /// </summary>
    public sealed class BossController : MonoBehaviour
    {
        /// <summary>追従と照準の対象になるプレイヤーです。</summary>
        [SerializeField] private Transform player;
        /// <summary>ボスの最大HPです。</summary>
        [SerializeField] private int maxHp = 80;
        /// <summary>プレイヤー前方に維持する距離です。</summary>
        [SerializeField] private float targetDistance = 38f;
        /// <summary>目標位置へ補間移動する速度です。</summary>
        [SerializeField] private float followLerp = 3f;
        /// <summary>上下ホバー移動の振れ幅です。</summary>
        [SerializeField] private float hoverAmplitude = 1.5f;
        /// <summary>上下ホバー移動の周期速度です。</summary>
        [SerializeField] private float hoverFrequency = 1.7f;
        /// <summary>ボス弾の発射間隔です。</summary>
        [SerializeField] private float shotInterval = 0.75f;
        /// <summary>ボス弾のダメージ量です。</summary>
        [SerializeField] private int shotDamage = 1;
        /// <summary>ボス弾を生成する位置です。</summary>
        [SerializeField] private Transform shotOrigin;
        /// <summary>ボスが発射する弾プレハブです。</summary>
        [SerializeField] private Projectile projectilePrefab;

        /// <summary>現在のボスHPです。</summary>
        private int hp;
        /// <summary>次の射撃までの残り時間です。</summary>
        private float shotTimer;
        /// <summary>撃破済みで更新を止めるかどうかです。</summary>
        private bool isDead;

        public int CurrentHp => hp;
        public int MaxHp => maxHp;

        /// <summary>
        /// 開始時にボス状態を初期化します。
        /// </summary>
        private void Start()
        {
            ResetBoss();
        }

        /// <summary>
        /// 生存中、プレイヤーを追従して射撃します。
        /// </summary>
        private void Update()
        {
            if (isDead || player == null)
            {
                return;
            }

            Move();
            ShootAtPlayer();
        }

        /// <summary>
        /// HPと射撃タイマーを初期状態へ戻します。
        /// </summary>
        public void ResetBoss()
        {
            hp = maxHp;
            isDead = false;
            shotTimer = shotInterval;
            GameEvents.RaiseHealthChanged(gameObject, hp, maxHp);
        }

        /// <summary>
        /// ダメージを受け、HPが0になったらボス撃破イベントを通知します。
        /// </summary>
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

        /// <summary>
        /// プレイヤー前方の目標位置へ移動し、プレイヤー方向を向きます。
        /// </summary>
        private void Move()
        {
            Vector3 targetPosition = player.position + Vector3.forward * targetDistance + Vector3.up * (4f + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude);
            transform.position = Vector3.Lerp(transform.position, targetPosition, followLerp * Time.deltaTime);
            transform.LookAt(player.position + Vector3.up * 1.5f);
        }

        /// <summary>
        /// 射撃間隔ごとにプレイヤーへ向けて弾を発射します。
        /// </summary>
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

        /// <summary>
        /// シーン生成時に追従対象、弾プレハブ、射撃位置を設定します。
        /// </summary>
        public void Configure(Transform playerTransform, Projectile prefab, Transform origin)
        {
            player = playerTransform;
            projectilePrefab = prefab;
            shotOrigin = origin;
        }
    }
}
