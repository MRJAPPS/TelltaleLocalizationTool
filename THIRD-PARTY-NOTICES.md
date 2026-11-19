# Third-party notices

This file identifies third-party material and dependencies used by
TelltaleLocalizationTool. Their terms are separate from the custom license
for this project's original contributions. When redistributing a build,
include any notices required by the licenses of the components actually
included in that build.

## Source included in this repository

### PersianMap

`Localize/PersianMap.cs` adapts code from Sean Ed-Man's Stack Overflow
answer, [“Get persian characters initial medial final unicode”](https://stackoverflow.com/a/37100583),
posted May 8, 2016. The answer is licensed under
[CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/).
The code was adapted for this project. This file and adaptations of that
material remain under CC BY-SA 3.0, including its attribution and
ShareAlike terms. This project's custom license does not add conditions
to that material.

## Referenced projects and tools

- [Telltale Texture Tool](https://github.com/Telltale-Modding-Group/Telltale-Texture-Tool)
  is credited as a technical foundation. Its repository provides an MIT
  license. Any copied or adapted portions remain subject to that license
  and its required notice.
- [TelltaleToolLib](https://github.com/LucasSaragosa/TelltaleToolLib) is
  cited in source as a reference for Telltale metadata symbols. No license
  file was found in the repository during this review. This project does
  not claim to relicense TelltaleToolLib material.
- [TelltaleInspector](https://github.com/LucasSaragosa/TelltaleInspector)
  is credited as a file-analysis reference; it is not a code dependency
  listed by this project.
- [AMD Compressonator](https://github.com/GPUOpen-Tools/compressonator)
  and [AngelCode BMFont](https://www.angelcode.com/products/bmfont/) are
  external tools configured by the user. Their binaries are not included
  in this repository; follow their own terms if you obtain or distribute
  them.

## NuGet dependencies

The versions below are declared in `TelltaleLocalizationTool.csproj`.
License information was checked in the package metadata and upstream
project pages where available.

- `BCnEncoder.Net 2.3.0` — MIT OR Unlicense.
- `Newtonsoft.Json 13.0.4` — MIT.
- `PVRTexLib.NET 1.0.3` — MIT for the .NET wrapper. This does not grant
  rights to the separate PVRTexLib/PVRTexTool binaries.
- `Hexa.NET.DirectXTex.Standalone 2.0.0`,
  `HexaGen.Runtime 1.1.4`, and `HexaGen.Runtime.COM 1.1.4` — MIT.
- `Spectre.Console 0.57.2`, `Spectre.Console.Ansi 0.57.2`, and
  `Spectre.Console.Cli 0.57.2` — MIT.
- `CommunityToolkit.HighPerformance 8.4.0` — MIT.
- `Microsoft.Extensions.* 2.1.0` packages — Apache License 2.0.
- `System.Memory 4.5.0` — MIT.

### MIT notices

For any MIT-licensed components included in a distribution, retain the
following notices and this permission notice:

- Telltale Texture Tool: Copyright (c) 2024 Telltale Modding Group.
- BCnEncoder.NET: Nominom.
- .NET Community Toolkit and System.Memory: .NET Foundation and
  Contributors.
- Hexa.NET.DirectXTex and HexaGen: Copyright (c) 2022 and 2023
  respectively, Juna Meinhold.
- Newtonsoft.Json: Copyright (c) 2007 James Newton-King.
- PVRTexLib.NET: YingFengTingYu.
- Spectre.Console packages: Patrik Svensson, Phil Scott, Nils Andresen,
  Cédric Luthi, and, for Spectre.Console.Cli, Frank Ray.

    Permission is hereby granted, free of charge, to any person obtaining a
    copy of this software and associated documentation files (the
    “Software”), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be included
    in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS
    OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

### Apache License 2.0

The `Microsoft.Extensions.* 2.1.0` packages identify the .NET Foundation's
Apache License 2.0 terms in their package metadata. Include a copy of the
[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0) with any
distribution containing those packages and retain applicable notices.