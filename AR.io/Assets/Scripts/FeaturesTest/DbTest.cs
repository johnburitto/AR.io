using UnityEngine;

using Assets.Scripts;
using Assets.Scripts.Entities;
using Assets.Scripts.DAL.Interfaces;
using Assets.Scripts.DAL.Implementations;

using Newtonsoft.Json;

public class DbTest : MonoBehaviour
{
	private IDbManager<ArPacket> _dbManager;

	void Start()
	{
		_dbManager = new ArPacketsDbManager();
	}

	// Update is called once per frame
	void Update()
	{
		CompositionRoot.Logger.WriteLog(JsonConvert.SerializeObject(_dbManager.GetAll()));
	}
}
