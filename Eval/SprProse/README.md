# Prose-Spreadsheet

In the file management domain, the DSL for Prose is from an [existing work](https://dl.acm.org/doi/10.1145/1993498.1993536).

This DSL adopts the following assumptions:
1. The row-major order of cells is preserved (i.e., in both input spreadsheet and output spreadsheet).
2. No new values are introduced.
3. The output spreadsheet can be unified from some sub-blocks, where each sub-block has a predefined layout.

Some of our benchmarks violate the above assumptions:
1. New values may be introduced.
2. The sub-block may not be in the predefined layout.
