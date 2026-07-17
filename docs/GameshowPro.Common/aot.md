# AOT and Trimming Guidance

`GameshowPro.Common` is built with AOT compatibility analysis enabled and with warnings treated as errors during CI builds.

This document explains what that means for package consumers, why it helps, and what to consider when shipping your own Native AOT application.

## Why this benefits consumers

When a library ships with AOT compatibility checks enabled, consumers get practical benefits:

1. Earlier compatibility feedback
- Reflection-heavy or linker-sensitive code paths are identified during library development instead of surprising app teams later.

2. More predictable behavior in trimmed/AOT apps
- Dynamic serialization and runtime type activation boundaries are made explicit, reducing hidden runtime risk.

3. Better deployment confidence
- Libraries that continuously validate AOT-related diagnostics are less likely to break when consumers adopt Native AOT, single-file, or aggressive trimming.

4. Faster consumer adoption of Native AOT
- App teams can spend more time on their own code paths because common library issues have already been triaged.

## What this library does

- Enables `IsAotCompatible` across projects.
- Enables `TreatWarningsAsErrors` so new warnings fail the build by default.
- Uses targeted suppressions only for intentional dynamic behavior (for example, dynamic type-based JSON patterns and generated WPF code where annotation is not directly possible).
- Adds explicit annotations on APIs and paths that rely on dynamic or reflection-based behavior.

## What consumers shipping with AOT should consider

If your app ships with Native AOT or aggressive trimming, treat these points as a checklist:

1. Prefer source-generated `System.Text.Json` metadata for known DTO graphs
- Source generation gives compile-time metadata and reduces runtime reflection needs.

2. Isolate dynamic serialization boundaries
- Any code that serializes/deserializes by `Type` at runtime should be treated as an explicit boundary with clear ownership.

3. Keep linker/AOT warning policy strict
- Use warnings as errors in your app build.
- Suppress only specific warning IDs and only where behavior is intentional.

4. Test the exact publish shape you ship
- Validate with `dotnet publish` using your real deployment settings (RID, single-file, trimming mode, AOT options).
- Do not rely only on `dotnet test` for AOT readiness.

5. Review runtime type-resolution flows
- If persisted payloads include type names/aliases, ensure all required types are preserved and available in the published artifact.

6. Validate platform-specific generated code
- WPF or other generated code may surface linker diagnostics that cannot be annotated in user code directly.
- Handle these with narrowly scoped suppression and clear rationale.

## Recommended consumer validation flow

1. Build and test normally.
2. Build with warnings as errors.
3. Publish using your real AOT/trimming settings.
4. Run end-to-end scenarios against the published binary.
5. Add targeted suppressions only after classifying each warning.

## Suppression policy used by this repo

The policy is:

- Default: warning is treated as an error.
- Exception: warning is suppressed only when one of these is true:
  - the behavior is intentionally dynamic and documented,
  - generated code cannot be directly annotated,
  - there is no safe static alternative for the specific scenario.

This keeps diagnostics actionable while preserving legitimate dynamic features that consumers rely on.
