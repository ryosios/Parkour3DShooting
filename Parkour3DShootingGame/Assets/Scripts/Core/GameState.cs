namespace Parkour3DShooting.Core
{
    /// <summary>
    /// ゲーム全体が現在どのフェーズにいるかを表します。
    /// </summary>
    public enum GameState
    {
        /// <summary>タイトルまたは開始待ち状態です。</summary>
        Title,
        /// <summary>ステージ進行中です。</summary>
        Stage,
        /// <summary>ボス戦進行中です。</summary>
        BossBattle,
        /// <summary>リザルト表示中です。</summary>
        Result,
        /// <summary>ゲームオーバー状態です。</summary>
        GameOver
    }
}
