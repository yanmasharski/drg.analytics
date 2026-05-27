namespace DRG.Analytics
{
	/// <summary>
	/// Handles a specific subset of analytics events inside a gateway, enabling per-event-type
	/// customisation without modifying the gateway implementation.
	/// <para>
	/// Register handlers on any <see cref="AnalyticsGatewayBase"/> via
	/// <see cref="AnalyticsGatewayBase.AddHandler"/>. Handlers are evaluated in registration order;
	/// the first handler that returns <c>true</c> short-circuits the chain.
	/// </para>
	/// <list type="bullet">
	///   <item><c>return true</c> — event fully handled; the gateway default is skipped.</item>
	///   <item><c>return false</c> — not handled; the next handler (or gateway default) runs.</item>
	/// </list>
	/// </summary>
	public interface IAnalyticsEventHandler
	{
		/// <returns>
		/// <c>true</c> if the event was handled and no further processing should occur;
		/// <c>false</c> to pass the event to the next handler or the gateway default.
		/// </returns>
		bool TryHandle(IAnalyticsEvent @event);
	}
}
