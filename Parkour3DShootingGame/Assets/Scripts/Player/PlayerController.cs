using Cysharp.Threading.Tasks;
using Parkour3DShooting.Core;
using Parkour3DShooting.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Parkour3DShooting.Player
{
    /// <summary>
    /// プレイヤーの自動前進、上下左右ブースト、壁走り吸着、被ダメージ、射撃をまとめて制御します。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        /// <summary>プレイヤーの最大HPです。</summary>
        [SerializeField] private int maxHp = 5;
        /// <summary>入力に関係なく前方向へ進む基本速度です。</summary>
        [SerializeField] private float forwardSpeed = 18f;
        /// <summary>左右ブースト開始時に横方向へ瞬間的に足す速度です。</summary>
        [FormerlySerializedAs("boostSpeed")]
        [SerializeField] private float horizontalBoostSpeed = 12f;
        /// <summary>横方向ブースト速度が減衰する速さです。</summary>
        [SerializeField] private float horizontalBoostDamping = 32f;
        /// <summary>上下入力ブーストの速度倍率です。</summary>
        [SerializeField] private float verticalBoostMultiplier = 1f;
        /// <summary>通常時に縦方向速度へ加える重力加速度です。</summary>
        [SerializeField] private float gravity = -85f;
        /// <summary>落下速度の下限値です。</summary>
        [SerializeField] private float terminalFallSpeed = -42f;
        /// <summary>接地中に地面へ軽く押し付けるための縦方向速度です。</summary>
        [SerializeField] private float groundedStickVelocity = -2f;
        /// <summary>ブースト開始直後に重力を無視する時間です。</summary>
        [SerializeField] private float boostGravityIgnoreDuration = 0.22f;
        /// <summary>上下左右ブースト開始時に前方へ瞬間的に足す速度です。</summary>
        [SerializeField] private float boostForwardSpeedBonus = 8f;
        /// <summary>前方ブースト速度が減衰する速さです。</summary>
        [SerializeField] private float boostForwardDamping = 28f;
        /// <summary>上入力ブースト時に与える上昇速度です。</summary>
        [SerializeField] private float upwardBoostVelocity = 18f;
        /// <summary>下入力ブースト時に与える下降速度です。</summary>
        [SerializeField] private float downwardBoostVelocity = -18f;
        /// <summary>壁走り吸着中に重力を無視し続ける時間です。</summary>
        [SerializeField] private float wallRunGravityIgnoreDuration = 0.15f;
        /// <summary>壁走り吸着中に壁面へ引き寄せる速度です。</summary>
        [SerializeField] private float wallRunAttractSpeed = 12f;
        /// <summary>壁から離れているほど吸着速度を強める倍率です。</summary>
        [SerializeField] private float wallRunDistanceAttractMultiplier = 2.5f;
        /// <summary>壁面からこの距離以内なら吸着移動を止めて揺れを抑えます。</summary>
        [SerializeField] private float wallRunAttractStopDistance = 0.55f;
        /// <summary>壁走り中の入力で吸着を解除してジャンプへ移るかどうかです。</summary>
        [SerializeField] private bool cancelWallRunOnInput = true;
        /// <summary>壁走り吸着中に足を壁へ向ける回転速度です。</summary>
        [SerializeField] private float wallRunRotationSpeed = 12f;
        /// <summary>壁走り姿勢に追加する足向きの角度オフセットです。</summary>
        [SerializeField] private float wallRunFootAngleOffset = 0f;
        /// <summary>壁走りを終えた後に直立姿勢へ戻る回転速度です。</summary>
        [SerializeField] private float uprightRecoverySpeed = 8f;
        /// <summary>この高さを下回ると落下死として扱うY座標です。</summary>
        [SerializeField] private float fallOutY = -24f;
        /// <summary>障害物ヒット時に移動速度へ掛ける減速倍率です。</summary>
        [SerializeField] private float speedDownMultiplier = 0.45f;
        /// <summary>自動射撃の発射間隔です。</summary>
        [SerializeField] private float shotInterval = 0.2f;
        /// <summary>プレイヤー弾のダメージ量です。</summary>
        [SerializeField] private int shotDamage = 1;
        /// <summary>弾を生成する位置です。</summary>
        [SerializeField] private Transform shotOrigin;
        /// <summary>自動射撃で生成する弾プレハブです。</summary>
        [SerializeField] private Projectile projectilePrefab;
        /// <summary>壁走り中に傾ける見た目用のルートです。未設定なら自動生成します。</summary>
        [SerializeField] private Transform visualRoot;

        /// <summary>キャラクター移動に使うUnity標準のCharacterControllerです。</summary>
        private CharacterController controller;
        /// <summary>現在のHPです。</summary>
        private int hp;
        /// <summary>減速などを反映する移動速度倍率です。</summary>
        private float speedMultiplier = 1f;
        /// <summary>次の自動射撃までの残り時間です。</summary>
        private float shotTimer;
        /// <summary>現在の縦方向速度です。</summary>
        private float verticalVelocity;
        /// <summary>インパルスとして加算された横方向ブースト速度です。</summary>
        private float horizontalBoostVelocity;
        /// <summary>ブーストによる重力無視の残り時間です。</summary>
        private float boostGravityIgnoreTimer;
        /// <summary>インパルスとして加算された前方ブースト速度です。</summary>
        private float boostForwardVelocity;
        /// <summary>壁走りによる重力無視の残り時間です。</summary>
        private float wallRunGravityIgnoreTimer;
        /// <summary>壁走りで吸着するワールド座標です。</summary>
        private Vector3 wallRunAttractPoint;
        /// <summary>プレイヤーから壁面へ向かう方向です。</summary>
        private Vector3 wallRunSurfaceDirection;
        /// <summary>現在または直近で壁走り吸着点を持っているかどうかです。</summary>
        private bool hasWallRunAttractPoint;
        /// <summary>現在吸着している壁走りトリガーです。</summary>
        private Transform wallRunSource;
        /// <summary>再吸着を禁止している壁走りトリガーです。</summary>
        private Transform suppressedWallRunSource;
        /// <summary>前フレームの移動入力です。</summary>
        private Vector2 previousInput;
        /// <summary>死亡済みで操作を止めるかどうかです。</summary>
        private bool isDead;
        /// <summary>見た目用カプセルの初期ローカル回転です。</summary>
        private Quaternion visualInitialLocalRotation = Quaternion.identity;

        public int CurrentHp => hp;
        public int MaxHp => maxHp;
        public int ShotDamage => shotDamage;

        /// <summary>
        /// コンポーネント参照を取得し、射撃位置が未設定なら自身を使います。
        /// </summary>
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            EnsureVisualRoot();
            if (shotOrigin == null)
            {
                shotOrigin = transform;
            }
        }

        /// <summary>
        /// ゲーム開始時にプレイヤー状態を初期化します。
        /// </summary>
        private void Start()
        {
            ResetPlayer();
        }

        /// <summary>
        /// 生存中だけ移動と自動射撃を毎フレーム更新します。
        /// </summary>
        private void Update()
        {
            if (isDead)
            {
                return;
            }

            Move();
            AutoShoot();
        }

        /// <summary>
        /// HP、速度、重力補助、入力履歴を初期値へ戻します。
        /// </summary>
        public void ResetPlayer()
        {
            hp = maxHp;
            isDead = false;
            speedMultiplier = 1f;
            verticalVelocity = groundedStickVelocity;
            horizontalBoostVelocity = 0f;
            boostGravityIgnoreTimer = 0f;
            boostForwardVelocity = 0f;
            wallRunGravityIgnoreTimer = 0f;
            hasWallRunAttractPoint = false;
            wallRunSource = null;
            suppressedWallRunSource = null;
            wallRunSurfaceDirection = Vector3.zero;
            previousInput = Vector2.zero;
            GameEvents.RaiseHealthChanged(gameObject, hp, maxHp);
        }

        /// <summary>
        /// 指定量のダメージを受け、HPが0になったらゲームオーバーを通知します。
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
                GameEvents.RaiseGameOver();
            }
        }

        /// <summary>
        /// 一定時間プレイヤーの移動速度を下げます。
        /// </summary>
        public void ApplySpeedDown(float duration)
        {
            SpeedDownAsync(duration).Forget();
        }

        /// <summary>
        /// 壁走り用に壁面へ吸着させ、短時間だけ落下重力を抑えます。
        /// </summary>
        public void ApplyWallRunAssist(Transform source, Vector3 attractPoint, Vector3 surfaceDirection)
        {
            if (source == suppressedWallRunSource)
            {
                return;
            }

            bool alreadyWallRunning = hasWallRunAttractPoint && wallRunGravityIgnoreTimer > 0f;
            wallRunSource = source;
            wallRunAttractPoint = attractPoint;
            wallRunSurfaceDirection = surfaceDirection.sqrMagnitude > 0.0001f ? surfaceDirection.normalized : Vector3.zero;
            hasWallRunAttractPoint = true;
            wallRunGravityIgnoreTimer = wallRunGravityIgnoreDuration;

            if (!alreadyWallRunning)
            {
                verticalVelocity = 0f;
                boostGravityIgnoreTimer = 0f;
                boostForwardVelocity = 0f;
            }
            else if (verticalVelocity < 0f)
            {
                verticalVelocity = 0f;
            }
        }

        /// <summary>
        /// 入力、重力、ブースト、壁走り補助を合成してCharacterControllerを動かします。
        /// </summary>
        private void Move()
        {
            Vector2 input = ReadMoveInput();
            bool boostStarted = input.sqrMagnitude > 0.01f && previousInput.sqrMagnitude <= 0.01f;
            bool wallRunActive = hasWallRunAttractPoint && wallRunGravityIgnoreTimer > 0f;
            bool cancelWallRunToJump = cancelWallRunOnInput && wallRunActive && boostStarted;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }

            if (cancelWallRunToJump)
            {
                EndWallRunAssist();
                boostGravityIgnoreTimer = boostGravityIgnoreDuration;
                AddForwardBoostImpulse();
                wallRunActive = false;

                if (input.y > 0.01f)
                {
                    verticalVelocity = Mathf.Max(verticalVelocity, upwardBoostVelocity * verticalBoostMultiplier);
                }
                else if (input.y < -0.01f)
                {
                    verticalVelocity = downwardBoostVelocity * verticalBoostMultiplier;
                }
                else if (verticalVelocity < 0f)
                {
                    verticalVelocity = 0f;
                }

                AddHorizontalBoostImpulse(input.x);
            }
            else if (boostStarted)
            {
                boostGravityIgnoreTimer = boostGravityIgnoreDuration;
                AddForwardBoostImpulse();

                if (input.y > 0.01f)
                {
                    verticalVelocity = Mathf.Max(verticalVelocity, upwardBoostVelocity * verticalBoostMultiplier);
                }
                else if (input.y < -0.01f)
                {
                    verticalVelocity = downwardBoostVelocity * verticalBoostMultiplier;
                }
                else if (verticalVelocity < 0f)
                {
                    verticalVelocity = 0f;
                }

                AddHorizontalBoostImpulse(input.x);
            }

            if (wallRunActive)
            {
                wallRunGravityIgnoreTimer -= Time.deltaTime;
                if (verticalVelocity < 0f)
                {
                    verticalVelocity = 0f;
                }
            }
            else if (boostGravityIgnoreTimer > 0f)
            {
                boostGravityIgnoreTimer -= Time.deltaTime;
            }
            else
            {
                verticalVelocity = Mathf.Max(terminalFallSpeed, verticalVelocity + gravity * Time.deltaTime);
            }

            if (boostForwardVelocity > 0f)
            {
                boostForwardVelocity = Mathf.MoveTowards(boostForwardVelocity, 0f, boostForwardDamping * Time.deltaTime);
            }

            if (!wallRunActive && Mathf.Abs(horizontalBoostVelocity) > 0f)
            {
                horizontalBoostVelocity = Mathf.MoveTowards(horizontalBoostVelocity, 0f, horizontalBoostDamping * Time.deltaTime);
            }

            // 入力開始直後は重力を短く止め、インパルス式の上下左右ブーストを前方加速と一緒に乗せる。
            float forwardVelocity = forwardSpeed + (wallRunActive ? 0f : boostForwardVelocity);
            Vector3 velocity = Vector3.forward * forwardVelocity;
            velocity += Vector3.right * (wallRunActive ? 0f : horizontalBoostVelocity);
            velocity += Vector3.up * (wallRunActive ? 0f : verticalVelocity);
            if (wallRunActive)
            {
                Vector3 toWall = wallRunAttractPoint - transform.position;
                float attractDistance = toWall.magnitude;
                if (attractDistance > wallRunAttractStopDistance)
                {
                    float attractSpeed = wallRunAttractSpeed + attractDistance * wallRunDistanceAttractMultiplier;
                    velocity += toWall.normalized * attractSpeed;
                }
            }

            RotateForWallRun(wallRunActive);
            controller.Move(velocity * speedMultiplier * Time.deltaTime);
            if (wallRunGravityIgnoreTimer <= 0f)
            {
                hasWallRunAttractPoint = false;
            }

            previousInput = input;

            if (transform.position.y < fallOutY)
            {
                ApplyDamage(maxHp);
            }
        }

        /// <summary>
        /// 壁走り中は足が壁側へ向くよう回転し、通常時は直立姿勢へ戻します。
        /// </summary>
        private void RotateForWallRun(bool wallRunActive)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            float rotationSpeed = uprightRecoverySpeed;

            if (wallRunActive)
            {
                Vector3 towardWall = wallRunSurfaceDirection;
                if (towardWall.sqrMagnitude <= 0.0001f)
                {
                    towardWall = wallRunAttractPoint - transform.position;
                }

                if (towardWall.sqrMagnitude > 0.0001f)
                {
                    Vector3 targetUp = -towardWall.normalized;
                    Vector3 targetForward = Vector3.ProjectOnPlane(Vector3.forward, targetUp);
                    if (targetForward.sqrMagnitude <= 0.0001f)
                    {
                        targetForward = Vector3.ProjectOnPlane(transform.forward, targetUp);
                    }

                    targetUp = Quaternion.AngleAxis(wallRunFootAngleOffset, targetForward.normalized) * targetUp;
                    targetRotation = Quaternion.LookRotation(targetForward.normalized, targetUp);
                    rotationSpeed = wallRunRotationSpeed;
                }
            }

            Quaternion localTargetRotation = Quaternion.Inverse(transform.rotation) * targetRotation;
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, visualInitialLocalRotation * localTargetRotation, rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// CharacterController本体を回さずに済むよう、見た目用カプセルを子オブジェクトへ分離します。
        /// </summary>
        private void EnsureVisualRoot()
        {
            if (visualRoot != null)
            {
                visualInitialLocalRotation = visualRoot.localRotation;
                return;
            }

            MeshFilter sourceMesh = GetComponent<MeshFilter>();
            MeshRenderer sourceRenderer = GetComponent<MeshRenderer>();
            if (sourceMesh == null || sourceRenderer == null)
            {
                visualRoot = transform;
                visualInitialLocalRotation = visualRoot.localRotation;
                return;
            }

            GameObject visualObject = new GameObject("PlayerVisual");
            visualObject.transform.SetParent(transform, false);
            visualObject.transform.localPosition = Vector3.zero;
            visualObject.transform.localRotation = Quaternion.identity;
            visualObject.transform.localScale = Vector3.one;

            MeshFilter visualMesh = visualObject.AddComponent<MeshFilter>();
            visualMesh.sharedMesh = sourceMesh.sharedMesh;
            MeshRenderer visualRenderer = visualObject.AddComponent<MeshRenderer>();
            visualRenderer.sharedMaterials = sourceRenderer.sharedMaterials;

            sourceRenderer.enabled = false;
            visualRoot = visualObject.transform;
            visualInitialLocalRotation = visualRoot.localRotation;
        }

        /// <summary>
        /// 壁走り吸着を解除し、通常のブーストや重力処理へ戻します。
        /// </summary>
        private void EndWallRunAssist()
        {
            suppressedWallRunSource = wallRunSource;
            wallRunSource = null;
            wallRunGravityIgnoreTimer = 0f;
            hasWallRunAttractPoint = false;
            wallRunSurfaceDirection = Vector3.zero;
            horizontalBoostVelocity = 0f;
        }

        /// <summary>
        /// 前方ブースト速度をインパルスとして瞬間的に加算します。
        /// </summary>
        private void AddForwardBoostImpulse()
        {
            boostForwardVelocity = boostForwardSpeedBonus;
        }

        /// <summary>
        /// 左右ブースト速度をインパルスとして瞬間的に加算します。
        /// </summary>
        private void AddHorizontalBoostImpulse(float inputX)
        {
            if (Mathf.Abs(inputX) <= 0.01f)
            {
                return;
            }

            horizontalBoostVelocity = Mathf.Sign(inputX) * horizontalBoostSpeed;
        }

        /// <summary>
        /// 指定した壁走りトリガーが再吸着禁止中かどうかを返します。
        /// </summary>
        public bool IsWallRunAssistSuppressed(Transform source)
        {
            return source == suppressedWallRunSource;
        }

        /// <summary>
        /// 指定した壁走りトリガーの再吸着禁止を解除します。
        /// </summary>
        public void ClearWallRunAssistSuppression(Transform source)
        {
            if (source == suppressedWallRunSource)
            {
                suppressedWallRunSource = null;
            }
        }

        /// <summary>
        /// キーボード入力から左右上下の移動方向を読み取ります。
        /// </summary>
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

        /// <summary>
        /// 一定間隔でプレイヤー弾を前方向へ発射します。
        /// </summary>
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

        /// <summary>
        /// 指定秒数だけ速度倍率を下げ、終了後に元へ戻します。
        /// </summary>
        private async UniTask SpeedDownAsync(float duration)
        {
            speedMultiplier = speedDownMultiplier;
            GameEvents.RaiseSpeedDown(gameObject, duration);
            await UniTask.Delay(Mathf.RoundToInt(duration * 1000f), cancellationToken: this.GetCancellationTokenOnDestroy());
            speedMultiplier = 1f;
        }

        /// <summary>
        /// シーン生成時に弾プレハブと射撃位置を外部から設定します。
        /// </summary>
        public void Configure(Projectile prefab, Transform origin)
        {
            projectilePrefab = prefab;
            shotOrigin = origin;
        }
    }
}
