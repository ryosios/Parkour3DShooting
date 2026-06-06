using UnityEngine;

namespace Parkour3DShooting.Camera
{
    public sealed class CameraFollowRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -12f);
        [SerializeField] private float positionLerp = 8f;
        [SerializeField] private float lookHeight = 2f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, target.position + offset, positionLerp * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }

        public void Configure(Transform followTarget)
        {
            target = followTarget;
        }

        public void Configure(Transform followTarget, Vector3 followOffset, float targetLookHeight)
        {
            target = followTarget;
            offset = followOffset;
            lookHeight = targetLookHeight;
        }
    }
}
