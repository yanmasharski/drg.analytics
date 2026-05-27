using System.Collections.Generic;

namespace DRG.Analytics
{
	/// <summary>
	/// Read-only contract for an analytics event. Implementations carry a name and an optional
	/// parameter dictionary. Gateways and handlers depend on this interface, never on concrete types.
	/// </summary>
	public interface IAnalyticsEvent
	{
		/// <summary>Provider-agnostic event name sent to analytics backends.</summary>
		string Name { get; }

		/// <summary>Key-value parameters attached to the event. Never null.</summary>
		IReadOnlyDictionary<string, object> Parameters { get; }
	}
}
