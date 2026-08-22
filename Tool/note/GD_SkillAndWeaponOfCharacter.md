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

#### 🪓 Vũ Khí Trấn Phái: GẬY NHƯ Ý (`Nimbus_Cudgel`)
* **Phẩm chất:** `Legendary (UR)` | **Hệ:** `Fighter`
* **Chỉ số cơ bản:** **HP:** `1,650` (+`165`/cấp) | **ATK:** `450` (+`45`/cấp)
* **Nội tại Trấn Phái (`psv_nimbus_cudgel`):**
  - **Chỉ số tĩnh:** Tăng **`[10%, 15%, 20%, 25%, 30%, 35%]` ATK** và **DEF**.
  - **Hiệu ứng Chiến Đấu (`Eff_Action_Point_Boost`):** Sau khi tung đòn tấn công đơn mục tiêu **đánh bại kẻ địch (`SingleTarget, TargetDead`)**, Điểm Hành Động (Action Point) của bản thân **tăng ngay `[20%, 25%, 30%, 35%, 40%, 50%]`** để lập tức có thêm lượt đi càn quét tiếp theo!

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

#### 🪓 Vũ Khí Trấn Phái: TAM TIÊM LƯỠNG NHẬN ĐAO (`Triple_Edged_Blade`)
* **Phẩm chất:** `Legendary (UR)` | **Hệ:** `Fighter`
* **Chỉ số cơ bản:** **HP:** `1,650` (+`165`/cấp) | **ATK:** `450` (+`45`/cấp)
* **Nội tại Trấn Phái (`psv_triple_edged_blade`):**
  - **Chỉ số tĩnh:** Tăng **`[30%, 36%, 42%, 48%, 54%, 60%]` Sát Thương Bạo Kích (CRIT_DMG)** và **`[10%, 12%, 14%, 17%, 20%, 25%]` Tấn Công (ATK)**.
  - **Hiệu ứng Chiến Đấu (`Eff_Reduce_Cooldown`):** Sau khi thi triển kỹ năng đơn mục tiêu **đánh bại kẻ địch (`IsSkill, SingleTarget, TargetDead`)**, có **`[30%, 40%, 50%, 60%, 70%, 80%]` xác suất** lập tức **giảm 1 lượt hồi chiêu (`-1 CD`)** cho toàn bộ kỹ năng của bản thân!

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
* **Phẩm chất:** `SSR` | **Hệ/Vai trò:** `Assassin` (Sát Thủ Tốc Độ & Bạo Kích Hai Lượt Càn Khôn)
* **Vũ khí đặc trưng:** **Ý Chí Tiên Phong (`Vanguard_Of_Volition`)** / **Phong Hỏa Luân**
* **Chỉ số cơ bản (Level 1):** **HP:** `3,444` | **ATK:** `3,589` | **DEF:** `344` | **Speed:** `111` | **Crit Rate:** `16%` | **Crit DMG:** `29%` | **Phá Giáp (DEF Shred):** `21%`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`ThirdPrinceNezha_B` - Hỏa Tiêm Đột Tấn):**
   - **Loại:** `MeleeAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[80%, 100%, 120%]` ATK | **Hồi chiêu:** `0` lượt.
