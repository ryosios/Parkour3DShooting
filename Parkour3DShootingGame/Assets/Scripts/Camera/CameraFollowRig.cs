using UnityEngine;

namespace Parkour3DShooting.Camera
{
    /// <summary>
    /// Cinemachineが使えない場合の保険として、対象を追従する簡易カメラリグです。
    /// </summary>
    public sealed class CameraFollowRig : MonoBehaviour
    {
        /// <summary>追従対象のTransformです。</summary>
        [SerializeField] private Transform target;
        /// <summary>追従対象から見たカメラ位置のオフセットです。</summary>
        [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -12f);
        /// <summary>目標位置へ補間する速度です。</summary>
        [SerializeField] private float positionLerp = 8f;
        /// <summary>注視点として使う対象の高さです。</summary>
        [SerializeField] private float lookHeight = 2f;

        /// <summary>
        /// 対象の後方へ滑らかに移動し、指定高さを向きます。
        /// </summary>
        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, target.position + offset, positionLerp * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }

        /// <summary>
        /// 追従対象だけを設定します。
        /// </summary>
        public void Configure(Transform followTarget)
        {
            target = followTarget;
        }

        /// <summary>
        /// 追従対象、オフセット、注視高さをまとめて設定します。
        /// </summary>
        public void Configure(Transform followTarget, Vector3 followOffset, float targetLookHeight)
        {
            target = followTarget;
            offset = followOffset;
            lookHeight = targetLookHeight;
        }
    }

}
