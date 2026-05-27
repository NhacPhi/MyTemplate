import pandas as pd

def main():
    shop_products = [
        # RareLimit
        ["SHOP_RARE_01", "RareLimit", "JadeShop", "SingleItem", "SunWukong", 1, "Jade", 100, 150, 1, "Daily", "", "", True, 1],
        ["SHOP_RARE_02", "RareLimit", "JadeShop", "SingleItem", "Nimbus_Cudgel", 1, "Jade", 500, 0, 1, "Lifetime", "", "", True, 2],
        ["SHOP_RARE_03", "RareLimit", "JadeShop", "SingleItem", "TangSanZang", 1, "Jade", 10000, 12000, 5, "Daily", "", "", True, 3],
        ["SHOP_RARE_04", "RareLimit", "JadeShop", "SingleItem", "common_exp", 1, "Jade", 5000, 0, 5, "Weekly", "", "", True, 4],

        ["SHOP_RARE_05", "RareLimit", "JadeShop", "SingleItem", "Fried_Radish_Balls", 1, "Jade", 1000, 12000, 5, "Daily", "", "", True, 3],
        ["SHOP_RARE_06", "RareLimit", "JadeShop", "SingleItem", "Dragon_Bread_Noodies", 1, "Jade", 1200, 0, 5, "Weekly", "", "", True, 4],

        ["SHOP_RARE_07", "RareLimit", "CoinShop", "SingleItem", "Murshroom_Steamed_Buns", 1, "Coin", 100, 150, 1, "Daily", "", "", True, 1],
        ["SHOP_RARE_08", "RareLimit", "CoinShop", "SingleItem", "Tofu_Soup", 1, "Coin", 500, 0, 1, "Lifetime", "", "", True, 2],
        ["SHOP_RARE_09", "RareLimit", "CoinShop", "SingleItem", "Lamian_Noodies", 1, "Coin", 10000, 12000, 5, "Daily", "", "", True, 3],
        ["SHOP_RARE_10", "RareLimit", "CoinShop", "SingleItem", "fine_exp", 1, "Coin", 5000, 0, 5, "Weekly", "", "", True, 4],

        ["SHOP_RARE_11", "RareLimit", "CoinShop", "SingleItem", "rare_exp", 1, "Coin", 1000, 12000, 5, "Daily", "", "", True, 3],
        ["SHOP_RARE_12", "RareLimit", "CoinShop", "SingleItem", "ZhuBajie_avatar", 1, "Coin", 1200, 0, 5, "Weekly", "", "", True, 4],
        
        # TimeSale
        ["SHOP_TIME_01", "TimeSale", "JadeShop", "SingleItem", "supreme_exp", 10, "Jade", 50, 100, -1, "None", "2026-05-01", "2026-05-30", True, 1],
        ["SHOP_TIME_02", "TimeSale", "JadeShop", "SingleItem", "Garnet_Gemstone", 1, "Jade", 80, 120, -1, "None", "2026-05-01", "2026-05-30", True, 2],
        ["SHOP_TIME_03", "TimeSale", "CoinShop", "SingleItem", "fine_exp", 10, "Coin", 5000, 8000, -1, "None", "2026-05-01", "2026-05-30", True, 3],
        ["SHOP_TIME_04", "TimeSale", "CoinShop", "SingleItem", "Lamian_Noodies", 5, "Coin", 8000, 10000, -1, "None", "2026-05-01", "2026-05-30", True, 4],
        
        # ComboPack
        ["SHOP_COMBO_01", "ComboPack", "JadeShop", "Bundle", "BUNDLE_JADE_01", 1, "Jade", 300, 500, 1, "Weekly", "", "", True, 1],
        ["SHOP_COMBO_02", "ComboPack", "JadeShop", "Bundle", "BUNDLE_JADE_02", 1, "Jade", 600, 800, 1, "Monthly", "", "", True, 2],
        ["SHOP_COMBO_03", "ComboPack", "CoinShop", "Bundle", "BUNDLE_COIN_01", 1, "Coin", 20000, 30000, 1, "Weekly", "", "", True, 3],
        ["SHOP_COMBO_04", "ComboPack", "CoinShop", "Bundle", "BUNDLE_COIN_02", 1, "Coin", 50000, 70000, 1, "Monthly", "", "", True, 4]
    ]

    cols1 = ["ProductID", "ShopCategory", "SubCategory", "SellType", "ReferenceID", "ItemAmount", "CurrencyType", "Price", "OriginalPrice", "LimitCount", "LimitType", "StartTime", "EndTime", "IsActive", "SortOrder"]
    df1 = pd.DataFrame(shop_products, columns=cols1)

    bundle_contents = [
        ["BUNDLE_JADE_01", "SunWukong", 5],
        ["BUNDLE_JADE_01", "supreme_exp", 10],
        
        ["BUNDLE_JADE_02", "Nimbus_Cudgel", 1],
        ["BUNDLE_JADE_02", "Garnet_Gemstone", 5],
        
        ["BUNDLE_COIN_01", "TangSanZang", 5],
        ["BUNDLE_COIN_01", "fine_exp", 20],
        
        ["BUNDLE_COIN_02", "Tofu_Soup", 10],
        ["BUNDLE_COIN_02", "common_exp", 50]
    ]

    cols2 = ["BundleID", "ItemID", "Amount"]
    df2 = pd.DataFrame(bundle_contents, columns=cols2)

    output_path = r"d:\UnityUdeme\MyTemplate\MyTemplate\Tool\data\Shop.xlsx"
    with pd.ExcelWriter(output_path) as writer:
        df1.to_excel(writer, sheet_name="ShopProducts", index=False)
        df2.to_excel(writer, sheet_name="BundleContents", index=False)
    print("Success")

if __name__ == "__main__":
    main()
