using Assets.Scripts;
using Assets.Scripts.Entities;
using Assets.Scripts.FileManagement.Interfaces;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
	/// Layout rectangle.
	/// </summary>
	[SerializeField] private GameObject _listItem;


	[Header("UI Elements")]
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

	[Header("UI Managers")]
	/// <summary>
	/// Ar tracked image manager.
	/// </summary>
	[SerializeField] private ArPacketsListUIManager _arPacketListUIManager;

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

	public async Task ShowDetails()
	{
		await PopulateData();
		
		EnableUIComponents(true);
		
		await LoadMarkers($"{_arPacket.Author}/{_arPacket.Name}/Markers");
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
	private async Task PopulateData()
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
	/// Set accept button click event.
	/// </summary>
	/// <param name="action">Action.</param>
	public void SetMarkersButtonOnClick(UnityAction action)
	{
		_markersButton.onClick.RemoveAllListeners();
		_markersButton.onClick.AddListener(action);
	}

	#endregion
}
