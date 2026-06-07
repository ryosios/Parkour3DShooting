using Parkour3DShooting.Events;
using Parkour3DShooting.Player;
using UnityEngine;

namespace Parkour3DShooting.Stage
{
    /// <summary>
    /// プレイヤーが近づいたり触れたりしたときにグレイズ加点と壁走り補助を行うトリガーです。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class GrazeTrigger : MonoBehaviour
    {
        /// <summary>1秒あたりに加算するグレイズスコアです。</summary>
        [SerializeField] private int score = 100;
        /// <summary>壁走り用の吸着と重力補助を有効にするかどうかです。</summary>
        [SerializeField] private bool enableWallRunAssist = true;
        /// <summary>吸着先として使う壁面のローカルX座標です。</summary>
        [SerializeField] private float wallSurfaceLocalX;
        /// <summary>壁走り用に検知コライダーを外側へ広げるかどうかです。</summary>
        [SerializeField] private bool expandWallRunDetection = true;
        /// <summary>壁走り吸着を開始する検知範囲の厚みです。</summary>
        [SerializeField] private float wallRunDetectionThickness = 3.6f;

        /// <summary>小数点以下のスコア加算分を次フレームへ持ち越す値です。</summary>
        private float scoreRemainder;
        /// <summary>グレイズ判定に使うトリガーコライダーです。</summary>
        private Collider triggerCollider;

        /// <summary>
        /// コライダーをトリガー化し、壁走り用の検知形状を整えます。
        /// </summary>
        private void Awake()
        {
            triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
            EnsureWallRunDetectionShape();
        }

        /// <summary>
        /// Inspectorで値を変えたとき、検知コライダーへ即時反映します。
        /// </summary>
        private void OnValidate()
        {
            if (!TryGetComponent(out triggerCollider))
            {
                return;
            }

            triggerCollider.isTrigger = true;
            EnsureWallRunDetectionShape();
        }

        /// <summary>
        /// プレイヤーが検知範囲内にいる間、壁走り補助と継続スコア加算を行います。
        /// </summary>
        private void OnTriggerStay(Collider other)
        {
            if (!other.TryGetComponent(out PlayerController player))
            {
                return;
            }

            if (enableWallRunAssist && !player.IsWallRunAssistSuppressed(transform))
            {
                Vector3 attractPoint = GetWallRunAttractPoint(player.transform.position);
                Vector3 surfaceDirection = attractPoint - player.transform.position;
                player.ApplyWallRunAssist(transform, attractPoint, surfaceDirection);
            }

            // 接触している時間に応じて加点する。小数分は保持して、低FPSでもスコアを落とさない。
            scoreRemainder += score * Time.deltaTime;
            int scoreToAdd = Mathf.FloorToInt(scoreRemainder);
            if (scoreToAdd <= 0)
            {
                return;
            }

            scoreRemainder -= scoreToAdd;
            GameEvents.RaiseGraze(transform.position, scoreToAdd);
        }

        /// <summary>
        /// プレイヤーが離れたとき、小数スコアの持ち越しをリセットします。
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                player.ClearWallRunAssistSuppression(transform);
                scoreRemainder = 0f;
            }
        }

        /// <summary>
        /// プレイヤー位置に対して最も自然な壁面上の吸着点を返します。
        /// </summary>
        private Vector3 GetWallRunAttractPoint(Vector3 playerPosition)
        {
            if (triggerCollider is BoxCollider boxCollider)
            {
                Vector3 localPosition = transform.InverseTransformPoint(playerPosition);
                Vector3 halfSize = boxCollider.size * 0.5f;
                Vector3 localCenter = boxCollider.center;
                localPosition.x = wallSurfaceLocalX;
                localPosition.y = Mathf.Clamp(localPosition.y, localCenter.y - halfSize.y, localCenter.y + halfSize.y);
                localPosition.z = Mathf.Clamp(localPosition.z, localCenter.z - halfSize.z, localCenter.z + halfSize.z);
                return transform.TransformPoint(localPosition);
            }

            return triggerCollider.ClosestPoint(playerPosition);
        }

        /// <summary>
        /// 壁面として扱うローカルX座標を設定します。
        /// </summary>
        public void ConfigureWallSurface(float localX)
        {
            wallSurfaceLocalX = localX;
        }

        /// <summary>
        /// 壁面座標と吸着検知範囲をまとめて設定します。
        /// </summary>
        public void ConfigureWallRunDetection(float localSurfaceX, float detectionThickness)
        {
            wallSurfaceLocalX = localSurfaceX;
            wallRunDetectionThickness = detectionThickness;
        }

        /// <summary>
        /// 表示用の薄い壁を保ったまま、実際の検知コライダーだけ外側へ広げます。
        /// </summary>
        private void EnsureWallRunDetectionShape()
        {
            if (!enableWallRunAssist || !expandWallRunDetection || triggerCollider is not BoxCollider boxCollider)
            {
                return;
            }

            float visualThickness = Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.x));
            int side = InferWallSide();
            if (Mathf.Approximately(wallSurfaceLocalX, 0f))
            {
                wallSurfaceLocalX = -side * 0.5f;
            }

            boxCollider.size = new Vector3(wallRunDetectionThickness / visualThickness, 1f, 1f);
            boxCollider.center = new Vector3(side * ((wallRunDetectionThickness * 0.5f - 0.05f) - 0.15f) / visualThickness, 0f, 0f);
        }

        /// <summary>
        /// オブジェクト名または位置から、建物の左右どちら側の壁かを推定します。
        /// </summary>
        private int InferWallSide()
        {
            int underscoreIndex = name.LastIndexOf('_');
            if (underscoreIndex >= 0 && int.TryParse(name[(underscoreIndex + 1)..], out int index))
            {
                return index % 2 == 0 ? -1 : 1;
            }

            return transform.position.x < 0f ? -1 : 1;
        }
    }
}
