// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LifetimeLayoutBase.TComponent.cs" company="FachIT 360 - Marcus Reinhart">
// (C) 2026 by FachIT 360 - Marcus Reinhart
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace FachIT360.Lifetime.Blazor
{
    /// <summary>
    /// Base class for components that require a lifetime scope.
    /// </summary>
    /// <typeparam name="TComponent">
    /// 1. The type of the component that inherits from this base class. This is used for logging purposes to provide context about the specific component type.
    /// </typeparam>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class LifetimeLayoutBase<TComponent> : LifetimeLayoutBase
    {
    #region Properties

        /// <summary>
        /// Injected logger.
        /// </summary>
        [Inject]
        protected ILogger<TComponent> Logger { get; set; } = null!;

    #endregion
    }
}
