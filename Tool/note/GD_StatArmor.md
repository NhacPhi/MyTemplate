# 🛡️ GAME DESIGN DOCUMENT: HỆ THỐNG ARMOR & WEAPON BALANCING

> **Tài liệu Game Design (GD Document)**  
> **Chủ đề:** Phân tích, Định hướng Cân bằng Trang bị (Armor & Weapon) để Đánh Boss  
> **Thư mục lưu trữ:** `Tool/note/GD_StatArmor.md`

---

## 📋 1. TỔNG QUAN HỆ THỐNG TRANG BỊ (CURRENT OVERVIEW)

Hệ thống quản lý trang bị hiện tại được chia thành 2 phần chính với các thông số khung (Core Framework):

* **Vũ khí (Weapon Slot):** 1 Slot duy nhất (Phân lớp theo Class: *Fighter, Assassin, Mage, Tanker, Normal*).
  * Level tối đa: `100` | Cấp Đột phá (Ascension): `6`.
* **Bộ Giáp (Armor Slots):** 6 Món bao gồm:
  1. `Helmet` (Nón)
  2. `Chestplate` (Áo giáp)
  3. `Gloves` (Bao tay)
  4. `Boots` (Giày)
  5. `Belt` (Thắt lưng)
  6. `Ring` (Nhẫn)
  * Level tối đa: `15` | Substats tối đa: `4 dòng`.
  * Quy luật Substat: Nâng lên mỗi **3 level** (3, 6, 9, 12, 15) sẽ tự động mở khóa hoặc tăng ngẫu nhiên 1 dòng Substat.

---

## ⚖️ 2. TRIẾT LÝ PHÂN CHIA MAIN STAT (FIXED VS RANDOM GACHA)

Để tạo ra sự cân bằng giữa **Trải nghiệm nền tảng mượt mà (Floor)** và **Động lực cày cuốc dài hạn (Ceiling)**, hệ thống 6 món giáp được thiết kế theo mô hình **3 Cố Định - 3 Random**:

```
 ┌─────────────────────────────────────────────────────────────┐
 │                     HỆ THỐNG 6 MÓN TRANG BỊ                 │
 └──────────────────────────────┬──────────────────────────────┘
                                │
        ┌───────────────────────┴───────────────────────┐
        ▼                                               ▼
┌───────────────────────────────┐               ┌───────────────────────────────┐
│     NHÓM NỀN TẢNG (3 MÓN)     │               │     NHÓM ĐIỀU CHỈNH (3 MÓN)   │
│       (Fixed Main Stat)       │               │      (Random Main Stat)       │
├───────────────────────────────┤               ├───────────────────────────────┤
│ 1. Helmet (Nón)   -> Flat HP  │               │ 4. Boots (Giày)    -> Speed/% │
│ 2. Gloves (Tay)   -> Flat ATK │               │ 5. Ring (Nhẫn)     -> Crit/%  │
│ 3. Chest (Áo)     -> Flat DEF │               │ 6. Belt (Thắt lưng)-> Penet/% │
└───────────────────────────────┘               └───────────────────────────────┘
```

### 🔹 A. Nhóm 1: Fixed Main Stat (Cố định 100%) - Nón, Áo, Bao tay
* **Giúp tạo chỉ số nền tảng:** Đảm bảo người chơi khi đeo và up level 3 món này luôn có lượng HP, ATK, DEF cơ bản để vượt qua các nhiệm vụ cốt truyện mà không bị chặn kẹt quá sớm do thảm họa RNG.
  * **Helmet (Nón):** Cố định `Flat HP` (Ví dụ: $+500 \rightarrow +4,780$ HP).
  * **Gloves (Bao tay):** Cố định `Flat ATK` (Ví dụ: $+50 \rightarrow +311$ ATK).
  * **Chestplate (Áo giáp):** Cố định `Flat DEF` (Ví dụ: $+50 \rightarrow +300$ DEF).

### 🔹 B. Nhóm 2: Random Main Stat (Gacha / Cày cuốc) - Giày, Nhẫn, Thắt lưng
* **Tạo động lực cày cuốc đánh Boss:** Dành cho việc tối ưu hóa sức mạnh chuyên biệt cho từng Class nhân vật.
  * **Boots (Giày):** Pool: `[SPD (Speed), % ATK, % HP, % DEF]` *(Vị trí duy nhất có chỉ số Tốc độ Speed để giành lượt đánh)*.
  * **Ring (Nhẫn):** Pool: `[Crit Rate %, Crit DMG %, % ATK, % HP]` *(Vị trí duy nhất chứa Bạo kích dồn sát thương kết liễu Boss)*.
  * **Belt (Thắt lưng):** Pool: `[% ATK, % HP, % DEF, Armor Penetration %]` *(Cung cấp chỉ số Xuyên giáp hoặc Chống chịu cao cấp)*.

---

## 🧩 3. THIẾT KẾ CÂN BẰNG TOÀN BỘ 8 BỘ TRANG BỊ (8 ARMOR SETS MATRIX)

Hệ thống 8 Bộ Giáp được chuẩn hóa toàn diện theo các Class và vai trò chiến thuật trong game:

