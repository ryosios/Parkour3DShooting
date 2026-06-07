using Parkour3DShooting.Boss;
using Parkour3DShooting.Core;
using UnityEngine;

namespace Parkour3DShooting.Player
{
    /// <summary>
    /// プレイヤーまたはボスが発射する直進弾です。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class Projectile : MonoBehaviour
    {
        /// <summary>弾の移動速度です。</summary>
        [SerializeField] private float speed = 45f;
        /// <summary>弾が自動で消えるまでの秒数です。</summary>
        [SerializeField] private float lifetime = 4f;

        /// <summary>この弾を発射した陣営です。</summary>
        private ProjectileOwner owner;
        /// <summary>ヒット時に与えるダメージ量です。</summary>
        private int damage = 1;
        /// <summary>弾が進むワールド方向です。</summary>
        private Vector3 direction = Vector3.forward;
        /// <summary>消滅までの残り時間です。</summary>
        private float lifeTimer;

        /// <summary>
        /// 弾のコライダーをトリガーとして設定します。
        /// </summary>
        private void Awake()
        {
            Collider projectileCollider = GetComponent<Collider>();
            projectileCollider.isTrigger = true;
        }

        /// <summary>
        /// 有効化されたときに寿命タイマーを初期化します。
        /// </summary>
        private void OnEnable()
        {
            lifeTimer = lifetime;
        }

        /// <summary>
        /// 弾を進行方向へ移動し、寿命が切れたら破棄します。
        /// </summary>
        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 発射方向、発射者、ダメージを設定して弾を初期化します。
        /// </summary>
        public void Launch(Vector3 launchDirection, ProjectileOwner projectileOwner, int projectileDamage)
        {
            direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector3.forward;
            owner = projectileOwner;
            damage = Mathf.Max(1, projectileDamage);
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        /// <summary>
        /// 敵対する対象に当たったらダメージを与えて弾を破棄します。
        /// </summary>
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
