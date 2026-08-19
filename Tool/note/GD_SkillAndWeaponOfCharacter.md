# 📜 TÀI LIỆU THIẾT KẾ GAME (GAME DESIGN DOCUMENT)
## HỆ THỐNG KỸ NĂNG NHÂN VẬT & VŨ KHÍ TRẤN PHÁI (GD_SkillAndWeaponOfCharacter)

---

## 🎯 1. NGUYÊN TẮC THIẾT KẾ CỐT LÕI (CORE DESIGN PILLARS)

Hệ thống nhân vật và chiến đấu của trò chơi được xây dựng dựa trên **2 Trụ cột cơ bản**:
1. **Tỷ lệ % Phụ thuộc vào Chỉ số Gốc (Zero-Flat Scaling):**
   - Toàn bộ kỹ năng, đòn đánh và hồi phục đều tính hoàn toàn dựa trên % chỉ số gốc của nhân vật (ví dụ: gây sát thương bằng $140\%$ ATK, hồi máu bằng $15\%$ HP, tạo khiên bằng $20\%$ DEF).
   - **Đã loại bỏ hoàn toàn sát thương phẳng (Flat Damage)** để tránh phá vỡ cân bằng game khi nâng cấp cấp độ và trang bị.
2. **Hệ Thống Nội Tại Kỹ Năng & Vũ Khí Trấn Phái (Skill & Signature Weapon Passives):**
   - Mỗi nhân vật có bản sắc riêng thông qua Nội tại Kỹ năng (Passive Skill).
   - Mỗi món vũ khí đóng vai trò là "Vũ Khí Trấn Phái" bổ trợ đúng điểm mạnh cốt lõi của nhân vật tương ứng.

---

## ⚔️ 2. DANH SÁCH CHI TIẾT TƯỚNG & KỸ NĂNG (CHARACTERS & SKILLS)

---

### 🐒 1. TÔN NGỘ KHÔNG (SunWukong)
* **Phẩm chất:** `UR` | **Hệ/Vai trò:** `Fighter` (Đấu Sĩ Càn Quét & Sinh Tồn)
* **Vũ khí đặc trưng:** **Gậy Như Ý (`Nimbus_Cudgel`)**
* **Chỉ số cơ bản (Level 1 Base):**
  - **HP:** `7,013` | **ATK:** `2,805` | **DEF:** `561` | **Speed:** `96`
  - **Tỷ lệ Bạo kích:** `8%` | **ST Bạo kích:** `14%` | **Xuyên Giáp:** `5%` | **Phá Giáp Phẳng:** `11`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`SunWukong_B` - Như Ý Bổng Kích):**
   - **Loại:** `MeleeAttack` | **Mục tiêu:** `SingleEnemy` (Đơn kẻ địch)
   - **Hệ số ST:** `[80%, 90%, 100%]` ATK | **Hồi chiêu:** `0` lượt.
2. **Kỹ Năng Major (`SunWukong_M` - Thiên Bổng Loạn Vũ):**
   - **Loại:** `EmpoweredAttack` | **Mục tiêu:** `AllEnemies` (Toàn bộ đội hình địch)
   - **Hệ số ST:** `[60%, 75%, 90%]` ATK | **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Nội tại (`psv_sunwukong_major`):** Sau khi gây sát thương cho toàn đội địch, **Hồi máu (Hút máu) tương đương `[20%, 25%, 30%]`** tổng lượng sát thương vừa gây ra.
3. **Tuyệt Kỹ Ultimate (`SunWukong_U` - Khai Thiên Bổng):**
   - **Loại:** `EmpoweredAttack` | **Mục tiêu:** `SingleEnemy` (Sốc ST đơn mục tiêu)
   - **Hệ số ST:** `[180%, 200%, 220%]` ATK | **Hồi chiêu:** `[5, 5, 4]` lượt.
   - **Nội tại (`psv_sunwukong_ultiamte`):** Đòn đánh có khả năng **Phá Giáp (DEF Shred) mục tiêu `[15%, 20%, 25%]`**.

---

