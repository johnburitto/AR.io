using System.Threading.Tasks;

using Assets.Scripts;
using Assets.Scripts.Entities;
using Assets.Scripts.DAL.Interfaces;
using Assets.Scripts.FileManagement.Interfaces;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Ar Packets list UI manager.
/// </summary>
public class ArPacketsListUIManager : MonoBehaviour
{
	#region Serialized Fields
	
	[Header("UI Elements")]
	/// <summary>
	/// Header.
	/// </summary>
	[SerializeField] private RectTransform _header;

	/// <summary>
	/// Button for show the list.
	/// </summary>
	[SerializeField] private Button _listButton;

	/// <summary>
	/// Button for hide the list.
	/// </summary>
	[SerializeField] private Button _cancelButton;

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

	[Header("Ar Components")]
	/// <summary>
	/// Ar tracked image manager.
	/// </summary>
	[SerializeField] private ARTrackedImageManager _arTrackedImageManager;

	/// <summary>
	/// Image tracking manager.
	/// </summary>
	[SerializeField] private ImageTrackingManager _imageTrackingManager;

	/// <summary>
	/// Ar QR scanner.
	/// </summary>
	[SerializeField] private ArQrCodeScanner _arQrScanner;

	/// <summary>
	/// Ar Packet loader.
	/// </summary>
	[SerializeField] private ArPacketLoader _arPacketLoader;

	[Header("UI Managers")]
	/// <summary>
	/// Ar tracked image manager.
	/// </summary>
	[SerializeField] private ArQrCodeScannerUIManager _arQrScannerUIManager;

	/// <summary>
	/// Ar tracked image manager.
	/// </summary>
	[SerializeField] private ArPacketDetailsUIManager _arPacketDetailsUIManager;

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

	private void Start()
	{
		_arPacketsDbManager = CompositionRoot.ArPacketsDbManager;
		_fileManager = CompositionRoot.FileManager;

		_listButton.onClick.AddListener(async () => await OpenList());
		_cancelButton.onClick.AddListener(() => HideList());
	}

	private void OnDestroy()
	{
		_listButton.onClick.RemoveAllListeners();
		_cancelButton.onClick.RemoveAllListeners();
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Close Ar Packets list.
	/// </summary>
	/// <param name="isShowListButton">Indicates whether open or close list button.</param>
	public void HideList(bool isShowListButton = true)
	{
		ClearList();

		_scrollView.SetActive(false);
		_header.gameObject.SetActive(isShowListButton);
		_cancelButton.gameObject.SetActive(false);

		EnableArComponents(true);
	}

	/// <summary>
	/// Open Ar Packets list.
	/// </summary>
	public async Task OpenList()
	{
		EnableArComponents(false);
		await PopulateArPacketsList();

		_scrollView.SetActive(true);
		_header.gameObject.SetActive(false);
		_cancelButton.gameObject.SetActive(true);
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
			var button = listItemInstance.GetComponent<Button>();

			listItemInstance.GetComponentInChildren<RawImage>().texture = logoTexture;
			listItemInstance.GetNamedChild("Ar Packet Name")
				.GetComponent<TMP_Text>().text = $"{arPacket.Name} by {arPacket.Author}";
			listItemInstance.GetNamedChild("Ar Packet Version")
				.GetComponent<TMP_Text>().text = $"v{arPacket.Version}";
			
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(async () => await OpeDetails(arPacket));
		}
	}

	/// <summary>
	/// Clear list.
	/// </summary>
	private void ClearList()
	{
		foreach (Transform child in _contentContainer.transform)
		{
			Destroy(child.gameObject);
		}
	}

	/// <summary>
	/// Enable/disable Ar components.
	/// </summary>
	/// <param name="isEnabled">Is enabled.</param>
	private void EnableArComponents(bool isEnabled)
	{
		_arTrackedImageManager.enabled = isEnabled;
		_imageTrackingManager.enabled = isEnabled;
		_arQrScanner.enabled = isEnabled;
		_arPacketLoader.enabled = isEnabled;

		_arQrScannerUIManager.UpdateQrUi(false, null, null);
	}

	/// <summary>
	/// Open Ar Packet details.
	/// </summary>
	/// <param name="arPacket">Ar Packet.</param>
	private async Task OpeDetails(ArPacket arPacket)
	{
		_arPacketDetailsUIManager.InitDetails(arPacket);
		_arPacketDetailsUIManager.SetMarkersButtonOnClick(async () => await _arPacketDetailsUIManager.LoadMarkers($"{arPacket.Author}/{arPacket.Name}/Markers"));
		await _arPacketDetailsUIManager.ShowDetails();
	}

	#endregion
}
