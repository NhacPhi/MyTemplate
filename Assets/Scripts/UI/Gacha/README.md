# Kế hoạch cấu trúc Database cho Hệ thống Gacha

Dựa trên yêu cầu của bạn, hệ thống Gacha cần đảm bảo 3 yếu tố cốt lõi:
1. Pool Character/Weapon với tỷ lệ rớt độc lập.
2. Cho phép người chơi "định chuẩn" (chọn trước nhân vật/vũ khí mong muốn).
3. Cơ chế bảo hiểm (Pity System) với mức trần là 40 lần quay.
4. Quản lý dữ liệu thông qua file `Tool/data/Gacha.xlsx` và xuất ra JSON thông qua Python script.

Dưới đây là kế hoạch chi tiết cho cấu trúc dữ liệu.

## 1. Tại sao Cấu trúc file Excel (`Gacha.xlsx`) lại được chia thành 3 sheet?

Trong thiết kế game, việc tổ chức dữ liệu rất quan trọng để đảm bảo tính mở rộng và dễ dàng chỉnh sửa cho Game Designer. Cấu trúc 3 sheet được chọn vì các lý do sau:

- **Tránh trùng lặp dữ liệu (Normalization)**: Nếu gộp tất cả vào 1 bảng, bạn sẽ phải ghi lặp lại thông tin như `PityLimit`, `Cost` cho từng nhân vật. Tách ra sheet **Banners** giúp mỗi thông tin chỉ lưu 1 lần.
- **Tách biệt "Tỷ lệ độ hiếm" và "Vật phẩm cụ thể"**: Sheet **Rates** quyết định khả năng ra 5 sao (1.6%), còn sheet **Pools** chứa danh sách các nhân vật 5 sao. Việc tách biệt này giúp Designer có thể thay đổi tỷ lệ ra 5 sao mà không cần đụng đến danh sách nhân vật, hoặc ngược lại (thêm bớt nhân vật mà không sợ làm tổng tỷ lệ vượt quá 100%).
- **Sử dụng hệ số Weight thay vì % trực tiếp trong Pool**: Trong sheet **Pools**, thay vì bắt Designer tính chính xác phần trăm (ví dụ: 33.33%), ta dùng `Weight` (trọng số). Nếu có 3 nhân vật 5 sao với Weight = 100, hệ thống tự hiểu tỉ lệ chia đều là 1:1:1. Rất linh hoạt khi muốn thêm nhân vật mới vào pool.

### Chi tiết 3 Sheet:

#### Sheet 1: `Banners` (Cấu hình cơ bản của các Banner)
Sheet này định nghĩa các thông tin chung của một banner gacha.

| BannerID | Name | BannerType | PityLimit | CostType | CostAmount | AllowSelection |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `banner_char_01` | Mùa Hè Rực Rỡ | Character | 40 | Ticket | 1 | TRUE |
| `banner_weap_01` | Vũ Khí Truyền Thuyết | Weapon | 40 | Ticket | 1 | TRUE |

- **BannerType**: Phân loại banner (`Character` hoặc `Weapon`).
- **PityLimit**: Số lần quay tối đa để kích hoạt bảo hiểm (40).
- **AllowSelection**: Banner này có cho phép người chơi chọn trước mục tiêu mong muốn hay không (`TRUE`/`FALSE`).

### Sheet 2: `Rates` (Tỷ lệ rớt theo độ hiếm)
Xác định tỷ lệ ra các vật phẩm theo độ hiếm. Có thể áp dụng chung hoặc riêng cho từng banner.

| BannerID | Rarity | BaseRate | PityGuarantee |
| :--- | :--- | :--- | :--- |
| `banner_char_01` | 5 | 0.016 | TRUE |
| `banner_char_01` | 4 | 0.134 | FALSE |
| `banner_char_01` | 3 | 0.850 | FALSE |

- **BaseRate**: Tỷ lệ rớt cơ bản (Ví dụ 0.016 = 1.6%).
- **PityGuarantee**: Đánh dấu độ hiếm nào sẽ được kích hoạt khi chạm mốc bảo hiểm 40 lần (Thường là vật phẩm 5 sao).

### Sheet 3: `Pools` (Danh sách vật phẩm trong Banner)
Chứa danh sách các nhân vật và vũ khí có thể xuất hiện trong từng banner.

