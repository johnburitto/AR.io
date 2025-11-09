using System.Linq;

using Assets.Scripts;
using Assets.Scripts.Enums;
using Assets.Scripts.LoadEntities;
using Assets.Scripts.DAL.Interfaces;
using Assets.Scripts.FileManagement.Interfaces;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using ZXing;

public class ArQrCodeScannerUIManager : MonoBehaviour
{
	#region Serialized Fields

	[Header("UI Elements")]
	/// <summary>
	/// Qr code outliner.
	/// </summary>
	[SerializeField] private RectTransform _qrOutliner;

	/// <summary>
	/// Qr code outliner.
	/// </summary>
	[SerializeField] private RectTransform _qrData;

	/// <summary>
	/// Accept button.
	/// </summary>
	[SerializeField] private RectTransform _acceptButton;

	/// <summary>
	/// Decline button.
	/// </summary>
	[SerializeField] private RectTransform _declineButton;

	[Header("UI Text Elements")]
	/// <summary>
	/// Ar Packet name text element.
	/// </summary>
	[SerializeField] private TextMeshProUGUI _arPacketName;

	/// <summary>
	/// Ar Packet current version text element.
	/// </summary>
	[SerializeField] private TextMeshProUGUI _arCurrentVersion;

	/// <summary>
	/// Ar Packet new version text element.
	/// </summary>
	[SerializeField] private TextMeshProUGUI _arNewVersion;

	#endregion

	#region Private Fields

	/// <summary>
	/// Accept button button element.
	/// </summary>
	private Button _acceptButtonElement;

	/// <summary>
	/// Declien button button element.
	/// </summary>
	private Button _declineButtonElement;

	/// <summary>
	/// Ar Packets db manager.
	/// </summary>
	private IArPacketsDbManager _arPacketsDbManager;

	/// <summary>
	/// File manager.
	/// </summary>
	private IFileManager _fileManager;

	/// <summary>
	/// QR outlinr renderer.
	/// </summary>
	private Image _qrOutlinerRenderer;

	/// <summary>
	/// QR data renderer.
	/// </summary>
	private Image _qrDataRenderer;

	#endregion

	#region Main Pipeline

	private void Awake()
	{
		GetUIComponents();
	}

