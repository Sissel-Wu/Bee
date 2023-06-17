# Prose-File

In the file management domain, the DSL for Prose is from an existing work [Hades](https://dl.acm.org/doi/10.1145/2908080.2908088).
Hades is for hierarchical data transformation.
Hades adopts the assumption that transformation on hierarchically structured data can be viewed as:
1. Split the data into a set of paths.
2. Perform transformation on each path independently.
3. Merge the transformed paths into a hierarchical data structure.

Some benchmarks may violate this assumption. For example, the path transformation may not be independent.

Hades can be customized for different domains, e.g., file system, XML tree.
The common part of Hades is implemented in the `LibProse` directory.
The customizations in `FileProse` and `XMLProse` include how to split data into paths and merge. The DSL also needs domain-specific predicates and transformations.

For the file management domain, the predicates include judgment on file extensions, modification time, paths, etc.
The transformation includes deletion, renaming, etc.