| BannerID | ItemID | Rarity | IsRateUp | IsSelectableTarget | Weight |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `banner_char_01` | `char_A_5star` | 5 | TRUE | TRUE | 100 |
| `banner_char_01` | `char_B_5star` | 5 | FALSE | FALSE | 50 |
| `banner_char_01` | `char_C_4star` | 4 | TRUE | FALSE | 100 |

- **IsRateUp**: Đánh dấu vật phẩm đang được tăng tỷ lệ.
- **IsSelectableTarget**: Các vật phẩm mà người chơi CÓ THỂ chọn làm mục tiêu mong muốn (Định chuẩn).
- **Weight**: Trọng số xuất hiện trong cùng một độ hiếm (Nếu Roll trúng 5 sao, hệ thống dựa vào Weight để bốc xem ra nhân vật 5 sao nào).

---

## 2. Cấu trúc JSON Output dự kiến

Sau khi chạy tool Python (`main.py` -> `gacha_builder.py`), dữ liệu JSON xuất ra sẽ có dạng gộp để Client dễ đọc:

```json
{
  "banner_char_01": {
    "bannerId": "banner_char_01",
    "name": "Mùa Hè Rực Rỡ",
    "type": "Character",
    "pityLimit": 40,
    "cost": {"type": "Ticket", "amount": 1},
    "allowSelection": true,
    "rates": {
      "3": {"baseRate": 0.85, "isGuarantee": false},
      "4": {"baseRate": 0.134, "isGuarantee": false},
      "5": {"baseRate": 0.016, "isGuarantee": true}
    },
    "pool": [
      {
        "itemId": "char_A_5star",
        "rarity": 5,
        "isRateUp": true,
        "isSelectableTarget": true,
        "weight": 100
      },
      ...
    ]
  }
}
```

---

## 3. Quy trình thực hiện (Các bước tiếp theo)

1. Tạo file `Tool/data/Gacha.xlsx` với 3 sheet như trên.
2. Viết file `Tool/src/builders/gacha_builder.py` để đọc file Excel này và build ra file `GachaConfig.json`.
3. Thêm lệnh gọi `GachaBuilder` vào trong `Tool/main.py`.
4. Trong Unity, tạo class C# `GachaConfig` và viết Data Model (`GachaDataModel`) để load JSON này vào Runtime.

## Giải thích Logic Bảo hiểm (Pity System 50/50)

Dựa trên cơ chế "bảo hiểm 40 lần và không lặp lại việc lệch rate 2 lần liên tiếp", chúng ta sẽ sử dụng các thông số trên như sau:

1. **PityLimit = 40 (Sheet Banners)**:
   - Trong quá trình quay, hệ thống (C# Client) sẽ đếm số lần bạn đã quay mà chưa ra SSR (5 sao).
   - Nếu bộ đếm này chạm mốc 40, lần quay thứ 40 sẽ bị ép (override) tỉ lệ rớt SSR thành 100%.

2. **IsRateUp và IsSelectableTarget (Sheet Pools)**:
   - Khi hệ thống quay trúng SSR (do may mắn hoặc do bảo hiểm 40 lần), nó sẽ bốc ngẫu nhiên 1 nhân vật trong nhóm SSR.
   - Lúc này, tỉ lệ "trúng rate" (quay ra nhân vật định chuẩn/nhân vật banner) thường được thiết lập là 50%.
   - **Lệch Rate**: Nếu người chơi quay ra một nhân vật SSR có `IsSelectableTarget = FALSE` hoặc `IsRateUp = FALSE`. Hệ thống (trong Unity) sẽ lưu lại một biến trạng thái (ví dụ: `isNextSSRGuaranteed = true`).
   - **Trúng Rate (Bảo hiểm 100%)**: Ở lần quay ra SSR tiếp theo (có thể là chạm mốc 40 lần thứ hai, hoặc ra sớm hơn), do `isNextSSRGuaranteed` đang là `true`, hệ thống sẽ bỏ qua việc bốc ngẫu nhiên toàn bộ pool SSR, mà CHỈ bốc trong nhóm SSR có `IsSelectableTarget = TRUE` (hoặc `IsRateUp = TRUE`).
   - Sau khi nhận được nhân vật định chuẩn, biến `isNextSSRGuaranteed` sẽ reset về `false`, vòng lặp 50/50 bắt đầu lại.

Với cấu trúc dữ liệu như file JSON hiện tại, hệ thống Unity hoàn toàn có đủ mọi tham số cần thiết để lập trình logic này.