2. **Kỹ Năng Major (`ThirdPrinceNezha_M` - Càn Khôn Phá Trận):**
   - **Loại:** `FireRing` (`RingOfUniverseSkill` - Đạn đạo bay 2 lượt) | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[40%, 50%, 60%]` ATK / hit | **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Cơ chế Độc Quyền (Double Hit - Guaranteed Return Crit):**
     - **Lượt Đi (Outward):** Vòng Càn Khôn bay tới xuyên qua mục tiêu gây sát thương `[40%, 50%, 60%]` ATK theo tỷ lệ bạo kích thông thường.
     - **Lượt Về (Returning):** Sau khi đạt điểm cực đại, Vòng Càn Khôn xoay cực tốc đâm ngược trở lại, **Chắc Chắn Gây Sát Thương Bạo Kích 100% (Guaranteed Critical Strike)** kèm theo hiệu ứng âm thanh đanh thép (Gây thêm $\approx 107\%$ ATK).
     - $\Rightarrow$ **Tổng Sát Thương Major:** $\approx \mathbf{111\% \to 167\%}$ ATK.
3. **Tuyệt Kỹ Ultimate (`ThirdPrinceNezha_U` - Hỏa Tiêm Liệt Hỏa):**
   - **Loại:** `NezhaExecution` (`NezhaExecutionSkill` - Đột Kích & Càn Quét Kích Sát) | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[140%, 160%, 180%]` ATK | **Hồi chiêu:** `[5, 5, 4]` lượt.
   - **Cơ chế Độc Quyền (Càn Quét Chiến Trường):** Khi đòn đánh **KẾT LIỄU mục tiêu**, Na Tra lập tức kích hoạt hiệu ứng **[Càn Quét]**, tự động đổi hướng lướt sang tấn công 1 kẻ địch hợp lệ ngẫu nhiên khác (tuân theo luật Hàng & Cột) với **`120% ATK`** (Tổng combo đạt **`300%` ATK**).

---

### 🐂 6. NGƯU MA VƯƠNG (BullDemonKing)
* **Phẩm chất:** `SSR` | **Hệ/Vai trò:** `Đấu Sĩ Chống Chịu (Bruiser)` (Đại Lực Ma Vương - Bá Khí Phản Đòn & Kịch Độc Bích Thủy)
* **Vũ khí đặc trưng:** **Hỗn Độn Thiết (`Ferrous_Chaos`)**
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

### 🍃 7. THIẾT PHIẾN CÔNG CHÚA (PrincessIronFan)
* **Phẩm chất:** `SSR` | **Hệ/Vai trò:** `Pháp Sư Tấn Công (Offensive Mage)` (Bà La Sát - Bão Tố Âm Phong & Trảm Giáp)
* **Vũ khí đặc trưng:** **Quạt Ba Tiêu (`Plantain_Fan`)** - Phẩm chất Legendary (UR)
  - **Chỉ số cơ bản:** HP `742` (+`74`/cấp) | ATK `182` (+`18`/cấp)
  - **Nội tại (`psv_plantain_fan`):** Tăng `[20, 25, 30, 35, 40, 50]%` ST Bạo Kích và `[10, 12, 14, 17, 20, 25]%` Tấn Công. Đòn tấn công kỹ năng có **60% xác suất** giảm `[15, 18, 21, 24, 27, 30]%` Phòng Thủ của mục tiêu trong 2 hiệp và gây thêm `[15, 18, 21, 24, 27, 30]%` Sát Thương lên kẻ địch đang chịu hiệu ứng bất lợi.
* **Chỉ số cơ bản nhân vật:** **HP:** `5,450` | **ATK:** `1,420` | **DEF:** `430` | **Speed:** `98` | **CRIT_RATE:** `15%` | **CRIT_DMG:** `150%`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`PrincessIronFan_B` - Phong Lôi Ba):**
   - **Loại:** `RangeMagicAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[100%, 120%, 140%]` ATK | **Hồi chiêu:** `0` lượt.
   - **Hiệu ứng:** Có `50%` xác suất giảm 10% Tốc Độ của mục tiêu trong 2 hiệp.
2. **Kỹ Năng Major (`PrincessIronFan_M` - Tam Muội Âm Phong):**
   - **Loại:** `TorandoSkill` | **Mục tiêu:** `FrontRowEnemies` (Hàng trước của địch)
   - **Hệ số ST:** `[130%, 150%, 180%]` ATK | **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Hiệu ứng [Bào Mòn]:** Giảm 20% Phòng Thủ (DEF) và khiến kẻ địch chịu thêm 15% Sát Thương Phép trong 2 hiệp.
