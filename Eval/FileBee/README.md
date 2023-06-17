# Bee-File

In the file management domain, Bee uses one table schema.
Each file (excluding folder) is a row in the table.
Attributes include basename, extension, file path, modTime, etc.

For instance, below is the table of example input files in Hades01.

```
----------------------------------------------------------------------------------------------------------------------------------------
|Fid   |Basename|Extension|FilePath|Size  |ModTime         |Readable|Writable|Executable|Group  |Year |Month|Day  |YearS |MonthS|DayS  |
|MyFile|String  |String   |String  |Int32 |DateTime        |Boolean |Boolean |Boolean   |String |Int32|Int32|Int32|String|String|String|
----------------------------------------------------------------------------------------------------------------------------------------
|i1    |members |csv      |./      |8132  |2016/6/5 0:00:00|True    |True    |False     |public |2016 |6    |5    |2016  |6     |5     |
|i2    |diaries |csv      |./      |202818|2016/6/5 0:00:00|True    |True    |False     |secrete|2016 |6    |5    |2016  |6     |5     |
|i3    |manual  |pdf      |./      |20329 |2016/6/5 0:00:00|True    |True    |False     |public |2016 |6    |5    |2016  |6     |5     |
|i4    |logs    |txt      |./      |996   |2016/6/5 0:00:00|True    |True    |False     |public |2016 |6    |5    |2016  |6     |5     |
----------------------------------------------------------------------------------------------------------------------------------------
```

The user actions include common user actions managing files.

* chmod: `<"chmod", file: Id, mod: String>`, e.g., `chmod file1 "+x"`
* copy: `<"copy", file: Id, path: String>`, e.g., `copy file1 "new/path/"`
* unzip: `<"unzip", file: Id, path: String>`, e.g., `unzip file1 "path/to/extract"`
* move: `<"move", file: Id, path: String>`, e.g., `move file1 "new/path"`
* rename: `<"rename", file: Id, newName: String>`, e.g., `rename file1 "new name"`
* delete: `<"delete", file: Id>`, e.g., `delete file1`
* chgrp: `<"chgrp", file: Id, newGroup: String>`, e.g., `chgrp file1 "new group"`
* chext: `<"chext", file: Id, newExtension: String>`, e.g., `chext file1 "new extension"`
* tar: `<"tar", files:IdSet, fileName: String>`, e.g., `tar {file1,file2,file3,...} "compressed.tar"`

`FileEntity.cs` specifies the detailed schema and actions using the annotation APIs provided by Bee.
