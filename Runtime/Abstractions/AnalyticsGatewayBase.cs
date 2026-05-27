using System.Collections.Generic;

namespace DRG.Analytics
{
	/// <summary>
	/// Base class for all provider gateway implementations. Runs a registered
	/// <see cref="IAnalyticsEventHandler"/> chain before falling through to the provider's own
	/// <see cref="TrackDefault"/> implementation.
	/// <para>
	/// Provider packages subclass this and implement <see cref="TrackDefault"/> with their
	/// SDK-specific routing (typically a <c>switch</c> on the concrete event type).
	/// Game projects can inject custom or override behaviour via <see cref="AddHandler"/> without
	/// touching the provider implementation.
	/// </para>
	/// </summary>
	public abstract class AnalyticsGatewayBase : IAnalyticsGateway
	{
		private readonly List<IAnalyticsEventHandler> _handlers = new List<IAnalyticsEventHandler>();

		/// <summary>
		/// Appends a handler to the chain. Handlers are evaluated in registration order.
		/// </summary>
		public void AddHandler(IAnalyticsEventHandler handler) => _handlers.Add(handler);

		/// <inheritdoc/>
		public void Track(IAnalyticsEvent @event)
		{
			foreach (var handler in _handlers)
			{
				if (handler.TryHandle(@event))
				{
					return;
				}
			}

			TrackDefault(@event);
		}

		/// <summary>
		/// Provider-specific event routing. Called only when no registered handler returned
		/// <c>true</c> for the event. Implement a <c>switch</c> on the concrete event type and
		/// fall through to generic name+parameters forwarding for unknown types.
		/// </summary>
		protected abstract void TrackDefault(IAnalyticsEvent @event);
	}
}
