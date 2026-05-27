using System.Collections.Generic;
using DRG.Core.Logs;

namespace DRG.Analytics
{
	/// <summary>
	/// Default <see cref="IAnalyticsEvent"/> implementation and base class for typed events.
	/// <para>
	/// Use directly for ad-hoc events:
	/// <code>new AnalyticsEvent("level_start", new() { ["level"] = 1 })</code>
	/// </para>
	/// <para>
	/// Subclass for typed events that carry strongly-typed properties:
	/// <code>
	/// public sealed class EventLevelStart : AnalyticsEvent
	/// {
	///     public EventLevelStart(int level) : base("level_start") => Set("level", level);
	/// }
	/// </code>
	/// </para>
	/// </summary>
	public class AnalyticsEvent : IAnalyticsEvent
	{
		/// <summary>Substituted value when a parameter is null or an empty string.</summary>
		public const string NullOrEmptyParameterValue = "nullOrEmpty";

		/// <summary>Logger for null/empty parameter warnings. Set once at startup (e.g. from bootstrap).</summary>
		public static ILogger Logger { get; set; }

		private readonly Dictionary<string, object> _parameters;

		/// <inheritdoc/>
		public string Name { get; }

		/// <inheritdoc/>
		public IReadOnlyDictionary<string, object> Parameters => _parameters;

		/// <summary>
		/// Creates an event with the given name and optional parameter dictionary.
		/// Pass <c>null</c> to start with an empty set. Null or empty string parameter values are
		/// replaced with <see cref="NullOrEmptyParameterValue"/> and a warning is logged.
		/// </summary>
		public AnalyticsEvent(string name, Dictionary<string, object> parameters = null)
		{
			Name = name;
			_parameters = new Dictionary<string, object>();
			if (parameters == null)
			{
				return;
			}

			foreach (var kv in parameters)
			{
				Set(kv.Key, kv.Value);
			}
		}

		/// <summary>Constructor for subclasses. Starts with an empty parameter dictionary.</summary>
		protected AnalyticsEvent(string name)
		{
			Name = name;
			_parameters = new Dictionary<string, object>();
		}

		/// <summary>Sets or overwrites a parameter value. For use by subclass constructors.</summary>
		protected void Set(string key, object value) => _parameters[key] = NormalizeParameter(key, value);

		private object NormalizeParameter(string key, object value)
		{
			if (value != null && (value is not string s || !string.IsNullOrEmpty(s)))
			{
				return value;
			}

			LogNullOrEmptyWarning(Name, key);
			return NullOrEmptyParameterValue;
		}

		private static void LogNullOrEmptyWarning(string eventName, string key) =>
			Logger?.LogWarning(
				$"[Analytics] Event '{eventName}' parameter '{key}' is null or empty; using '{NullOrEmptyParameterValue}'.");
	}
}
