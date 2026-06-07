using UnityEngine;

namespace Parkour3DShooting.Utility
{
    /// <summary>
    /// GameObjectとComponent操作を簡潔にする拡張メソッド群です。
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// 指定コンポーネントがあれば取得し、なければ追加して返します。
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent(out T component))
            {
                return component;
            }

            return gameObject.AddComponent<T>();
        }
    }
}
