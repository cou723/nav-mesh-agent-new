#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using IceMilkTea.Core;
using System;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class Npc : MonoBehaviour
{
    public Animator Animator;
    public GameObject FoodTarget;

    public void TriggerAnimation(AnimationTrigger trigger)
    {
        Debug.Log($"Triggering animation: {trigger.TriggerName}");
        Animator.SetTrigger(trigger.TriggerName);
    }

    private ImtStateMachine<Npc> stateMachine;

    public NavMeshAgent Agent;

    public Health Health = new(200f);

    // アニメーション終了イベント用のデリゲート
    public Action? OnEatingAnimationEndEvent;
    public Action? OnDeadAnimationEndEvent;

    public readonly float IdleConsumptionRate = 10f;
    public readonly float MoveConsumptionRate = 20f;

    public readonly float FeelingHungryThreshold = 180f;

    // イベントID定義
    public enum NpcEvent
    {
        MoveToFood = 2,
        ToIdle = 3,
        Hungry = 4,
        Dead = 5,
        TryEating
    }

    void Start()
    {
        if (!TryGetComponent(out Agent))
        {
            Debug.LogError("NavMeshAgent component is missing on the Npc GameObject.");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            return;
        }

        if (!TryGetComponent(out Animator))
        {
            Debug.LogError("Animator component is missing on the Npc GameObject.");
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
        stateMachine.AddTransition<HungryState, IdleState>((int)NpcEvent.ToIdle);

        stateMachine.AddTransition<MoveNextFoodState, IdleState>((int)NpcEvent.ToIdle);
        stateMachine.AddTransition<MoveNextFoodState, DeadState>((int)NpcEvent.Dead);
        stateMachine.AddTransition<MoveNextFoodState, EatingState>((int)NpcEvent.TryEating);

        stateMachine.AddTransition<EatingState, IdleState>((int)NpcEvent.ToIdle);

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

    // Animatorから呼び出されるメソッド（デリゲートを実行）
    public void OnEatingAnimationEnd()
    {
        OnEatingAnimationEndEvent?.Invoke();
    }

    public void OnDeadAnimationEnd()
    {
        OnDeadAnimationEndEvent?.Invoke();
    }
}