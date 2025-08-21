using System.Linq;
using System.Collections.Generic;

using Assets.Scripts.Enums;
using Assets.Scripts.Logger.Entities;
using Assets.Scripts.Logger.Interfaces;

namespace Assets.Scripts.Logger.Implementations
{
	/// <summary>
	/// Realisation of <see cref="ILogger"/>.
	/// </summary>
	public class CustomLogger : ILogger
	{
		#region Properties

		/// <inheritdoc/>
		public List<ILoggerProvider> Providers { get; set; } = new();

		#endregion

		#region Constructor
		
		/// <summary>
		/// Creates instance of <see cref="CustomLogger"/> class.
		/// </summary>
		/// <param name="providers">Logger providers.</param>
		public CustomLogger(List<ILoggerProvider> providers)
		{
			Providers = providers;
		}

		#endregion

		#region Implementation of ILogger

		/// <inheritdoc/>
		public void WriteLog(string message, LogLevel level = LogLevel.Debug)
			=> Providers.ForEach(provider => provider.WriteLog(new()
			{
				Level = level,
				Message = message
			}));

		/// <inheritdoc/>
		public void WriteLogs(params (string message, LogLevel logLevel)[] logs)
			=> Providers.ForEach((provider) => provider.WriteLogs(logs.ToList().Select(log => new Log()
			{
				Level = log.logLevel,
				Message = log.message
			}).ToArray()));

		#endregion
	}
}
