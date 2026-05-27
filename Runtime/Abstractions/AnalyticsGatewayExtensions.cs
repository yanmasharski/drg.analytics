namespace DRG.Analytics
{
	/// <summary>
	/// Convenience extensions for <see cref="IAnalyticsGateway"/>.
	/// </summary>
	public static class AnalyticsGatewayExtensions
	{
		/// <summary>
		/// Tracks a name-only event with no parameters.
		/// Equivalent to <c>gateway.Track(new AnalyticsEvent(eventName))</c>.
		/// </summary>
		public static void Track(this IAnalyticsGateway gateway, string eventName)
			=> gateway.Track(new AnalyticsEvent(eventName));
	}
}
