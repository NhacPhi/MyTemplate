# 📜 Hướng Dẫn & Quy Tắc Thiết Kế Dữ Liệu QuestLine - Quest - Step (Node Tree)

Tài liệu này tổng hợp quy tắc đặt tên (Naming Conventions) chuẩn cho 3 cấp **QuestLine ➔ Quest ➔ Step**, cùng cấu trúc dữ liệu và hướng dẫn nhập liệu Excel.

---

## 🏛️ 1. Cấu Trúc Phân Cấp (3-Tier Hierarchy)

Hệ thống quản lý cốt truyện gồm 3 cấp bậc rõ ràng:

```
[QuestLine] (Tập hợp tuyến cốt truyện lớn / Chương câu chuyện, VD: QL_01_JourneyToWest)
   │
   └── [Quest] (Nhiệm vụ cụ thể trong chuỗi, VD: Q_QL01_01_BorrowWeapon)
          │
          └── [Step] (Các bước thực hiện nhiệm vụ, VD: STEP_Q01_01_MeetDragonKing)
                 │
                 ├── PreviousDialogue ─────► [DialogueConfig] (Cây Node thoại nhận step)
                 ├── IncompleteDialogue ──► [DialogueConfig] (Cây Node thoại nhắc nhở)
                 └── CompletedDialogue ───► [DialogueConfig] (Cây Node thoại trả step)
```

---

## 🏷️ 2. Quy Tắc Đặt Tên Naming Conventions (Đầy Đủ 3 Cấp)

### 2.1. Quy tắc đặt tên `QuestLineID` (Tuyến cốt truyện lớn)
Cấu trúc chuẩn: **`QL_[STT]_[TênTuyếnCốtTruyện]`** hoặc **`QL_[Loại]_[TênTuyến]`**

📌 **Ví dụ:**
- `QL_01_JourneyToWest` ➔ Tuyến cốt truyện chính Chương 1: Hành Trình Tây Du.
- `QL_02_HavocInHeaven` ➔ Tuyến cốt truyện chính Chương 2: Đại Náo Thiên Cung.
- `QL_SIDE_EasternSea` ➔ Chuỗi nhiệm vụ phụ tại Đông Hải.

---

### 2.2. Quy tắc đặt tên `QuestID` (Nhiệm vụ trong QuestLine)
Cấu trúc chuẩn: **`Q_[MãQuestLine]_[STT]_[TênNhiệmVụ]`**

📌 **Ví dụ (Các Quests thuộc QuestLine `QL_01_JourneyToWest`):**
- `Q_QL01_01_BorrowWeapon` ➔ Quest 1: Đến Đông Hải mượn binh khí.
- `Q_QL01_02_ObtainJinguBang` ➔ Quest 2: Tìm và lấy Gậy Như Ý.
- `Q_QL01_03_ReturnFlowerMountain` ➔ Quest 3: Trở về Hoa Quả Sơn.

---

### 2.3. Quy tắc đặt tên `StepID` (Các bước trong Quest)
Cấu trúc chuẩn: **`STEP_[MãQuest]_[STT]_[TênHànhĐộng]`**

📌 **Ví dụ (Các Steps thuộc Quest `Q_QL01_01_BorrowWeapon`):**
- `STEP_Q01_01_MeetDragonKing` ➔ Step 1: Đến gặp Long Vương chào hỏi.
- `STEP_Q01_02_TryWeapons` ➔ Step 2: Thử các binh khí thường.
- `STEP_Q01_03_RejectWeapons` ➔ Step 3: Chê binh khí thường nhẹ và hỏi thần binh.

---

### 2.4. Quy tắc đặt tên `DialogueID` (Mã file hội thoại)
Cấu trúc chuẩn: **`DLG_[MãQuestLine]_[MãQuest]_S[SốStep]_[GiaiĐoạn]`**

📌 **Ví dụ:**
- `DLG_QL01_Q01_S1_Start` ➔ Thoại gặp Long Vương xin mượn binh khí.
- `DLG_QL01_Q01_S1_Incomplete` ➔ Thoại nhắc nhở nếu chưa chọn xong vũ khí.
- `DLG_QL01_Q01_S1_Complete` ➔ Thoại sau khi thử vũ khí xong.
- `DLG_NPC_DragonKing_Default` ➔ Thoại dạo với Long Vương khi không có quest.

