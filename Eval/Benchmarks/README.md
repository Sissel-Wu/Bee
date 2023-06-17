# Benchmarks

The benchmarks in this folder are used to evaluate the tools under `Eval`.

Each of the three folders (`File`, `Spreadsheet`, `XML`) contains around 20 subdirectories. Each subdirectory, e.g., `File\hades-f01` is a benchmark(PBE task).

Each benchmark contains at least one description file,
some input files as example input (e.g., `*.csv`), and some output files as example output (e.g., `actions`).
The exact format and how to interpret the file vary according to the domain and tool implementation.
The used formats and provided constants can be found in `config.json`.

Some benchmarks have variants (with `-simplified`, `-1`, `-2` as suffix), which are modified from the original ones by adding/removing/adjusting examples.
These variants are used to analyze the tools further when experiments on the original benchmarks failed or were over-fitted.
