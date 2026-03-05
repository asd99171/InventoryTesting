# RPG Inventory System - Unity 적용 가이드

## 프로젝트 구조

```
Assets/Scripts/Inventory/
├── Data/                      ← ScriptableObject 기반 아이템 정의
│   ├── Enums.cs               (ItemType, ArmorSlotType, Rarity)
│   ├── ItemData.cs            (abstract SO - 공통 필드)
│   ├── WeaponData.cs          (무기 SO)
│   ├── ArmorData.cs           (방어구 SO)
│   ├── ConsumableData.cs      (소비 아이템 SO)
│   ├── ItemDatabase.cs        (아이템 레지스트리 SO)
│   └── ItemStack.cs           (런타임 슬롯 래퍼)
├── Logic/                     ← 비즈니스 로직
│   ├── SingletonMono.cs       (제네릭 싱글톤 베이스)
│   ├── InventoryManager.cs    (인벤토리 CRUD)
│   ├── EquipmentManager.cs    (장착/해제)
│   └── PlayerStats.cs         (장비 기반 스탯 합산)
└── UI/                        ← (아직 미구현)
```

---

## 1단계: 아이템 SO 에셋 만들기

### 1-1. 무기 생성

1. Project 창에서 `Assets/Data/Items/Weapons` 폴더 생성
2. 우클릭 → **Create → Inventory → Weapon Data**
3. Inspector에서 다음 필드 입력:

| 필드 | 예시 값 | 설명 |
|------|---------|------|
| Item ID | `weapon_iron_sword` | 고유 식별자 (중복 불가) |
| Item Name | `철검` | 표시 이름 |
| Icon | (Sprite 할당) | 인벤토리 아이콘 |
| Description | `기본적인 철제 검` | 툴팁 설명 |
| Item Type | `Weapon` | 자동 설정됨 |
| Rarity | `Common` | 등급 |
| Max Stack Count | `1` | 장비는 1 고정 |
| Sell Price | `50` | 판매 가격 |
| Attack Power | `15` | 공격력 |
| Attack Speed | `1.2` | 공격 속도 |
| Weapon Range | `1.5` | 사거리 |

### 1-2. 방어구 생성

1. `Assets/Data/Items/Armors` 폴더 생성
2. 우클릭 → **Create → Inventory → Armor Data**
3. Inspector에서 필드 입력:

| 필드 | 예시 값 |
|------|---------|
| Item ID | `armor_iron_helmet` |
| Item Name | `철 투구` |
| Slot Type | `Head` |
| Defense | `8` |
| Magic Resistance | `3` |

> **Slot Type 종류**: Head, Body, Legs, Hands, Feet (5부위)

### 1-3. 소비 아이템 생성

1. `Assets/Data/Items/Consumables` 폴더 생성
2. 우클릭 → **Create → Inventory → Consumable Data**
3. Inspector에서 필드 입력:

| 필드 | 예시 값 |
|------|---------|
| Item ID | `consumable_hp_potion` |
| Item Name | `체력 포션` |
| Max Stack Count | `99` |
| Heal Amount | `50` |
| Buff Duration | `0` |
| Cooldown | `1.5` |

---

## 2단계: ItemDatabase 에셋 만들기

1. `Assets/Data` 폴더에서 우클릭 → **Create → Inventory → Item Database**
2. 이름을 `MainItemDatabase`로 지정
3. Inspector의 **Items** 리스트에 위에서 만든 모든 아이템 SO를 드래그하여 등록

```
MainItemDatabase (ItemDatabase SO)
└── Items
    ├── [0] weapon_iron_sword
    ├── [1] armor_iron_helmet
    ├── [2] consumable_hp_potion
    └── ...
```

> ID로 검색이 가능합니다: `database.GetItemByID("weapon_iron_sword")`

---

## 3단계: 씬에 매니저 배치하기

### 방법 A: 자동 생성 (권장)

`SingletonMono<T>`가 Instance 접근 시 자동으로 GameObject를 생성합니다.
아무 스크립트에서든 `InventoryManager.Instance`를 호출하면 됩니다.

### 방법 B: 수동 배치

씬에서 Inspector로 `maxSlotCount`, `baseAttack` 등을 직접 조정하고 싶다면:

