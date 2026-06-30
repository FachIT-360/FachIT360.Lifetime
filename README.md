# FachIT360.Lifetime

A small .NET utility for binding cleanup actions, disposable resources, event subscriptions, and asynchronous operations to the lifetime of an owning object.

`FachIT360.Lifetime` provides a simple `LifetimeScope` that helps keep cleanup logic close to the code that creates or registers resources.

## Features

* Register cleanup actions
* Register IDisposable resources
* Subscribe and automatically unsubscribe from events
* Use a lifetime-bound CancellationToken
* Dispose all registered cleanup actions in reverse registration order
* No dependency on Blazor, ASP.NET Core, dependency injection, or logging

## Installation
```bash
dotnet add package FachIT360.Lifetime
```

## Basic usage

```csharp
using FachIT360.Lifetime;

public sealed class MyService : IDisposable
{
    private readonly LifetimeScope _lifetime = new();

    public MyService(SomeEventSource source)
    {
        // Subscribe register and unregister actions                                                                                                                                   1
        _lifetime.Subscribe(
        () => source.Changed += OnChanged,
        () => source.Changed -= OnChanged);
    }

    private void OnChanged()
    {
        // React to event
    }

    public void Dispose()
    {
        _lifetime.Dispose();
    }
}
```

## Add Custom Cleanup actions

Use `AddCleanup` for custom cleanup logic that should run when the scope is disposed.

```csharp
_lifetime.AddCleanup(() => Console.WriteLine("Cleaning up!"));
```

## Add Disposable resources

Use `AddDisposable` for resources that implement IDisposable.

```csharp
var timer = new Timer(_ => DoWork(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
_lifetime.AddDisposable(timer);
```

## Add Event subscriptions

Use `Subscribe` to register an action immediately and provide the matching cleanup action.

```csharp
_lifetime.Subscribe(
() => service.Changed += OnChanged,
() => service.Changed -= OnChanged);
```

The first action is executed immediately.
The second action is executed when the lifetime scope is disposed.

This pattern is useful for event subscriptions because it keeps subscription and unsubscription next to each other.

## Cancellation token

`LifetimeScope.Token` is canceled when the scope is disposed.

```csharp
await dbRepository.LoadAsync(_lifetime.Token);
```

Use this token for asynchronous operations that should stop when the owning object is disposed.

The token represents the lifetime of the owning object. It is not intended to control individual operations such as canceling a single search or uplo ad. For operation-specific cancellation, use a separate `CancellationTokenSource`.

## Disposal behavior

When `LifetimeScope.Dispose()` is called:

1. The lifetime token is canceled.
2. Registered cleanup actions are executed in reverse registration order.
3. Cleanup exceptions are ignored.
4. The internal CancellationTokenSource is disposed.

## Intended use

* Services
* Workers
* View models, provided no dependencies arise (e.g., WPF, WinForms, MAUI, etc.)
* Background tasks
* Coordinator objects
* Domain objects
* Any object with a deterministic lifetime

The owning object should dispose the scope when its own lifetime ends.

# FachIT360.Lifetime.Blazor

FachIT360.Lifetime.Blazor integrates the LifetimeScope utility into the lifecycle of Blazor components and layouts.
It provides deterministic cleanup, cancellation mechanisms tied to the lifetime, and optional support for typed logging for Blazor Server and WebAssembly.

This package extends the core library FachIT360.Lifetime, adding abstractions specifically tailored to Blazor applications.

## Installation

```bash
dotnet add package FachIT360.Lifetime.Blazor
```

The FachIT360.Lifetime.Blazor package already references FachIT360.Lifetime internally and does not need to be specified explicitly.

### FachIT360.Lifetime.Blazor — API Overview

> **Component Inheritance Hierarchy**
> ```
> ComponentBaae
>     ↓
> LifetimeComponentBase
>     ↓
> LifetimeComponentBase<TComponent>
> ```

> **Layout Inheritance Hierarchy**
> ```
> LayoutComponentBase
>     ↓
> LifetimeLayoutBase
>     ↓
> LifetimeLayoutBase<TComponent>
> ```

### Members

```csharp
protected LifetimeScope Lifetime { get; }
protected CancellationToken CancellationToken { get; } 
protected virtual void OnDisposeManaged();
```

### Usage Examples

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

## Intended Use

FachIT360.Lifetime.Blazor is intended

* Blazor components with event subscriptions
* Components using JS interop
* Layouts with shared UI state
* Components with long-running asynchronous operations
* Components requiring deterministic cleanup logic

## Usage in a layout

```csharp
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

# License

MIT