// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LifetimeScope.cs" company="FachIT 360 - Marcus Reinhart">
// (C) 2026 by FachIT 360 - Marcus Reinhart
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace FachIT360.Lifetime
{
    /// <summary>
    ///     Represents a closed lifetime scope for an owning object.
    /// </summary>
    /// <remarks>
    ///     A lifetime scope is used to bind cleanup actions, disposable resources,
    ///     event subscriptions, and asynchronous operations to the lifetime of an
    ///     owning object.
    ///     <para>
    ///         When the scope is disposed, its cancellation token is canceled first.
    ///         Then all registered cleanup actions are executed in reverse registration
    ///         order.
    ///     </para>
    ///     <para>
    ///         This type is intended to be owned by a single object instance, such as
    ///         a component, page, layout, service, worker, or view model.
    ///     </para>
    /// </remarks>
    public sealed class LifetimeScope : IDisposable
    {
    #region Constants - Static fields - Fields

        private readonly List<Action> _cleanupActions = [];

        private readonly CancellationTokenSource _cts = new();
        private          bool                    _disposed;

    #endregion

    #region Properties

        /// <summary>
        ///     Gets a cancellation token that is canceled when this lifetime scope is disposed.
        /// </summary>
        /// <remarks>
        ///     Pass this token to asynchronous operations that should stop when the owning
        ///     object is disposed. The token represents the lifetime of the owning object,
        ///     not the lifetime of an individual operation.
        /// </remarks>
        public CancellationToken Token
        {
            get
            {
                if (_disposed)
                {
                    return CancellationToken.None;
                }

                try
                {
                    return _cts.Token;
                }
                catch (ObjectDisposedException)
                {
                    return CancellationToken.None;
                }
            }
        }

    #endregion

    #region Dispose

        /// <summary>
        ///     Ends this lifetime scope and executes all registered cleanup actions.
        /// </summary>
        /// <remarks>
        ///     Disposal first cancels the lifetime token and then executes all registered
        ///     cleanup actions in reverse registration order. Exceptions thrown by cleanup
        ///     actions are ignored so that all cleanup actions are attempted.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Cancel();
            
            _disposed = true;

            foreach (var cleanup in _cleanupActions.AsEnumerable().Reverse())
            {
                try
                {
                    cleanup();
                }
                catch (Exception)
                {
                    // Ignore exceptions during disposal to ensure all cleanup actions are attempted
                }
            }

            _cleanupActions.Clear();
            _cts.Dispose();
        }

    #endregion

    #region Methods

        /// <summary>
        ///     Registers a cleanup action that is executed when this lifetime scope is disposed.
        /// </summary>
        /// <param name="cleanup">
        ///     The cleanup action to execute during disposal.
        /// </param>
        /// <remarks>
        ///     Use this method for custom cleanup logic that is not represented by
        ///     <see cref="IDisposable"/> or an explicit subscription pair.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="cleanup"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     Thrown when this lifetime scope has already been disposed.
        /// </exception>
        public void AddCleanup(Action cleanup)
        {
            ArgumentNullException.ThrowIfNull(cleanup);
            
            ThrowIfDisposed();

            _cleanupActions.Add(cleanup);
        }

        /// <summary>
        ///     Registers a disposable resource to be disposed when this lifetime scope is disposed.
        /// </summary>
        /// <param name="disposable">
        ///     The disposable resource to dispose during lifetime disposal.
        /// </param>
        /// <remarks>
        ///     This method is equivalent to registering <see cref="IDisposable.Dispose"/>
        ///     as a cleanup action.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="disposable"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     Thrown when this lifetime scope has already been disposed.
        /// </exception>
        public void AddDisposable(IDisposable disposable)
        {
            ArgumentNullException.ThrowIfNull(disposable);
            
            AddCleanup(disposable.Dispose);
        }

        /// <summary>
        ///     Executes a subscription or setup action immediately and registers the corresponding
        ///     unsubscription or cleanup action for disposal.
        /// </summary>
        /// <param name="register">
        ///     The action that subscribes, registers, attaches, starts, or otherwise enables
        ///     something. This action is executed immediately.
        /// </param>
        /// <param name="unregister">
        ///     The action that unsubscribes, unregisters, detaches, stops, or otherwise cleans up
        ///     what <paramref name="register"/> enabled. This action is executed when this scope
        ///     is disposed.
        /// </param>
        /// <remarks>
        ///     This method is commonly used for event subscriptions:
        ///     <code>
        ///     lifetime.Subscribe(
        ///         () =&gt; service.Changed += OnChanged,
        ///         () =&gt; service.Changed -= OnChanged);
        ///     </code>
        ///     It can also be used for other paired setup/cleanup operations:
        ///     <code>
        ///     lifetime.Subscribe(
        ///         () =&gt; registry.Register(key),
        ///         () =&gt; registry.Unregister(key));
        ///     </code>
        ///     The cleanup action is only registered if the setup action completes successfully.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="register"/> or <paramref name="unregister"/> is
        ///     <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     Thrown when this lifetime scope has already been disposed.
        /// </exception>
        public void Subscribe(Action register, Action unregister)
        {
            ArgumentNullException.ThrowIfNull(register);
            ArgumentNullException.ThrowIfNull(unregister);
            
            ThrowIfDisposed();

            register();
            _cleanupActions.Add(unregister);
        }

        /// <summary>
        /// Cancel pending operations associated with this lifetime scope.
        /// </summary>
        private void Cancel()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Is possible in race conditions
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LifetimeScope));
            }
        }

    #endregion
    }
}