3. **Tuyệt Kỹ Ultimate (`PrincessIronFan_U` - Ba Tiêu Diệt Địa Phong):**
   - **Loại:** `DivineWindSkill` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[300%, 360%, 420%]` ATK | **Hồi chiêu:** `[5, 5, 4]` lượt.
   - **Hiệu ứng [Hất Tung Vạn Dặm]:** Giảm 30% Điểm Hành Động của mục tiêu. Nếu mục tiêu đang chịu hiệu ứng bất lợi/giảm DEF, đòn đánh chắc chắn Bạo Kích (100% Crit) và tăng thêm 40% Sát Thương Bạo Kích.

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
* **Phẩm chất:** `SR` *(Boss: `MarshalTianpeng` SSR)* | **Hệ/Vai trò:** `Tanker / Bruiser` (Đấu Sĩ Chống Chịu - Thủy Giáp & Phá Giáp Trảm)
* **Vũ khí đặc trưng:** **Cửu Xỉ Đinh Ba (`Nine_ToolThed_Rake`)**
* **Chỉ số cơ bản:** **HP:** `5,800` | **ATK:** `1,096` | **DEF:** `457` | **Speed:** `79`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`ZhuBaJie_B` - Cửu Xỉ Thần Bát):**
   - **Loại:** `MeleeAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[80%, 100%, 120%]` ATK | **Hồi chiêu:** `0` lượt.
2. **Kỹ Năng Major (`ZhuBaJie_M` - Thiên Bồng Giáp Trận):**
   - **Loại:** `BuffShield` (`NonAttackSkill`) | **Mục tiêu:** `Self`
   - **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Hiệu ứng:** Vận linh lực tạo cho bản thân một lớp Giáp Ảo hấp thụ sát thương bằng **`[25%, 30%, 35%]` HP Tối Đa** của Bát Giới trong 2 hiệp.
3. **Tuyệt Kỹ Ultimate (`ZhuBaJie_U` - Thần Thể Biến Hóa):**
   - **Loại:** `EmpoweredAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hồi chiêu:** `[5, 5, 4]` lượt.
   - **Hệ số ST:** `[140%, 160%, 180%]` ATK.
   - **Hiệu ứng đặc biệt (`EFF_Zhu_DefShred`):** Dốc toàn lực giáng một đòn trời giáng xuống mục tiêu, đồng thời khiến đối phương rơi vào trạng thái **[Suy Yếu]**, **giảm `20%` Phòng Thủ (`DEF`) trong `2 hiệp`**.

#### 🪓 Vũ Khí Trấn Phái: CỬU XỈ ĐINH BA (`Nine_ToolThed_Rake`)
* **Phẩm chất:** `Legendary (UR)` | **Hệ:** `Tanker`
* **Chỉ số cơ bản:** **HP:** `2,000` (+`200`/cấp) | **ATK:** `100` (+`10`/cấp)
* **Nội tại Trấn Phái (`psv_nine_toothed_rake` - Thượng Bảo Kim Thân):**
  - **Chỉ số tĩnh:** Tăng **`[10%, 14%, 18%, 22%, 26%, 32%]` HP** và **DEF**.
  - **Hiệu ứng Chiến Đấu (`Eff_Stackable_DEF_Buff`):** Mỗi khi chịu sát thương, Phòng Thủ (DEF) tăng thêm **`[4%, 5%, 6%, 7%, 8%, 10%]`**, tối đa cộng dồn **5 tầng** *(Tối đa $+50\%$ DEF)*. Khi đạt đủ 5 tầng, lập tức **hồi phục `10% HP Tối Đa`**.

---

