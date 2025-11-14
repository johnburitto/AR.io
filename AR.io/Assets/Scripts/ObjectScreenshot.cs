using UnityEngine;
using UnityEngine.UI;

public class ObjectScreenshot : MonoBehaviour
{
	[Header("Preview setup")]
	[SerializeField] private Camera _previewCamera;
	[SerializeField] private Transform _previewRoot;
	[SerializeField] private GameObject[] _modelPrefabs;
	[SerializeField] private RawImage[] _previewSlots;

	[Header("Preview params")]
	[SerializeField] private int _textureSize = 512;
	[SerializeField] private float _cameraDistance = 1.5f;
	[SerializeField] private Vector3 _modelOffset = Vector3.zero;

	private RenderTexture[] _textures;
	private GameObject[] _clones;

	private void Awake()
	{
		if (_previewCamera == null)
		{
			Debug.LogError("Preview camera is not assigned!");
			return;
		}

		_previewCamera.enabled = false;

		int count = Mathf.Min(_modelPrefabs.Length, _previewSlots.Length);

		_textures = new RenderTexture[count];
		_clones = new GameObject[count];

		for (int i = 0; i < count; i++)
		{
			_clones[i] = Instantiate(_modelPrefabs[i], _previewRoot);
			SetLayerRecursively(_clones[i], LayerMask.NameToLayer("Preview"));

			PositionModelInFrontOfCamera(_clones[i].transform);

			var rt = new RenderTexture(_textureSize, _textureSize, 16, RenderTextureFormat.ARGB32);
			rt.Create();
			_textures[i] = rt;

			RenderModelToTexture(_clones[i].transform, rt);

			_previewSlots[i].texture = rt;
		}
	}

	private void RenderModelToTexture(Transform model, RenderTexture targetTexture)
	{
		var previousTarget = _previewCamera.targetTexture;

		_previewCamera.targetTexture = targetTexture;

		Vector3 camDir = Vector3.forward;
		Vector3 camPos = model.position - camDir * _cameraDistance;

		_previewCamera.transform.position = camPos;
		_previewCamera.transform.LookAt(model.position + _modelOffset, Vector3.up);

		_previewCamera.Render();

		_previewCamera.targetTexture = previousTarget;
	}

	private void PositionModelInFrontOfCamera(Transform model)
	{
		model.localPosition = Vector3.zero;
		model.localRotation = Quaternion.identity;
		model.localScale = new Vector3(0.1f, 0.1f, 0.1f);
	}

	private void SetLayerRecursively(GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, layer);
		}
	}
}
