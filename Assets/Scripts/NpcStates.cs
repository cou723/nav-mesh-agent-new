#nullable enable
using UnityEngine;
using IceMilkTea.Core;

/// <summary>
/// 新しい設計によるIdleState - ヘルスチェック機能付き
/// </summary>
public class IdleState : BaseNpcState
{
    public IdleState() : base(needsHealthCheck: true, needsMovement: false)
    {
    }

    protected override void OnEnterState()
    {
        Context.TriggerAnimation(new Npc.Idle());
    }

    protected override void UpdateState()
    {
        // ヘルス消費
        Context.Health.Consume(Time.deltaTime * Context.IdleConsumptionRate);

        // 空腹チェック
        if (Context.Health.Value < Context.FeelingHungryThreshold)
        {
            Debug.Log("NPCはお腹が空いています");
            StateMachine.SendEvent((int)Npc.NpcEvent.Hungry);
        }
    }
}

/// <summary>
/// 新しい設計によるMoveNextFoodState - ヘルスチェック + 移動機能付き
/// </summary>
public class MoveNextFoodState : BaseNpcState
{
    public MoveNextFoodState() : base(needsHealthCheck: true, needsMovement: true)
    {
    }

    protected override void OnEnterState()
    {
        Debug.Log($"MoveNextFoodState状態に入りました: {Context.Health.Value}");
        Context.TriggerAnimation(new Npc.Moving());
    }

    protected override void UpdateState()
    {
        // ヘルス消費
        Context.Health.Consume(Time.deltaTime * Context.MoveConsumptionRate);
        if (Context.Agent.isStopped || !Context.Agent.hasPath)
            stateMachine.SendEvent((int)Npc.NpcEvent.TryEating);
    }

}

/// <summary>
/// 新しい設計によるHungryState - 機能なし（既存の処理をそのまま維持）
/// </summary>
public class HungryState : BaseNpcState
{
    public HungryState() : base(needsHealthCheck: false, needsMovement: false)
    {
    }

    protected override void OnEnterState()
    {
        Debug.Log("HungryState状態に入りました");

        // FeedingGroundsが空の場合は取得を試行
        if (Context.GetFoundFeedingGrounds().Count == 0)
        {
            Debug.Log("次の場所が見つかりませんでした。Idleに戻ります。");
            StateMachine.SendEvent((int)Npc.NpcEvent.ToIdle);
            return;
        }

        Context.FoodTarget = Context.GetFoundFeedingGrounds()[0];
        Debug.Log($"次の目的地を見つけました: {Context.FoodTarget}");

        Context.Agent.SetDestination(Context.FoodTarget.transform.position);

        StateMachine.SendEvent((int)Npc.NpcEvent.MoveToFood);
    }

    protected override void UpdateState()
    {
    }

}

public class DeadState : BaseNpcState
{
    public DeadState() : base(needsHealthCheck: false, needsMovement: false)
    {
    }

    protected override void OnEnterState()
    {
        Context.TriggerAnimation(new Npc.Dead());
        Debug.Log("NPCは死亡しました。ゲームオーバーです。");
        // Context.gameObject.SetActive(false); // この行を削除（アニメーション終了後に処理）
    }

    protected override void UpdateState()
    {
        // DeadStateでは更新処理は不要
    }

    protected override void SubscribeToAnimationEvents()
    {
        Context.OnDeadAnimationEndEvent += OnDeadAnimationEnd;
    }

    protected override void UnsubscribeFromAnimationEvents()
    {
        Context.OnDeadAnimationEndEvent -= OnDeadAnimationEnd;
    }

    protected override void OnDeadAnimationEnd()
    {
        Debug.Log("Deadアニメーションが終了しました。ゲームオブジェクトを破棄します。");
        Object.Destroy(Context.gameObject);
    }
}

public class EatingState : BaseNpcState
{
    public EatingState() : base(needsHealthCheck: true, needsMovement: false)
    {
    }

    protected override void OnEnterState()
    {
        Debug.Log("EatingState状態に入りました");
        Context.TriggerAnimation(new Npc.Eating());
        if (Context.FoodTarget == null)
        {
            Debug.LogWarning("nextFoodが設定されていません。Idleに戻ります。");
            StateMachine.SendEvent((int)Npc.NpcEvent.ToIdle);
            return;
        }

        var feedingComponent = Context.FoodTarget.GetComponent<FeedingGround>();
        if (feedingComponent == null)
        {
            Debug.LogWarning("Feedingコンポーネントが見つかりません。Idleに戻ります。");
            StateMachine.SendEvent((int)Npc.NpcEvent.ToIdle);
            return;
        }

        // 食事処理
        if (feedingComponent.Ate())
        {
            Context.Health.Add(feedingComponent.HealthAmount);
        }
        SubscribeToAnimationEvents();
    }

    protected override void UpdateState()
    {

    }

    protected override void SubscribeToAnimationEvents()
    {
        Context.OnEatingAnimationEndEvent += OnEatingAnimationEnd;
    }

    protected override void UnsubscribeFromAnimationEvents()
    {
        Context.OnEatingAnimationEndEvent -= OnEatingAnimationEnd;
    }

    protected override void OnEatingAnimationEnd()
    {
        Debug.Log("Eatingアニメーションが終了しました");
        StateMachine.SendEvent((int)Npc.NpcEvent.ToIdle);
        UnsubscribeFromAnimationEvents();
    }
}