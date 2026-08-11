# 🎮 Tài Liệu Thiết Kế Game & Cân Bằng Hệ Thống (Game Design & Balance Document)

> **Dự án**: Turn-Based RPG Hero Collector (MyTemplate)  
> **Ngày cập nhật**: 11/08/2026  
> **Tác giả**: Chuyên gia Cân bằng Game & AI Assistant  

---

## 📌 1. Tổng Quan Hệ Thống Chỉ Số Nhân Vật (Stats System)

Game sử dụng hệ thống 9 chỉ số chính được phân chia thành 2 nhóm rõ rệt:

### A. Chỉ Số Cơ Bản (Base Stats)
*Tăng trưởng theo Level nhân vật, Đột phá (Ascension) và Nâng sao (StarUp).*
* **`hp` (Max Health)**: Lượng máu tối đa của nhân vật.
* **`atk` (Attack)**: Sát thương tấn công cơ bản.
* **`def` (Defense)**: Giáp giảm sát thương nhận vào.

### B. Chỉ Số Nâng Cao / Chỉ Số Phụ (Secondary Stats)
*Chỉ số gốc phụ thuộc vào Chất tướng (Role/Rarity). Phần lớn chỉ số tăng thêm đến từ Trang bị (Armor/Gear), Đồ ăn (Food Buff) và Kỹ năng bị động (Passives).*
* **`speed` (Speed)**: Tốc độ quyết định thứ tự lượt đánh (Turn Order).
* **`crit_rate` (Critical Rate)**: Tỉ lệ chí mạng (%).
* **`crit_dmg` (Critical Damage)**: Sát thương chí mạng cộng thêm (% trên nền 175% Base Crit Damage).
* **`def_shred` (Sát Lực / Def Shred)**: Giảm giáp phẳng (Trừ thẳng điểm giáp của mục tiêu).
* **`penetration` (Xuyên Giáp % / Penetration)**: Bỏ qua giáp theo phần trăm (% giáp của mục tiêu).
* **`crit_dmg_res` (Kháng ST Chí Mạng / Crit Dmg Resistance)**: Giảm trừ phần trăm sát thương bạo kích nhận vào.

---

## ⚔️ 2. Quy Tắc & Công Thức Tính Sát Thương Chuẩn (Damage Formulas)

### A. Công Thức Giáp Thực Tế (Effective Defense Formula)
Quy tắc bắt buộc: **Xuyên giáp % (`PENETRATION`) được tính TRƯỚC, Sát lực phẳng (`DEF_SHRED`) được tính SAU CÙNG**.

$$DEF_{after\_pen} = DEF_{target} \times \left(1 - \frac{PENETRATION_{attacker}}{100}\right)$$

$$DEF_{effective} = \max\left(0, \, DEF_{after\_pen} - DEF\_SHRED_{attacker}\right)$$

$$\text{Hệ số Giảm ST do Giáp} = \frac{400}{400 + DEF_{effective}}$$

$$\text{Damage}_{after\_def} = \text{Damage}_{raw} \times \text{Hệ số Giảm ST do Giáp}$$

### C. Quy Tắc Tăng Trưởng Chỉ Số Khi Nâng Level (`CharacterUpgrade`)
*Chỉ số tăng trưởng mỗi Cấp (`hp`, `atk`, `def`) được điều chỉnh về mốc mượt màng chuẩn RPG (Tăng x8.4 lần ở Level 100):*

$$\text{StatGrowth}(\text{level}) = \text{Upgrade\_Stat} \times (\text{level} - 1) \times \left(1.0 + 0.005 \times (\text{level} - 1)\right)$$

$$\text{Upgrade\_Stat} = \text{ROUND}(\text{Base\_Stat} \times 5\%, \, 0)$$

- **Tác dụng**: Giữ cho dải chỉ số ở Level 100 luôn gọn gàng, đẹp mắt, triệt tiêu lạm phát con số và nâng tầm giá trị cho hệ thống **Trang bị (Armor/Gear)** lên mức tối đa ở End-game!

---

### B. Công Thức Sát Thương Chí Mạng & Kháng Chí Mạng
Quy tắc: **Kháng ST Chí Mạng (`CRIT_DMG_RES`) giảm trừ trực tiếp % sát thương bạo kích nhận vào**.