### 👁️ 2. DƯƠNG TIỄN (ErlangShen - Nhị Lang Thần)
* **Phẩm chất:** `UR` | **Hệ/Vai trò:** `Fighter` (Đấu Sĩ Sát Thủ Solo 1 vs 1 & Chí Mạng Khủng)
* **Vũ khí đặc trưng:** **Tam Tiêm Đao (`Triple_Edged_Blade`)**
* **Chỉ số cơ bản (Level 1 Base):**
  - **HP:** `7,013` | **ATK:** `2,805` | **DEF:** `561` | **Speed:** `96`
  - **Tỷ lệ Bạo kích:** `8%` | **ST Bạo kích:** `14%` | **Xuyên Giáp:** `5%` | **Phá Giáp Phẳng:** `11`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`ErlangShen_B` - Tam Tiêm Đao):**
   - **Loại:** `MeleeAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[80%, 90%, 100%]` ATK | **Hồi chiêu:** `0` lượt.
2. **Kỹ Năng Major (`ErlangShen_M` - Khai Thiên Nhãn / Phóng Thần Quang):**
   - **Loại:** `MajorAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[120%, 140%, 160%]` ATK | **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Nội tại Siêu Bạo Kích (`psv_erlangshen_major`):** Tăng thêm **`[+60%, +80%, +100%]` Sát Thương Bạo Kích (Crit DMG)** cho đòn đánh này. *(Khi build 100% Crit, đòn này nổ ra $\approx 370\%$ ATK!)*
3. **Tuyệt Kỹ Ultimate (`ErlangShen_U` - Tam Nhẫn Trảm Không / MoonBlade):**
   - **Loại:** `MoonBladeSkill` | **Mục tiêu:** `SingleEnemy` (Chuyển mục tiêu nếu kẻ địch chết)
   - **Cơ chế:** Tung liên tiếp **3 nhát chém**. Mỗi nhát gây **`[50%, 60%, 70%]`** ATK (Tổng $150\% \to 210\%$ ATK).
   - **Hiệu ứng đặc biệt:** Kỹ năng có sẵn **`30% Xuyên Giáp (Penetration)`**. Nếu mục tiêu chết trước khi hết 3 hit, các nhát chém còn lại tự động bay sang kẻ địch kế bên. Hồi chiêu: `[5, 5, 4]` lượt.

---

### 🪷 3. QUAN ÂM BỒ TÁT (GuanyinBodhisattva)
* **Phẩm chất:** `UR` | **Hệ/Vai trò:** `Mage / Support` (Đại Lượng Hồi Phục & Hóa Giải Hiệu Ứng)
* **Vũ khí đặc trưng:** **Phù Loan (`Wings_of_Phoenix`)**
* **Chỉ số cơ bản:** **HP:** `7,013` | **ATK:** `2,805` | **DEF:** `561` | **Speed:** `96`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`GuanyinBodhisattva_B` - Liễu Chi Phất):** Gây `[80%, 100%, 120%]` ATK cho 1 kẻ địch.
2. **Kỹ Năng Major (`GuanyinBodhisattva_M` - Cam Lồ Phổ Độ):** Hồi phục cho toàn đội lượng máu tương đương `[10%, 15%, 15%]` HP tối đa của bản thân. Hồi chiêu: `[4, 4, 3]`.
3. **Tuyệt Kỹ Ultimate (`GuanyinBodhisattva_U` - Tịnh Bình Hộ Thể):** Hồi máu toàn đội `[15%, 18%, 18%]` HP tối đa, đồng thời ban cho toàn đội lớp Khiên Hộ Thể. Hồi chiêu: `[5, 5, 4]`.

---

### 🔥 4. HỒNG HÀI NHI (TheBoySage)
* **Phẩm chất:** `UR` | **Hệ/Vai trò:** `Assassin / Mage` (Hỏa Diệm Sơn - Tam Muội Chân Hỏa Thiêu Đốt)
* **Vũ khí đặc trưng:** **Võng Sinh (`Reincarnation`)** / **Phong Hỏa Luân (`Windfire_Wheel`)**
* **Chỉ số cơ bản:** **HP:** `4,570` | **ATK:** `3,812` | **DEF:** `366` | **Speed:** `111` | **Crit Rate:** `16%`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`TheBoySage_B` - Hỏa Xung):** Gây `[80%, 100%, 120%]` ATK.
2. **Kỹ Năng Major (`TheBoySage_M` - Tam Muội Chân Hỏa):** Bắn cầu lửa gây `[100%, 150%, 150%]` ATK và áp dụng hiệu ứng Thiêu Đốt.
3. **Tuyệt Kỹ Ultimate (`TheBoySage_U` - Liệt Diệm Phá Thiên):** Gây `[150%, 180%, 180%]` ATK và bùng nổ sát thương diện rộng.