| Mã Set | Tên Bộ Giáp (VI / EN) | Phẩm Cấp | Phân Lớp Archetype | Kích Hoạt (6 Món) | Nhân Vật Phù Hợp |
| :---: | :--- | :---: | :--- | :--- | :--- |
| **Set 01**<br>`Armor01` | **Kim Vũ Long Lân Khải**<br>*Golden Dragon-Scale* | `Legendary` | 👑 **Đệ Nhất Đấu Sĩ (Fighter ATK)** | `+20.0% ATK` & `+15.0% CRIT_DMG` | Tôn Ngộ Không, Dương Tiễn |
| **Set 02**<br>`Armor02` | **Hồng Vân Tịnh Đới**<br>*Red Celestial Girdle* | `Rare` | 🪽 **Hỗ Trợ Tốc Độ (Support)** | `+8.0% SPEED` & `+10.0% HP` | Quan Âm Bồ Tát, Đông Hải Long Vương, Đường Tăng |
| **Set 03**<br>`Armor03` | **Quan Thiên Cương**<br>*Celestial Vanguard* | `Uncommon` | 🩸 **Tanker Bể Máu (Tanker HP)** | `+20.0% HP` | Trư Bát Giới, Sa Tăng, Lý Tịnh |
| **Set 04**<br>`Armor04` | **Viêm Ngưu Thần Khải**<br>*Inferno Minotaur Divine* | `Epic` | 🛡️ **Tanker Chống Chịu (Tanker HP/DEF)** | `+10.0% HP` & `+10.0% DEF` | Ngưu Ma Vương, Lý Tịnh |
| **Set 05**<br>`Armor05` | **Trọng Sơn Trấn Nhạc**<br>*Mountain-Subduing* | `Common` | ⚔️ **Tân Thủ Cơ Bản (Starter)** | `+8.0% ATK` & `+8.0% HP` | Mọi tướng giai đoạn đầu game |
| **Set 06**<br>`Armor06` | **Hoàng Kim Nữ Vương Giáp**<br>*Golden Empress* | `Epic` | 🔮 **Pháp Sư (Mage / Spell & Crit DMG)** | `+15.0% ATK` & `+15.0% CRIT_DMG` | Thiết Phiến Công Chúa, Đường Tam Tạng |
| **Set 07**<br>`Armor07` | **Huyết Chiến Vương Giáp**<br>*Blood-Battle Lord's Armor* | `Legendary` | 🗡️ **Sát Thủ Chí Mạng (Assassin)** | `+10.0% CRIT_RATE` & `+20.0% CRIT_DMG` | Na Tra, Lục Nhĩ Mỹ Hầu, The Boy Sage, Kim Giác |
| **Set 08**<br>`Armor08` | **Thanh Long Bố Lân Bào**<br>*Azure Dragon Scaled Robe* | `Legendary` | 🐉 **Đấu Sĩ Xuyên Phá (Fighter Pen)** | `+12.0% ATK` & `+10.0% PENETRATION` | Dương Tiễn, Tôn Ngộ Không, Tiểu Bạch Long |

---

## ⚔️ 4. PHÂN BỔ CHỈ SỐ VŨ KHÍ THEO CLASS (WEAPON STAT MATRIX)

Vũ khí bổ trợ trực tiếp cho vai trò của từng Class nhân vật trong trận chiến:

| Class Vũ Khí | Primary Stat (Tăng tiến chính) | Secondary Stat (Hỗ trợ) | Special Boss Passive (Nội tại ẩn/Kích hoạt) |
| :--- | :--- | :--- | :--- |
| **Fighter Weapon** | High ATK, Mid HP | +Crit Rate / Attack Speed | Mỗi đòn đánh tích dồn $+2\%$ ATK (Tối đa 10 tầng) khi đánh Boss |
| **Assassin Weapon** | Very High ATK, Low HP | +Crit DMG, +Armor Pen | Đòn đánh từ phía sau hoặc khi Boss tung chiêu được $+25\%$ DMG |
| **Tanker Weapon** | High HP, DEF | +Block Rate, +HP Regen | Kích hoạt khi HP $< 30\%$: Nhận lớp giáp bằng $20\%$ Max HP |
| **Mage Weapon** | High Magic ATK | +Energy Regen, +CDR | Chiêu thức gây thêm $3\%$ Max HP của Boss dưới dạng DoT |

---

## 📊 5. CÔNG THỨC VÀ BENCHMARK CÂN BẰNG BOSS (DEF MITIGATION)

### 🔹 A. Công thức giảm sát thương của DEF (Damage Mitigation)
Sử dụng công thức **Diminishing Returns** để tránh bị ngắt nghẽn hoặc phòng thủ quá bá đạo ($100\%$):

$$\text{Damage Reduction \%} = \frac{\text{DEF}}{\text{DEF} + K}$$
*(Trong đó $K$ là hằng số cân bằng, ví dụ $K = 500$ hoặc $1000$ tùy theo thang chỉ số endgame).*

### 🔹 B. Boss Encounter Benchmark (Tiêu chuẩn đồ khi gặp Boss)
* **Chuẩn bị đồ:** Nhân vật trang bị 6 món giáp Tím/Cam đạt mốc Level 10+.
* **Chỉ số sinh tồn:** Phải đỡ được tối thiểu **8-10 đòn đánh thường** của Boss hoặc sống sót qua **1 chiêu Ultimate diện rộng** của Boss mà không bị One-shot.

---

## 📝 6. ROADMAP THỰC HIỆN KẾ TIẾP (ACTION ITEMS)

- [ ] Cập nhật lại `SetBonusConfig.json` (Thêm các mốc 2 món, 4 món với chỉ số đa dạng).
- [ ] Cập nhật lại `SubstatPoolConfig.json` & Main Stat Pool theo đúng bảng phân bổ 3 Fixed - 3 Random.
- [ ] Cập nhật code C# `CharacterStatsBuilder.cs` để tính toán đầy đủ Weapon Stats + Armor Substats + Set Bonus vào nhân vật.
