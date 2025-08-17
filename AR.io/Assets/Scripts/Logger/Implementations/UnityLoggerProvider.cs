using System.Linq;

using UnityEngine;

using Assets.Scripts.Enums;
using Assets.Scripts.Logger.Entities;
using Assets.Scripts.Logger.Interfaces;

namespace Assets.Scripts.Logger.Implementations
{
	/// <summary>
	/// Realisation of <see cref="ILoggerProvider"/>.
	/// </summary>
	public class UnityLoggerProvider : ILoggerProvider
	{
		#region Implementation of ILoggerProvider

		/// <inheritdoc/>
		public void WriteLog(Log log)
			=> ProcessLog(log);

		/// <inheritdoc/>
		public void WriteLogs(params Log[] logs)
			=> logs.ToList().ForEach(log => ProcessLog(log));

		#endregion

		#region Private Methods

		/// <summary>
		/// Process the log.
		/// </summary>
		/// <param name="log">Log.</param>
		private void ProcessLog(Log log)
		{
			switch (log.Level)
			{
				case LogLevel.Information: Debug.Log(log.Message); break;
				case LogLevel.Debug: Debug.Log(log.Message); break;
				case LogLevel.Error: Debug.LogError(log.Message); break;
				case LogLevel.Warning: Debug.LogWarning(log.Message); break;
				default: Debug.Log(log.Message); break;
			}
		}
		
		#endregion 
	}
}
