using System.Collections.Generic;

using Assets.Scripts.Enums;

namespace Assets.Scripts.Logger.Interfaces
{
	/// <summary>
	/// Describes all logger behavior.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Providers.
		/// </summary>
		List<ILoggerProvider> Providers { get; set; }

		/// <summary>
		/// Write log.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="level">Log level.</param>
		void WriteLog(string message, LogLevel level = LogLevel.Debug);

		/// <summary>
		/// Write logs.
		/// </summary>
		/// <param name="logs">Logs.</param>
		void WriteLogs(params (string message, LogLevel level)[] logs);
	}
}
