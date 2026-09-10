# PnachToLuaConverter

> Portfolio project by Arseniy Makhonin.

A .NET command-line converter that translates PCSX2 PNACH memory patches into PS4 emulator Lua commands.

## Highlights

- Maintained as a reproducible, source-first portfolio project.
- Build outputs, local secrets, proprietary dumps, and generated runtime data are excluded from version control.
- The repository keeps project documentation close to the implementation.

## Technology

.NET / C#

## Repository layout

Primary areas: `PnachToLuaConverter/`.

## Build and verification

```powershell
dotnet build PnachToLuaConverter.sln -c Release
```

Exact requirements may vary by platform. Check project-specific documentation and configuration before building.

## Legal

Original source code is available under the MIT License. Third-party dependencies retain their respective licenses.

## Русский

Консольный .NET-конвертер патчей PCSX2 PNACH в Lua-команды эмулятора PS4.

Репозиторий оформлен как портфолио: локальные секреты, результаты сборки и сторонние игровые/медиафайлы не должны попадать в Git.
