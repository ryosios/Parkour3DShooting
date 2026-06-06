using UnityEngine;

namespace Parkour3DShooting.Utility
{
    public static class ComponentExtensions
    {
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
