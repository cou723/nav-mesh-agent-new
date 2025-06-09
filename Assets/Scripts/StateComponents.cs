#nullable enable
using UnityEngine;
using UnityEngine.AI;
using IceMilkTea.Core;

/// <summary>
/// ヘルスチェック機能の実装クラス
/// </summary>
public class HealthChecker : IHealthCheckable
{
    private readonly Npc context;

    public HealthChecker(Npc context)
    {
        this.context = context;
    }

    public bool CheckHealth()
    {
        if (context.Health.Value <= 0)
        {
            OnHealthDepleted();
            return false;
        }
        return true;
    }

    public void OnHealthDepleted()
    {
        Debug.Log("NPCは死亡しました");
        // ステートマシンに死亡イベントを送信
        var stateMachineField = typeof(Npc).GetField("stateMachine",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var stateMachine = stateMachineField?.GetValue(context) as ImtStateMachine<Npc>;
        stateMachine?.SendEvent((int)Npc.NpcEvent.Dead);
    }
}

/// <summary>
/// NavMeshAgentを使用した移動機能の実装クラス
/// </summary>
public class NavMeshMover : IMovable
{
    private readonly NavMeshAgent agent;
    private float movementSpeed;
    private float arrivalThreshold = 0.5f;

    public NavMeshMover(NavMeshAgent agent)
    {
        this.agent = agent;
    }

    public bool IsMoving => agent.pathPending || agent.remainingDistance > arrivalThreshold;

    public void Move()
    {
    }

    public void ReflectMovementSpeed(Rigidbody rigidbody, Animator animator)
    {
        movementSpeed = rigidbody.linearVelocity.magnitude;
        animator.SetFloat("MovementSpeed", movementSpeed);
    }
}