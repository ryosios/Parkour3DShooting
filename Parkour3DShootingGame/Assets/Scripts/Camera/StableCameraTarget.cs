using UnityEngine;
using Parkour3DShooting.Player;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;

namespace Parkour3DShooting.Camera
{
    /// <summary>
    /// プレイヤーの回転に影響されず、位置だけを追従するカメラ用ターゲットです。
    /// </summary>
    public sealed class StableCameraTarget : MonoBehaviour
    {
        /// <summary>自動生成する安定カメラターゲットの名前です。</summary>
        private const string CameraTargetName = "PlayerCameraTarget";
        /// <summary>Cinemachine Follow Cameraのシーン上の名前です。</summary>
        private const string FollowCameraName = "Cinemachine Follow Camera";
        /// <summary>安定ターゲットを使うカメラの追従オフセットです。</summary>
        private static readonly Vector3 FollowCameraOffset = new Vector3(0f, 1f, -7f);
        /// <summary>フォールバックカメラが注視する高さです。</summary>
        private const float FollowCameraLookHeight = 1.4f;

        /// <summary>位置を追従する対象です。</summary>
        [SerializeField] private Transform target;
        /// <summary>追従対象からのワールド座標オフセットです。</summary>
        [SerializeField] private Vector3 offset;
        /// <summary>ターゲット位置へ補間する速度です。0以下なら即時追従します。</summary>
        [SerializeField] private float positionLerp = 0f;

        /// <summary>
        /// 対象の位置だけを追従し、回転は常にワールド基準へ戻します。
        /// </summary>
        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 targetPosition = target.position + offset;
            transform.position = positionLerp > 0f
                ? Vector3.Lerp(transform.position, targetPosition, positionLerp * Time.deltaTime)
                : targetPosition;
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// 追従対象と位置オフセットを設定します。
        /// </summary>
        public void Configure(Transform followTarget, Vector3 followOffset)
        {
            target = followTarget;
            offset = followOffset;
            transform.position = target != null ? target.position + offset : transform.position;
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// シーン読み込み後、カメラがプレイヤーの回転ではなく安定ターゲットを追うように補正します。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallForMainSceneCamera()
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            GameObject followCameraObject = GameObject.Find(FollowCameraName);
            if (player == null || followCameraObject == null)
            {
                return;
            }

            StableCameraTarget stableTarget = FindFirstObjectByType<StableCameraTarget>();
            if (stableTarget == null)
            {
                GameObject targetObject = new GameObject(CameraTargetName);
                stableTarget = targetObject.AddComponent<StableCameraTarget>();
            }

            stableTarget.Configure(player.transform, Vector3.zero);

            CinemachineCamera virtualCamera = followCameraObject.GetComponent<CinemachineCamera>();
            if (virtualCamera != null)
            {
                virtualCamera.Follow = stableTarget.transform;
                virtualCamera.LookAt = stableTarget.transform;
            }

            CinemachineFollow follow = followCameraObject.GetComponent<CinemachineFollow>();
            if (follow != null)
            {
                follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
                follow.FollowOffset = FollowCameraOffset;
            }

            CameraFollowRig fallbackRig = followCameraObject.GetComponent<CameraFollowRig>();
            if (fallbackRig != null)
            {
                fallbackRig.Configure(stableTarget.transform, FollowCameraOffset, FollowCameraLookHeight);
            }
        }
    }
}