	private void Start()
	{
		_arPacketsDbManager = CompositionRoot.ArPacketsDbManager;
		_fileManager = CompositionRoot.FileManager;
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Shows/hides and updates UI elements for QR code.
	/// </summary>
	/// <param name="isShow">Indicates whether show or hide UI.</param>
	/// <param name="resultPoints">Points of QR code.</param>
	/// <param name="arPacketSource">Ar Packet source.</param>
	public void UpdateQrUi(bool isShow, ResultPoint[] resultPoints, ArPacketSource arPacketSource)
	{
		if (!isShow)
		{
			_qrOutliner.gameObject.SetActive(false);
			_qrData.gameObject.SetActive(false);
			_acceptButton.gameObject.SetActive(false);
			_declineButton.gameObject.SetActive(false);
			_arPacketName.gameObject.SetActive(false);
			_arCurrentVersion.gameObject.SetActive(false);
			_arNewVersion.gameObject.SetActive(false);

			return;
		}

		PlaceUIElements(resultPoints);
		AdjsutUIColors(arPacketSource);
		PopulateValueToQrData(arPacketSource);
		SetVisibility(arPacketSource);
	}

	/// <summary>
	/// Set accept button click event.
	/// </summary>
	/// <param name="action">Action.</param>
	public void SetAcceptButtonOnClick(UnityAction action)
	{
		_acceptButtonElement.onClick.RemoveAllListeners();
		_acceptButtonElement.onClick.AddListener(action);
	}

	/// <summary>
	/// Set decline button click event.
	/// </summary>
	/// <param name="action">Action.</param>
	public void SetDeclineButtonOnClick(UnityAction action)
	{
		_acceptButtonElement.onClick.RemoveAllListeners();
		_declineButtonElement.onClick.AddListener(action);
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Get buttons elements.
	/// </summary>
	private void GetUIComponents()
	{
		_acceptButtonElement = _acceptButton.gameObject.GetComponent<Button>();
		_declineButtonElement = _declineButton.gameObject.GetComponent<Button>();
		_qrOutlinerRenderer = _qrOutliner.gameObject.GetComponent<Image>();
		_qrDataRenderer = _qrData.gameObject.GetComponent<Image>();
	}

	/// <summary>
	/// Place UI over QR code.
	/// </summary>
	/// <param name="resultPoints">Points of QR code.</param>
	private void PlaceUIElements(ResultPoint[] resultPoints)
	{
		var minX = resultPoints.Min(p => p.X);
		var minY = resultPoints.Min(p => p.Y);
		var maxX = resultPoints.Max(p => p.X);
		var maxY = resultPoints.Max(p => p.Y);

		var textureWidth = (maxX - minX) * 1.5f;
		var textureHeight = (maxY - minY) * 1.5f;
		var buttonsHeight = _acceptButton.rect.height;

		_qrOutliner.sizeDelta = new Vector2(textureWidth, textureHeight);
		_qrData.anchoredPosition = new Vector2(0, textureHeight / 1.5f - 5f);
		_acceptButton.anchoredPosition = new Vector2(-textureWidth / 4, -textureHeight / 2 - buttonsHeight);
		_declineButton.anchoredPosition = new Vector2(textureWidth / 4, -textureHeight / 2 - buttonsHeight);
	}

	/// <summary>
	/// Change UI colors due to data from QR.
	/// </summary>
	/// <param name="arPacketSource">Ar Packet source.</param>
	private void AdjsutUIColors(ArPacketSource arPacketSource)
	{
		var (outlineColor, textColor) = GetOutlinerAndTextColor(arPacketSource);

		_qrOutlinerRenderer.color = outlineColor;
		_qrDataRenderer.color = outlineColor;
		_arPacketName.color = textColor;
		_arCurrentVersion.color = textColor;
		_arNewVersion.color = textColor;
	}

	/// <summary>
	/// Populates data to QR data.
	/// </summary>
	/// <param name="arPacketSource">Ar Packet source.</param>
	private void PopulateValueToQrData(ArPacketSource arPacketSource)
	{
		var arPacket = _arPacketsDbManager.GetArPacketByAuthorAndName(arPacketSource.Author, arPacketSource.Name);

		_arPacketName.text = arPacketSource.Name;
		_arCurrentVersion.text = arPacket.Version;
		_arNewVersion.text = arPacketSource.Version;
	}

	/// <summary>
	/// Sets visibility of UI elements.
	/// </summary>
	/// <param name="arPacketSource">Ar Packet source.</param>
	private void SetVisibility(ArPacketSource arPacketSource)
	{
		var arPacketDbState = _arPacketsDbManager.GetArPacketDbState(arPacketSource.Author, arPacketSource.Name, arPacketSource.Version);
		var isArPacketDownloaded = _fileManager.IsArPacketDownloaded(arPacketSource.Author, arPacketSource.Name);

		_qrOutliner.gameObject.SetActive(true);
		_qrData.gameObject.SetActive(true);
		_acceptButton.gameObject.SetActive(true);
		_declineButton.gameObject.SetActive(true);
		_arPacketName.gameObject.SetActive(true);
		_arCurrentVersion.gameObject.SetActive(true);
		_arNewVersion.gameObject.SetActive(true);

		if (!isArPacketDownloaded)
		{
			_acceptButton.gameObject.SetActive(false);
		}
		
		if (arPacketDbState == ArPacketDbState.None)
		{
			_arNewVersion.gameObject.SetActive(false);
		}
		else if (arPacketDbState == ArPacketDbState.InDb)
		{
			_acceptButton.gameObject.SetActive(false);
			_arNewVersion.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Get colors for outliner and text.
	/// </summary>
	/// <param name="arPacketSource">Ar Packet source.</param>
	/// <returns>Outliner and text colors.</returns>
	private (Color, Color) GetOutlinerAndTextColor(ArPacketSource arPacketSource)
	{
		var arPacketDbState = _arPacketsDbManager.GetArPacketDbState(arPacketSource.Author, arPacketSource.Name, arPacketSource.Version);
		var isArPacketDownloaded = _fileManager.IsArPacketDownloaded(arPacketSource.Author, arPacketSource.Name);

		if (!isArPacketDownloaded)
		{
			if (arPacketDbState == ArPacketDbState.None)
			{
				return (Color.gray, Color.black);
			}

			return (Color.red, Color.white);
		}

		if (arPacketDbState == ArPacketDbState.InDb)
		{
			return (Color.green, Color.white);
		}
		else
		{
			return (Color.orange, Color.black);
		}
	}
	
	#endregion
}
