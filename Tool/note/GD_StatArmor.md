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

## 🧩 3. THIẾT KẾ CẢI TIẾN SET BONUS (BỘ GIÁP)

Khắc phục hạn chế của hệ thống cũ (bắt buộc đủ 6 món mới được $+15\%$ ATK). Chuyển sang cơ chế mốc **2 món** và **4/6 món**:

| Bộ Giáp (Armor Set) | Định hướng Archetype | Kích hoạt 2 món (Bonus 2-pc) | Kích hoạt 4/6 món (Bonus 4/6-pc) | Tác dụng chiến thuật khi đánh Boss |
| :--- | :--- | :--- | :--- | :--- |
| **Guardian Set (Bảo Vệ)** | Tanker / Chống chịu | $+15\%$ DEF / $+10\%$ Max HP | Giảm $15\%$ Sát thương nhận từ Boss (Damage Reduction) | Nhân vật không bị Boss solo 1-hit chết |
| **Vanguard Set (Cuồng Phong)** | Fighter / Đấu sĩ | $+12\%$ ATK | $+10\%$ Lifesteal (Hút máu) | Có khả năng hồi phục duy trì nhịp đánh với Boss |
| **Shadow Set (Bóng Đêm)** | Assassin / Bạo kích | $+10\%$ Crit Rate | $+35\%$ Crit DMG & $+15\%$ Armor Pen | Đánh xuyên giáp bùng nổ damage kết liễu Boss |
| **Archmage Set (Pháp Sư)** | Mage / Xả chiêu | $+15\%$ Skill DMG | $-15\%$ Cooldown Reduction (CDR) | Xả skill liên tục ngắt nhịp tung chiêu của Boss |

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
