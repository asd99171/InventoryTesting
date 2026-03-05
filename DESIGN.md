# RPG 인벤토리 + 장비 시스템 설계

## 아키텍처 개요

```
┌─────────────────────────────────────────────────────┐
│                    UI Layer                          │
│  InventoryUI · EquipmentUI · TooltipUI · SlotUI     │
├─────────────────────────────────────────────────────┤
│                  Logic Layer                         │
│  InventoryManager · EquipmentManager                │
├─────────────────────────────────────────────────────┤
│                  Data Layer                          │
│  ScriptableObject 기반 아이템 정의                     │
│  ItemDatabase · ItemData · WeaponData · ArmorData …  │
└─────────────────────────────────────────────────────┘
```

---

## 1. Data Layer — ScriptableObject 기반 아이템 정의

### 1-1. 열거형 (Enums)

| 이름 | 값 | 설명 |
|------|-----|------|
| `ItemType` | Weapon, Armor, Consumable | 아이템 대분류 |
| `ArmorSlotType` | Head, Body, Legs, Hands, Feet | 방어구 부위 |
| `Rarity` | Common, Uncommon, Rare, Epic, Legendary | 등급(확장용) |

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
- **역할**: 무기 전용 데이터
- **추가 필드**
  - `attackPower` (int)
  - `attackSpeed` (float)
  - `weaponRange` (float)

#### `ArmorData` : ItemData
- **역할**: 방어구 전용 데이터
- **추가 필드**
  - `slotType` (ArmorSlotType) — 장착 부위
  - `defense` (int)
  - `magicResistance` (int)

#### `ConsumableData` : ItemData
- **역할**: 소비 아이템 전용 데이터
- **추가 필드**
  - `healAmount` (int)
  - `buffDuration` (float)
  - `cooldown` (float)

#### `ItemDatabase` (SO)
- **역할**: 모든 ItemData 에셋을 한 곳에서 참조·검색
- **주요 필드**
  - `items` (List\<ItemData\>)
- **주요 메서드**
  - `GetItemByID(string id) → ItemData`

---

## 2. 런타임 래퍼

#### `ItemStack`
- **역할**: 인벤토리 한 칸의 런타임 상태 (SO는 원본이므로 직접 수정 불가)
- **주요 필드**
  - `data` (ItemData) — 원본 SO 참조
  - `currentStack` (int) — 현재 수량
- **주요 메서드**
  - `AddStack(int amount) → int` (넘치면 잔여분 반환)
  - `RemoveStack(int amount) → bool`
  - `IsStackFull() → bool`

---

## 3. Logic Layer — 싱글톤 매니저

#### `InventoryManager` (싱글톤, MonoBehaviour)
- **역할**: 인벤토리 CRUD 및 비즈니스 로직 전담
- **주요 필드**
  - `slots` (List\<ItemStack\>) — 인벤토리 슬롯 배열
  - `maxSlotCount` (int)
- **주요 메서드**
  - `AddItem(ItemData item, int amount) → bool`
  - `RemoveItem(string itemID, int amount) → bool`
  - `UseItem(int slotIndex)` — 소비 아이템 사용 / 장비 장착 분기
  - `SwapSlots(int from, int to)`
  - `SortInventory()`
  - `HasItem(string itemID, int amount) → bool`
- **이벤트**
  - `OnInventoryChanged` (Action) — UI 갱신 트리거

#### `EquipmentManager` (싱글톤, MonoBehaviour)
- **역할**: 장비 슬롯 관리 및 스탯 반영
- **주요 필드**
  - `equippedSlots` (Dictionary\<ArmorSlotType, ItemStack\>) — 방어구 5부위
  - `equippedWeapon` (ItemStack) — 무기 슬롯
- **주요 메서드**
  - `Equip(ItemStack item) → ItemStack` (기존 장비 반환, null 가능)
  - `Unequip(ArmorSlotType slot) → ItemStack`
  - `UnequipWeapon() → ItemStack`
  - `GetTotalStats() → EquipmentStats` (합산 스탯 계산)