---

### 🛞 5. NA TRA TAM THÁI TỬ (ThirdPrinceNezha)
* **Phẩm chất:** `SSR` | **Hệ/Vai trò:** `Assassin` (Tốc Độ Siêu Tốc & Đòn Đánh Bạo Kích)
* **Vũ khí đặc trưng:** **Ý Chí Tiên Phong (`Vanguard_Of_Volition`)** / **Phong Hỏa Luân**
* **Chỉ số cơ bản:** **HP:** `4,133` | **ATK:** `3,230` | **DEF:** `331` | **Speed:** `111` | **Crit Rate:** `16%` | **Crit DMG:** `29%`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`ThirdPrinceNezha_B` - Hỏa Tiêm Đột Tấn):** Đâm thương gây `[80%, 100%, 120%]` ATK.
2. **Kỹ Năng Major (`ThirdPrinceNezha_M` - Càn Khôn Phá Trận):** Phóng Vòng Càn Khôn gây `[100%, 150%, 150%]` ATK.
3. **Tuyệt Kỹ Ultimate (`ThirdPrinceNezha_U` - Hỏa Tiêm Liệt Hỏa):** Sốc sát thương đâm liên hoàn gây `[150%, 180%, 180%]` ATK.

---

### 🐂 6. NGƯU MA VƯƠNG (BullDemonKing)
* **Phẩm chất:** `SSR` | **Hệ/Vai trò:** `Đấu Sĩ Chống Chịu (Bruiser)` (Đại Lực Ma Vương - Bá Khí Phản Đòn & Kịch Độc Bích Thủy)
* **Vũ khí đặc trưng:** **Quạt Ba Tiêu (`Plantain_Fan`)**
* **Chỉ số cơ bản:** **HP:** `6,670` | **ATK:** `1,175` | **DEF:** `598` | **Speed:** `84` | **Kháng Bạo Kích:** `14%`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`BullDemonKing_B` - Bình Thiên Trảm):**
   - **Loại:** `MeleeAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[100%, 120%, 150%]` ATK | **Hồi chiêu:** `0` lượt.
   - **Nội tại Phản Đòn (`psv_bulldemonking_counter` - Ma Vương Phản Kích):** Mỗi khi bị kẻ địch tấn công (`OnTakeDamage`), Ngưu Ma Vương lập tức phản đòn gây `[60%, 80%, 100%]` ATK lên kẻ tấn công.
2. **Kỹ Năng Major (`BullDemonKing_M` - Ma Vương Bạt Khí):**
   - **Loại:** `StatModifier` (Tự cường hóa) | **Mục tiêu:** `Self`
   - **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Hiệu ứng (`EFF_Bull_Buff_ATK`):** Ngưu Ma Vương gầm lên một tiếng làm chấn động càn khôn, bộc phát ma khí ngút trời giúp bản thân vào trạng thái [Bình Thiên], tăng `[30%, 40%, 50%]` Tấn Công duy trì **`3 hiệp`**.
3. **Tuyệt Kỹ Ultimate (`BullDemonKing_U` - Bích Thủy Tinh Thú Kích):**
   - **Loại:** `PoisonBall` | **Mục tiêu:** `SingleEnemy`
   - **Hồi chiêu:** `[5, 5, 4]` lượt.
   - **Công thức ST & Hiệu ứng (`EFF_Bull_Poison`):** Bích Thủy Kim Tinh Thú phóng ra luồng sóng chấn động gây ST bằng `[250%, 300%, 350%]` Tấn Công cho một kẻ địch, đồng thời khiến đối phương rơi vào trạng thái [Kịch Độc], mỗi hiệp chịu ST Độc bằng `8%` Máu Tối Đa (hoặc 8% Tấn Công) trong **`3 hiệp`**.

---

