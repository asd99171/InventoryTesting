# RPG 인벤토리 + 장비 시스템 설계

## 설계 원칙

- **ScriptableObject 기반** — 아이템 원본 데이터는 SO 에셋으로 관리, 런타임 상태는 래퍼 클래스로 분리
- **Data / Logic / UI 3계층** — 각 레이어가 단방향으로만 의존 (UI → Logic → Data)
- **싱글톤 매니저** — InventoryManager, EquipmentManager를 전역 접근점으로 사용
- **이벤트 기반 UI 갱신** — Manager가 C# Action 이벤트를 발행하면 UI가 구독하여 갱신

## 아키텍처 개요

```
┌─────────────────────────────────────────────────────────┐
│                      UI Layer                           │
│  InventoryUI · EquipmentUI · TooltipUI · SlotUI         │
│  (Manager 이벤트 구독, 사용자 입력 → Manager 호출)         │
├─────────────────────────────────────────────────────────┤
│                    Logic Layer                           │
│  InventoryManager(싱글톤) · EquipmentManager(싱글톤)      │
│  (ItemStack 조작, 비즈니스 규칙, 이벤트 발행)               │
├─────────────────────────────────────────────────────────┤
│                    Data Layer                            │
│  ScriptableObject 기반 아이템 정의                        │
│  ItemDatabase · ItemData · WeaponData · ArmorData ·      │
│  ConsumableData · ItemStack(런타임 래퍼)                  │
└─────────────────────────────────────────────────────────┘
```

### 의존 방향

```
UI Layer ──참조──▶ Logic Layer ──참조──▶ Data Layer
   ▲                    │
   └── 이벤트 구독 ◀─────┘ (OnInventoryChanged / OnEquipmentChanged)
```

---

## 1. Data Layer — ScriptableObject 기반 아이템 정의

### 1-1. 열거형 (Enums)

| 이름 | 값 | 설명 |
|------|-----|------|
| `ItemType` | Weapon, Armor, Consumable | 아이템 대분류 |
| `ArmorSlotType` | Head, Body, Legs, Hands, Feet | 방어구 부위 (5부위) |
| `Rarity` | Common, Uncommon, Rare, Epic, Legendary | 등급 (확장용) |

### 1-2. ScriptableObject 클래스

#### `ItemData` (추상 SO, 모든 아이템의 베이스)
- **역할**: 아이템 공통 필드를 정의하는 최상위 데이터 클래스
- **주요 필드**
  - `itemID` (string) — 고유 식별자
  - `itemName` (string) — 표시 이름
  - `icon` (Sprite) — 인벤토리 아이콘
  - `description` (string) — 설명 텍스트
  - `itemType` (ItemType) — 대분류
  - `rarity` (Rarity) — 등급
  - `maxStackCount` (int) — 최대 중첩 수 (장비류는 1 고정)
  - `sellPrice` (int) — 판매 가격

#### `WeaponData` : ItemData
- **역할**: 무기 전용 스탯 데이터
- **추가 필드**
  - `attackPower` (int)
  - `attackSpeed` (float)
  - `weaponRange` (float)

#### `ArmorData` : ItemData
- **역할**: 방어구 전용 스탯 데이터
- **추가 필드**
  - `slotType` (ArmorSlotType) — 장착 부위 (Head/Body/Legs/Hands/Feet)
  - `defense` (int)
  - `magicResistance` (int)

#### `ConsumableData` : ItemData
- **역할**: 소비 아이템 전용 데이터
- **추가 필드**
  - `healAmount` (int)
  - `buffDuration` (float)
  - `cooldown` (float)

#### `ItemDatabase` (SO)
- **역할**: 프로젝트 내 모든 ItemData SO 에셋을 한 곳에서 참조·검색하는 레지스트리
- **주요 필드**
  - `items` (List\<ItemData\>)
- **주요 메서드**
  - `GetItemByID(string id) → ItemData`

