using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Assets.Scripts;
using Assets.Scripts.Entities;
using Assets.Scripts.FileManagement.Interfaces;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Ar Packet details UI manager.
/// </summary>
public class ArPacketDetailsUIManager : MonoBehaviour
{
	#region Serialized Fields

	[Header("UI Rects")]
	/// <summary>
	/// Layout rectangle.
	/// </summary>
	[SerializeField] private RectTransform _gridRect;

	[Header("UI Elements")]
	/// <summary>
	/// Ar Packet logo.
	/// </summary>
	[SerializeField] private RawImage _logo;

	/// <summary>
	/// Grid layout group.
	/// </summary>
	[SerializeField] private GridLayoutGroup _grid;

	/// <summary>
	/// Markers button.
	/// </summary>
	[SerializeField] private Button _markersButton;

	/// <summary>
	/// Models button
	/// </summary>
	[SerializeField] private Button _modelsButton;

	/// <summary>
	/// Cancel button.
	/// </summary>
	[SerializeField] private Button _cancelButton;

	/// <summary>
	/// Layout rectangle.
	/// </summary>
	[SerializeField] private GameObject _listItem;

	[Header("UI Text Elements")]
	/// <summary>
	/// Ar Packet name.
	/// </summary>
	[SerializeField] private TextMeshProUGUI _arPacketName;

	/// <summary>
	/// Ar Packet author.
	/// </summary>
	[SerializeField] private TextMeshProUGUI _arPacketAuthor;

	/// <summary>
	/// Ar Packet version.
	/// </summary>
	[SerializeField] private TextMeshProUGUI _arPacketVersion;

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
	/// Ar Qr code scanner UI manager.
	/// </summary>
	[SerializeField] private ArQrCodeScannerUIManager _arQrScannerUIManager;

	/// <summary>
	/// Ar Packet list UI manager.
	/// </summary>
	[SerializeField] private ArPacketsListUIManager _arPacketListUIManager;

	[Header("Utils")]
	/// <summary>
	/// Model previewer.
	/// </summary>
	[SerializeField] private ModelPreviewer _modelPreviewer;

	#endregion

	#region Private Feilds

	/// <summary>
	/// Ar Packet.
	/// </summary>
	private ArPacket _arPacket;

	/// <summary>
	/// File manager.
	/// </summary>
	private IFileManager _fileManager;

	#endregion

	#region Main Pipeline

	private void Start()
	{
		ResizeLayoutCell();

		_fileManager = CompositionRoot.FileManager;
		_cancelButton.onClick.AddListener(async () => await CloseDetails());
	}

	private void OnDestroy()
	{
		_cancelButton.onClick.RemoveAllListeners();
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Inits details UI with ar packet data.
	/// </summary>
	/// <param name="arPacket"></param>
	public void InitDetails(ArPacket arPacket)
	{
		_arPacket = arPacket;
	}

	/// <summary>
	/// Show details.
	/// </summary>
	/// <returns></returns>
	public async Task ShowDetails()
	{
		_arPacketListUIManager.HideList(false, true);

		EnableArComponents(false);
		await PopulateHeaderData();
		await LoadMarkers($"{_arPacket.Author}/{_arPacket.Name}/Markers");
		SetModelsButtonOnClick(() => LoadModels($"{_arPacket.Author}/{_arPacket.Name}/Models"));
		EnableUIComponents(true);
	}

	/// <summary>
	/// Load markers.
	/// </summary>
	/// <param name="path">Path.</param>
	public async Task LoadMarkers(string path)
	{
		ClearList();

		var filePathes = _fileManager.GetElementsPathes(path);
		var markers = await _fileManager.GetMarkers(filePathes);

		foreach (var marker in markers.Select((value, i) => new { i, value }))
		{
			var listItem = Instantiate(_listItem, _grid.transform);
			var preview = listItem.GetNamedChild("Preview").GetComponent<RawImage>();
			var text = listItem.GetNamedChild("Name").GetComponent<TMP_Text>();

			preview.texture = marker.value;
			text.text = Path.GetFileNameWithoutExtension(filePathes[marker.i]);
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Resize layout cell based on screen size.
	/// </summary>
	private void ResizeLayoutCell()
	{
		var gridWidth = _gridRect.rect.width;
		var cellSize = gridWidth / _grid.constraintCount;

		_grid.cellSize = new Vector2(cellSize, cellSize);
	}

	/// <summary>
	/// Populate data in UI elements.
	/// </summary>
	private async Task PopulateHeaderData()
	{
		var logo = await _fileManager.GetLogo($"{_fileManager.BasePath}/{_arPacket.Author}/{_arPacket.Name}/logo.png");

		_logo.texture = logo;
		_arPacketName.text = _arPacket.Name;
		_arPacketAuthor.text = $"by {_arPacket.Author}";
		_arPacketVersion.text = $"v{_arPacket.Version}";
	}

	/// <summary>
	/// Enable/disable UI components.
	/// </summary>
	/// <param name="isEnabled">Is enabled.</param>
	private void EnableUIComponents(bool isEnabled)
	{
		_gridRect.gameObject.SetActive(isEnabled);
		_logo.gameObject.SetActive(isEnabled);
		_grid.gameObject.SetActive(isEnabled);
		_markersButton.gameObject.SetActive(isEnabled);
		_modelsButton.gameObject.SetActive(isEnabled);
		_cancelButton.gameObject.SetActive(isEnabled);
		_arPacketName.gameObject.SetActive(isEnabled);
		_arPacketAuthor.gameObject.SetActive(isEnabled);
		_arPacketVersion.gameObject.SetActive(isEnabled);
	}

	/// <summary>
	/// Clear list.
	/// </summary>
	private void ClearList()
	{
		foreach (Transform child in _grid.transform)
		{
			Destroy(child.gameObject);
		}
	}

	/// <summary>
	/// Set markers button click event.
	/// </summary>
	/// <param name="action">Action.</param>
	public void SetMarkersButtonOnClick(UnityAction action)
	{
		_markersButton.onClick.RemoveAllListeners();
		_markersButton.onClick.AddListener(action);
	}

	/// <summary>
	/// Set models button click event.
	/// </summary>
	/// <param name="action">Action.</param>
	public void SetModelsButtonOnClick(UnityAction action)
	{
		_modelsButton.onClick.RemoveAllListeners();
		_modelsButton.onClick.AddListener(action);
	}

	/// <summary>
	/// Close details.
	/// </summary>
	private async Task CloseDetails()
	{
		ClearList();
		EnableUIComponents(false);
		EnableArComponents(true);

		await _arPacketListUIManager.OpenList();
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

	private void LoadModels(string path)
	{
		ClearList();

		var modelsNames = _fileManager.GetElementsPathes(path).Select(file => Path.GetFileNameWithoutExtension(file));

		foreach (var modelName in modelsNames)
		{
			var listItem = Instantiate(_listItem, _grid.transform);
			var preview = listItem.GetNamedChild("Preview").GetComponent<RawImage>();
			var text = listItem.GetNamedChild("Name").GetComponent<TMP_Text>();

			preview.texture = _modelPreviewer.GetPreview(_imageTrackingManager[modelName]);
			text.text = modelName;
		}
	}

	#endregion
}
