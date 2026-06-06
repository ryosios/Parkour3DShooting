using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Parkour3DShooting.Animation
{
    public abstract class UIAnimationBase : MonoBehaviour
    {
        public abstract UniTask PlayAsync();

        public void Play()
        {
            PlayAsync().Forget();
        }
    }
}
