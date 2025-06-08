#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using IceMilkTea.Core;


namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

[RequireComponent(typeof(NavMeshAgent))]
public class Npc : MonoBehaviour
{
    private GameObject nextFood = null;
    private NavMeshAgent agent;
    private ImtStateMachine<Npc> stateMachine;

    public Health Health = new(200f);

    public readonly float IdleConsumptionRate = 2f;
    public readonly float MoveConsumptionRate = 3f;

    public readonly float FeelingHungryThreshold = 180f;

    // イベントID定義
    public enum NpcEvent
    {
        MoveToFood = 2,
        Arrived = 3,
        Hungry = 4,
        Dead = 5
    }

    // NPCステートの基底クラス - 共通の健康チェック処理を含む
    public abstract class HealthCheckableState : ImtStateMachine<Npc>.State
    {
        protected internal sealed override void Update()
        {
            if (Context.Health.Value <= 0)
            {
                Debug.Log("NPCは死亡しました");
                StateMachine.SendEvent((int)NpcEvent.Dead);
                return;
            }

            // 各ステート固有の更新処理を実行
            UpdateState();
        }

        // 各ステートで実装する更新処理
        protected abstract void UpdateState();
    }

    public class IdleState : HealthCheckableState
    {
        protected internal override void Enter()
        {
            Debug.Log("Idle状態に入りました");
        }

        protected override void UpdateState()
        {
            Context.Health.Consume(Time.deltaTime * Context.IdleConsumptionRate);

            if (Context.Health.Value < Context.FeelingHungryThreshold)
            {
                Debug.Log("NPCはお腹が空いています");
                StateMachine.SendEvent((int)NpcEvent.Hungry);
            }
        }
    }

    public class HungryState : ImtStateMachine<Npc>.State
    {
        protected internal override void Enter()
        {
            Debug.Log("HungryState状態に入りました");

            // FeedingGroundsが空の場合は取得を試行
            if (Context.GetFoundFeedingGrounds().Count == 0)
            {
                Debug.Log("次の場所が見つかりませんでした。Idleに戻ります。");
                // 見つからなかった場合はIdleへ遷移
                StateMachine.SendEvent((int)NpcEvent.Arrived);
                return;
            }

            // GetFoundFeedingGroundsの最初の値を参照
            Context.nextFood = Context.GetFoundFeedingGrounds()[0];
            Debug.Log($"次の目的地を見つけました: {Context.nextFood}");

            // agentにsetDestinationする
            Context.agent.SetDestination(Context.nextFood.transform.position);

            // MoveNextPositionに遷移
            StateMachine.SendEvent((int)NpcEvent.MoveToFood);
            return;
        }
    }

    public class MoveNextFoodState : HealthCheckableState
    {
        private float arrivalThreshold = 0.5f;

        protected internal override void Enter()
        {
            Debug.Log($"MoveNextFoodState状態に入りました: {Context.Health}");
            Debug.Log("MoveNextFood状態に入りました");
        }

        protected override void UpdateState()
        {
            Context.Health.Consume(Time.deltaTime * Context.MoveConsumptionRate);

            // 目的地に到着したかチェック
            if (!Context.agent.pathPending && Context.agent.remainingDistance < arrivalThreshold)
            {
                Debug.Log("目的地に到着しました");
                var feedingComponent = Context.nextFood.GetComponent<FeedingGround>();
                if (feedingComponent == null)
                {
                    Debug.LogWarning("Feedingコンポーネントが見つかりません。Idleに戻ります。");
                    StateMachine.SendEvent((int)NpcEvent.Arrived);
                    return;
                }
                if (feedingComponent.Ate())
                    Context.Health.Add(feedingComponent.HealthAmount);
                // Idleに遷移
                StateMachine.SendEvent((int)NpcEvent.Arrived);
            }
        }

        // ステート固有の設定メソッド
        public void SetArrivalThreshold(float threshold)
        {
            arrivalThreshold = threshold;
        }
    }

    public class DeadState : ImtStateMachine<Npc>.State
    {
        protected internal override void Enter()
        {
            Debug.Log("NPCは死亡しました。ゲームオーバーです。");
            Context.gameObject.SetActive(false);
        }
    }
    void Start()
    {
        if (!TryGetComponent(out agent))
        {
            Debug.LogError("NavMeshAgent component is missing on the Npc GameObject.");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            return;
        }

        // ステートマシンの初期化
        stateMachine = new ImtStateMachine<Npc>(this);

        // 遷移テーブルの設定
        stateMachine.AddTransition<IdleState, HungryState>((int)NpcEvent.Hungry);
        stateMachine.AddTransition<IdleState, DeadState>((int)NpcEvent.Dead);

        stateMachine.AddTransition<HungryState, MoveNextFoodState>((int)NpcEvent.MoveToFood);
        stateMachine.AddTransition<HungryState, IdleState>((int)NpcEvent.Arrived);

        stateMachine.AddTransition<MoveNextFoodState, IdleState>((int)NpcEvent.Arrived);
        stateMachine.AddTransition<MoveNextFoodState, DeadState>((int)NpcEvent.Dead);

        // 開始ステートの設定
        stateMachine.SetStartState<IdleState>();
    }

    void Update()
    {
        // ステートマシンの更新
        stateMachine?.Update();
    }

    public List<GameObject> GetFoundFeedingGrounds()
    {
        return GameObject.FindGameObjectsWithTag("FeedingGround")
        .ToList();
    }
}