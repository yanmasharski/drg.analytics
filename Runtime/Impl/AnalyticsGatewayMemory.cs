using System.Collections.Generic;

namespace DRG.Analytics
{
	/// <summary>
	/// In-memory <see cref="IAnalyticsGateway"/> that records every tracked event.
	/// Intended for testing and debug tooling — not for production analytics pipelines.
	/// </summary>
	public sealed class AnalyticsGatewayMemory : AnalyticsGatewayBase
	{
		private readonly List<IAnalyticsEvent> _events = new List<IAnalyticsEvent>();

		/// <summary>All events recorded since the last <see cref="Clear"/> call, in insertion order.</summary>
		public IReadOnlyList<IAnalyticsEvent> Events => _events;

		protected override void TrackDefault(IAnalyticsEvent @event) => _events.Add(@event);

		/// <summary>Removes all recorded events.</summary>
		public void Clear() => _events.Clear();
	}
}
