# Biểu đồ lớp cho PassiveInstance

Dưới đây là biểu đồ mô tả cấu trúc và luồng hoạt động của `PassiveInstance` cùng với các thành phần liên quan. Biểu đồ này giúp bạn hình dung được cách mà hệ thống Passive (Kỹ năng bị động) được nạp, cấu hình, bắt sự kiện và thực thi hiệu ứng trong trò chơi.

```mermaid
classDiagram
    class PassiveInstance {
        +PassiveConfig Config
        -int _level
        -CharacterProfileModel _owner
        +List~EquipModifier~ Modifiers
        +Activate()
        +Dispose()
        -ApplyStaticModifiers()
        +SubscribeToEntity(Entity ownerEntity)
        +UnsubscribeFromEntity(Entity ownerEntity)
        -TriggerPassiveEffect(CombatEventConfig evtConfig, Entity owner, Transform attackerTarget)
    }

    class PassiveConfig {
        +long DescTemplateHash
        +List~StaticModifierConfig~ StaticModifiers
        +List~CombatEventConfig~ CombatEvents
        +GetDescription(int currentUpgrade) string
    }

    class CombatEventConfig {
        +string EventType
        +string EffectId
        +List~float~ ModifyByUpgrade
        +string Target
        +string ConditionFilter
        +HashSet~string~ ConditionTags
        +float EffectParam
        +int InternalCooldown
    }

    class Entity {
        +Action~Entity~ OnTurnStart
        +GetCoreComponent~T~() T
    }
    
    class EntityStats {
        +Action~float, Transform~ OnHit
        +Action OnDeath
    }

    class CombatContext {
        +Entity Source
        +Entity Target
        +float Value
        +string EventContextInfo
        +HashSet~string~ Tags
        +AddTag(string tag) CombatContext
        +HasTag(string tag) bool
    }

    class PassiveEventListener {
        <<static>>
        +EvaluateAndExecute(CombatEventConfig evtConfig, int passiveLevel, CombatContext context)
        -IsConditionMatched(CombatEventConfig evtConfig, CombatContext context) bool
        -ExecuteEffect(CombatEventConfig evtConfig, int passiveLevel, CombatContext context)
        -ResolveTarget(string targetType, CombatContext context) Entity
    }

    class PassiveEffectFactory {
        <<static>>
        +ExecuteEffect(string effectId, Entity finalTarget, float effectValue, CombatContext context)
    }

    %% Quan hệ giữa các lớp
    PassiveInstance "1" o-- "1" PassiveConfig : chứa
    PassiveConfig "1" *-- "*" CombatEventConfig : định nghĩa sự kiện
    
    PassiveInstance ..> Entity : Đăng ký sự kiện (Subscribe)
    Entity *-- EntityStats : chứa component
    
    PassiveInstance ..> CombatContext : Tạo ra (khi trigger)
    PassiveInstance ..> PassiveEventListener : Giao phó thực thi (EvaluateAndExecute)

    PassiveEventListener ..> CombatContext : Phân tích (Matching & Target)
    PassiveEventListener ..> PassiveEffectFactory : Ủy quyền (ExecuteEffect)
```

### Luồng hoạt động chính của PassiveInstance:

1. **Khởi tạo & Cấp chỉ số tĩnh (`Activate`)**: 
   - `PassiveInstance` nhận `PassiveConfig` và `Level`.
   - Lấy các chỉ số tĩnh (`StaticModifiers`) từ cấu hình và biến thành `EquipModifier` để cộng trực tiếp vào nhân vật.
   
2. **Đăng ký sự kiện (`SubscribeToEntity`)**:
   - `PassiveInstance` lắng nghe các sự kiện từ `Entity` chủ (ví dụ: `OnTurnStart` của `Entity`, hoặc `OnHit`, `OnDeath` của `EntityStats`).
   
3. **Kích hoạt sự kiện (`TriggerPassiveEffect`)**:
   - Khi sự kiện xảy ra trong trận (ví dụ: bị đánh trúng), hàm `TriggerPassiveEffect` chạy.
   - Hàm này sẽ gói gọn thông tin của trận đánh (Người đánh, Mục tiêu) vào một đối tượng `CombatContext`.
   
4. **Đánh giá và Thực thi (`PassiveEventListener`)**:
   - `PassiveInstance` đẩy `CombatEventConfig` và `CombatContext` vừa tạo sang cho `PassiveEventListener`.
   - `PassiveEventListener` sẽ kiểm tra (`IsConditionMatched`) xem có thỏa mãn điều kiện Tag không.
   - Nếu thỏa mãn, nó sẽ xác định đối tượng chịu tác động (`ResolveTarget` - VD: Self, Target).
   - Cuối cùng, nó gọi `PassiveEffectFactory.ExecuteEffect` để áp dụng hiệu ứng thực tế lên mục tiêu.
