using Cysharp.Threading.Tasks;
using Parkour3DShooting.Core;
using Parkour3DShooting.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parkour3DShooting.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private int maxHp = 5;
        [SerializeField] private float forwardSpeed = 18f;
        [SerializeField] private float boostSpeed = 12f;
        [SerializeField] private float gravity = -28f;
        [SerializeField] private float terminalFallSpeed = -42f;
        [SerializeField] private float groundedStickVelocity = -2f;
        [SerializeField] private float boostGravityIgnoreDuration = 0.22f;
        [SerializeField] private float upwardBoostVelocity = 18f;
        [SerializeField] private float downwardBoostVelocity = -18f;
        [SerializeField] private float fallOutY = -24f;
        [SerializeField] private float speedDownMultiplier = 0.45f;
        [SerializeField] private float shotInterval = 0.2f;
        [SerializeField] private int shotDamage = 1;
        [SerializeField] private Transform shotOrigin;
        [SerializeField] private Projectile projectilePrefab;

        private CharacterController controller;
        private int hp;
        private float speedMultiplier = 1f;
        private float shotTimer;
        private float verticalVelocity;
        private float boostGravityIgnoreTimer;
        private Vector2 previousInput;
        private bool isDead;

        public int CurrentHp => hp;
        public int MaxHp => maxHp;
        public int ShotDamage => shotDamage;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (shotOrigin == null)
            {
                shotOrigin = transform;
            }
        }

        private void Start()
        {
            ResetPlayer();
        }

        private void Update()
        {
            if (isDead)
            {
                return;
            }

            Move();
            AutoShoot();
        }

        public void ResetPlayer()
        {
            hp = maxHp;
            isDead = false;
            speedMultiplier = 1f;
            verticalVelocity = groundedStickVelocity;
            boostGravityIgnoreTimer = 0f;
            previousInput = Vector2.zero;
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
                GameEvents.RaiseGameOver();
            }
        }

        public void ApplySpeedDown(float duration)
        {
            SpeedDownAsync(duration).Forget();
        }

        private void Move()
        {
            Vector2 input = ReadMoveInput();
            bool boostStarted = input.sqrMagnitude > 0.01f && previousInput.sqrMagnitude <= 0.01f;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }

            if (boostStarted)
            {
                boostGravityIgnoreTimer = boostGravityIgnoreDuration;

                if (input.y > 0.01f)
                {
                    verticalVelocity = Mathf.Max(verticalVelocity, upwardBoostVelocity);
                }
                else if (input.y < -0.01f)
                {
                    verticalVelocity = downwardBoostVelocity;
                }
                else if (verticalVelocity < 0f)
                {
                    verticalVelocity = 0f;
                }
            }

            if (boostGravityIgnoreTimer > 0f)
            {
                boostGravityIgnoreTimer -= Time.deltaTime;
            }
            else
            {
                verticalVelocity = Mathf.Max(terminalFallSpeed, verticalVelocity + gravity * Time.deltaTime);
            }

            // 入力開始直後は重力を短く止め、連続ブーストでも上方向へ押し上げられるようにする。
            Vector3 velocity = Vector3.forward * forwardSpeed;
            velocity += Vector3.right * input.x * boostSpeed;
            velocity += Vector3.up * verticalVelocity;
            controller.Move(velocity * speedMultiplier * Time.deltaTime);
            previousInput = input;

            if (transform.position.y < fallOutY)
            {
                ApplyDamage(maxHp);
            }
        }

        private Vector2 ReadMoveInput()
        {
            if (Keyboard.current == null)
            {
                return Vector2.zero;
            }

            Vector2 input = Vector2.zero;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                input.y += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private void AutoShoot()
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
            Projectile projectile = Instantiate(projectilePrefab, shotOrigin.position, Quaternion.identity);
            projectile.Launch(Vector3.forward, ProjectileOwner.Player, shotDamage);
        }

        private async UniTask SpeedDownAsync(float duration)
        {
            speedMultiplier = speedDownMultiplier;
            GameEvents.RaiseSpeedDown(gameObject, duration);
            await UniTask.Delay(Mathf.RoundToInt(duration * 1000f), cancellationToken: this.GetCancellationTokenOnDestroy());
            speedMultiplier = 1f;
        }

        public void Configure(Projectile prefab, Transform origin)
        {
            projectilePrefab = prefab;
            shotOrigin = origin;
        }
    }
}