### 🌊 9. SA NGỘ TĨNH (ShaWujing)
* **Phẩm chất:** `SR` | **Hệ/Vai trò:** `Tanker / Support` (Đấu Sĩ Hỗ Trợ - Tự Phục Hồi & Lưu Sa Giảm Tốc)
* **Vũ khí đặc trưng:** **Hàng Yêu Bảo Trượng (`Demon_Subduing_Staff`)** / **Xẻng Tứ Minh (`Sunburst_Spade`)**
* **Chỉ số cơ bản:** **HP:** `5,800` | **ATK:** `1,096` | **DEF:** `457` | **Speed:** `79`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`ShaWujing_B` - Bảo Trượng Trảm):**
   - **Loại:** `MeleeAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[80%, 100%, 120%]` ATK | **Hồi chiêu:** `0` lượt.
2. **Kỹ Năng Major (`ShaWujing_M` - Lưu Cát Linh Dược):**
   - **Loại:** `Healing` (`NonAttackSkill`) | **Mục tiêu:** `Self`
   - **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Cơ chế Hồi Phục & Phòng Thủ:** Lập tức hồi phục lượng Máu tương đương **`[100%, 150%, 150%]` ATK** cho bản thân, đồng thời **tăng `20%` Phòng Thủ (`EFF_BufDEF_def`) trong `2 hiệp`**.
3. **Tuyệt Kỹ Ultimate (`ShaWujing_U` - Lưu Cát Trầm Luân):**
   - **Loại:** `EmpoweredAttack` (`ActiveSkill`) | **Mục tiêu:** `SingleEnemy`
   - **Hồi chiêu:** `[5, 5, 4]` lượt.
   - **Hệ số ST:** `[150%, 180%, 180%]` ATK.
   - **Hiệu ứng đặc biệt (`EFF_DefSpd_def`):** Xoay mạnh Huyền Trượng rồi giáng một đòn cực uy lực xuống mục tiêu, đồng thời khiến kẻ địch rơi vào trạng thái **[Cát Lún] - Giảm `20%` Tốc Độ (`SPEED`) trong `2 hiệp`**.

---

### 📜 10. ĐƯỜNG TAM TẠNG (TangSanZang)
* **Phẩm chất:** `R` | **Hệ/Vai trò:** `Mage / Support` (Pháp Sư Hỗ Trợ - Buff Công Hàng & Khống Chế Choáng)
* **Vũ khí đặc trưng:** **Cửu Hằng Trượng (`Ninefold_Staff`)**
* **Chỉ số cơ bản:** **HP:** `3,830` | **ATK:** `1,532` | **DEF:** `306` | **Speed:** `96`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`TangSanZang_B` - Tích Trượng Kim Quang):**
   - **Loại:** `Range` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[80%, 100%, 120%]` ATK | **Hồi chiêu:** `0` lượt.
2. **Kỹ Năng Major (`TangSanZang_M` - Kinh Mạn Kim Quang):**
   - **Loại:** `StatModifier` (`NonAttackSkill`) | **Mục tiêu:** `SameRowAllies` (Đồng minh cùng hàng)
   - **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Cơ chế Buff Hàng:** Ngưng tụ màn kinh thư vàng bao bọc đồng đội, tăng **`20%` Tấn Công (`ATK`)** cho **toàn bộ đồng minh cùng hàng** trong **`2 hiệp` (`EFF_Tang_Buff_ATK`)**.
3. **Tuyệt Kỹ Ultimate (`TangSanZang_U` - Kính Bát Trấn Ma):**
   - **Loại:** `EmpowerAttack` (`ActiveSkill`) | **Mục tiêu:** `SingleEnemy`
   - **Hồi chiêu:** `[5, 5, 4]` lượt.
   - **Hệ số ST:** `[150%, 180%, 180%]` ATK.
   - **Hiệu ứng đặc biệt (`EFF_Stun_def`):** Áp sát đưa Bát Vàng ra phóng chiếu hào quang Phật pháp chói lọi, khiến mục tiêu rơi vào trạng thái **`[Choáng (Stun)]` trong `1 hiệp`**.

---

### 🐉 11. BẠCH LONG MÃ / TIỂU BẠCH LONG (LittleWhiteDragon)
* **Phẩm chất:** `R` | **Hệ/Vai trò:** `ADCarry` (Sát Thương Chủ Lực Tốc Độ - Diệt Giáp Ảo & Xuyên Giáp)
* **Vũ khí đặc trưng:** **Võng Sinh (`Reincarnation`)** / **Xiên Chín Đầu (`Ennead_spear`)**
* **Chỉ số cơ bản:** **HP:** `3,830` | **ATK:** `1,532` | **DEF:** `306` | **Speed:** `96`

#### 🗡️ Bộ Kỹ Năng:
1. **Đánh Thường (`LittleWhiteDragon_B` - Bạch Long Kiếm Trảm):**
   - **Loại:** `MeleeAttack` | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[80%, 100%, 120%]` ATK | **Hồi chiêu:** `0` lượt.
   - **Nội tại:** Đòn đánh tự nhiên sở hữu **`30% Xuyên Giáp (Armor Penetration)`**, bỏ qua 30% Phòng Thủ của mục tiêu.
