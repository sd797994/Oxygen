using SkyApm.Utilities.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using SkyApm;

namespace Oxygen.SkyApm.Extensions
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddOxygen(this SkyApmExtensions extensions)
        {
            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, OxygenTracingDiagnosticProcessor>();
            return extensions;
        }
    }
}
