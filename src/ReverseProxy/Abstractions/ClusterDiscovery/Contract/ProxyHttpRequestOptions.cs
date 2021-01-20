using System;
using System.Net.Http;

namespace Microsoft.ReverseProxy.Abstractions
{
    /// <summary>
    /// Outgoing request configuration.
    /// </summary>
    public sealed record ProxyHttpRequestOptions : IEquatable<ProxyHttpRequestOptions>
    {
        /// <summary>
        /// The time allowed to send the request and receive the response headers. This may include
        /// the time needed to send the request body.
        /// </summary>
        public TimeSpan? Timeout { get; init; }

        /// <summary>
        /// Preferred version of the outgoing request.
        /// </summary>
        public Version Version { get; init; }

#if NET
        /// <summary>
        /// The policy applied to version selection, e.g. whether to prefer downgrades, upgrades or
        /// request an exact version.
        /// </summary>
        public HttpVersionPolicy? VersionPolicy { get; init; }
#endif

        /// <inheritdoc />
        public bool Equals(ProxyHttpRequestOptions other)
        {
            if (other == null)
            {
                return false;
            }

            return Timeout == other.Timeout
#if NET
                && VersionPolicy == other.VersionPolicy
#endif
                && Version == other.Version;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Timeout,
#if NET
                VersionPolicy,
#endif
                Version);
        }
    }
}
