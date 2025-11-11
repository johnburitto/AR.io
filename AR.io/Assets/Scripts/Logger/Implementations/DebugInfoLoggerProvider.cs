using System.Linq;

using Assets.Scripts.Logger.Entities;
using Assets.Scripts.Logger.Interfaces;

using TMPro;

namespace Assets.Scripts.Logger.Implementations
{
	/// <summary>
	/// Implementation of <see cref="ILoggerProvider"/>
	/// </summary>
	public class DebugInfoLoggerProvider : ILoggerProvider
	{
		#region Private Fields

		/// <summary>
		/// Debug info UI element.
		/// </summary>
		private readonly TextMeshProUGUI _debugInfo;

		#endregion

		#region Constructor

		/// <summary>
		/// Creates instance of <see cref="DebugInfoLoggerProvider"/>.
		/// </summary>
		/// <param name="debugInfo">Debug info UI element.</param>
		public DebugInfoLoggerProvider(TextMeshProUGUI debugInfo)
		{
			_debugInfo = debugInfo;
			_debugInfo.gameObject.SetActive(true);
		}

		#endregion

		#region Implementation of ILoggerProvider

		/// <inheritdoc/>
		public void WriteLog(Log log)
		{
			_debugInfo.text = log.Message;
		}

		/// <inheritdoc/>
		public void WriteLogs(params Log[] logs)
		{
			_debugInfo.text = string.Join("\n", logs.Select(log => log.Message));
		}

		#endregion
	}
}
