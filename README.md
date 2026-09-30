# ROMulus .NET

> Puerto de **ROMulus** (Python/PySide6) a **C# / Windows Forms / .NET 8.0**  
> Gestor de colecciones de ROMs local-first, portable y sin base de datos externa.

[![Build](https://img.shields.io/badge/build-passing-brightgreen)](#)
[![Tests](https://img.shields.io/badge/tests-38%20passing-brightgreen)](#)
[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue)](../LICENSE)

---

## Tabla de contenidos

- [Características](#características)
- [Stack técnico](#stack-técnico)
- [Arquitectura](#arquitectura)
- [Requisitos](#requisitos)
- [Inicio rápido](#inicio-rápido)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Tests](#tests)
- [Hoja de ruta](#hoja-de-ruta)

---

## Características

| Feature | Estado |
|---|---|
| Esquema SQLite (14 tablas) con WAL y FK | ✅ Sesión 1 |
| Modelos de dominio (`record` C#) | ✅ Sesión 1 |
| Repositorio de configuración key-value | ✅ Sesión 1 |
| Carga de registro de sistemas (`builtin.yaml`) | ✅ Sesión 2 |
| Quick Scan: walkthrough recursivo de biblioteca | ✅ Sesión 2 |
| Parser de nombres No-Intro / GoodTools / TOSEC | ✅ Sesión 2 |
| Fuzzy key para deduplicación | ✅ Sesión 2 |
| Tombstone sweep (detección de ROMs desaparecidas) | ✅ Sesión 2 |
| Formulario principal WinForms (árbol + grid) | ✅ Sesión 2 |
| Carga de ROMs en el grid por sistema | 🔜 Sesión 8 |
| Heavy Scan (hashing + match DAT) | 🔜 Sesión 10 |
| Diálogo de configuración | 🔜 Sesión 11 |
| Sincronización a destinos externos | 🔜 Sesión 14 |

---

## Stack técnico

| Capa | Tecnología |
|---|---|
| **UI** | Windows Forms / .NET 8.0-windows |
| **Arquitectura** | Layered (App → Core ← Infrastructure) |
| **DI** | `Microsoft.Extensions.DependencyInjection` 8.x |
| **Base de datos** | SQLite · `Microsoft.Data.Sqlite` · `Dapper` |
| **YAML** | `YamlDotNet` 18.x |
| **Logging** | `Serilog` (consola + archivo rotativo) |
| **Tests** | xUnit · FluentAssertions · SQLite in-memory |

---

## Arquitectura

```
ROMulus.sln
├── src/
│   ├── ROMulus.Core            # Dominio puro — sin dependencias externas
│   │   ├── Models/             # Rom, SystemInfo, DestinationProfile…
│   │   └── Scanner/            # IRomRepository, FilenameParser, LibraryScanner
│   │
│   ├── ROMulus.Infrastructure  # Implementaciones concretas
│   │   ├── Database/           # Schema, ConnectionFactory, Config/RomRepository
│   │   └── SystemRegistry/     # SystemRegistryLoader (YAML → SystemInfo)
│   │
│   └── ROMulus.App             # Windows Forms (.NET 8.0-windows)
│       ├── MainForm.cs         # Lógica del formulario principal
│       ├── MainForm.Designer.cs# Diseño visual (editable en VS Designer)
│       └── Program.cs          # DI container + bootstrap
│
└── tests/
    ├── ROMulus.Core.Tests      # ModelSmokeTests, FilenameParserTests
    └── ROMulus.Infrastructure.Tests  # DatabaseSmokeTests, ScannerIntegrationTests
```

### Regla de dependencias

```
App  ──►  Core  ◄──  Infrastructure
          (IRomRepository)
```

`Core` no depende de ninguna capa concreta; `Infrastructure` implementa las interfaces definidas en `Core`.

---

## Requisitos

- **Windows 10 / 11** (x64)
- **.NET 8.0 SDK** → [descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** v17.8+ (para el diseñador WinForms)  
  _o_ cualquier editor con soporte .NET (Rider, VS Code + C# Dev Kit)

---

## Inicio rápido

```powershell
# Clonar
git clone https://github.com/scorpio21/Romulous_net.git
cd Romulous_net

# Restaurar dependencias y compilar
dotnet build ROMulus.sln

# Ejecutar la aplicación
dotnet run --project src/ROMulus.App

# Ejecutar los tests
dotnet test ROMulus.sln
```

**Abrir en Visual Studio:**  
`Archivo → Abrir → Solución/Proyecto` → seleccionar `dotnet\ROMulus.sln`  
El diseñador WinForms está disponible haciendo doble clic en `MainForm.cs`.

---

## Estructura del proyecto

```
dotnet/
├── ROMulus.sln                    ← Solución VS 2022 (.sln estándar)
├── Directory.Build.props          ← Configuración global (C# 12, Nullable, NoWarn)
├── src/
│   ├── ROMulus.Core/
│   │   ├── Models/
│   │   │   ├── Rom.cs
│   │   │   ├── SystemInfo.cs
│   │   │   ├── DestinationProfile.cs
│   │   │   ├── Catalog.cs          # ScanHistory, DatEntry, RomCollection
│   │   │   ├── RomData.cs          # RomMetadata, RomCover, RomHash
│   │   │   └── MatchConfidence.cs
│   │   └── Scanner/
│   │       ├── IRomRepository.cs   # Contrato de persistencia
│   │       ├── NoIntroTokens.cs    # FrozenSet de tokens región/revisión
│   │       ├── FilenameParser.cs   # parse_filename + generate_fuzzy_key
│   │       └── LibraryScanner.cs  # Quick Scan + tombstone sweep
│   ├── ROMulus.Infrastructure/
│   │   ├── Database/
│   │   │   ├── Schema.cs           # 14 tablas SQLite (idempotente)
│   │   │   ├── ConnectionFactory.cs# WAL + FK + busy_timeout
│   │   │   ├── ConfigRepository.cs # key-value persistente
│   │   │   └── RomRepository.cs   # upsert ROM + COALESCE DAT safety
│   │   └── SystemRegistry/
│   │       └── SystemRegistryLoader.cs  # YAML → IReadOnlyList<SystemInfo>
│   └── ROMulus.App/
│       ├── Program.cs
│       ├── MainForm.cs
│       ├── MainForm.Designer.cs
│       └── app.manifest            # DPI PerMonitorV2 + longPathAware
└── tests/
    ├── ROMulus.Core.Tests/
    │   ├── ModelSmokeTests.cs
    │   └── FilenameParserTests.cs
    └── ROMulus.Infrastructure.Tests/
        ├── DatabaseSmokeTests.cs
        └── ScannerIntegrationTests.cs
```

---

## Tests

```
dotnet test ROMulus.sln --logger "console;verbosity=minimal"
```

| Suite | Tests | Cubre |
|---|---|---|
| `ModelSmokeTests` | 8 | Records de dominio, MatchConfidence |
| `DatabaseSmokeTests` | 11 | Schema, ConnectionFactory, ConfigRepository |
| `FilenameParserTests` | 11 | parse_filename, fuzzy_key, side-file, ZIP |
| `ScannerIntegrationTests` | 8 | Enroll, tombstone, un-tombstone, scoped scan |
| **Total** | **38** | **0 fallos** |

---

## Hoja de ruta

| Sesión | Objetivo |
|---|---|
| ~~1~~ | ~~Scaffold, DB schema, modelos, ConfigRepository~~ |
| ~~2~~ | ~~System Registry YAML, Quick Scan, WinForms shell~~ |
| 3 | DAT parser (No-Intro XML/ZIP) |
| 4 | Heavy Scan: hashing MD5/SHA1/CRC32 + match DAT |
| 5 | ROM detail panel + cover art |
| 6 | Diálogo de configuración + perfiles de exportación |
| 7 | Sincronización a destinos (USB, carpeta de red) |
| 8–14 | Enrichment (IGDB/Screenscraper), UI pulido, packaging |

---

## Licencia

Apache License 2.0 — ver [`LICENSE`](../LICENSE)
