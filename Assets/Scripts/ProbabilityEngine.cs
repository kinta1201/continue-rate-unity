using UnityEngine;

/// <summary>
/// 確率判定エンジン：完全独立抽選のみを担当
/// </summary>
public static class ProbabilityEngine
{
    /// <summary>
    /// 成功判定（完全独立）
    /// </summary>
    /// <param name="successRate01">成功率（0.0〜1.0）</param>
    public static bool IsSuccess(float successRate01)
    {
        successRate01 = Mathf.Clamp01(successRate01);
        return Random.value < successRate01;
    }
}
