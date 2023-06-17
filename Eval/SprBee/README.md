# Bee-Spreadsheet

For Spreadsheet transformation, we have two schemas: (a) taking each cell as a table row and (b) taking each row in the spreadsheet as a table row.
Note that they are the same set of data. We explain the reason in our tutorial of Bee (see the root directory).

For instance, below are the tables of example input spreadsheets in the benchmark of Forum01.

```
---------------------------------------------
|Row  |Col  |RowHead|ColHead|Content|ReadOrd|
|Int32|Int32|String |String |String |Int32  |
---------------------------------------------
|1    |1    |CatA   |CatA   |CatA   |0      |
|1    |2    |CatA   |Jan    |Jan    |1      |
|1    |3    |CatA   |1.2    |1.2    |2      |
|2    |1    |CatA   |CatA   |CatA   |3      |
|2    |2    |CatA   |Jan    |Feb    |4      |
|2    |3    |CatA   |1.2    |2.5    |5      |
|3    |1    |CatA   |CatA   |CatA   |6      |
|3    |2    |CatA   |Jan    |Mar    |7      |
|3    |3    |CatA   |1.2    |3.6    |8      |
|4    |1    |CatA   |CatA   |CatA   |9      |
|4    |2    |CatA   |Jan    |Apr    |10     |
|4    |3    |CatA   |1.2    |4.9    |11     |
|5    |1    |CatB   |CatA   |CatB   |12     |
|5    |2    |CatB   |Jan    |Jan    |13     |
|5    |3    |CatB   |1.2    |2.5    |14     |
|6    |1    |CatB   |CatA   |CatB   |15     |
|6    |2    |CatB   |Jan    |Feb    |16     |
|6    |3    |CatB   |1.2    |2.9    |17     |
|7    |1    |CatB   |CatA   |CatB   |18     |
|7    |2    |CatB   |Jan    |Mar    |19     |
|7    |3    |CatB   |1.2    |3.1    |20     |
|8    |1    |CatB   |CatA   |CatB   |21     |
|8    |2    |CatB   |Jan    |Apr    |22     |
|8    |3    |CatB   |1.2    |3.5    |23     |
|9    |1    |CatC   |CatA   |CatC   |24     |
|9    |2    |CatC   |Jan    |Jan    |25     |
|9    |3    |CatC   |1.2    |5.9    |26     |
|10   |1    |CatC   |CatA   |CatC   |27     |
|10   |2    |CatC   |Jan    |Feb    |28     |
|10   |3    |CatC   |1.2    |6.9    |29     |
|11   |1    |CatC   |CatA   |CatC   |30     |
|11   |2    |CatC   |Jan    |Mar    |31     |
|11   |3    |CatC   |1.2    |10.9   |32     |
---------------------------------------------
----------------------------
|Row  |Cols1 |Cols2 |Cols3 |
|Int32|String|String|String|
----------------------------
|1    |CatA  |Jan   |1.2   |
|2    |CatA  |Feb   |2.5   |
|3    |CatA  |Mar   |3.6   |
|4    |CatA  |Apr   |4.9   |
|5    |CatB  |Jan   |2.5   |
|6    |CatB  |Feb   |2.9   |
|7    |CatB  |Mar   |3.1   |
|8    |CatB  |Apr   |3.5   |
|9    |CatC  |Jan   |5.9   |
|10   |CatC  |Feb   |6.9   |
|11   |CatC  |Mar   |10.9  |
----------------------------
```

The only user action is `Fill`, which is parameterized by the content to fill and the coordinate (row and column).
* fill: `<"fill", content: String, row: Int, col: Int>`

The `SprSheetEntities.cs` specifies the detailed schema and actions using the annotation APIs provided by Bee.
