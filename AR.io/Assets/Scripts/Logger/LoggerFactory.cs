using System.Collections.Generic;

using UnityEngine;

using Assets.Scripts;
using Assets.Scripts.Enums;
using Assets.Scripts.Logger.Interfaces;
using Assets.Scripts.Logger.Implementations;

/// <summary>
/// Logger factory for <see cref="ILoggerProvider"/>.
/// </summary>
public class LoggerFactory : MonoBehaviour
{
	#region Serialized Properties

	/// <summary>
	/// List of logger provider types.
	/// </summary>
	[SerializeField] private List<LoggerPoviderType> _types;

	#endregion

	#region Private Fields

	/// <summary>
	/// List of logger providers.
	/// </summary>
	private List<ILoggerProvider> _providers = new();

	#endregion

	#region Main pipeline

	void Awake()
	{
		_types.ForEach(type => _providers.Add(GetLoggerProvider(type)));

		CompositionRoot.Logger = new CustomLogger(_providers);
	}

	#endregion

	#region Private methods

	/// <summary>
	/// Creates logger provider by type.
	/// </summary>
	/// <param name="type">Logger provider type.</param>
	/// <returns>Logger provider.</returns>
	private ILoggerProvider GetLoggerProvider(LoggerPoviderType type)
		=> type switch
		{
			LoggerPoviderType.Unity => new UnityLoggerProvider(),
			_ => new UnityLoggerProvider()
		};

	#endregion
}