---

## 2. 런타임 래퍼

#### `ItemStack` (일반 C# 클래스)
- **역할**: 인벤토리 한 칸의 런타임 상태를 표현 (SO 원본은 불변이므로 별도 래퍼 필요)
- **주요 필드**
  - `data` (ItemData) — 원본 SO 참조
  - `currentStack` (int) — 현재 수량
- **주요 메서드**
  - `AddStack(int amount) → int` — 넘치면 잔여분 반환
  - `RemoveStack(int amount) → bool` — 수량 부족 시 false
  - `IsStackFull() → bool`

---

## 3. Logic Layer — 싱글톤 매니저

### 싱글톤 베이스

#### `SingletonMono<T>` (제네릭 MonoBehaviour)
- **역할**: 싱글톤 보일러플레이트 제거. `DontDestroyOnLoad` 처리 포함
- **사용**: `InventoryManager : SingletonMono<InventoryManager>`

### 매니저 클래스

#### `InventoryManager` (싱글톤, MonoBehaviour)
- **역할**: 인벤토리 CRUD 및 비즈니스 로직 전담
- **주요 필드**
  - `slots` (List\<ItemStack\>) — 인벤토리 슬롯 배열
  - `maxSlotCount` (int) — 최대 슬롯 수
- **주요 메서드**
  - `AddItem(ItemData item, int amount) → bool` — 스택 가능 슬롯 우선 탐색 → 빈 슬롯 할당
  - `RemoveItem(string itemID, int amount) → bool`
  - `UseItem(int slotIndex)` — 소비 아이템 → 효과 적용, 장비 아이템 → EquipmentManager.Equip 호출
  - `SwapSlots(int from, int to)` — 드래그 앤 드롭용
  - `SortInventory()` — ItemType/Rarity 기준 정렬
  - `HasItem(string itemID, int amount) → bool`
- **이벤트**
  - `OnInventoryChanged` (Action) — 슬롯 변경 시 UI 갱신 트리거

#### `EquipmentManager` (싱글톤, MonoBehaviour)
- **역할**: 장비 슬롯 관리 및 장비 합산 스탯 계산
- **주요 필드**
  - `equippedArmors` (Dictionary\<ArmorSlotType, ItemStack\>) — 방어구 5부위
  - `equippedWeapon` (ItemStack) — 무기 슬롯
- **주요 메서드**
  - `Equip(ItemStack item) → ItemStack` — 장착, 기존 장비 있으면 반환 (null 가능)
  - `Unequip(ArmorSlotType slot) → ItemStack` — 방어구 해제
  - `UnequipWeapon() → ItemStack` — 무기 해제
  - `GetTotalStats() → EquipmentStats` — 전 슬롯 합산 스탯
- **이벤트**
  - `OnEquipmentChanged` (Action) — 장비 변경 시 UI 갱신 트리거

#### `EquipmentStats` (구조체)
- **역할**: 장비 합산 스탯을 담는 값 타입
- **필드**
  - `totalAttack` (int)
  - `totalDefense` (int)
  - `totalMagicResist` (int)

---

## 4. UI Layer

#### `InventoryUI` (MonoBehaviour)
- **역할**: 인벤토리 패널 열기/닫기, SlotUI 풀 생성·갱신
- **동작**: `InventoryManager.OnInventoryChanged` 구독 → `RefreshSlots()`
- **참조**: SlotUI 프리팹, ScrollView 컨테이너

#### `EquipmentUI` (MonoBehaviour)
- **역할**: 장비 창 패널, 6개 고정 슬롯 (무기 1 + 방어구 5) 표시
- **동작**: `EquipmentManager.OnEquipmentChanged` 구독 → `RefreshSlots()`

