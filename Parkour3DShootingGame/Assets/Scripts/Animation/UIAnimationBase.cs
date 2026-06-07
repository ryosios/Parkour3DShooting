using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Parkour3DShooting.Animation
{
    /// <summary>
    /// UIアニメーションの共通インターフェースです。
    /// </summary>
    public abstract class UIAnimationBase : MonoBehaviour
    {
        /// <summary>
        /// アニメーションを非同期で再生します。
        /// </summary>
        public abstract UniTask PlayAsync();

        /// <summary>
        /// 戻り値を待たずにアニメーションを再生します。
        /// </summary>
        public void Play()
        {
            PlayAsync().Forget();
        }
    }
}