1. 빈 GameObject 생성 → 이름: `[GameManagers]`
2. 다음 컴포넌트를 추가:
   - **InventoryManager** — `Max Slot Count` 설정 (기본값: 30)
   - **EquipmentManager** — 추가 설정 없음
   - **PlayerStats** — `Base Attack`, `Base Defense`, `Base Magic Resist` 설정

```
Hierarchy
└── [GameManagers]          ← DontDestroyOnLoad 자동 적용
    ├── InventoryManager    (maxSlotCount: 30)
    ├── EquipmentManager
    └── PlayerStats         (baseAttack: 10, baseDefense: 5, baseMagicResist: 3)
```

> 싱글톤이므로 씬 전환 시에도 유지됩니다. 여러 씬에 중복 배치하지 마세요.

---

## 4단계: 코드에서 사용하기

### 아이템 추가

```csharp
using Inventory.Data;
using Inventory.Logic;

// ItemDatabase에서 검색하여 추가
public class LootDrop : MonoBehaviour
{
    [SerializeField] private ItemDatabase database;

    public void GiveItemToPlayer(string itemID, int amount)
    {
        var item = database.GetItemByID(itemID);
        bool success = InventoryManager.Instance.AddItem(item, amount);

        if (!success)
            Debug.Log("인벤토리가 가득 찼습니다!");
    }
}
```

### 아이템 직접 추가 (SO 참조)

```csharp
[SerializeField] private WeaponData ironSword;   // Inspector에서 SO 드래그
[SerializeField] private ConsumableData hpPotion;

void Start()
{
    InventoryManager.Instance.AddItem(ironSword, 1);
    InventoryManager.Instance.AddItem(hpPotion, 10);
}
```

### 아이템 사용 (장착 / 소비 자동 분기)

```csharp
// 슬롯 인덱스 0번 아이템 사용
// - Weapon/Armor → 자동으로 EquipmentManager.Equip() 호출
// - Consumable  → 수량 차감 (효과 적용은 TODO)
InventoryManager.Instance.UseItem(0);
```

### 장비 해제

```csharp
// 머리 방어구 해제 → 인벤토리로 되돌림
bool success = EquipmentManager.Instance.UnequipToInventory(ArmorSlotType.Head);

// 무기 해제 → 인벤토리로 되돌림
bool success = EquipmentManager.Instance.UnequipWeaponToInventory();
```

### 스탯 조회

```csharp
var stats = PlayerStats.Instance;

Debug.Log($"공격력: {stats.TotalAttack}");      // 기본 + 무기
Debug.Log($"방어력: {stats.TotalDefense}");      // 기본 + 방어구 합산
Debug.Log($"마법저항: {stats.TotalMagicResist}"); // 기본 + 방어구 합산

// 장비 보너스만 따로 확인
var bonus = stats.GetEquipmentBonus();
Debug.Log(bonus); // "ATK +15 / DEF +20 / MRES +8"
```

### 인벤토리 조회

```csharp
var inv = InventoryManager.Instance;

// 특정 아이템 보유 확인
if (inv.HasItem("consumable_hp_potion", 3))
    Debug.Log("포션 3개 이상 보유 중");

// 전체 슬롯 순회
for (int i = 0; i < inv.UsedSlotCount; i++)
{
    var slot = inv.GetSlot(i);
    Debug.Log($"[{i}] {slot.data.itemName} x{slot.currentStack}");
}

// 정렬 (타입 → 등급(높은순) → 이름)
inv.SortInventory();
```

---

## 5단계: 이벤트 구독 (UI 연결 준비)

각 매니저는 `Action` 이벤트를 제공합니다. UI를 만들 때 이벤트를 구독하면 됩니다.

```csharp
using Inventory.Logic;

public class InventoryUI : MonoBehaviour
{
    void OnEnable()
    {
        InventoryManager.Instance.OnInventoryChanged += RefreshSlots;
        EquipmentManager.Instance.OnEquipmentChanged += RefreshEquipment;
        PlayerStats.Instance.OnStatsChanged += RefreshStatPanel;
    }

    void OnDisable()
    {
        InventoryManager.Instance.OnInventoryChanged -= RefreshSlots;
        EquipmentManager.Instance.OnEquipmentChanged -= RefreshEquipment;
        PlayerStats.Instance.OnStatsChanged -= RefreshStatPanel;
    }

    void RefreshSlots() { /* 인벤토리 슬롯 UI 갱신 */ }
    void RefreshEquipment() { /* 장비 슬롯 UI 갱신 */ }
    void RefreshStatPanel() { /* 스탯 텍스트 갱신 */ }
}
```

