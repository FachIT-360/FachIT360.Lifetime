// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LifetimeComponentBase.TComponent.cs" company="FachIT 360 - Marcus Reinhart">
// (C) 2026 by FachIT 360 - Marcus Reinhart
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace FachIT360.Lifetime.Blazor
{
    public abstract class LifetimeComponentBase<TComponent> : LifetimeComponentBase
    {
        [Inject]
        protected ILogger<TComponent> Logger { get; set; } = null!;
    }
}