### 🏯 7. LÝ THIÊN VƯƠNG (LiJing)
* **Phẩm chất:** `SSR` | **Hệ/Vai trò:** `Tanker` (Đệ Nhất Thành Trì - Giáp Ảo Đồng Hàng & Nuke Sát Thương Theo HP)
* **Vũ khí đặc trưng:** **Lung Linh Bảo Tháp (`LingLong_Pagoda`)** - Phẩm chất Legendary
  - **Chỉ số cơ bản:** HP `2,200` (+`220`/cấp) | ATK `120` (+`12`/cấp)
  - **Nội tại (`psv_linglong_pagoda`):** Tăng `[15%, 20%, 25%, 30%, 35%, 40%]` HP Tối Đa. Đồng thời giảm `18%` sát thương nhận vào ở lần đầu tiên, mỗi lần chịu đòn sau đó giảm thêm `2%`, tối đa cộng dồn 4 lần.
* **Chỉ số cơ bản nhân vật:** **HP:** `6,670` | **ATK:** `1,175` | **DEF:** `598` | **Speed:** `84`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`LiJing_B` - Bạt Phong Trảm):**
   - **Loại:** `MeleeAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[100%, 120%, 150%]` ATK | **Hồi chiêu:** `0` lượt.
   - **Nội tại (`psv_lijing_base`):** Nhận một lớp Giáp Ảo bằng **`5% HP Tối Đa`**, có thể cộng dồn tối đa **5 tầng (Max 25% HP)**.
2. **Kỹ Năng Major (`LiJing_M` - Bảo Tháp Hộ Trận):**
   - **Loại:** `BuffShield` | **Mục tiêu:** `SameRowAllies` (Toàn bộ đồng minh cùng hàng)
   - **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Hệ số Khiên:** Tạo lớp Giáp Ảo bằng **`[15%, 20%, 25%]` HP Tối Đa của Lý Thiên Vương** cho toàn bộ nhân vật cùng hàng trong 2 hiệp.