### 이벤트 요약

| 매니저 | 이벤트 | 발행 시점 |
|--------|--------|-----------|
| `InventoryManager` | `OnInventoryChanged` | AddItem, RemoveItem, UseItem, SwapSlots, SortInventory |
| `EquipmentManager` | `OnEquipmentChanged` | Equip, Unequip, UnequipToInventory |
| `PlayerStats` | `OnStatsChanged` | 장비 변경 시 자동 재계산 후 |

---

## 권장 에셋 폴더 구조

```
Assets/
├── Data/
│   ├── Items/
│   │   ├── Weapons/
│   │   │   ├── IronSword.asset
│   │   │   └── SteelAxe.asset
│   │   ├── Armors/
│   │   │   ├── IronHelmet.asset
│   │   │   └── LeatherBoots.asset
│   │   └── Consumables/
│   │       ├── HPPotion.asset
│   │       └── MPPotion.asset
│   └── MainItemDatabase.asset
├── Scripts/
│   └── Inventory/
│       ├── Data/
│       ├── Logic/
│       └── UI/
└── Sprites/
    └── Items/
        ├── icon_iron_sword.png
        └── icon_hp_potion.png
```

---

## 빠른 테스트 스크립트

씬에 빈 GameObject를 만들고 이 스크립트를 붙이면 바로 테스트할 수 있습니다.

```csharp
using UnityEngine;
using Inventory.Data;
using Inventory.Logic;

public class InventoryTestRunner : MonoBehaviour
{
    [Header("테스트할 아이템 SO를 드래그하세요")]
    [SerializeField] private WeaponData testWeapon;
    [SerializeField] private ArmorData testHelmet;
    [SerializeField] private ConsumableData testPotion;

    void Start()
    {
        // 이벤트 로깅
        InventoryManager.Instance.OnInventoryChanged += () => Debug.Log("[이벤트] 인벤토리 변경됨");
        EquipmentManager.Instance.OnEquipmentChanged += () => Debug.Log("[이벤트] 장비 변경됨");
        PlayerStats.Instance.OnStatsChanged += () => Debug.Log($"[이벤트] 스탯 갱신 → ATK:{PlayerStats.Instance.TotalAttack} DEF:{PlayerStats.Instance.TotalDefense}");

        // 아이템 추가
        InventoryManager.Instance.AddItem(testWeapon, 1);
        InventoryManager.Instance.AddItem(testHelmet, 1);
        InventoryManager.Instance.AddItem(testPotion, 5);

        Debug.Log($"인벤토리 슬롯: {InventoryManager.Instance.UsedSlotCount}/{InventoryManager.Instance.MaxSlotCount}");

        // 무기 장착 (슬롯 0번)
        InventoryManager.Instance.UseItem(0);

        // 투구 장착 (슬롯 0번 — 무기가 빠졌으므로 투구가 0번)
        InventoryManager.Instance.UseItem(0);

        // 스탯 확인
        Debug.Log($"최종 스탯 → {PlayerStats.Instance.GetEquipmentBonus()}");

        // 포션 사용
        InventoryManager.Instance.UseItem(0);
        Debug.Log($"남은 포션: {InventoryManager.Instance.GetSlot(0)?.currentStack}");
    }
}
```

Console 출력 예시:
```
[이벤트] 인벤토리 변경됨
[이벤트] 인벤토리 변경됨
[이벤트] 인벤토리 변경됨
인벤토리 슬롯: 3/30
[이벤트] 장비 변경됨
[이벤트] 스탯 갱신 → ATK:25 DEF:5
[이벤트] 인벤토리 변경됨
[이벤트] 장비 변경됨
[이벤트] 스탯 갱신 → ATK:25 DEF:13
[이벤트] 인벤토리 변경됨
최종 스탯 → ATK +15 / DEF +8 / MRES +3
[이벤트] 인벤토리 변경됨
남은 포션: 4
```
