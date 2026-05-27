namespace DRG.Analytics
{
	/// <summary>
	/// Provider-agnostic surface for recording analytics events. Implementations wrap SDKs or backends.
	/// <para>
	/// Use the <see cref="AnalyticsGatewayExtensions.Track(IAnalyticsGateway,string)"/> extension for
	/// quick ad-hoc events that need no parameters.
	/// </para>
	/// </summary>
	public interface IAnalyticsGateway
	{
		/// <summary>Records an analytics event.</summary>
		void Track(IAnalyticsEvent @event);
	}
}
