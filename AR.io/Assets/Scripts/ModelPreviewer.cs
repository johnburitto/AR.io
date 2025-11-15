using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates preview of Ar Packet models.
/// </summary>
public class ModelPreviewer : MonoBehaviour
{
	#region Serialized Fields

	[Header("Preview setup")]
	/// <summary>
	/// Preview camera.
	/// </summary>
	[SerializeField] private Camera _previewCamera;
	
	/// <summary>
	/// Place for models.
	/// </summary>
	[SerializeField] private Transform _previewRoot;

	[Header("Preview params")]
	/// <summary>
	/// Camera distance.
	/// </summary>
	[SerializeField] private float _cameraDistance = 1.5f;

	/// <summary>
	/// Model offset.
	/// </summary>
	[SerializeField] private Vector3 _modelOffset = Vector3.zero;

	/// <summary>
	/// Model rotation.
	/// </summary>
	[SerializeField] private Vector3 _modelRotation = Vector3.zero;

	/// <summary>
	/// Prefered size of model.
	/// </summary>
	[SerializeField] private float _preferredSize = 0.6f;

	[Header("Models Container")]
	/// <summary>
	/// Models container.
	/// </summary>
	[SerializeField] private RectTransform _container;

	/// <summary>
	/// Container layout.
	/// </summary>
	[SerializeField] private GridLayoutGroup _layout;

	#endregion

	#region Public Methods

	/// <summary>
	/// Get preview texture.
	/// </summary>
	/// <param name="model">Model.</param>
	/// <returns>Texture of preview.</returns>
	public Texture2D GetPreview(GameObject model)
	{
		_previewCamera.enabled = false;

		var textureSize = (int)(_container.rect.width / _layout.constraintCount);
		var clone = Instantiate(model, _previewRoot);

		SetLayerRecursively(clone, LayerMask.NameToLayer("Preview"));
		PositionModelInFrontOfCamera(clone.transform);

		var renderTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
		
		renderTexture.Create();

		RenderModelToTexture(clone.transform, renderTexture);

		var texture = ConvertRenderTextureToTexture2D(renderTexture);

		DestroyImmediate(clone);
		DestroyImmediate(renderTexture);

		return texture;
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Renders model to texture.
	/// </summary>
	/// <param name="model">Model.</param>
	/// <param name="texture">Texture.</param>
	private void RenderModelToTexture(Transform model, RenderTexture texture)
	{
		_previewCamera.targetTexture = texture;

		Vector3 cameraDirection = Vector3.forward;
		Vector3 cameraPosition = model.position - cameraDirection * _cameraDistance;

		_previewCamera.transform.position = cameraPosition;
		_previewCamera.transform.LookAt(model.position + _modelOffset, Vector3.up);

		model.transform.LookAt(_previewCamera.transform.position);
		model.transform.Rotate(_modelRotation);

		_previewCamera.Render();
	}

	/// <summary>
	/// Adjust model in front of camera.
	/// </summary>
	/// <param name="model">Model.</param>
	private void PositionModelInFrontOfCamera(Transform model)
	{
		var realWidth = GetModelSize(model.gameObject);
		var scale = _preferredSize / realWidth;

		model.gameObject.SetActive(true);
		model.transform.localPosition = Vector3.zero;
		model.localScale = Vector3.one * scale;
	}

	/// <summary>
	/// Sets layer for model components.
	/// </summary>
	/// <param name="model">Model.</param>
	/// <param name="layer">Layer.</param>
	private void SetLayerRecursively(GameObject model, int layer)
	{
		model.layer = layer;

		foreach (Transform child in model.transform)
		{
			SetLayerRecursively(child.gameObject, layer);
		}
	}

	/// <summary>
	/// Converst render texture to texture 2D.
	/// </summary>
	/// <param name="renderTexture">Render texture.</param>
	/// <returns>Preview 2D texture.</returns>
	private Texture2D ConvertRenderTextureToTexture2D(RenderTexture renderTexture)
	{
		RenderTexture.active = renderTexture;

		var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
		
		texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();

		RenderTexture.active = null;

		return texture;
	}

	/// <summary>
	/// Get model width.
	/// </summary>
	/// <param name="model">Model.</param>
	/// <returns>Model width.</returns>
	private float GetModelSize(GameObject model)
	{
		Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

		if (renderers.Length == 0)
		{
			return 0;
		}

		Bounds combinedBounds = renderers[0].bounds;

		foreach (var r in renderers)
		{
			combinedBounds.Encapsulate(r.bounds);
		}

		return combinedBounds.size.x;
	}

	#endregion
}
