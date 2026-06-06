using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Parkour3DShooting.Animation
{
    public sealed class ResultOpenAnimation : UIAnimationBase
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panel;
        [SerializeField] private float duration = 0.35f;

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

        public void Configure(CanvasGroup group, RectTransform rectTransform)
        {
            canvasGroup = group;
            panel = rectTransform;
        }
    }
}