- **이벤트**
  - `OnEquipmentChanged` (Action) — UI 갱신 트리거

#### `EquipmentStats` (구조체)
- **역할**: 장비 합산 스탯을 담는 값 타입
- **필드**: `totalAttack`, `totalDefense`, `totalMagicResist` 등

---

## 4. UI Layer

#### `InventoryUI` (MonoBehaviour)
- **역할**: 인벤토리 패널 열기/닫기, 슬롯 목록 생성·갱신
- **동작**: `InventoryManager.OnInventoryChanged` 구독 → `RefreshSlots()`

#### `EquipmentUI` (MonoBehaviour)
- **역할**: 장비 창 패널, 6개 슬롯(무기+방어구5) 표시
- **동작**: `EquipmentManager.OnEquipmentChanged` 구독 → `RefreshSlots()`

#### `SlotUI` (MonoBehaviour)
- **역할**: 개별 슬롯 한 칸의 표현 (아이콘, 수량 텍스트, 클릭/드래그 처리)
- **주요 메서드**
  - `SetSlot(ItemStack stack)`
  - `OnClick()` — 사용/장착
  - `OnBeginDrag()` / `OnDrop()` — 슬롯 간 스왑

#### `TooltipUI` (MonoBehaviour)
- **역할**: 아이템 호버 시 이름/설명/스탯 팝업

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
                       ▼
                 ┌───────────┐       ┌────────────────┐
                 │ ItemStack │◄──────│ ItemDatabase   │
                 │ (런타임)   │  참조  │ (SO Registry)  │
                 └─────┬─────┘       └────────────────┘
                       │
          ┌────────────┴────────────┐
          ▼                         ▼
  ┌──────────────────┐   ┌───────────────────┐
  │InventoryManager  │   │EquipmentManager   │
  │  (싱글톤)         │──▶│  (싱글톤)          │
  │                  │   │                   │
  │ - slots[]        │   │ - equippedSlots{} │
  │ - AddItem()      │   │ - Equip()         │
  │ - RemoveItem()   │   │ - GetTotalStats() │
  └────────┬─────────┘   └────────┬──────────┘
           │ 이벤트                │ 이벤트
           ▼                      ▼
  ┌──────────────────┐   ┌───────────────────┐
  │  InventoryUI     │   │  EquipmentUI      │
  │  └─ SlotUI[]     │   │  └─ SlotUI[]      │
  └──────────────────┘   └───────────────────┘
           │                      │
           └──────────┬───────────┘
                      ▼
               ┌────────────┐
               │ TooltipUI  │
               └────────────┘
```

## 6. 핵심 데이터 흐름

### 아이템 획득
```
아이템 드롭 → InventoryManager.AddItem(data, amount)
           → 기존 스택 가능한 슬롯 탐색 → 빈 슬롯 할당
           → OnInventoryChanged 발행
           → InventoryUI.RefreshSlots()
```

### 장비 장착
```
SlotUI 클릭 → InventoryManager.UseItem(index)
           → 장비 아이템이면 EquipmentManager.Equip(stack)
           → 기존 장비 있으면 인벤토리로 반환
           → OnEquipmentChanged + OnInventoryChanged 발행
           → 양쪽 UI 갱신
```

### 소비 아이템 사용
```
SlotUI 클릭 → InventoryManager.UseItem(index)
           → ConsumableData라면 효과 적용 + 수량 차감
           → 수량 0이면 슬롯 비우기
           → OnInventoryChanged 발행
```

---

## 7. 확장 포인트

| 향후 기능 | 변경 지점 |
|-----------|-----------|
| 아이템 강화 | ItemStack에 `enhanceLevel` 필드 추가 |
| 세트 효과 | ArmorData에 `setID`, EquipmentManager에 세트 판정 로직 |
| 인벤토리 저장/로드 | ItemStack ↔ JSON 직렬화, SaveManager 연동 |
| 장신구 슬롯 | ArmorSlotType에 Ring/Necklace 추가 |
| 드래그 앤 드롭 | SlotUI에 IBeginDragHandler/IDropHandler 구현 |