2. **Kỹ Năng Major (`LittleWhiteDragon_M` - Càn Lôi Truy Phong):**
   - **Loại:** `ThunderBall` (`ActiveSkill` - Vừa tấn công vừa buff tốc) | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[100%, 150%, 150%]` ATK | **Hồi chiêu:** `[4, 4, 3]` lượt.
   - **Cơ chế Buff Tốc:** Phóng cầu sét lôi long tấn công kẻ địch, đồng thời bản thân lập tức nhận trạng thái **[Truy Phong] - Tăng `30%` Tốc Độ (`SPEED`) trong `2 hiệp` (`EFF_Dragon_Speed`)**.
3. **Tuyệt Kỹ Ultimate (`LittleWhiteDragon_U` - Hải Sóng Nộ Trào):**
   - **Loại:** `EmpowerAttack` (`ActiveSkill`) | **Mục tiêu:** `SingleEnemy`
   - **Hệ số ST:** `[150%, 180%, 180%]` ATK | **Hồi chiêu:** `[5, 5, 4]` lượt.
   - **CƠ CHẾ DIỆT GIÁP ẢO (`ShieldBreaker`):** Nếu mục tiêu **ĐANG CÓ GIÁP ẢO (`Shield > 0`)**, sát thương gây ra được **NHÂN ĐÔI ($\times 200\% \rightarrow \mathbf{360\%}$ ATK)**!

---

## 🛡️ 3. BẢNG TỔNG HỢP VŨ KHÍ & NỘI TẠI TRẤN PHÁI (SIGNATURE WEAPONS)

| Tên Vũ Khí (ID) | Phẩm Chất | Loại Tướng | Tướng Đặc Trưng | Chỉ Số Cơ Bản (HP/ATK) | Chỉ Số Tăng Mỗi Cấp | Nội Tại Tĩnh (Static Modifiers) | Hiệu Ứng Trận Đấu (Combat Events) |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- | :--- |
| **Gậy Như Ý**<br>`Nimbus_Cudgel` | **Legendary (UR)** | `Fighter` | **Tôn Ngộ Không** | HP: `1375`<br>ATK: `225` | +138 HP<br>+23 ATK | **+% ATK & +% DEF:**<br>`[10, 15, 20, 25, 30, 35]%` | **Kích sát tăng Điểm Hành Động:**<br>`[20, 25, 30, 35, 40, 50]%` Action Point |
| **Tam Tiêm Đao**<br>`Triple_Edged_Blade` | **Legendary (UR)** | `Fighter` | **Dương Tiễn** | HP: `1375`<br>ATK: `225` | +138 HP<br>+23 ATK | **+% Sát Thương Bạo Kích & +% Tấn Công:**<br>CRIT DMG: `[30, 36, 42, 48, 54, 60]%`<br>ATK: `[10, 12, 14, 17, 20, 25]%` | **Kích sát giảm 1 lượt hồi chiêu:**<br>Tỷ lệ: `[30, 40, 50, 60, 70, 80]%` |
| **Tùy Tâm Thiết Can Binh**<br>`Vanguard_Of_Volition` | **Legendary (UR)** | `Assassin` | **Lục Nhĩ Mỹ Hầu / Na Tra / Sát Thủ UR** | HP: `875`<br>ATK: `325` | +88 HP<br>+33 ATK | **+% Sát Thương Bạo Kích & +% Tấn Công:**<br>CRIT DMG: `[20, 25, 30, 35, 40, 50]%`<br>ATK: `[10, 12, 14, 17, 20, 25]%` | **1. Bạo Kích tăng Điểm Hành Động:**<br>`[15, 18, 21, 24, 27, 30]%` Action Point<br>**2. Đánh đơn không chết -> Truy Kích Đánh Thường:**<br>Tỷ lệ: `[50, 60, 70, 80, 90, 100]%` |
| **Phù Loan**<br>`Wings_of_Phoenix` | **Legendary (UR)** | `Support / Tanker` | **Quan Âm Bồ Tát / Tanker Trị Liệu** | HP: `1750`<br>ATK: `150` | +175 HP<br>+15 ATK | **+% HP & +% DEF:**<br>HP: `[15, 18, 21, 25, 30, 40]%`<br>DEF: `[10, 12, 14, 17, 20, 25]%` | **Tăng hiệu quả Kỹ Năng Hồi Máu:**<br>Tăng thêm `[30, 40, 50, 60, 70, 80]%` hiệu quả |
| **Quạt Quế Phách**<br>`Moonlit_Firelfy` | **Legendary (UR)** | `Mage` | **Pháp Sư Hỗ Trợ UR** | HP: `1125`<br>ATK: `275` | +113 HP<br>+28 ATK | **+% Tốc Độ & +% Tấn Công:**<br>SPEED: `[8, 10, 12, 15, 18, 25]%`<br>ATK: `[10, 12, 14, 17, 20, 25]%` | **Tăng hiệu quả Kỹ Năng Cường Hóa & Hỗ Trợ:**<br>Tăng thêm `[40, 50, 60, 70, 80, 100]%` hiệu quả |
| **Cửu Xỉ Đinh Ba**<br>`Nine_ToolThed_Rake` | **Legendary (UR)** | `Tanker` | **Trư Bát Giới** | HP: `2000`<br>ATK: `100` | +200 HP<br>+10 ATK | **+% HP & +% Chính Xác:**<br>`[5, 7.5, 10, 12.5, 16, 20]%` | **Chịu đòn đơn tăng DEF dồn 6 tầng:**<br>`[4, 5, 6, 7, 8, 10]%` DEF/tầng |
| **Võng Sinh**<br>`Reincarnation` | **Epic (SSR)** | `ADCarry` | **Tiểu Bạch Long / Tân Thủ** | HP: `578`<br>ATK: `214` | +58 HP<br>+21 ATK | **+% Tấn Công (ATK):**<br>`[8, 11, 14, 17, 20, 25]%` | **Đòn tấn công đơn mục tiêu tăng ST:**<br>`[10, 13, 16, 19, 22, 25]%` Sát Thương |
| **Cửu Hằng Trượng**<br>`Ninefold_Staff` | **Rare (SR)** | `Support` | **Đường Tam Tạng** | HP: `495`<br>ATK: `121` | +50 HP<br>+12 ATK | **+% Tốc Độ (SPEED):**<br>`[6, 8, 10, 12, 15, 20]%` | **Tăng hiệu quả Kỹ Năng Cường Hóa & Hỗ Trợ:**<br>Tăng thêm `[30, 35, 40, 45, 50, 60]%` hiệu quả |
| **Cân Đẩu Vân**<br>`Nimbus_Cloud` | **Rare (SR)** | `Mage` | **Pháp Sư Khởi Đầu / Mage SR** | HP: `495`<br>ATK: `121` | +50 HP<br>+12 ATK | **+% Tốc Độ & +% Tấn Công:**<br>SPEED: `[4, 5, 6, 7, 8.5, 10]%`<br>ATK: `[5, 6.5, 8, 10, 12, 15]%` | **Khởi Đầu Thần Tốc (Tăng Điểm Hành Động khi vào lượt):**<br>`[10, 12, 14, 16, 18, 20]%` Action Point |
| **Trùy Đồng**<br>`Bronze_Hammer` | **Uncommon (R)** | `Tanker` | **Tướng Chống Chịu Đầu Game** | HP: `600`<br>ATK: `30` | +60 HP<br>+3 ATK | **+% Phòng Thủ (DEF):**<br>`[5, 7.5, 10, 12.5, 16, 20]%` | *Không có* |
| **Xiên Chín Đầu**<br>`Ennead_spear` | **Uncommon (R)** | `Normal` | **Tướng Sát Thương Đầu Game** | HP: `450`<br>ATK: `60` | +45 HP<br>+6 ATK | **+% Tỷ Lệ Bạo Kích (CRIT_RATE):**<br>`[5, 7.5, 10, 12.5, 16, 20]%` | *Không có* |
| **Quạt Ba Tiêu**<br>`Plantain_Fan` | **Legendary (UR)** | `Mage` | **Thiết Phiến Công Chúa** | HP: `742`<br>ATK: `182` | +74 HP<br>+18 ATK | **+% Sát Thương Bạo Kích & +% Tấn Công:**<br>CRIT DMG: `[20, 25, 30, 35, 40, 50]%`<br>ATK: `[10, 12, 14, 17, 20, 25]%` | **1. 60% xác suất Giảm Phòng Thủ khi đánh Kỹ Năng:** `[15, 18, 21, 24, 27, 30]%` DEF (2 hiệp)<br>**2. Tăng Sát Thương lên kẻ địch có Debuff:** `[15, 18, 21, 24, 27, 30]%` |
| **Xẻng Tứ Minh**<br>`Sunburst_Spade` | **Common (Normal)** | `Normal` | **Vũ Khí Khởi Đầu Tân Thủ** | HP: `300`<br>ATK: `40` | +30 HP<br>+4 ATK | **+% Tấn Công (ATK):**<br>`[5, 7.5, 10, 12.5, 15, 20]%` | *Không có* |

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

### 4. Công thức Tính Chiến Lực (Combat Power Rating):
Chiến Lực phản ánh toàn diện Sức mạnh Tấn công, Độ sinh tồn và Tốc độ ra đòn của nhân vật:
$$\text{OffensivePower} = \text{ATK} \times \left(1 + \frac{\text{CritRate}}{100} \times \frac{50 + \text{CritDMG}}{100}\right) \times \left(1 + \frac{\text{Penetration}}{100} + \frac{\text{FlatPen}}{400}\right)$$
$$\text{DefensivePower} = \frac{\text{HP} \times \left(1 + \frac{\text{DEF}}{400}\right)}{2.5} \times \left(1 + \frac{\text{CritDMGRes}}{100}\right)$$
$$\text{SpeedFactor} = \frac{\text{Speed}}{100}$$
$$\text{UtilityPoints} = (\text{EHR} \times 15) + (\text{RES} \times 15)$$
$$\text{StarBonus} = \text{StarUp} \times 250$$
$$\mathbf{\text{Tổng Chiến Lực}} = \mathbf{\text{Round}}\Big( (\text{OffensivePower} + \text{DefensivePower}) \times \text{SpeedFactor} + \text{UtilityPoints} + \text{StarBonus} \Big)$$

