# Spreadsheet Transformation Benchmarks

For `Bee`, the spreadsheet information is given in the `.csv` files.
Each line stands for a spreadsheet cell.
Not all the cells in the original spreadsheet are taken.
Typically, the "data" region is selected.
The columns in the `.csv` file stand for:
1. id (NOT USED)
2. row number of the cell
3. column number of the cell
4. row header of the cell
5. column header of the cell
6. NOT USED
7. NOT USED
8. cell value
9. NOT USED
10. is an example for the PBE task

Only one action `fill <value> <row> <column>` is used in `actions`.