3. **Tuyệt Kỹ Ultimate (`LiJing_U` - Trấn Ma Thiên Tinh / Phong cách Zhongli):**
   - **Loại:** `EmpoweredAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hồi chiêu:** `[5, 5, 4]` lượt.
   - **Công thức ST:** Gây **`[220%, 250%, 300%]` ATK** cộng thêm **`30% HP Tối Đa (Max HP)`** của Lý Thiên Vương thẳng vào Sát Thương Gốc. Càng lên nhiều Máu ném Tháp đè đối phương càng đau!

---

### 🐷 8. TRƯ BÁT GIỚI (ZhuBaJie)
* **Phẩm chất:** `SR` | **Hệ/Vai trò:** `Tanker` (Hồi Máu Sinh Tồn & Cào Phá Giáp)
* **Vũ khí đặc trưng:** **Cửu Xỉ Đinh Ba (`Nine_ToolThed_Rake`)**
* **Chỉ số cơ bản:** **HP:** `5,800` | **ATK:** `1,096` | **DEF:** `457` | **Speed:** `79`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`ZhuBaJie_B` - Cào Cửu Xỉ):** Gây `[80%, 100%, 120%]` ATK.
2. **Kỹ Năng Major (`ZhuBaJie_M` - Ăn Uống Dưỡng Sức):** Tự ăn đùi gà hồi `[10%, 15%, 15%]` HP tối đa.
3. **Tuyệt Kỹ Ultimate (`ZhuBaJie_U` - Thiên Bồng Giáng Thế):** Vung đinh ba đập mạnh gây `[150%, 180%, 180%]` ATK.

---

### 🌊 9. SA NGỘ TĨNH (ShaWujing)
* **Phẩm chất:** `SR` | **Hệ/Vai trò:** `Tanker / Support` (Lưu Sa Hộ Thể)
* **Vũ khí đặc trưng:** **Xẻng Tứ Minh (`Sunburst_Spade`)**
* **Chỉ số cơ bản:** **HP:** `5,800` | **ATK:** `1,096` | **DEF:** `457` | **Speed:** `79`

---

### 📜 10. ĐƯỜNG TAM TẠNG (TangSanZang)
* **Phẩm chất:** `R` | **Hệ/Vai trò:** `Mage / Support` (Khắc Chế Yêu Quái & Hồi Máu)
* **Vũ khí đặc trưng:** **Cửu Hằng Trượng (`Ninefold_Staff`)**
* **Chỉ số cơ bản:** **HP:** `3,830` | **ATK:** `1,532` | **DEF:** `306` | **Speed:** `96`

---

### 🐉 11. BẠCH LONG MÃ (LittleWhiteDragon)
* **Phẩm chất:** `R` | **Hệ/Vai trò:** `ADCarry` (Tầm Xa Bắn Tỉa)
* **Vũ khí đặc trưng:** **Xiên Chín Đầu (`Ennead_spear`)**
* **Chỉ số cơ bản:** **HP:** `3,830` | **ATK:** `1,532` | **DEF:** `306` | **Speed:** `96`

---

## 🛡️ 3. BẢNG TỔNG HỢP VŨ KHÍ & NỘI TẠI TRẤN PHÁI (SIGNATURE WEAPONS)

| Tên Vũ Khí (ID) | Phẩm Chất | Loại Tướng | Tướng Đặc Trưng | Chỉ Số Cơ Bản (HP/ATK) | Chỉ Số Tăng Mỗi Cấp | Nội Tại Tĩnh (Static Modifiers) | Hiệu Ứng Trận Đấu (Combat Events) |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- | :--- |
| **Gậy Như Ý**<br>`Nimbus_Cudgel` | **Legendary (UR)** | `Fighter` | **Tôn Ngộ Không** | HP: `1375`<br>ATK: `225` | +138 HP<br>+23 ATK | **+% ATK & +% DEF:**<br>`[10, 15, 20, 25, 30, 35]%` | **Kích sát tăng Điểm Hành Động:**<br>`[20, 25, 30, 35, 40, 50]%` Action Point |
| **Tam Tiêm Đao**<br>`Triple_Edged_Blade` | **Legendary (UR)** | `Fighter` | **Dương Tiễn** | HP: `1375`<br>ATK: `225` | +138 HP<br>+23 ATK | **+% Sát Thương Bạo Kích:**<br>`[30, 36, 42, 48, 54, 60]%` | **Kích sát giảm 1 lượt hồi chiêu:**<br>Tỷ lệ: `[30, 40, 50, 60, 70, 80]%` |
| **Ý Chí Tiên Phong**<br>`Vanguard_Of_Volition` | **Legendary (UR)** | `Assassin` | **Na Tra / Sát Thủ** | HP: `875`<br>ATK: `325` | +88 HP<br>+33 ATK | **+% Sát Thương Bạo Kích:**<br>`[10, 15, 20, 25, 31, 34]%` | **Đánh đơn không chết -> Đánh thường truy kích:**<br>Tỷ lệ: `[30, 40, 50, 60, 70, 80]%` |
| **Phù Loan**<br>`Wings_of_Phoenix` | **Legendary (UR)** | `Support` | **Quan Âm Bồ Tát** | HP: `1750`<br>ATK: `150` | +175 HP<br>+15 ATK | **+% Kháng Hiệu Ứng (RES):**<br>`[10, 15, 20, 25, 34, 41]%` | **Bị tấn công tự động xóa 2 debuff:**<br>Tỷ lệ: `[40, 50, 60, 70, 80, 90]%` |
| **Quạt Quế Phách**<br>`Moonlit_Firelfy` | **Legendary (UR)** | `Mage` | **Pháp Sư UR** | HP: `1125`<br>ATK: `275` | +113 HP<br>+28 ATK | **+% ATK & +% DEF:**<br>`[5, 7.5, 10, 12.5, 17, 24]%` | **Dùng chiêu buff -> Giảm 1 CD:**<br>Tỷ lệ: `[30, 40, 50, 60, 70, 80]%` |
| **Cửu Xỉ Đinh Ba**<br>`Nine_ToolThed_Rake` | **Legendary (UR)** | `Tanker` | **Trư Bát Giới** | HP: `2000`<br>ATK: `100` | +200 HP<br>+10 ATK | **+% HP & +% Chính Xác:**<br>`[5, 7.5, 10, 12.5, 16, 20]%` | **Chịu đòn đơn tăng DEF dồn 6 tầng:**<br>`[4, 5, 6, 7, 8, 10]%` DEF/tầng |
| **Võng Sinh**<br>`Reincarnation` | **Epic (SSR)** | `Assassin` | **Hồng Hài Nhi** | HP: `578`<br>ATK: `214` | +58 HP<br>+21 ATK | **+% Tấn Công (ATK):**<br>`[10, 15, 20, 25, 35, 40]%` | **Đánh đơn gây hiệu ứng Xé Toạc:**<br>Tỷ lệ: `[30, 40, 50, 60, 70, 80]%` |
| **Quạt Ba Tiêu**<br>`Plantain_Fan` | **Epic (SSR)** | `Mage` | **Ngưu Ma Vương** | HP: `742`<br>ATK: `182` | +74 HP<br>+18 ATK | **+% Chính Xác (EHR):**<br>`[10, 15, 20, 25, 34, 40]%` | **Đòn đơn gây hiệu ứng Đánh Dấu:**<br>Tỷ lệ: `[40, 50, 60, 70, 80, 90]%` |
| **Cửu Hằng Trượng**<br>`Ninefold_Staff` | **Rare (SR)** | `Mage` | **Đường Tam Tạng** | HP: `495`<br>ATK: `121` | +50 HP<br>+12 ATK | **+% Chính Xác (EHR):**<br>`[10, 15, 20, 25, 31, 34]%` | **Bắt đầu hiệp tự xóa 1 debuff:**<br>Tỷ lệ: `[30, 40, 50, 60, 70, 80]%` |
| **Cân Đẩu Vân**<br>`Nimbus_Cloud` | **Rare (SR)** | `Mage` | **Tôn Ngộ Không / Mage** | HP: `495`<br>ATK: `121` | +50 HP<br>+12 ATK | *Không có* | **Bị đánh AOE tăng Điểm Hành Động:**<br>`[10, 15, 20, 25, 30, 40]%` Action Point |
| **Trùy Đồng**<br>`Bronze_Hammer` | **Uncommon (R)** | `Tanker` | **Lý Thiên Vương** | HP: `600`<br>ATK: `30` | +60 HP<br>+3 ATK | **+% Tốc Độ (Speed):**<br>`[5, 7.5, 10, 12.5, 17, 22]%` | *Không có* |
| **Xiên Chín Đầu**<br>`Ennead_spear` | **Uncommon (R)** | `Normal` | **Bạch Long Mã** | HP: `450`<br>ATK: `60` | +45 HP<br>+6 ATK | **+% Tỷ Lệ Bạo Kích:**<br>`[5, 7.5, 10, 12.5, 16, 20]%` | *Không có* |
| **Xẻng Tứ Minh**<br>`Sunburst_Spade` | **Common (Normal)** | `Normal` | **Sa Ngộ Tĩnh** | HP: `300`<br>ATK: `40` | +30 HP<br>+4 ATK | *Không có* | **Tăng ST Đòn Đánh Đơn:**<br>`[10, 15, 20, 25, 30, 40]%` |

---

## 🧮 4. CÔNG THỨC SÁT THƯƠNG & CÂN BẰNG HỆ THỐNG (FORMULAS)

### 1. Công thức Phòng Ngự (Armor Mitigation):
$$\text{Giáp Sau Xuyên} = \text{DEF}_{\text{gốc}} \times (1 - \text{Xuyên Giáp \%})$$
$$\text{DEF}_{\text{hiệu dụng}} = \max(0, \text{Giáp Sau Xuyên} - \text{Phá Giáp Phẳng})$$
$$\text{Hệ Số Sát Thương Nhận} = \frac{400}{400 + \text{DEF}_{\text{hiệu dụng}}}$$

### 2. Công thức Bạo Kích (Critical Damage):
$$\text{Tổng Hệ Số Crit} = 150\% (\text{Base}) + \text{CritDMG}_{\text{tướng}} + \text{CritDMG}_{\text{vũ khí}} + \text{CritDMG}_{\text{skill}} - \text{Kháng Bạo Kích Mục Tiêu}$$

### 3. Công thức Sát Thương Thực Nhận (Final Damage):
$$\text{Damage Cuối} = \text{ATK} \times \text{DamageMultiplier} \times \text{DamageBonus} \times \text{Hệ Số Crit} \times \text{Hệ Số Sát Thương Nhận}$$