#### `SlotUI` (MonoBehaviour)
- **역할**: 개별 슬롯 한 칸의 시각적 표현 및 입력 처리
- **주요 컴포넌트**: Image(아이콘), Text(수량), Button(클릭)
- **주요 메서드**
  - `SetSlot(ItemStack stack)` — 아이콘·수량 갱신
  - `ClearSlot()` — 빈 슬롯 표시
  - `OnClick()` — InventoryManager.UseItem 호출
  - `OnBeginDrag()` / `OnDrop()` — 슬롯 간 스왑 (IBeginDragHandler, IDropHandler)
  - `OnPointerEnter()` / `OnPointerExit()` — 툴팁 표시/숨김

#### `TooltipUI` (MonoBehaviour)
- **역할**: 아이템 호버 시 이름/설명/스탯 팝업
- **주요 메서드**
  - `Show(ItemData data, Vector2 position)`
  - `Hide()`

---

## 5. 클래스 관계도

```
                          ┌──────────────┐
                          │   ItemData   │  (abstract SO)
                          └──────┬───────┘
                 ┌───────────────┼───────────────┐
                 ▼               ▼               ▼
          ┌────────────┐  ┌────────────┐  ┌────────────────┐
          │ WeaponData │  │ ArmorData  │  │ ConsumableData │
          └────────────┘  └────────────┘  └────────────────┘
                 │               │               │
                 └───────┬───────┘───────────────┘
                         │ SO 참조
                         ▼
                   ┌───────────┐         ┌────────────────┐
                   │ ItemStack │◄────────│ ItemDatabase   │
                   │ (런타임)   │  ID검색  │ (SO Registry)  │
                   └─────┬─────┘         └────────────────┘
                         │
            ┌────────────┴────────────┐
            │ 소유                     │ 소유
            ▼                         ▼
  ┌──────────────────┐     ┌───────────────────┐
  │InventoryManager  │     │EquipmentManager   │
  │  (싱글톤)         │────▶│  (싱글톤)          │
  │                  │장착시│                   │
  │ - slots[]        │호출  │ - equippedArmors{}│
  │ - AddItem()      │     │ - equippedWeapon  │
  │ - RemoveItem()   │     │ - Equip()         │
  │ - UseItem()      │     │ - GetTotalStats() │
  └────────┬─────────┘     └────────┬──────────┘
           │                        │
           │ OnInventoryChanged     │ OnEquipmentChanged
           │ (C# Action 이벤트)      │ (C# Action 이벤트)
           ▼                        ▼
  ┌──────────────────┐     ┌───────────────────┐
  │  InventoryUI     │     │  EquipmentUI      │
  │  └─ SlotUI[]     │     │  └─ SlotUI[]      │
  └──────────────────┘     └───────────────────┘
           │                        │
           └──────────┬─────────────┘
                      │ 호버 시 호출
                      ▼
               ┌────────────┐
               │ TooltipUI  │
               └────────────┘

  ※ 싱글톤 공통 베이스:  SingletonMono<T> : MonoBehaviour
```

---

## 6. 핵심 데이터 흐름

### 아이템 획득
```
외부(드롭/보상 등) → InventoryManager.AddItem(data, amount)
                  → 동일 아이템 스택 가능 슬롯 탐색
                  → 없으면 빈 슬롯에 새 ItemStack 생성
                  → OnInventoryChanged 발행
                  → InventoryUI.RefreshSlots()
```

### 장비 장착
```
SlotUI 클릭 → InventoryManager.UseItem(slotIndex)
           → ItemType이 Weapon/Armor이면:
             → EquipmentManager.Equip(stack)
             → 기존 장비 있으면 인벤토리로 반환 (AddItem)
             → 인벤토리에서 해당 슬롯 제거
           → OnEquipmentChanged + OnInventoryChanged 발행
           → EquipmentUI + InventoryUI 양쪽 갱신
```

### 장비 해제
```
EquipmentUI의 SlotUI 클릭 → EquipmentManager.Unequip(slot)
                          → 반환된 ItemStack을 InventoryManager.AddItem
                          → 인벤토리 가득 차면 해제 실패
                          → 양쪽 이벤트 발행 → UI 갱신
```

