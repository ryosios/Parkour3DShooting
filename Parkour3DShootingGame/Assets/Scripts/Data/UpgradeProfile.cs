using UnityEngine;

namespace Parkour3DShooting.Data
{
    /// <summary>
    /// 将来のアップグレード要素で使う強化値を保持するScriptableObjectです。
    /// </summary>
    [CreateAssetMenu(menuName = "Parkour3DShooting/Upgrade Profile")]
    public sealed class UpgradeProfile : ScriptableObject
    {
        /// <summary>移動速度に加算する強化値です。</summary>
        [SerializeField] private float moveSpeedBonus = 2f;
        /// <summary>弾ダメージに加算する強化値です。</summary>
        [SerializeField] private int shotPowerBonus = 1;
        /// <summary>最大HPに加算する強化値です。</summary>
        [SerializeField] private int maxHpBonus = 1;

        public float MoveSpeedBonus => moveSpeedBonus;
        public int ShotPowerBonus => shotPowerBonus;
        public int MaxHpBonus => maxHpBonus;
    }
}
