#nullable enable
using UnityEngine;
using IceMilkTea.Core;

/// <summary>
/// NPCステートの基底クラス - Interface + Composition パターンを使用
/// </summary>
public abstract class BaseNpcState : ImtStateMachine<Npc>.State
{
    protected IHealthCheckable? healthChecker;
    protected IMovable? mover;

    /// <summary>
    /// BaseNpcStateのコンストラクタ
    /// </summary>
    /// <param name="needsHealthCheck">ヘルスチェックが必要かどうか</param>
    /// <param name="needsMovement">移動機能が必要かどうか</param>
    protected BaseNpcState(bool needsHealthCheck = false, bool needsMovement = false)
    {
        // 注意: Contextはステートマシンに登録された後に利用可能になるため、
        // コンストラクタではなくEnterメソッドで初期化する
        this.needsHealthCheck = needsHealthCheck;
        this.needsMovement = needsMovement;
    }

    private readonly bool needsHealthCheck;
    private readonly bool needsMovement;

    protected internal override void Enter()
    {
        // 機能コンポーネントの初期化
        if (needsHealthCheck)
        {
            healthChecker = new HealthChecker(Context);
        }

        if (needsMovement)
        {
            // NavMeshAgentを取得
            var agent = Context.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                mover = new NavMeshMover(agent);
            }
            else
            {
                Debug.LogWarning("NavMeshAgentが見つかりません。移動機能を無効化します。");
            }
        }

        // アニメーション終了イベントの購読
        SubscribeToAnimationEvents();

        // 各ステート固有のEnter処理
        OnEnterState();
    }

    protected internal sealed override void Update()
    {
        // ヘルスチェック（必要な場合のみ）
        if (healthChecker != null && !healthChecker.CheckHealth())
            return;

        // 移動処理（必要な場合のみ）
        if (mover != null)
        {
            if (!Context.TryGetComponent<Rigidbody>(out var rb))
            {
                Debug.LogWarning("Rigidbodyが見つかりません。移動機能を無効化します。");
                return;
            }

            if(!Context.TryGetComponent<Animator>(out var animator)) {
                Debug.LogWarning("Animatorが見つかりません。移動機能を無効化します。");
                return;
            }

            mover.ReflectMovementSpeed(rb, animator);
        }


        // 各ステート固有の更新処理
        UpdateState();
    }

    protected internal override void Exit()
    {
        // 各ステート固有のExit処理
        OnExitState();

        // アニメーション終了イベントの購読解除
        UnsubscribeFromAnimationEvents();

        // リソースのクリーンアップ
        healthChecker = null;
        mover = null;
    }

    /// <summary>
    /// ステートに入った時の処理（各ステートで実装）
    /// </summary>
    protected virtual void OnEnterState()
    {
        // デフォルトでは何もしない
    }

    /// <summary>
    /// ステートの更新処理（各ステートで実装）
    /// </summary>
    protected abstract void UpdateState();

    /// <summary>
    /// ステートから出る時の処理（各ステートで実装）
    /// </summary>
    protected virtual void OnExitState()
    {
        // デフォルトでは何もしない
    }

    /// <summary>
    /// ヘルスチェック機能が有効かどうか
    /// </summary>
    protected bool HasHealthCheck => healthChecker != null;

    /// <summary>
    /// 移動機能が有効かどうか
    /// </summary>
    protected bool HasMovement => mover != null;

    /// <summary>
    /// アニメーション終了イベントの購読
    /// </summary>
    protected virtual void SubscribeToAnimationEvents()
    {
        // デフォルトでは何もしない
        // 必要なステートでオーバーライドする
    }

    /// <summary>
    /// アニメーション終了イベントの購読解除
    /// </summary>
    protected virtual void UnsubscribeFromAnimationEvents()
    {
        // デフォルトでは何もしない
        // 必要なステートでオーバーライドする
    }

    /// <summary>
    /// Eatingアニメーション終了時の処理
    /// </summary>
    protected virtual void OnEatingAnimationEnd()
    {
        // デフォルトでは何もしない
    }

    /// <summary>
    /// Deadアニメーション終了時の処理
    /// </summary>
    protected virtual void OnDeadAnimationEnd()
    {
        // デフォルトでは何もしない
    }
}