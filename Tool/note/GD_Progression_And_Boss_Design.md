# TÀI LIỆU THIẾT KẾ TIẾN TRÌNH GAME & CÂN BẰNG TÀI NGUYÊN (GAME PROGRESSION & RESOURCE ECONOMY DESIGN)

---

## 🗺️ I. TỔNG QUAN TIẾN TRÌNH 4 VÙNG ĐẤT CỐT TRUYỆN (CAMPAIGN PROGRESSION)

Hệ thống cốt truyện được thiết lập chính xác theo lộ trình 4 Vùng Đất từ **Level 5 đến Level 30**:

```mermaid
graph TD
    T[0. Tutorial: Mở Đầu<br>Lính Hổ Lv 5] --> W1
    
    subgraph VÙNG 1: ĐÔNG HẢI LONG CUNG [Level 10 - 20]
        W1[Trận 1: Linh Cảm Đại Vương<br>GoldfishDemon Lv 12 + Lính Cá Lv 10] --> W2[Trận 2: Tam Thái Tử Ngao Bính<br>ThirdDragonPrince Lv 20 + GoldfishDemon Lv 20 + Cua/Mực Lv 15]
    end
    
    subgraph VÙNG 2: THIÊN ĐÌNH - ĐẠI NÁO [Level 20 - 25]
        W2 --> W3[Trận 1: Thác Tháp Lý Thiên Vương<br>LiJing Lv 25 + Na Tra/Bát Giới Lv 20 + Thiên Binh Lv 20]
        W3 --> W4[Trận 2: Nhị Lang Thần Dương Tiễn<br>ErlangShen Lv 25 + Thiên Binh Lv 20]
    end
    
    subgraph VÙNG 3: CAO LÃO TRANG & HOÀNG PHONG [Level 20 - 28]
        W4 --> W5[Trận 1: Hoàng Phong Quái<br>YellowWindMonster Lv 28 + Lính Hổ/Chuột Lv 20]
        W5 --> W6[Trận 2: Kim Giác & Ngân Giác<br>GoldHornKing Lv 28 + SilverHornKing Lv 26 + Yêu Binh Lv 25]
    end
    
    subgraph VÙNG 4: HỎA DIỆM SƠN [Level 25 - 30]
        W6 --> W7[Trận 1: Thánh Anh Đại Vương<br>TheBoySage Lv 30 + Bò Ma Lv 25]
        W7 --> W8[Trận 2: TRÙM CUỐI: Ngưu Ma Vương<br>BullDemonKing Lv 30 + Hồng Hài Nhi Lv 30 + Đàn Bò Ma Lv 25]
    end
    
    subgraph ENDGAME: THÔNG THIÊN THÁP [Level 30 - 100]
        W8 --> Tower[THÔNG THIÊN THÁP / TOWER OF ETERNITY<br>Tầng 1 - 100: Cày Substats, Giáp Hoàng Kim +15, Tranh Hạng]
    end
```

---

## 📋 II. BẢNG CHI TIẾT CÁC MÀN ĐẤU & LEVEL QUÁI / BOSS