---

### 2.5. Quy tắc đặt tên `NodeID` Cho Cuộc Trò Chuyện 2 Hoặc Nhiều Người

#### 🔹 Cách 1: Đặt Tên Theo STT Số Tuần Tự (Nhanh & Gọn Nhất)
Cấu trúc: **`node_[STT]`** hoặc **`node_[STT]_[TênNhánh]`**

📌 **Ví dụ:**
- `node_01`: Long Vương chào Ngộ Không.
- `node_02`: Ngộ Không đáp lời hỏi mượn binh khí.
- `node_03`: Long Vương đem binh khí thường ra thử.
- `node_04_agree`: (Nhánh người chơi chọn Đồng ý) ➔ Ngộ Không nhấc thử thấy nhẹ.
- `node_04_refuse`: (Nhánh người chơi chọn Từ chối) ➔ Ngộ Không chê vũ khí phế thải.

---

#### 🔹 Cách 2: Kèm Mã Nhân Vật + Nội Dung Short (Rõ Ràng Nhất Khi Tra Cứu)
Cấu trúc: **`node_[STT]_[MãNgườiNói]_[HànhĐộng]`**

📌 **Ví dụ:**
- `node_01_DragonKing_Greeting` ➔ Long Vương chào mừng.
- `node_02_Wukong_AskWeapon` ➔ Ngộ Không hỏi mượn binh khí.
- `node_03_DragonKing_ShowSword` ➔ Long Vương đem kiếm ra.
- `node_04_Wukong_RejectSword` ➔ Ngộ Không chê kiếm nhẹ.
- `node_05_DragonKing_MentionJinguBang` ➔ Long Vương nhắc tới Định Hải Thần Kim.

---

### 2.6. Quy tắc cho Thoại Mặc Định NPC (`Default Dialogue`)

#### 🔹 Cấu trúc tên DialogueID: **`DLG_NPC_[TênNPC]_Default`**
- Ví dụ: `DLG_NPC_DragonKing_Default`, `DLG_NPC_Blacksmith_Default`.

#### 🔹 Dạng 1: Thoại xã giao 1 câu đơn giản (Single Line)
- **`NodeID`**: `node_default` (hoặc `node_01`).
- **`Text`**: *"Gần đây vùng biển này yên bình quá, Đại Thánh ghé chơi sao?"*.
- **`Choices`**: `[]` (Mảng rỗng ➔ Bấm phím bất kỳ là đóng thoại).

#### 🔹 Dạng 2: Thoại NPC Cửa Hàng / Menu Lựa Chọn
- **`node_start`**: 
  - `Text`: *"Chào Đại Thánh, ta có nhiều thần binh bảo giáp lắm, ngài muốn xem thử không?"*
  - `Choices`:
    - Choice 1: `Text`: *"1. Mở cửa hàng"* ➔ `action_type`: `OpenShop` (`5`), `param`: `Shop_Armor`
    - Choice 2: `Text`: *"2. Tạm biệt"* ➔ `action_type`: `CloseDialogue` (`6`)

---

### 2.7. Quy tắc đặt tên `TextID` / `TextKey` (Khóa dịch đa ngôn ngữ)
- **Cho câu thoại:** `TXT_DLG_[MãDialogue]_[MãNode]`
- **Cho nút lựa chọn:** `TXT_CHOICE_[MãDialogue]_[TênLựaChọn]`

📌 **Ví dụ:**
- `TXT_DLG_QL01_Q01_S1_START_NODE_01` ➔ *"Hải Tộc đang gặp nạn..."*
- `TXT_CHOICE_QL01_Q01_S1_AGREE` ➔ *"Lão Tôn sẵn sàng giúp ngài!"*
- `TXT_DLG_NPC_DRAGONKING_DEFAULT` ➔ *"Vùng biển này dạo này yên bình quá..."*

---

## 📊 3. Danh Sách Các Cột Cần Có Trong Excel (3-Tier Structure)

### 📜 3.1. File `QuestLines.xlsx` (Nhiệm Vụ 3 Cấp)

#### Sheet `QuestLines`:
- `ID`, `Name`.

