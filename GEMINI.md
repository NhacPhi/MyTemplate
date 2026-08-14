# Repository Rules for MyTemplate

## Excel Modification Rules (CRITICAL)
- **NEVER** use `df.to_excel()` or pandas to overwrite `.xlsx` files directly. Pandas destroys cell colors, fills, column widths, font styles, formatting, and reorders sheets.
- **ALWAYS** use `openpyxl` to update cell values in-place (`ws.cell(row, col).value = new_val`).
- **Sheet Isolation:** When modifying a specific sheet, ONLY edit that target sheet. DO NOT touch, modify, or rewrite any other sheets in the workbook.
- **Preserve Sheet Order:** DO NOT change, reorder, or alter the position/sequence of sheets in any Excel workbook.
- Preserve 100% of column widths, cell colors, fills, borders, fonts, and styles when editing Excel files in `Tool/data/`.