$$\text{CritMultiplier} = \frac{175\% + CRIT\_DMG_{attacker}}{100}$$

$$\text{CritResMultiplier} = \max\left(0, \, 1 - \frac{CRIT\_DMG\_RES_{defender}}{100}\right)$$

$$\text{Damage}_{critical} = \text{Damage}_{raw} \times \text{CritMultiplier} \times \text{CritResMultiplier}$$

---

## 📊 3. Ma Trận Cân Bằng Tướng Theo Phẩm Chất & Vai Trò

### A. Hệ Số Phẩm Chất (Rarity Multipliers)
Phẩm chất quy định Tổng ngân sách chỉ số cơ bản (Base Stat Budget) ở Cấp 1:
* **R (Rare)**: `1.0x` (100% Base Budget)
* **SR (Super Rare)**: `1.2x` (+20% Stats)
* **SSR (Super Super Rare)**: `1.45x` (+45% Stats)
* **UR (Ultra Rare)**: `1.7x` (+70% Stats)

---

### B. Định Hướng Chỉ Số Theo Vai Trò (Role Archetypes)

| Vai Trò | HP Ratio | ATK Ratio | DEF Ratio | SPD Gốc | Chỉ Số Phụ Nổi Bật |
| :--- | :---: | :---: | :---: | :---: | :--- |
| 🛡️ **Tanker** | **160%** | **60%** | **150%** | 80 - 86 | **`crit_dmg_res`** (Kháng chí mạng: 5% - 11%) |
| ⚔️ **Fighter** | **110%** | **110%** | **110%** | 90 - 96 | **`crit_rate`** (5%-8%), **`def_shred`** (5-11 điểm), **`penetration`** (2%-5%) |
| 🗡️ **Assassin** | **80%** | **150%** | **80%** | 105 - 114 | **`crit_rate`** (10%-14%), **`crit_dmg`** (15%-21%), **`def_shred`** (10-16 điểm), **`penetration`** (5%-9%) |
| 🔮 **Mage** | **90%** | **140%** | **85%** | 95 - 101 | **`penetration`** (5%-11%), **`crit_dmg`** (10%-19%) |
| 🩺 **Support** | **120%** | **70%** | **100%** | 100 - 109 | **`speed`** cao, **`crit_dmg_res`** (3%-9%) |
| 🏹 **ADCarry** | **88%** | **146%** | **88%** | 100 - 109 | **`crit_rate`** (8%-12%), **`crit_dmg`** (10%-16%), **`penetration`** (4%-7%) |

---

### C. Phân Loại Entity Theo Class (`Character`, `Creep`, `Boss`)
Phân loại Class quyết định vai trò thực tế của Entity trên chiến trường và điều chỉnh quy mô chỉ số:

| Class | HP Multiplier | ATK Multiplier | DEF Multiplier | Điều Chỉnh Chỉ Số Phụ & Đặc Điểm Chiến Thuật |
| :--- | :---: | :---: | :---: | :--- |
| 🧑‍💻 **`Character`** | **1.0x** (100%) | **1.0x** (100%) | **1.0x** (100%) | Đội hình 3-4 Tướng của người chơi. Giữ nguyên chỉ số chuẩn. |
| 👾 **`Creep`** | **0.5x** (50%) | **0.7x** (70%) | **0.6x** (60%) | Quái nhỏ / Quái thường. Máu & Dame thấp để làm bao cát tích Ulti. |
| 👑 **`Boss`** | **1.8x** (180%) | **1.2x** (120%) | **1.2x** (120%) | Trùm màn chơi (Không thể mặc đồ). **HP chỉ nhỉnh hơn Tướng 1.8 lần**, giáp + **`crit_dmg_res` +10%**. |

---

### D. Hệ Thống Đa Dạng Hóa Tướng Cùng Role (`Stat_Bias`)
Cột **`Stat_Bias`** cho phép Designer thiết lập phong cách cá thể cho từng tướng:

| Phong Cách (`Stat_Bias`) | HP Mult | ATK Mult | DEF Mult | SPD Add | Chỉ Số Phụ Tăng Thêm | Định Hướng Chiến Thuật |
| :--- | :---: | :---: | :---: | :---: | :--- | :--- |
| **`Balanced`** | `1.00` | `1.00` | `1.00` | `0` | Giữ nguyên baseline. | Tướng toàn diện, cân bằng. |
| ⚔️ **`Offensive`** | `0.95` | **`1.10`** | `0.95` | `0` | **+2% CR, +8% CD, +5 Sát lực** | Thiên Công: Sát thương dồn cực sốc. |
| 🛡️ **`Defensive`** | **`1.15`** | `0.90` | **`1.10`** | `0` | **+5% Kháng chí mạng (`res`)** | Thiên Thủ: Trâu bò, chống dồn dame. |
| ⚡ **`Swift`** | `0.95` | `0.95` | `0.95` | **`+8`** | **+5% Crit Rate (`cr`)** | Thiên Tốc: Ra chiêu trước, cơ động. |
| 🔨 **`Heavy`** | `1.00` | `1.05` | `1.05` | `-5` | **+15 Sát lực (`def_shred`)** | Sát Lực: Chuyên đấm thấu giáp trâu. |

---

## 🛠️ 4. Tự Động Hóa Dữ Liệu Excel & Tooling Pipeline

### A. Cấu Trúc File Excel (`Tool/data/GameConfig.xlsx`)
1. **Sheet `Stat_Matrix`**: 
   - Lưu trữ Bảng tra cứu Multiplier Phẩm chất, Bảng chỉ số cơ sở của Vai trò, Bảng gia tăng chỉ số phụ theo Rarity Tier, Bảng hệ số Class và Bảng phong cách `Stat_Bias`.
2. **Sheet `CharacterStat` & `CharacterUpgrade` (Công thức tự động)**:
   - Tất cả các ô chỉ số đều được đánh công thức `=VLOOKUP(...)` và `=ROUND(...)` tự động.
   - Khi thêm Tướng mới, chỉ cần gõ `ID`, chọn `Rare`, `Type`, `Class`, và `Stat_Bias`, Excel sẽ tự động tính ra toàn bộ chỉ số.

### B. Python Builder (`Tool/src/builders/character_builder.py`)
- Đọc tự động toàn bộ chỉ số và xuất dữ liệu ra file `Assets/Data/GameConfig/CharacterConfig.json` theo đúng định dạng `lowercase` chuẩn hóa.

---

## 💻 5. Hệ Thống Code Engine Unity (C# Updates)

1. **`DamageFormular.cs`**:
   - Áp dụng hằng số giáp mới `DEF_CONSTANT = 400f`.
   - Công thức: `damageResult * (400f / (400f + effectiveDef))`.

2. **`StatType.cs`**:
   - Khai báo chuẩn hóa duy nhất Enum member: `PENETRATION`.

3. **`StatsController.cs`**:
   - Hàm `InitStats()` duyệt qua toàn bộ giá trị trong `StatType` Enum để khởi tạo đầy đủ 9 chỉ số cho mọi Entity.

4. **`CharacterCardInfo.cs`**:
   - Cập nhật UI hiển thị chỉ số động cho `txtDEFShred`, `txtPenetration`, và `txtCritDGMRes` thông qua `GetStatText(...)`.

5. **`EnemyProfileModel.cs`**:
   - Cập nhật hàm `GetBaseStat` và `GetTotalStat` sử dụng công thức tăng trưởng mượt màng `Utility.GetStatGrowthLevel(_level, BaseConfig.GetUpdateStat(type))`.
   - Giúp toàn bộ Quái nhỏ (Creep) và Boss trong từng Màn chơi (Stage) tăng trưởng chỉ số chính xác 100% theo Level được khai báo trong sheet `StageEnemies`!

6. **`HealthBar.cs`**:
   - Tích hợp **Thuật toán Tự Động Chia Mốc Đẹp (`GetNiceRoundNumber`) & Mục Tiêu 6 Vạch Lớn Cố Định**:
     - Loại bỏ hoàn toàn hằng số cứng `TICK_UNIT = 2000f`.
     - Dù Máu MaxHP là **3,000 HP** hay **600,000 HP** hay **10,000,000 HP**, thanh máu **LUÔN LUÔN hiển thị đúng 6 Vạch Lớn** (full height) và **3 Vạch Nhỏ** (50% height) giữa mỗi vạch lớn.
     - **Triệt tiêu 100% hiện tượng thanh máu đen kịt** do nhồi hàng trăm vạch ở Boss cấp cao!