#### Sheet `Quests`:
- `ID`, `QuestLineID`, `Name`, `Description`, `ChapterID`, `PrerequisiteQuestIDs`, `RequiredLevel`, `QuestType`, `RewardID`.

#### Sheet `Steps`:
- `ID`, `QuestID`, `ActorID`, `Type`, `Description`, `DialogueBeforeStep`, `IncompleteDialogue`, `CompleteDialogue`, `TargetID`, `RequiredAmount`.

---

### 📄 3.2. File `Dialogues.xlsx` (Hội Thoại Node Tree)

#### Sheet `Dialogues`:
| Cột | Bắt buộc | Mô tả |
| :--- | :---: | :--- |
| `ID` | ✅ | Mã ID cuộc thoại (VD: `DLG_QL01_Q01_S1_Start`) |
| `Type` | ✅ | Loại thoại (`Start`, `Normal`, `Completion`, `Default`) |

#### Sheet `Nodes`:
| Cột | Bắt buộc | Mô tả |
| :--- | :---: | :--- |
| `NodeID` | ✅ | ID Node trong cuộc thoại (VD: `node_01_DragonKing_AskHelp`) |
| `DialogueID` | ✅ | ID cuộc thoại sở hữu Node (VD: `DLG_QL01_Q01_S1_Start`) |
| `ActorID` | ✅ | ID người nói (VD: `Dragon_king_Eastern_Sea`) |
| `Text` | ✅ | Câu thoại hiển thị |
| `NextNodeID` | ⚪ | ID Node tiếp theo sẽ tự chuyển sang khi đọc xong (không dùng Choice) |

#### Sheet `Choices` *(Không cần cột ID)*:
| Cột | Bắt buộc | Mô tả |
| :--- | :---: | :--- |
| `NodeID` | ✅ | ID Node sở hữu lựa chọn này (VD: `node_01_DragonKing_AskHelp`) |
| `Text` | ✅ | Chữ hiển thị trên nút lựa chọn (VD: *"1. Ta sẵn sàng giúp!"*) |
| `ActionChoiceType` | ✅ | Hành động (`NextNode`, `AcceptQuest`, `Reject`, `CompleteStep`, `OpenShop`, `CloseDialogue`...) |
| `TargetNodeID` | ⚪ | ID Node nhảy tới (khi `NextNode`) |
| `Param` | ⚪ | Tham số phụ đi kèm (VD: ID Quest `Q_QL01_01_BorrowWeapon` khi `AcceptQuest`) |

> 💡 **Lưu ý:** Sheet `Choices` **KHÔNG CẦN CỘT `ID`** vì các lựa chọn sẽ được gom tự động vào mảng `"choices": [...]` bên trong từng Node tương ứng.

---

## 📊 4. Cheatsheet Bảng Đặt Tên Tra Cứu Nhanh

| Cấp Bậc | Cấu trúc tên chuẩn | Ví dụ mẫu |
| :--- | :--- | :--- |
| **1. QuestLine** | `QL_[STT]_[TênChương]` | `QL_01_JourneyToWest` |
| **2. Quest** | `Q_[MãQuestLine]_[STT]_[TênQuest]` | `Q_QL01_01_BorrowWeapon` |
| **3. Step** | `STEP_[MãQuest]_[STT]_[TênHànhĐộng]` | `STEP_Q01_01_MeetDragonKing` |
| **4. Dialogue Quest** | `DLG_[MãQuestLine]_[MãQuest]_S[STT]_[GiaiĐoạn]` | `DLG_QL01_Q01_S1_Start` |
| **4. Dialogue Default**| `DLG_NPC_[TênNPC]_Default` | `DLG_NPC_DragonKing_Default` |
| **5. Node** | `node_[STT]` / `node_[STT]_[MãNgườiNói]_[HànhĐộng]` | `node_01`, `node_01_DragonKing_Greeting` |
| **6. Choice** | *(Không cần ID)* | Nằm trong Sheet `Choices` trỏ theo `NodeID` |

---

## 🚀 5. Hướng Dẫn Build Dữ Liệu JSON
Mở Terminal tại thư mục `Tool/` và chạy:
```bash
python main.py
```
Tool sẽ tự động băm mã Hash ngôn ngữ, cập nhật `LocKeys.cs` và xuất các file JSON chuẩn vào `Assets/Data/Narrative/`.
