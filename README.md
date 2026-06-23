# FachIT360.Lifetime

A small .NET utility for binding cleanup actions, disposable resources, event subscriptions, and asynchronous operations to the lifetime of an owning object.

`FachIT360.Lifetime` provides a simple `LifetimeScope` that helps keep cleanup logic close to the code that creates or registers resources.

## Features

- Register cleanup actions
- Register `IDisposable` resources
- Subscribe and automatically unsubscribe from events
- Use a lifetime-bound `CancellationToken`
- Dispose all registered cleanup actions in reverse registration order
- No dependency on Blazor, ASP.NET Core, dependency injection, or logging

## Installation

```shell
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

## Cleanup actions

Use `AddCleanup` for custom cleanup logic that should run when the scope is disposed.

```csharp
_lifetime.AddCleanup(() => Console.WriteLine("Cleaning up!"));
```

## Disposable resources

Use `AddDisposable` for resources that implement `IDisposable`.

```csharp
var timer = new Timer(_ => DoWork(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
_lifetime.AddDisposable(timer);
```

When the lifetime scope is disposed, the timer is disposed automatically.

## Event subscriptions

Use `Subscribe` to register an action immediately and provide the matching cleanup action.

```csharp
_lifetime.Subscribe( () => service.Changed += OnChanged, () => service.Changed -= OnChanged);
```

The first action is executed immediately.  
The second action is executed when the lifetime scope is disposed.

This pattern is useful for event subscriptions because it keeps subscription and unsubscription next to each other.

## Cancellation token

`LifetimeScope.Token` is canceled when the scope is disposed.

```csharp
await repository.LoadAsync(_lifetime.Token);
```

Use this token for asynchronous operations that should stop when the owning object is disposed.

The token represents the lifetime of the owning object. It is not intended to control individual operations such as canceling a single search or upload. For operation-specific cancellation, use a separate `CancellationTokenSource`.

## Blazor example

`LifetimeScope` can be used in Blazor components, pages, or layouts through a base class.

```csharp
using FachIT360.Lifetime;
using Microsoft.AspNetCore.Components;
public abstract class LifetimeComponentBase : ComponentBase, IDisposable
{
    private bool _disposed;
    protected LifetimeScope Lifetime { get; } = new();
    
    protected CancellationToken CancellationToken => Lifetime.Token;
    
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
    
        _disposed = true;
        Lifetime.Dispose();
    }
}
```

Usage in a component:

```csharp
protected override async Task OnInitializedAsync()
{
    Lifetime.Subscribe( () => 
        UiState.Changed += OnUiStateChanged,
        () => UiState.Changed -= OnUiStateChanged );
    
    var data = await DataService.LoadAsync(CancellationToken);
}
```


## Disposal behavior

When `LifetimeScope.Dispose()` is called:

1. The lifetime token is canceled.
2. Registered cleanup actions are executed in reverse registration order.
3. Cleanup exceptions are ignored so that all cleanup actions are attempted.
4. The internal `CancellationTokenSource` is disposed.

## Intended use

`LifetimeScope` is intended to be owned by a single object instance, for example:

- a Blazor component
- a page
- a layout
- a service
- a worker
- a view model
- a coordinator object

The owning object should dispose the scope when its own lifetime ends.

## License

MIT