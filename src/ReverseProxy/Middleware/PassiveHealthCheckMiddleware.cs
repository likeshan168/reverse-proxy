// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.ReverseProxy.Service.HealthChecks;
using Microsoft.ReverseProxy.Service.Proxy;
using Microsoft.ReverseProxy.Utilities;

namespace Microsoft.ReverseProxy.Middleware
{
    public class PassiveHealthCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDictionary<string, IPassiveHealthCheckPolicy> _policies;

        public PassiveHealthCheckMiddleware(RequestDelegate next, IEnumerable<IPassiveHealthCheckPolicy> policies)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _policies = policies?.ToDictionaryByUniqueId(p => p.Name) ?? throw new ArgumentNullException(nameof(policies));
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            var proxyFeature = context.GetRequiredProxyFeature();
            var options = proxyFeature.ClusterConfig.Options.HealthCheck?.Passive;

            // Do nothing if no target destination has been chosen for the request.
            if (!(options?.Enabled).GetValueOrDefault() || proxyFeature.SelectedDestination == null)
            {
                return;
            }

            // Policy must always be present if the passive health check is enabled for a cluster.
            // It's validated and ensured by a configuration validator.
            var policy = _policies.GetRequiredServiceById(options.Policy);
            var cluster = context.GetRequiredRouteConfig().Cluster;
            policy.RequestProxied(cluster, proxyFeature.SelectedDestination, context);
        }
    }
}
