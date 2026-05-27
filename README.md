# DRG Analytics

Provider-agnostic analytics contract for DRG packages. Use `Runtime/Abstractions` first; concrete trackers and SDK adapters belong in `Runtime/Impl` or dedicated provider packages when you add them.

For package layout and assembly naming conventions, see the DRG SDK structure guide in [`drg.core`](https://github.com/yanmasharski/drg.core) (`~Documentation/STRUCTURE.md`).

## Assemblies

| Assembly | Contains |
|---|---|
| `DRG.Analytics` | `IAnalyticsGateway` |

## Dependencies

- `com.drg.core`

## Install

```
https://github.com/yanmasharski/drg.analytics.git#0.9.0
```
