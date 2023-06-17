# Bee-XML

In the XML domain, we adopt two schemas: (a) represents each XML element in a row; (b) represents each attribute in a row.
They are linked via a key column, i.e., the `owner` column in (b).

For instance, below is the XML tree and corresponding tables of example input XML in the benchmark of online01.

```
<root>
  <item name="Bonny" size="">
    <title></title>
    <artist>Bonnie Tyler</artist>
    <country>UK</country>
    <company>CBS Records</company>
    <price>19.90</price>
    <year>1928</year>
  </item>
  <item name="Bonny" size="">
    <title>Bonny</title>
    <artist>Bonnie Tyler</artist>
    <country>UK</country>
    <company>CBS Records</company>
    <price>19.90</price>
    <year>1928</year>
  </item>
  <item name="Alice" size="">
    <title>Alice</title>
    <artist>Alice Tyler</artist>
    <country>US</country>
    <company>CNN</company>
    <price>20.00</price>
    <year>1998</year>
  </item>
  <item name="Cindy" size="">
    <title></title>
    <artist>Alice Cindy</artist>
    <country>Japan</country>
    <price>10.1</price>
    <year>2005</year>
  </item>
</root>
```

```
------------------------------------------------------------------------------
|Self         |Parent       |Prev         |Next         |Tag    |Text        |
|ElementEntity|ElementEntity|ElementEntity|ElementEntity|String |String      |
------------------------------------------------------------------------------
|e0           |-1           |-1           |-1           |root   |            |
|e1           |e0           |-1           |e8           |item   |            |
|e2           |e1           |-1           |e3           |title  |            |
|e3           |e1           |e2           |e4           |artist |Bonnie Tyler|
|e4           |e1           |e3           |e5           |country|UK          |
|e5           |e1           |e4           |e6           |company|CBS Records |
|e6           |e1           |e5           |e7           |price  |19.90       |
|e7           |e1           |e6           |-1           |year   |1928        |
|e8           |e0           |e1           |e15          |item   |            |
|e9           |e8           |-1           |e10          |title  |Bonny       |
|e10          |e8           |e9           |e11          |artist |Bonnie Tyler|
|e11          |e8           |e10          |e12          |country|UK          |
|e12          |e8           |e11          |e13          |company|CBS Records |
|e13          |e8           |e12          |e14          |price  |19.90       |
|e14          |e8           |e13          |-1           |year   |1928        |
|e15          |e0           |e8           |e22          |item   |            |
|e16          |e15          |-1           |e17          |title  |Alice       |
|e17          |e15          |e16          |e18          |artist |Alice Tyler |
|e18          |e15          |e17          |e19          |country|US          |
|e19          |e15          |e18          |e20          |company|CNN         |
|e20          |e15          |e19          |e21          |price  |20.00       |
|e21          |e15          |e20          |-1           |year   |1998        |
|e22          |e0           |e15          |-1           |item   |            |
|e23          |e22          |-1           |e24          |title  |            |
|e24          |e22          |e23          |e25          |artist |Alice Cindy |
|e25          |e22          |e24          |e26          |country|Japan       |
|e26          |e22          |e25          |e27          |price  |10.1        |
|e27          |e22          |e26          |-1           |year   |2005        |
------------------------------------------------------------------------------
---------------------------------------------
|Self           |Owner        |Key   |Val   |
|AttributeEntity|ElementEntity|String|String|
---------------------------------------------
|a0             |e1           |name  |Bonny |
|a1             |e1           |size  |      |
|a2             |e8           |name  |Bonny |
|a3             |e8           |size  |      |
|a4             |e15          |name  |Alice |
|a5             |e15          |size  |      |
|a6             |e22          |name  |Cindy |
|a7             |e22          |size  |      |
---------------------------------------------
```

The actions are similar to common XML manipulation APIs.
* deleteElem: `<"deleteElem", elem: Id (ElementEntity)>`
* modifyText: `<"modifyText", elem: Id (ElementEntity), newText: String>`
* modifyAttr: `<"modifyAttr", elem: Id (AttributeEntity), newVal: String>`
* modifyTag: `<"modifyTag", elem: Id (ElementEntity), newTag: String>`
* addElem: `<"addElem", elem: Id (ElementEntity), newTag: String, newText: String>`
* addElemAbove: `<"addElemAbove", elem: Id (ElementEntity), newTag: String, newText: String>`
* addAttr: `<"addAttr", elem: Id (ElementEntity), key: String, val: String>`
* wrap: `<"wrap", elem: Id (ElementEntity), tag: String>`
* moveBelow: `<"moveBelow", elem: Id (ElementEntity), target: Id (ElementEntity)>`
* appendChild: `<"appendChild", elem: Id (ElementEntity), target: Id (ElementEntity)>`

`XmlEntities.cs` specifies the detailed schema, and `XmlActions` specifies the actions using the annotation APIs provided by Bee.
