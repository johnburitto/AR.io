using Assets.Scripts;
using Assets.Scripts.DAL.Interfaces;

using TMPro;

using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.UI;
using Assets.Scripts.FileManagement.Interfaces;
using System.Threading.Tasks;

/// <summary>
/// Ar Packets list UI manager.
/// </summary>
public class ArPacketsListUIManager : MonoBehaviour
{
	#region Serialized Fields
	
	[Header("UI Elements")]
	/// <summary>
	/// Button for show/hide the list.
	/// </summary>
	//[SerializeField] private Button _toggleListButton;

	/// <summary>
	/// Scroll view container.
	/// </summary>
	[SerializeField] private GameObject _scrollView;

	/// <summary>
	/// Content cointainer.
	/// </summary>
	[SerializeField] private Transform _contentContainer;

	/// <summary>
	/// List item.
	/// </summary>
	[SerializeField] private GameObject _listItem;

	#endregion

	#region Private Fields

	/// <summary>
	/// Ar Packets db manager.
	/// </summary>
	private IArPacketsDbManager _arPacketsDbManager;

	/// <summary>
	/// File manager.
	/// </summary>
	private IFileManager _fileManager;

	#endregion

	#region Main Pipeline
	
	private async void Start()
	{
		_arPacketsDbManager = CompositionRoot.ArPacketsDbManager;
		_fileManager = CompositionRoot.FileManager;

		await PopulateArPacketsList();
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Populate AR Packets list.
	/// </summary>
	private async Task PopulateArPacketsList()
	{
		var arPackets = _arPacketsDbManager.GetEnabledArPackets();

		foreach (var arPacket in arPackets)
		{
			var listItemInstance = Instantiate(_listItem, _contentContainer);
			var logoTexture = await _fileManager.GetLogo($"{_fileManager.BasePath}/{arPacket.Author}/{arPacket.Name}/logo.png");

			listItemInstance.GetComponentInChildren<RawImage>().texture = logoTexture;
			listItemInstance.GetNamedChild("Ar Packet Name")
				.GetComponent<TMP_Text>().text = $"{arPacket.Name} by {arPacket.Author}";
			listItemInstance.GetNamedChild("Ar Packet Version")
				.GetComponent<TMP_Text>().text = $"v{arPacket.Version}";
		}
	}

	#endregion
}
