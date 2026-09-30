# ROMulus .NET

A local-first desktop ROM collection manager for retro game consoles — C# / .NET 10 / WPF port of [ROMulous](https://github.com/Sphexi/ROMulous).

Scan, identify, enrich with metadata + cover art, organize, and **sync** your collection to whatever device you actually play on — Anbernic handhelds, Batocera setups, MiSTer FPGAs, Analogue Pocket, RetroPie, muOS, Onion OS.

No server. No cloud account. No external services to keep running. SQLite + files on disk, nothing else.

**Project status:** v0.5.0 (in development — active port from Python/PySide6).  
**Original Python project:** [Sphexi/ROMulous](https://github.com/Sphexi/ROMulous)  
**License:** [Apache License 2.0](LICENSE)

---

## Why the .NET port?

| | Python original | .NET port |
|---|---|---|
| **Runtime** | CPython 3.12 + PySide6 (~300 MB) | Self-contained native exe (~60 MB) |
| **Startup** | 2–4 s (interpreter warm-up) | < 0.5 s |
| **UI** | Qt 6 via PySide6 | WPF (Windows Presentation Foundation) |
| **Packaging** | PyInstaller --onefile | `dotnet publish --self-contained` |
| **Type safety** | Type hints + mypy/ruff | C# nullable + Roslyn analyzers |

---

## Tech Stack

| Concern | Choice |
|---|---|
| **Language** | C# 12 / .NET 10 |
| **UI** | WPF + CommunityToolkit.Mvvm (MVVM pattern) |
| **Database** | SQLite via `Microsoft.Data.Sqlite` + Dapper |
| **HTTP** | `System.Net.Http.HttpClient` (built-in) |
| **YAML** | YamlDotNet |
| **Logging** | Serilog (rolling file + console) |
| **Tests** | xUnit + FluentAssertions + Moq |
| **Build** | `dotnet publish --self-contained -r win-x64` |

---

## Project Structure

```
ROMulus.slnx
├── src/
│   ├── ROMulus.App/              # WPF application (exe)
│   │   ├── Views/                # XAML views
│   │   ├── ViewModels/           # MVVM view-models
│   │   ├── Controls/             # Custom WPF controls
│   │   └── Resources/            # Themes, artwork, icons
│   │
│   ├── ROMulus.Core/             # Business logic (class library)
│   │   ├── Models/               # C# record types (Rom, SystemInfo, DestinationProfile…)
│   │   ├── Scanner/              # Filesystem walk + L1/L2 identification
│   │   ├── Hashing/              # SHA-1/CRC32 + header stripping + archives
│   │   ├── DatParsing/           # No-Intro XML DAT parser
│   │   ├── Organizing/           # Library reorganization (preview/commit)
│   │   ├── Exporting/            # Destination profile export engine
│   │   ├── Syncing/              # 5-mode sync + 4-tier identity match (O(N+M))
│   │   ├── Importing/            # Staging-folder import (analyse → apply)
│   │   ├── Scrubbing/            # Reverse-direction DB ↔ disk verifier
│   │   ├── Metadata/             # 6-source enrichment chain
│   │   ├── Covers/               # Cover art discovery (local + libretro)
│   │   └── IO/                   # AtomicWriter (tempfile + File.Replace)
│   │
│   └── ROMulus.Infrastructure/   # DB + config (class library)
│       ├── Database/             # Schema, ConnectionFactory, Repositories
│       └── SystemRegistry/       # YAML → List<SystemInfo> loader
│
└── tests/
    ├── ROMulus.Core.Tests/        # Unit tests for Core
    └── ROMulus.Infrastructure.Tests/ # Integration tests for DB layer
```

---

## Getting Started (from source)

**Prerequisites:** .NET 10 SDK, Windows 10/11, Visual Studio 2022 (or VS Code + C# Dev Kit).

```powershell
git clone https://github.com/scorpio21/Romulous_net.git
cd Romulous_net

# Build
dotnet build ROMulus.slnx

# Run tests
dotnet test ROMulus.slnx

# Launch the app
dotnet run --project src/ROMulus.App
```

---

## Building a portable ZIP

```powershell
dotnet publish src/ROMulus.App/ROMulus.App.csproj `
    --configuration Release `
    --self-contained `
    --runtime win-x64 `
    -p:PublishSingleFile=true `
    --output dist/
```

Then copy alongside the exe:
- `profiles/` — destination profiles (YAML)
- `systems/` — system registry (YAML)
- `data/dats/` — bundled No-Intro DAT files
- `data/gamedb/` — bundled GameDB JSON snapshots
- `data/libretro-metadat/` — bundled libretro metadata DATs

---

## Migration Progress

This is a port of the Python codebase. Completed sessions:

- [x] **Session 1** — Scaffolding + DB schema + C# models + Config repository (17 tests passing)
- [ ] **Session 2** — System Registry + Quick Scan (L1 + L2)
- [ ] **Session 3** — Hasher + DAT Parser (Heavy Scan, L3)
- [ ] **Session 4** — Metadata enrichment chain (6 sources)
- [ ] **Session 5** — Cover art (local + libretro thumbnails)
- [ ] **Session 6** — Organizer + Import + AtomicIO
- [ ] **Session 7** — Sync Engine + Export
- [ ] **Session 8** — WPF MainWindow + MVVM skeleton
- [ ] **Session 9** — Game Table + Detail Panel
- [ ] **Session 10** — Background workers + Progress dialogs
- [ ] **Session 11** — Preview dialogs + Settings
- [ ] **Session 12** — Portable build + CI

See [migration plan](docs/migration-dotnet-plan.md) for the full roadmap.

---

## Design Rules (inherited from original)

1. **Local-first.** No server, no Docker, no external dependencies to run.
2. **Quick scan must be fast.** L1 (fuzzy filename) + L2 (header) run during scan; L3 (hash+DAT) is opt-in Heavy Scan.
3. **Never modify files without preview.** Every destructive action requires confirm.
4. **Atomic writes only.** `AtomicWriter` uses `Path.GetTempFileName()` + `File.Replace()`.
5. **Single library at a time.** Switching `library_path` prompts to wipe prior rows.
6. **Tombstone, don't delete.** Missing files become `missing=1`; re-scan un-tombstones them.
7. **One rom = one game.** The identity unit is the ROM file (strict 1:1 model, v0.4.0+).

---

## License

Apache License 2.0 — see [LICENSE](LICENSE).

Original Python codebase © Sphexi/ROMulous contributors (Apache 2.0).  
C# port © 2026 scorpio21.
