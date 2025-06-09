#nullable enable
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ヘルスチェック機能を提供するインターフェース
/// </summary>
public interface IHealthCheckable
{
    /// <summary>
    /// ヘルスをチェックし、生存状態を返す
    /// </summary>
    /// <returns>生存している場合はtrue、死亡した場合はfalse</returns>
    bool CheckHealth();

    /// <summary>
    /// ヘルスが0になった時の処理
    /// </summary>
    void OnHealthDepleted();
}

/// <summary>
/// 移動機能を提供するインターフェース
/// </summary>
public interface IMovable
{
    /// <summary>
    /// 移動処理を実行
    /// </summary>
    void Move();

    void ReflectMovementSpeed(Animator animator, NavMeshAgent agent);
}