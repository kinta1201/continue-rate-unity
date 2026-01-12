using UnityEngine;

/// <summary>
/// 確率判定エンジン：完全独立抽選
/// </summary>
public static class ProbabilityEngine
{
    /// <summary>
    /// 成功判定（毎回独立、補正なし）
    /// </summary>
    /// <param name="successRate">成功率 0.0〜1.0</param>
    /// <returns>true=成功、false=失敗</returns>
    public static bool IsSuccess(float successRate)
    {
        return Random.value < successRate;
    }
}