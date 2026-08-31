// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="LifetimeScope.cs" company="FachIT 360 - Marcus Reinhart">
//   Copyright (c) 2026 FachIT 360 - Marcus Reinhart.
//   Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace FachIT360.Lifetime.Blazor
{
    /// <summary>
    ///     Base class for components that require a lifetime scope.
    /// </summary>
    /// <typeparam name = "TComponent">
    ///     1. The type of the component that inherits from this base class. This is used for logging purposes to provide
    ///     context about the specific component type.
    /// </typeparam>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public abstract class LifetimeLayoutBase<TComponent> : LifetimeLayoutBase
    {
    #region Properties

        /// <summary>
        ///     Injected logger.
        /// </summary>
        [Inject]
        protected ILogger<TComponent> Logger { get; set; } = null!;

    #endregion
    }
}
