using System.Linq;

using Assets.Scripts.LoadEntities;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

	#endregion

	#region Main Pipeline

	private void Awake()
	{
		GetButtons();
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Shows/hides and updates UI elements for QR code.
	/// </summary>
	/// <param name="isShow">Indicates whether show or hide UI.</param>
	/// <param name="arPacket">Ar Packet.</param>
	public void UpdateQrUi(bool isShow, ResultPoint[] resultPoints, ArPacketSource arPacketSource)
	{
		if (!isShow)
		{
			_qrOutliner.gameObject.SetActive(false);
			_qrData.gameObject.SetActive(false);
			_acceptButton.gameObject.SetActive(false);
			_declineButton.gameObject.SetActive(false);

			return;
		}

		var minX = resultPoints.Min(p => p.X);
		var minY = resultPoints.Min(p => p.Y);
		var maxX = resultPoints.Max(p => p.X);
		var maxY = resultPoints.Max(p => p.Y);

		var textureWidth = (maxX - minX) * 1.5f;
		var textureHeight = (maxY - minY) * 1.5f;
		var buttonsHeight = _acceptButton.rect.height;
		var qrOutlinerRenderer = _qrOutliner.gameObject.GetComponent<UnityEngine.UI.Image>();
		var qrDataRenderer = _qrData.gameObject.GetComponent<UnityEngine.UI.Image>();

		_qrOutliner.sizeDelta = new Vector2(textureWidth, textureHeight);
		_qrData.anchoredPosition = new Vector2(0, textureHeight / 1.5f - 5f);
		_acceptButton.anchoredPosition = new Vector2(-textureWidth / 4, -textureHeight / 2 - buttonsHeight);
		_declineButton.anchoredPosition = new Vector2(textureWidth / 4, -textureHeight / 2 - buttonsHeight);

		//if (ChekIfArPackAlreadyDownloaded(arPacketSource))
		//{
		//	qrOutlinerRenderer.color = Color.green;
		//	qrDataRenderer.color = Color.green;
		//}
		//else
		//{
		//	qrOutlinerRenderer.color = Color.gray;
		//	qrDataRenderer.color = Color.gray;
		//}

		_arPacketName.text = $"{arPacketSource.Name} [{arPacketSource.Author}]";
		_arCurrentVersion.text = "1.0.0";
		_arNewVersion.text = "1.0.0";

		_qrOutliner.gameObject.SetActive(true);
		_qrData.gameObject.SetActive(true);
		_acceptButton.gameObject.SetActive(true);
		_declineButton.gameObject.SetActive(true);
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
	private void GetButtons()
	{
		_acceptButtonElement = _acceptButton.gameObject.GetComponent<Button>();
		_declineButtonElement = _declineButton.gameObject.GetComponent<Button>();
	}
	
	#endregion
}
