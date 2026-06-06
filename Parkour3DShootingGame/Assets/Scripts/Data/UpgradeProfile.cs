using UnityEngine;

namespace Parkour3DShooting.Data
{
    [CreateAssetMenu(menuName = "Parkour3DShooting/Upgrade Profile")]
    public sealed class UpgradeProfile : ScriptableObject
    {
        [SerializeField] private float moveSpeedBonus = 2f;
        [SerializeField] private int shotPowerBonus = 1;
        [SerializeField] private int maxHpBonus = 1;

        public float MoveSpeedBonus => moveSpeedBonus;
        public int ShotPowerBonus => shotPowerBonus;
        public int MaxHpBonus => maxHpBonus;
    }
}
