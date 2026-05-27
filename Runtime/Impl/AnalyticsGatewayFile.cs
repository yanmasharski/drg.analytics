using System;
using System.IO;
using System.Text;

namespace DRG.Analytics
{
	/// <summary>
	/// File-backed <see cref="IAnalyticsGateway"/> that appends every tracked event to a log file.
	/// <para>
	/// Each line is formatted as: <c>ISO-8601 UTC timestamp TAB event name TAB JSON-like parameters</c>.
	/// </para>
	/// Intended for debug builds and QA pipelines — not for production analytics.
	/// </summary>
	public sealed class AnalyticsGatewayFile : AnalyticsGatewayBase
	{
		private readonly string _filePath;

		/// <param name="filePath">Path to the log file. Parent directory is created if missing.</param>
		public AnalyticsGatewayFile(string filePath)
		{
			_filePath = filePath;
			var directory = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);
		}

		protected override void TrackDefault(IAnalyticsEvent @event)
		{
			var sb = new StringBuilder();
			sb.Append(DateTime.UtcNow.ToString("O"));
			sb.Append('\t');
			sb.Append(@event.Name);

			if (@event.Parameters.Count > 0)
			{
				sb.Append('\t');
				sb.Append('{');
				var first = true;
				foreach (var kv in @event.Parameters)
				{
					if (!first) sb.Append(", ");
					sb.Append(kv.Key);
					sb.Append('=');
					sb.Append(kv.Value);
					first = false;
				}
				sb.Append('}');
			}

			sb.Append(Environment.NewLine);
			File.AppendAllText(_filePath, sb.ToString());
		}
	}
}
