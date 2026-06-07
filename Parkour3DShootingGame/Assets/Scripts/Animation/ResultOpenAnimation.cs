using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Parkour3DShooting.Animation
{
    /// <summary>
    /// DOTweenでリザルトUIをフェードインとスケール演出付きで開きます。
    /// </summary>
    public sealed class ResultOpenAnimation : UIAnimationBase
    {
        /// <summary>フェードイン対象のCanvasGroupです。</summary>
        [SerializeField] private CanvasGroup canvasGroup;
        /// <summary>スケール演出対象のパネルRectTransformです。</summary>
        [SerializeField] private RectTransform panel;
        /// <summary>表示アニメーションにかける秒数です。</summary>
        [SerializeField] private float duration = 0.35f;

        /// <summary>
        /// 未設定のUI参照を自身のコンポーネントから補完します。
        /// </summary>
        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (panel == null)
            {
                panel = transform as RectTransform;
            }
        }

        /// <summary>
        /// フェードとスケールを同時に再生し、完了まで待機します。
        /// </summary>
        public override async UniTask PlayAsync()
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = 0f;
            if (panel != null)
            {
                panel.localScale = Vector3.one * 0.94f;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Join(canvasGroup.DOFade(1f, duration));
            if (panel != null)
            {
                sequence.Join(panel.DOScale(1f, duration).SetEase(Ease.OutBack));
            }

            UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
            sequence.OnComplete(() => completionSource.TrySetResult());
            await completionSource.Task.AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
        }

        /// <summary>
        /// シーン生成時にアニメーション対象の参照を設定します。
        /// </summary>
        public void Configure(CanvasGroup group, RectTransform rectTransform)
        {
            canvasGroup = group;
            panel = rectTransform;
        }
    }
}