### 소비 아이템 사용
```
SlotUI 클릭 → InventoryManager.UseItem(slotIndex)
           → ItemType이 Consumable이면:
             → 효과 적용 (힐/버프 등)
             → currentStack 차감
             → 수량 0이면 슬롯 비우기
           → OnInventoryChanged 발행
```

---

## 7. 권장 폴더 구조

```
Assets/
└── Scripts/
    └── Inventory/
        ├── Data/
        │   ├── Enums.cs              (ItemType, ArmorSlotType, Rarity)
        │   ├── ItemData.cs           (abstract SO)
        │   ├── WeaponData.cs
        │   ├── ArmorData.cs
        │   ├── ConsumableData.cs
        │   ├── ItemDatabase.cs       (SO Registry)
        │   └── ItemStack.cs          (런타임 래퍼)
        ├── Logic/
        │   ├── SingletonMono.cs      (제네릭 싱글톤 베이스)
        │   ├── InventoryManager.cs
        │   ├── EquipmentManager.cs
        │   └── EquipmentStats.cs
        └── UI/
            ├── InventoryUI.cs
            ├── EquipmentUI.cs
            ├── SlotUI.cs
            └── TooltipUI.cs
```

---

## 8. 클래스 요약표

| # | 클래스 | 레이어 | 타입 | 역할 |
|---|--------|--------|------|------|
| 1 | `ItemData` | Data | abstract SO | 모든 아이템 공통 필드 정의 |
| 2 | `WeaponData` | Data | SO (상속) | 무기 전용 스탯 (공격력, 공속, 사거리) |
| 3 | `ArmorData` | Data | SO (상속) | 방어구 전용 스탯 (부위, 방어력, 마저) |
| 4 | `ConsumableData` | Data | SO (상속) | 소비 아이템 전용 (힐량, 버프, 쿨다운) |
| 5 | `ItemDatabase` | Data | SO | 전체 아이템 레지스트리, ID로 검색 |
| 6 | `ItemStack` | Data | C# class | 슬롯 1칸의 런타임 상태 (SO 참조 + 수량) |
| 7 | `SingletonMono<T>` | Logic | MonoBehaviour | 싱글톤 보일러플레이트 제거 |
| 8 | `InventoryManager` | Logic | 싱글톤 MB | 인벤토리 CRUD, 아이템 사용/장착 분기 |
| 9 | `EquipmentManager` | Logic | 싱글톤 MB | 장비 슬롯 관리, 합산 스탯 계산 |
| 10 | `EquipmentStats` | Logic | struct | 장비 합산 스탯 값 타입 |
| 11 | `InventoryUI` | UI | MonoBehaviour | 인벤토리 패널 표시/갱신 |
| 12 | `EquipmentUI` | UI | MonoBehaviour | 장비 창 패널 표시/갱신 |
| 13 | `SlotUI` | UI | MonoBehaviour | 슬롯 1칸 (아이콘, 수량, 클릭/드래그) |
| 14 | `TooltipUI` | UI | MonoBehaviour | 아이템 툴팁 팝업 |

---

## 9. 확장 포인트

| 향후 기능 | 변경 지점 |
|-----------|-----------|
| 아이템 강화 | ItemStack에 `enhanceLevel` 필드 추가 |
| 세트 효과 | ArmorData에 `setID`, EquipmentManager에 세트 판정 로직 |
| 인벤토리 저장/로드 | ItemStack ↔ JSON 직렬화, SaveManager 연동 |
| 장신구 슬롯 | ArmorSlotType에 Ring/Necklace 추가 |
| 드래그 앤 드롭 | SlotUI에 IBeginDragHandler/IDropHandler 구현 |
| NPC 상점 | ShopManager 추가, InventoryManager의 AddItem/RemoveItem 재활용 |
