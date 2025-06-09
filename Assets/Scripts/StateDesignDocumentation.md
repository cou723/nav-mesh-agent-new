# NPCステート設計の改善：Interface + Composition パターン

## 概要

従来の継承ベースの設計から、Interface + Composition パターンに移行することで、機能の組み合わせを柔軟に行えるようになりました。

## 問題点

### 従来の設計（継承ベース）
```csharp
// 問題：C#は単一継承のため、複数の機能を組み合わせできない
public abstract class HealthCheckableState : ImtStateMachine<Npc>.State
{
    // ヘルスチェック機能
}

public class IdleState : HealthCheckableState  // ✓ ヘルスチェック可能
{
    // しかし、MovableStateも継承したい場合は不可能
}
```

### 制限事項
- 単一継承による機能組み合わせの制限
- コードの重複
- 新機能追加時の拡張性の低さ

## 新しい設計（Interface + Composition）

### 1. 機能をInterfaceで定義
```csharp
public interface IHealthCheckable
{
    bool CheckHealth();
    void OnHealthDepleted();
}

public interface IMovable
{
    void Move();
    void SetDestination(Vector3 destination);
    bool IsMoving { get; }
}
```

### 2. 機能の具体実装
```csharp
public class HealthChecker : IHealthCheckable { /* 実装 */ }
public class NavMeshMover : IMovable { /* 実装 */ }
```

### 3. 基底クラスでComposition
```csharp
public abstract class BaseNpcState : ImtStateMachine<Npc>.State
{
    protected IHealthCheckable? healthChecker;
    protected IMovable? mover;
    
    protected BaseNpcState(bool needsHealthCheck, bool needsMovement)
    {
        // 必要な機能のみを組み込み
    }
}
```

### 4. 柔軟な機能組み合わせ
```csharp
// ヘルスチェックのみ
public class IdleState : BaseNpcState
{
    public IdleState() : base(needsHealthCheck: true, needsMovement: false) { }
}

// ヘルスチェック + 移動
public class MoveNextFoodState : BaseNpcState
{
    public MoveNextFoodState() : base(needsHealthCheck: true, needsMovement: true) { }
}

// 機能なし
public class HungryState : BaseNpcState
{
    public HungryState() : base(needsHealthCheck: false, needsMovement: false) { }
}
```

## 利点

### 1. 柔軟性
- 任意の機能組み合わせが可能
- 新しい機能（IAttackable、IAnimatableなど）を簡単に追加

### 2. 再利用性
- 各機能が独立しており、他のプロジェクトでも使用可能
- 機能の実装を差し替え可能（例：NavMeshMover → RigidbodyMover）

### 3. テスタビリティ
- 各機能を個別にテスト可能
- モック実装を簡単に作成可能

### 4. 保守性
- 機能ごとに責任が分離
- 変更の影響範囲が限定的

## 使用例

### 新しい機能の追加
```csharp
// 攻撃機能を追加
public interface IAttackable
{
    void Attack(GameObject target);
    bool CanAttack { get; }
}

public class MeleeAttacker : IAttackable
{
    public void Attack(GameObject target) { /* 近接攻撃実装 */ }
    public bool CanAttack => /* 攻撃可能条件 */;
}

// 攻撃可能なState
public class CombatState : BaseNpcState
{
    private IAttackable? attacker;
    
    public CombatState() : base(needsHealthCheck: true, needsMovement: true)
    {
        // 攻撃機能も追加
        attacker = new MeleeAttacker();
    }
}
```

### 機能の組み合わせ例
```csharp
// パターン1：ヘルスチェックのみ
new IdleState()  // HealthCheck: ✓, Movement: ✗

// パターン2：移動のみ
new PatrolState() // HealthCheck: ✗, Movement: ✓

// パターン3：両方
new MoveNextFoodState() // HealthCheck: ✓, Movement: ✓

// パターン4：なし
new HungryState() // HealthCheck: ✗, Movement: ✗

// 将来の拡張例
new CombatState() // HealthCheck: ✓, Movement: ✓, Attack: ✓
```

## マイグレーション手順

### 段階1：新しいファイルの追加
1. `StateInterfaces.cs` - 機能のInterface定義
2. `StateComponents.cs` - 機能の具体実装
3. `BaseNpcState.cs` - 新しい基底クラス
4. `NewNpcStates.cs` - 新しい設計のStateクラス

### 段階2：テスト用クラスの作成
1. `NpcWithNewStates.cs` - 新しい設計を使用するNpcクラス

### 段階3：段階的移行
1. 新しい設計のテストと検証
2. 既存コードの段階的置き換え
3. 古いコードの削除

### 段階4：機能拡張
1. 新しいInterface（IAttackable、IAnimatableなど）の追加
2. 対応する実装クラスの作成
3. 新しい機能を使用するStateの実装

## ファイル構成

```
Assets/Scripts/
├── Npc.cs                      # 既存のNpcクラス（従来設計）
├── StateInterfaces.cs          # 機能のInterface定義
├── StateComponents.cs          # 機能の具体実装
├── BaseNpcState.cs            # 新しい基底Stateクラス
├── NewNpcStates.cs            # 新しい設計のStateクラス
├── NpcWithNewStates.cs        # 新しい設計を使用するNpcクラス（サンプル）
└── StateDesignDocumentation.md # このドキュメント
```

## 今後の拡張可能性

### 追加可能な機能例
- `IAttackable` - 攻撃機能
- `IAnimatable` - アニメーション制御
- `IInteractable` - オブジェクトとの相互作用
- `IInventoryManageable` - インベントリ管理
- `IDialogable` - 会話機能

### 実装パターン
```csharp
// 複数機能を持つ高度なState
public class AdvancedCombatState : BaseNpcState
{
    private IAttackable? attacker;
    private IAnimatable? animator;
    
    public AdvancedCombatState() : base(
        needsHealthCheck: true, 
        needsMovement: true)
    {
        // 追加機能の初期化
    }
}
```

この設計により、NPCの行動をより柔軟かつ拡張可能な方法で実装できるようになります。