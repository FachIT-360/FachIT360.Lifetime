// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="LifetimeScope.cs" company="FachIT 360 - Marcus Reinhart">
//   Copyright (c) 2026 FachIT 360 - Marcus Reinhart.
//   Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;

namespace FachIT360.Lifetime.Blazor
{
    /// <summary>
    ///     Base class for components that require a <see cref = "LifetimeScope" />.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract class LifetimeLayoutBase : LayoutComponentBase, IDisposable
    {
    #region Constants - Static fields - Fields

        private bool _disposed;

    #endregion

    #region Properties

        /// <inheritdoc cref = "LifetimeScope.Token" />
        protected CancellationToken CancellationToken => Lifetime.Token;

        /// <summary>
        ///     Gets the lifetime scope associated with this component.
        ///     Do not dispose of this instance manually; it is disposed by this base component.
        /// </summary>
        protected LifetimeScope Lifetime { get; } = new();

    #endregion

    #region Dispose

        /// <summary>
        ///     Disposes this component and its associated lifetime scope.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases resources used by this component.
        /// </summary>
        /// <param name = "disposing">
        ///     <see langword = "true" /> when called from <see cref = "Dispose()" />.
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
        ///     Called once during managed disposal after the lifetime scope has been disposed.
        /// </summary>
        protected virtual void OnDisposeManaged() { }

    #endregion
    }
}
