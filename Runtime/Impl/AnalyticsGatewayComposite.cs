using System.Collections.Generic;

namespace DRG.Analytics
{
	/// <summary>
	/// Single entry point that fans out every event to all registered provider gateways.
	/// Implements the Composite pattern over <see cref="IAnalyticsGateway"/>.
	/// <para>
	/// Because it extends <see cref="AnalyticsGatewayBase"/>, handlers can be attached at the
	/// composite level for global cross-cutting behaviour (e.g. event suppression or enrichment)
	/// that runs once, before any provider sees the event.
	/// </para>
	/// <example>
	/// <code>
	/// var analytics = new AnalyticsGatewayComposite();
	/// analytics.Add(new AnalyticsGatewayFirebase());
	/// analytics.Add(new AnalyticsGatewayGameAnalytics());
	///
	/// // Global handler — intercepts before every provider:
	/// analytics.AddHandler(new DebugLoggingHandler());
	///
	/// // Single call reaches all providers:
	/// analytics.Track(new EventLevelStart(5, "hard"));
	/// </code>
	/// </example>
	/// </summary>
	public sealed class AnalyticsGatewayComposite : AnalyticsGatewayBase
	{
		private readonly List<IAnalyticsGateway> _gateways = new List<IAnalyticsGateway>();

		/// <summary>Registers a provider gateway. Events are forwarded in registration order.</summary>
		public void Add(IAnalyticsGateway gateway) => _gateways.Add(gateway);

		/// <summary>Removes a previously registered provider gateway.</summary>
		public void Remove(IAnalyticsGateway gateway) => _gateways.Remove(gateway);

		protected override void TrackDefault(IAnalyticsEvent @event)
		{
			foreach (var gateway in _gateways)
				gateway.Track(@event);
		}
	}
}
