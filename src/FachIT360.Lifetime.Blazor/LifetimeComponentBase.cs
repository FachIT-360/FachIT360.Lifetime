// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LifetimeComponentBase.cs" company="FachIT 360 - Marcus Reinhart">
// (C) 2026 by FachIT 360 - Marcus Reinhart
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components;

namespace FachIT360.Lifetime.Blazor
{
    public abstract class LifetimeComponentBase : ComponentBase, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Gets the lifetime scope associated with this component.
        /// Do not dispose of this instance manually; it is disposed by this base component.
        /// </summary>
        protected LifetimeScope Lifetime { get; } = new();
        
        /// <summary>
        /// Gets a cancellation token that will be canceled when this component is disposed of.
        /// </summary>
        protected CancellationToken CancellationToken => Lifetime.Token;
        
        /// <summary>
        /// Disposes this component and its associated lifetime scope.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Releases resources used by this component.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> when called from <see cref="Dispose()"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            
            _disposed = true;
            
            if (disposing)
            {
                Lifetime.Dispose();
                OnDisposeManaged();
            }
        }
        
        /// <summary>
        /// Called once during managed disposal after the lifetime scope has been disposed.
        /// </summary>
        protected virtual void OnDisposeManaged()
        {
        }
    }
}
