# Integration: `com.drg.analytics`

**For LLMs:** step-by-step integration notes for Unity package `com.drg.analytics` — provider-agnostic analytics event tracking.

## Package ID & location

| Field       | Value                                          |
|-------------|------------------------------------------------|
| Package ID  | `com.drg.analytics`                            |
| Folder      | `drg.analytics/`                               |
| Namespace   | `DRG.Analytics`                                |
| Unity       | 2022.3+                                        |
| Dependencies| `com.drg.core`                                 |

## Key types

| Type                        | Assembly                 | Purpose                                                              |
|-----------------------------|--------------------------|----------------------------------------------------------------------|
| `IAnalyticsEvent`           | `DRG.Analytics`          | Read-only event contract: `Name` + `Parameters`                      |
| `AnalyticsEvent`            | `DRG.Analytics`          | Default implementation; also base class for typed events             |
| `IAnalyticsGateway`         | `DRG.Analytics`          | Provider surface: `void Track(IAnalyticsEvent)`                      |
| `IAnalyticsEventHandler`    | `DRG.Analytics`          | Per-event customisation hook registered on any `AnalyticsGatewayBase`|
| `AnalyticsGatewayBase`      | `DRG.Analytics`          | Abstract base for all provider gateways; runs handler chain          |
| `AnalyticsGatewayExtensions`| `DRG.Analytics`          | `Track(string)` convenience extension                                |
| `AnalyticsGatewayComposite` | `DRG.Analytics.Runtime`  | Fan-out to multiple providers; single entry point for the game       |
| `AnalyticsGatewayMemory`    | `DRG.Analytics.Runtime`  | In-memory impl; stores `IAnalyticsEvent` objects; for tests/debug    |
| `AnalyticsGatewayFile`      | `DRG.Analytics.Runtime`  | File-backed impl; logs timestamp + name + parameters                 |

---

## Composite — single entry point for multiple providers

A game typically sends every event to several analytics backends simultaneously. Use
`AnalyticsGatewayComposite` as the single `IAnalyticsGateway` reference in the game:

```csharp
var analytics = new AnalyticsGatewayComposite();
analytics.Add(new AnalyticsGatewayFirebase());       // drg.analytics.firebase
analytics.Add(new AnalyticsGatewayGameAnalytics());  // drg.analytics.gameanalytics
analytics.Add(new AnalyticsGatewayFile(logPath));    // local debug log

// One call, all providers receive it:
analytics.Track(new EventLevelStart(5, "hard"));
analytics.Track("tutorial_complete");                // extension shorthand
```

Because `AnalyticsGatewayComposite` extends `AnalyticsGatewayBase`, global cross-cutting handlers
can be registered on it. They run once before any provider processes the event:

```csharp
// Suppress all events in certain conditions globally:
analytics.AddHandler(new GdprConsentGuard());

// Debug log before every provider (return false = let providers still run):
analytics.AddHandler(new DebugLoggingHandler());
```

Two-level handler architecture:

```
Track(event)
    │
    ▼
[Composite handlers]   ← global (runs once, shared across all providers)
    │  false
    ▼
TrackDefault → [Provider A] → [Provider A handlers] → ProviderA.TrackDefault
             → [Provider B] → [Provider B handlers] → ProviderB.TrackDefault
```

---

## Usage

### Ad-hoc event (no parameters)

```csharp
IAnalyticsGateway analytics = new AnalyticsGatewayMemory();
analytics.Track("tutorial_complete");       // extension method shorthand
```

### Ad-hoc event with parameters

```csharp
analytics.Track(new AnalyticsEvent("level_start", new Dictionary<string, object>
{
    ["level"] = 5,
    ["difficulty"] = "hard"
}));
```

### Typed event (recommended for structured payloads)

Define typed events in your game project or a shared events package:

```csharp
public sealed class EventLevelStart : AnalyticsEvent
{
    public EventLevelStart(int level, string difficulty) : base("level_start")
    {
        Set("level", level);
        Set("difficulty", difficulty);
    }
}

// Usage:
analytics.Track(new EventLevelStart(5, "hard"));
```

### In-memory gateway for tests

```csharp
var memory = new AnalyticsGatewayMemory();
IAnalyticsGateway analytics = memory;

analytics.Track(new EventLevelStart(1, "normal"));
analytics.Track("tutorial_complete");

Debug.Assert(memory.Events.Count == 2);
Debug.Assert(memory.Events[0].Name == "level_start");

memory.Clear();
```

---

## Extending a provider gateway

Provider packages (e.g. `drg.analytics.firebase`) ship `AnalyticsGatewayBase` subclasses with
built-in SDK routing in `TrackDefault`. Game projects can customise behaviour per event type by
registering `IAnalyticsEventHandler` implementations — no need to modify the gateway class.

### Override DRG default for a specific event type

```csharp
// Handler returns true → DRG default is skipped for this event.
class MyPurchaseHandler : IAnalyticsEventHandler
{
    public bool TryHandle(IAnalyticsEvent e)
    {
        if (e is not EventPurchase purchase) return false;
        // custom SDK call with game-specific params
        MySDK.LogPurchase(purchase.ProductId, purchase.Revenue);
        return true;
    }
}

var gateway = new AnalyticsGatewayFirebase();   // hypothetical provider
gateway.AddHandler(new MyPurchaseHandler());
```

### Augment (run extra logic, then let DRG default run too)

```csharp
// Handler returns false → DRG default still executes after this.
class LevelStartAugmenter : IAnalyticsEventHandler
{
    public bool TryHandle(IAnalyticsEvent e)
    {
        if (e is not EventLevelStart) return false;
        MyDashboard.Notify(e.Name);             // side-effect only
        return false;                           // DRG default still runs
    }
}

gateway.AddHandler(new LevelStartAugmenter());
```

### Evaluation order

Handlers run in registration order. The first handler that returns `true` short-circuits the chain
and skips `TrackDefault`. Handlers that return `false` are transparent.

---

## Implementing a provider gateway

Provider package authors subclass `AnalyticsGatewayBase` and implement `TrackDefault`:

```csharp
public sealed class AnalyticsGatewayGameAnalytics : AnalyticsGatewayBase
{
    protected override void TrackDefault(IAnalyticsEvent e)
    {
        switch (e)
        {
            case EventLevelStart ls:
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, ls.Name);
                break;
            case EventLevelFail lf:
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, lf.Name);
                break;
            default:
                // Generic forwarding for all other event types
                GameAnalytics.NewDesignEvent(e.Name, ConvertParams(e.Parameters));
                break;
        }
    }
}
```

`TrackDefault` is the provider's built-in routing. It is only reached when no registered handler
returned `true` for the event.
