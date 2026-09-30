# Changelog

Todos los cambios notables de **ROMulus .NET** se documentan aquí.  
Formato basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).  
Versiones siguiendo [Semantic Versioning](https://semver.org/lang/es/).

---

## [Unreleased]

---

## [0.2.0] — 2026-09-30

### Añadido

#### System Registry
- `SystemRegistryLoader`: carga `systems/builtin.yaml` vía `YamlDotNet`.  
  Mapea el YAML a `IReadOnlyList<SystemInfo>` con `IgnoreUnmatchedProperties()` para compatibilidad futura con campos nuevos (`logo_dark`, `gamedb_file`, etc.).
- `RomRepository.UpsertSystems()`: siembra la tabla `systems` desde el registro YAML.
- `RomRepository.GetAliasBySystem()`: devuelve el mapa `alias → system_id` para el resolver del scanner.
- `RomRepository.GetExtensionsBySystem()`: devuelve extensiones aceptadas por sistema.

#### Quick Scan (L1 + L2)
- `NoIntroTokens`: tablas `FrozenSet` de tokens de región, idioma y revisión compartidas entre el scanner y el parser de DATs.
- `FilenameParser.Parse()`: extrae título limpio, región, revisión, número de disco, flags `IsHack / IsHomebrew / IsBadDump / IsVerified / IsTranslation / IsPrototype / IsBeta / IsDemo` de nombres No-Intro, GoodTools y TOSEC.
- `FilenameParser.GenerateFuzzyKey()`: normalización en 7 pasos (artículo, romanos, sufijo de versión, lowercase, no-alfanumérico).
- `FilenameParser.IsSideFile()` / `IsRomFile()`: filtros de archivos compañía (`.cue`, `.m3u`, `.txt`…) y archivos ZIP/7z siempre aceptados.
- `IRomRepository`: interfaz de persistencia en `Core` para romper la dependencia circular `Core → Infrastructure`.
- `LibraryScanner.Scan()`: walkthrough recursivo `Directory.EnumerateFiles`, detección de sistema por alias de carpeta (sube hasta `libraryRoot`), upsert de ROMs y tombstone sweep post-walk.  
  Soporta `IProgress<(int, string)>` para barra de progreso y `CancellationToken`.
- `RomRepository.UpsertRom()`: upsert con semántica `COALESCE` — preserva valores DAT-derived (`dat_verified`) cuando ya existen.
- `RomRepository.MarkMissingUnderRoot()`: tombstone sweep vía tabla temporal `_visited_ids` para evitar límites de parámetros SQLite.
- `RomRepository.InsertScanHistory()` / `UpdateScanHistory()`: registra estadísticas de cada escaneo en `scan_history`.

#### Windows Forms Shell
- `Program.cs`: bootstrap con `Microsoft.Extensions.DependencyInjection`, `Serilog` (log rotativo diario), `HighDpiMode.PerMonitorV2`.
- `MainForm` / `MainForm.Designer.cs`: formulario principal con `SplitContainer` doble (árbol de sistemas | grid de ROMs | panel de detalle), `MenuStrip`, `ToolStrip` y `StatusStrip`.
- `app.manifest`: DPI `PerMonitorV2` + `longPathAware` para rutas > 260 caracteres.
- `GlobalUsings.cs` (Core): añade `System.Collections.Frozen` globalmente para `FrozenSet<T>`.

#### Solución y stack
- Migración de `.slnx` → **`ROMulus.sln`** (formato VS 2022 estándar, compatible con diseñador WinForms).
- Migración de **WPF / .NET 10** → **Windows Forms / .NET 8.0-windows**.
- Todos los paquetes NuGet fijados a versiones `8.x` compatibles.

### Corregido
- `MainForm.Designer.cs` reescrito en formato estricto del diseñador de VS 2022:
  - Eliminadas lambdas de `InitializeComponent` (motor del designer no las parsea).
  - `AddRange([...])` → `AddRange(new ToolStripItem[] { ... })`.
  - Añadidos `ISupportInitialize.BeginInit/EndInit` para `SplitContainer` y `DataGridView`.
  - `SuspendLayout/ResumeLayout` en todos los contenedores.
  - Tipos fully-qualified (`System.Windows.Forms.*`).
- `CA1308`, `CA1812`, `CA1859`, `CA1303`, `WFAC010` añadidos a la lista global `NoWarn` con justificaciones documentadas.
- `StringComparison.Ordinal` añadido a `string.Replace` en `RomRepository`.

### Tests
- `FilenameParserTests` — 11 tests nuevos (parse, fuzzy_key, side-file, ZIP, artículo, romanos).
- `ScannerIntegrationTests` — 8 tests nuevos (enroll, tombstone, un-tombstone, scoped scan, YAML loader).
- **Total acumulado: 38 tests, 0 fallos.**

---

## [0.1.0] — 2026-09-29

### Añadido

#### Infraestructura de base de datos
- `Schema.cs`: 14 tablas SQLite con índices, misma estructura que `schema.py`:  
  `roms`, `hashes`, `rom_metadata`, `covers`, `dat_entries`, `dat_files`, `systems`, `scan_history`, `collections`, `collection_members`, `sync_destinations`, `sync_dest_systems`, `dest_inventory`, `config`.
- `ConnectionFactory`: conexión SQLite con WAL, llaves foráneas, `busy_timeout = 5 000 ms`, protección contra esquemas legacy.
- `ConfigRepository`: almacén key-value persistente con `SeedDefaults()`, `Get()`, `Set()`, `SetMany()`.

#### Modelos de dominio
- `Rom` — espejo del tipo SQLite `roms`, con `MatchConfidence` enum.
- `SystemInfo` — plataforma retro con extensiones, aliases de carpeta y regla de header.
- `DestinationProfile` / `SystemMapping` — perfiles de exportación (mirrors de `profile.py`).
- `ScanHistory`, `DatEntry`, `RomCollection` — historial de escaneo y catálogo DAT.
- `RomMetadata`, `RomCover`, `RomHash` — datos de enriquecimiento.

#### Infraestructura de proyecto
- Solución `ROMulus.slnx` con 5 proyectos: `App`, `Core`, `Infrastructure`, `Core.Tests`, `Infrastructure.Tests`.
- `Directory.Build.props`: C# 12, `Nullable enable`, `ImplicitUsings enable`, supresiones de analizadores documentadas.
- `.gitignore` estándar .NET.
- `README.md` y `LICENSE` (Apache 2.0).

### Tests
- `ModelSmokeTests` — 8 tests (records inmutables, MatchConfidence).
- `DatabaseSmokeTests` — 11 tests (schema idempotente, WAL, FK, ConfigRepository CRUD).
- **Total: 19 tests, 0 fallos.**

---

[Unreleased]: https://github.com/scorpio21/Romulous_net/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/scorpio21/Romulous_net/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/scorpio21/Romulous_net/releases/tag/v0.1.0