| Vùng Đất / Màn Đấu | Mã Trận Đấu | Slot & Kẻ Địch | Level Cụ Thể | Vai Trò / Phân Loại |
| :--- | :--- | :--- | :---: | :--- |
| **Tutorial** | `Tutorial_Default` | Slot 1, 2, 3: VanguardTiger | **Lv 5** | Lính Hổ mở đầu game |
| **Vùng 1: Đông Hải** | `Battle_GoldfishDemon` | • Slot 1, 2, 3: Benborba & Baborben<br>• Slot 5: **GoldfishDemon** | **Lv 10**<br>**Lv 12** | Lính Cá Hàng Trước<br>**Boss Linh Cảm Đại Vương** |
| | `Battle_ThirdDragonPrince` | • Slot 1, 2, 3: CrabSolider & SquidSolider<br>• Slot 4: **GoldfishDemon**<br>• Slot 5: **ThirdDragonPrince** | **Lv 15**<br>**Lv 20**<br>**Lv 20** | Lính Cua / Mực Hộ Vệ<br>Linh Cảm Đại Vương phụ công<br>**Boss Tam Thái Tử Ngao Bính** |
| **Vùng 2: Thiên Đình** | `Battle_LiJing_Boss` | • Slot 3, 4, 6: HeavenlySoddier<br>• Slot 1: **MarshalTianpeng** (Bát Giới)<br>• Slot 5: **ThirdPrinceNezha_Boss** (Na Tra)<br>• Slot 2: **LiJing_Boss** | **Lv 20**<br>**Lv 20**<br>**Lv 20**<br>**Lv 25** | Thiên Binh Hộ Vệ<br>Tướng Đỡ Đòn<br>Sát Thủ Bạo Kích<br>**Boss Thác Tháp Lý Thiên Vương** |
| | `Battle_ErlangShen_Boss` | • Slot 1, 2, 3: HeavenlySoddier<br>• Slot 5: **ErlangShen_Boss** | **Lv 20**<br>**Lv 25** | Thiên Binh Hộ Vệ<br>**Boss Nhị Lang Thần Dương Tiễn** |
| **Vùng 3: Cao Lão Trang** | `Battle_YellowWindMonster` | • Slot 1, 2, 3: VanguardTiger & RatMonster<br>• Slot 5: **YellowWindMonster** | **Lv 20**<br>**Lv 28** | Lính Hổ & Chuột Tinh<br>**Boss Hoàng Phong Quái** |
| | `Battle_GoldHornKing` | • Slot 1, 2, 3: Minions<br>• Slot 4: **SilverHornKing**<br>• Slot 5: **GoldHornKing** | **Lv 25**<br>**Lv 26**<br>**Lv 28** | Yêu Binh Hàng Trước<br>Boss Phụ: Ngân Giác Đại Vương<br>**Boss Chính: Kim Giác Đại Vương** |
| **Vùng 4: Hỏa Diệm Sơn** | `Battle_TheBoySage` | • Slot 1, 2, 3: YoungBufflalo<br>• Slot 5: **TheBoySage** | **Lv 25**<br>**Lv 30** | Đàn Bò Ma Hàng Trước<br>**Boss Hồng Hài Nhi** |
| | `Battle_BullDemonKing` | • Slot 1, 3, 4, 6: YoungBufflalo<br>• Slot 5: **TheBoySage**<br>• Slot 2: **BullDemonKing_Boss** | **Lv 25**<br>**Lv 30**<br>**Lv 30** | Đàn Bò Ma Hộ Vệ<br>Hồng Hài Nhi Phụ Công<br>**TRÙM CUỐI: Ngưu Ma Vương** |

---

## 📊 III. BẢNG CÂN BẰNG TÀI NGUYÊN & TIẾN TRÌNH ĐỘT PHÁ (RESOURCE BALANCING)

### 1. Phân Tầng Đột Phá Nhân Vật (Ascension Tiers)
* **Tier 0 (Lv 1 - 20):** Áp dụng cho Tutorial, Vùng 1 (Đông Hải) và giai đoạn đầu Vùng 2.
* **Tier 1 (Lv 20 - 40):** Mở khóa khi đạt Level 20 để vượt qua Thiên Đình, Cao Lão Trang và Hỏa Diệm Sơn (Boss Lv 25 - 30).
* **Tier 2 - 4 (Lv 40 - 100):** Dành riêng cho chế độ **Thông Thiên Tháp (Endgame Tower)** và các Bản Mở Rộng tương lai.

### 2. Phân Bổ Cấp Trang Bị
* **Vùng 1 (Đông Hải - Lv 10-20):** Giáp Xanh / Tím (+0 ~ +3).
* **Vùng 2 (Thiên Đình - Lv 20-25):** Giáp Tím (+3 ~ +6), kích hoạt **Set 2 món**.
* **Vùng 3 (Cao Lão Trang - Lv 20-28):** Giáp Tím / Vàng (+6 ~ +9), kích hoạt **Set 4 món**.
* **Vùng 4 (Hỏa Diệm Sơn - Lv 25-30):** Giáp Hoàng Kim (+9 ~ +12), Set 4 món hoàn chỉnh.
* **Thông Thiên Tháp (Endgame - Lv 30-100):** Cường hóa Giáp +15, Tối ưu hóa Substats hoàn hảo.

---

*Tài liệu này được cập nhật chính xác và lưu trữ tại `Tool/note/GD_Progression_And_Boss_Design.md`.*
