# FachIT360.Lifetime

Stop hand-rolling `IDisposable` in every Blazor component just to unhook an event or cancel a background task.

`FachIT360.Lifetime.Blazor` gives your components and layouts automatic, deterministic cleanup — no `@implements IDisposable`, no manual unsubscribes, no forgotten teardown.

The same idea also works outside Blazor: `FachIT360.Lifetime` is the framework-agnostic core that any service, worker, or view model can use directly.

## The problem

In Blazor, wiring up an event subscription or a long-running task usually means splitting setup and teardown across two unrelated places in the file — and hoping you remember both.

```razor
@implements IDisposable
@inject IDataService DataService

@code {
    protected override async Task OnInitializedAsync()
    {
        DataService.OnUpdated += OnDataUpdated;

        var data = await DataService.FetchAsync(CancellationToken.None);
    }

    private void OnDataUpdated(object? sender, EventArgs e)
    {
        // Some logic after data is updated
    }

    public void Dispose()
    {
        DataService.OnUpdated -= OnDataUpdated;
        DataService.Dispose();
    }
}
```

Miss the unsubscribe in `Dispose()`, and you've got a memory leak that won't show up until the component has unmounted hundreds of times in production.

## The fix: `LifetimeComponentBase`

```razor
@inherits LifetimeComponentBase<Counter>
@inject IDataService DataService

@code {
    protected override async Task OnInitializedAsync()
    {
        // Dispose this together with the component
        Lifetime.AddDisposable(DataService);

        // Subscribe and unsubscribe declared in one place
        Lifetime.Subscribe(
            () => DataService.OnUpdated += OnDataUpdated,
            () => DataService.OnUpdated -= OnDataUpdated);

        // Cancelled automatically when the component unmounts
        var data = await DataService.FetchAsync(CancellationToken);
    }

    private void OnDataUpdated(object? sender, EventArgs e)
    {
        // Some logic after data is updated
    }

    // Optional: hook for custom teardown logic
    protected override void OnDisposeManaged()
    {
        Logger.LogInformation("Component is unmounting. Performing custom cleanup...");
    }
}
```

Setup and its matching cleanup live on adjacent lines. No `IDisposable` implementation, no separate `Dispose()` method to keep in sync.

## Installation

```
dotnet add package FachIT360.Lifetime.Blazor
```

This already references the core `FachIT360.Lifetime` package — no need to install it separately.

## Adapters for Blazor's base classes

`FachIT360.Lifetime.Blazor` ships two adapters, one for components and one for layouts, so you get the same `Lifetime` scope and `CancellationToken` regardless of which Blazor base class you're already inheriting from:

```
ComponentBase                  LayoutComponentBase
   ↓                                ↓
LifetimeComponentBase          LifetimeLayoutBase
   ↓                                ↓
LifetimeComponentBase<T>       LifetimeLayoutBase<T>
```

### Base members (both adapters)

```csharp
protected LifetimeScope Lifetime { get; }
protected CancellationToken CancellationToken { get; }
protected virtual void OnDisposeManaged();
```

### Intended for

- Components with event subscriptions
- Components using JS interop
- Layouts with shared UI state
- Components with long-running asynchronous operations
- Anything that currently needs `IDisposable` just to clean up after itself

### Component example

```csharp
public sealed partial class Dashboard : LifetimeComponentBase<Dashboard>
{
    protected override async Task OnInitializedAsync()
    {
        Lifetime.Subscribe(
            () => UiState.Changed += OnUiStateChanged,
            () => UiState.Changed -= OnUiStateChanged);

        var data = await DataService.LoadAsync(CancellationToken);

        Logger.LogInformation("Dashboard initialized.");
    }
}
```

### Layout example

```razor
@inherits LifetimeLayoutBase<MainLayout>

<div class="page">
    <main>
        @Body
    </main>
</div>

@code {
    protected override void OnInitialized()
    {
        Logger.LogInformation("MainLayout initialized.");

        _ = SomeService.LoadAsync(CancellationToken);
    }
}
```

---

## Under the hood: `FachIT360.Lifetime`

The Blazor adapters are built on `LifetimeScope`, a small, dependency-free primitive that you can also use directly — no Blazor required. It's a good fit for anything with a clear, deterministic lifetime: services, background workers, view models (outside UI frameworks with their own disposal model), coordinator or domain objects.

```
dotnet add package FachIT360.Lifetime
```

```csharp
using FachIT360.Lifetime;

public sealed class MyService : IDisposable
{
    private readonly LifetimeScope _lifetime = new();

    public MyService(SomeEventSource source)
    {
        _lifetime.Subscribe(
            () => source.Changed += OnChanged,
            () => source.Changed -= OnChanged);
    }

    private void OnChanged()
    {
        // React to event
    }

    public void Dispose() => _lifetime.Dispose();
}
```

### API guide

**Custom cleanup actions**

```csharp
_lifetime.AddCleanup(() => Console.WriteLine("Cleaning up!"));
```

**Disposable resources**

```csharp
var timer = new Timer(_ => DoWork(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
_lifetime.AddDisposable(timer);
```

**Event subscriptions**

```csharp
_lifetime.Subscribe(
    () => service.Changed += OnChanged,
    () => service.Changed -= OnChanged);
```

The first action runs immediately; the second runs on disposal. Subscription and unsubscription stay next to each other instead of being split across the class.

**Lifetime-bound cancellation**

```csharp
await dbRepository.LoadAsync(_lifetime.Token);
```

`Token` is cancelled when the scope is disposed. It represents the lifetime of the owning object — not an individual operation. For operation-specific cancellation, use a separate `CancellationTokenSource`.

**Disposal order**

1. The lifetime token is cancelled.
2. Registered cleanup actions run in reverse registration order.
3. Cleanup exceptions are ignored so every action gets a chance to run.
4. The internal `CancellationTokenSource` is disposed.

No dependency on Blazor, ASP.NET Core, dependency injection, or logging — the core package stays usable anywhere.

### FAQ: Why not just use the built-in DI container's disposal / scopes?

Fair question, and worth answering honestly: **the DI container already solves a different problem.** ASP.NET Core's container disposes scoped/transient services it created when their scope ends. If your object is being resolved from the container, you get that for free — `LifetimeScope` doesn't replace it and isn't meant to.

What the container does *not* do is manage wiring **inside** an object it created: pairing an event subscription with its unsubscription, tying a `CancellationTokenSource` to that specific instance, or keeping several unrelated pieces of teardown logic organized as a class grows. That's still on you, split between the constructor and `Dispose()`.

You can absolutely write that by hand — `LifetimeScope` doesn't add a capability C# doesn't already have. What it changes is where the wiring lives: setup and its matching cleanup sit on adjacent lines instead of being split across the file, which mostly matters once a class accumulates several subscriptions and it gets easy to add one without adding its unhook. In the Blazor case specifically, `LifetimeComponentBase` also removes the need to implement `IDisposable` per component at all — that part isn't just readability, it's boilerplate you don't write.

So: **use the container's disposal for your service graph. Use `LifetimeScope` for the wiring inside an instance, if you find that wiring gets messy enough to be worth it.** For a small class with one subscription, it probably isn't — plain `IDisposable` is fine.

## License

MIT
