# DRG Analytics

Provider-agnostic analytics contract and built-in debug implementations for DRG packages.
SDK adapters live in dedicated provider packages (e.g. `drg.analytics.firebase`).

For package layout and assembly naming conventions, see the DRG SDK structure guide in [`drg.core`](https://github.com/yanmasharski/drg.core) (`~Documentation/STRUCTURE.md`).

## Assemblies

| Assembly | Contains |
|---|---|
| `DRG.Analytics` | `IAnalyticsGateway`, `IAnalyticsEvent`, `IAnalyticsEventHandler`, `AnalyticsEvent`, `AnalyticsGatewayBase`, `AnalyticsGatewayExtensions` |
| `DRG.Analytics.Runtime` | `AnalyticsGatewayComposite`, `AnalyticsGatewayMemory`, `AnalyticsGatewayFile` |

## Dependencies

- `com.drg.core`

## Built-in implementations

### `AnalyticsGatewayComposite`
Fan-out gateway that forwards every event to all registered provider gateways. Use as the single
`IAnalyticsGateway` entry point in the game.

### `AnalyticsGatewayMemory`
Records every tracked event in an in-memory list. Intended for unit tests and debug tooling.

```csharp
var memory = new AnalyticsGatewayMemory();
memory.Track(new EventLevelStart(5));
Debug.Log(memory.Events.Count); // 1
memory.Clear();
```

### `AnalyticsGatewayFile`
Appends every tracked event to a log file as TSV lines (`timestamp TAB name TAB {params}`).
Intended for debug builds and QA pipelines.

```csharp
var file = new AnalyticsGatewayFile(Application.persistentDataPath + "/analytics.log");
file.Track(new EventLevelStart(5));
```

## Install

```
https://github.com/yanmasharski/drg.analytics.git#0.9.0
```
