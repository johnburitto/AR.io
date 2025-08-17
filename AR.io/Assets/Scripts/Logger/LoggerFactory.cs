using System.Collections.Generic;

using UnityEngine;

using Assets.Scripts;
using Assets.Scripts.Enums;
using Assets.Scripts.Logger.Interfaces;
using Assets.Scripts.Logger.Implementations;

public class LoggerFactory : MonoBehaviour
{
	#region Serialized Properties

	[SerializeField] private List<LoggerPoviderType> _types;

	#endregion

	#region Private Fields

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

	private ILoggerProvider GetLoggerProvider(LoggerPoviderType type)
		=> type switch
		{
			LoggerPoviderType.Unity => new UnityLoggerProvider(),
			_ => new UnityLoggerProvider()
		};

	#endregion
}
