# File Management Benchmarks

The file information is given in the `.csv` files, where the columns are:
1. basename
2. extension
3. path
4. size in bytes
5. modification time
6. NOT_USED
7. executable
8. readable
9. writable
10. NOT_USED
11. is an example for the PBE task
12. id
13. group

The `actions` used in the file management benchmarks are common file operations, e.g., move, rename, copy, delete, tar, etc.

The `path_mapping` is required in the Hades algorithm to specify the correspondence between input paths and output paths.
If the `path_mapping` file is not given, the input and output paths are the same.

The algorithm of Hades seems not well scalable w.r.t. the number of examples and path length, which makes it timed out on two benchmarks.
We manually simplified the data (the two `-simplified` folders).
The algorithm could successfully solve the benchmarks under the simplified settings.
