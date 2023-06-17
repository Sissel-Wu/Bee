# Benchmarks

Please go to the `Benchmarks` subdirectory for information about our benchmarks.

# Setup

The evaluation projects are managed as a Visual Studio solution. Running this solution needs a Windows environment.

Make sure you can open the `.sln` file in Visual Studio and have the dependent NuGet packages installed with the correct versions.

Besides, this project relies on some external components:

- Please properly include `libz3.dll` in the library path.
- Install [PostSharp](https://www.postsharp.net/) plug-in for Visual Studio. The community version is enough.
- Please properly include the `BeeInterfaces.dll` (should be auto-configured by VS).

To run the evaluation, use the Unit Test function bundled in Visual Studio.

# Other Notes

You may see how the size and complexity of codes can be reduced in the `*Bee` projects compared to the `*Prose` projects.
