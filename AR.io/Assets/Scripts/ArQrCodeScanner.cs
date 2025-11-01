using System;
using System.Threading.Tasks;

using Assets.Scripts;
using Assets.Scripts.Entities;
using Assets.Scripts.LoadEntities;
using Assets.Scripts.DAL.Interfaces;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;

using ZXing;
using ZXing.Common;

using TMPro;

using static UnityEngine.XR.ARSubsystems.XRCpuImage;

/// <summary>
/// Reads freames from ar camera to finq qr codes.
/// </summary>
public class ArQrCodeScanner : MonoBehaviour
{
	#region Serialiez Fields

	/// <summary>
	/// Period before scans.
	/// </summary>
	[SerializeField] private float _scanInterval = 1f;

	/// <summary>
	/// Ar camera manager.
	/// </summary>
	[SerializeField] private ARCameraManager _cameraManager;

	/// <summary>
	/// Ar Packets loader.
	/// </summary>
	[SerializeField] private ArPacketLoader _arPacketsLoader;

	/// <summary>
	/// Debug info.
	/// </summary>
	[SerializeField] private TextMeshProUGUI _debugInfo;

	#endregion

	#region Private Fields

	/// <summary>
	/// Qr code readed.
	/// </summary>
	private IBarcodeReader _barcodeReader;

	/// <summary>
	/// Time spend from last camera scan.
	/// </summary>
	private float _timeSinceLastScan = 0f;

	/// <summary>
	/// Ar Packets db manager.
	/// </summary>
	private IArPacketsDbManager _arPacketsDbManager;

	#endregion

	#region Main Pipeline

	void Awake()
	{
		_debugInfo.text = $"Start Awake";
		_barcodeReader = new BarcodeReader()
		{
			AutoRotate = true,
			Options = new DecodingOptions()
			{
				TryInverted = true
			}
		};
		try
		{
			_debugInfo.text = $"Try to get _arPacketsDbManager";
			_arPacketsDbManager = CompositionRoot.ArPacketsDbManager;
			_debugInfo.text = $"End Awake";
		}
		catch (Exception ex)
		{
			_debugInfo.text = $"{Application.persistentDataPath}\n\n\n{ex.Message}\n\n\n{ex.InnerException}";
		}
	}

	async void FixedUpdate()
	{
		_timeSinceLastScan += Time.fixedDeltaTime;

		if (_timeSinceLastScan < _scanInterval)
		{
			return;
		}

		_timeSinceLastScan = 0;

		if (TryReadQrCode(out var sourceUrl))
		{
			var (isSuccess, arPacketSource) = await TryDownloadArPacketSource(sourceUrl);

			if (ChekIfArPackAlreadyDownloaded(arPacketSource))
			{
				_debugInfo.text = $"Ar Packet '{arPacketSource.Name}' by {arPacketSource.Author} already downloaded!";

				return;
			}

			if (isSuccess)
			{
				await DownloadArPacket(arPacketSource);
			}
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Gets ar camera frame and try to read qr code.
	/// </summary>
	/// <param name="sourceUrl">Ar packet source url.</param>
	/// <returns>Whether qr code was readed or not.</returns>
	private bool TryReadQrCode(out string sourceUrl)
	{
		if (_cameraManager.TryAcquireLatestCpuImage(out var image))
		{
			using (image)
			{
				var conversionParams = new ConversionParams()
				{
					inputRect = new RectInt(0, 0, image.width, image.height),
					outputDimensions = new Vector2Int(image.width, image.height),
					outputFormat = TextureFormat.RGBA32,
					transformation = Transformation.MirrorY
				};
				var texture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
				var rawTextureData = texture.GetRawTextureData<byte>();

				image.Convert(conversionParams, rawTextureData);
				texture.Apply();

				var result = _barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);
				Destroy(texture);

				if (result != null)
				{
					_debugInfo.text = $"Data from QR code: {result.Text}";

					sourceUrl = result.Text;

					return true;
				}
				else
				{
					//_debugInfo.text = $"Can't read QR code";

					sourceUrl = null;

					return false;
				}
			}
		}

		sourceUrl = null;

		return false;
	}

	/// <summary>
	/// Tries to download Ar Packet source via url scaned from QR code.
	/// </summary>
	/// <param name="sourceUrl">Url to Ar Packet source.</param>
	/// <returns>Whether Ar Packet source is downloaded and Ar Packet source.</returns>
	private async Task<(bool, ArPacketSource)> TryDownloadArPacketSource(string sourceUrl)
	{
		if (sourceUrl == null)
		{
			return (false, null);
		}

		using (UnityWebRequest request = UnityWebRequest.Get(sourceUrl))
		{
			await request.SendWebRequest();

			if (request.result == UnityWebRequest.Result.Success)
			{
				try
				{
					var result = request.downloadHandler.text;
					var arPacketSource = JsonConvert.DeserializeObject<ArPacketSource>(result);

					if (arPacketSource != null)
					{
						_debugInfo.text = $"Successfully download Ar Packet source '{arPacketSource.Name}' by {arPacketSource.Author}";

						return (true, arPacketSource);
					}
					else
					{
						_debugInfo.text = $"Url doesn't contatains any Ar Packet sources.";

						return (false, null);
					}
				}
				catch (Exception )
				{
					_debugInfo.text = $"Url doesn't contatains any Ar Packet sources.";

					return (false, null);
				}
			}
			else
			{
				_debugInfo.text = $"Can't reach Ar Packet source. Response code: {request.responseCode}";

				return (false, null);
			}
		}
	}

	/// <summary>
	/// Checks if Ar Packet already downloaded.
	/// </summary>
	/// <param name="arPacketSource">Ar packet source.</param>
	/// <returns>Whether Ar Packet is downloaded.</returns>
	private bool ChekIfArPackAlreadyDownloaded(ArPacketSource arPacketSource)
		=> _arPacketsDbManager.GetArPacketByNameAndAuthor(arPacketSource.Name, arPacketSource.Author) != null;

	/// <summary>
	/// Process Ar Packet source and download all packet's elements.
	/// </summary>
	/// <param name="arPacketSource">Ar Packet source.</param>
	private async Task DownloadArPacket(ArPacketSource arPacketSource)
	{
		if (arPacketSource == null)
		{
			return;
		}

		await _arPacketsLoader.ProcessArPacketSource(arPacketSource);
		_arPacketsDbManager.Create(new ArPacket
		{
			Name = arPacketSource.Name,
			Author = arPacketSource.Author,
			IsEnabled = true,
			AddedDate = DateTime.Now
		});
	}

	#endregion
}
