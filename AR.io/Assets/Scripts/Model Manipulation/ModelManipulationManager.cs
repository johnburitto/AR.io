using System.Collections.Generic;

using Assets.Scripts;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.EnhancedTouch;

using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using ILogger = Assets.Scripts.Logger.Interfaces.ILogger;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Model manipulation manager.
/// </summary>
public class ModelManipulationManager : MonoBehaviour
{
	#region Serialized Fields

	/// <summary>
	/// Rotation speed.
	/// </summary>
	[SerializeField] private float _rotationSpeed;

	#endregion

	#region Private Fields

	/// <summary>
	/// Raycast hits.
	/// </summary>
	private static readonly List<ARRaycastHit> _hits = new();

	/// <summary>
	/// Initial distance between two touches.
	/// </summary>
	private float _initialDistance;

	/// <summary>
	/// Initial scale of the object.
	/// </summary>
	private Vector3 _initialScale;

	/// <summary>
	/// Logger.
	/// </summary>
	private ILogger _logger;

	#endregion

	#region Main Pipeline

	private void Start()
	{
		_logger = CompositionRoot.Logger;
	}

	private void Update()
	{
		var activeTouches = Touch.activeTouches;

		_logger.WriteLog($"Active touches count: {activeTouches.Count}\n Does we have palced object: {PlacedObjectHolder.PlacedObject != null}");

		if (PlacedObjectHolder.PlacedObject == null || activeTouches.Count == 0)
		{
			return;
		}

		ScaleModel(activeTouches);
		RotateModel(activeTouches);
	}

	void OnEnable()
	{
		EnhancedTouchSupport.Enable();
	}

	void OnDisable()
	{
		EnhancedTouchSupport.Disable();
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Scales object based on pinch gesture.
	/// </summary>
	/// <param name="activeTouches">Active touches.</param>
	private void ScaleModel(ReadOnlyArray<Touch> activeTouches)
	{
		_logger.WriteLog($"Try to Scale object");

		if (activeTouches.Count != 2)
		{
			return;
		}

		var firstTouch = activeTouches[0];
		var secondTouch = activeTouches[1];

		if (firstTouch.phase == TouchPhase.Began || secondTouch.phase == TouchPhase.Began)
		{
			_initialDistance = Vector2.Distance(firstTouch.screenPosition, secondTouch.screenPosition);
			_initialScale = PlacedObjectHolder.PlacedObject.transform.localScale;
		}

		if (firstTouch.phase == TouchPhase.Moved || secondTouch.phase == TouchPhase.Moved)
		{
			float currentDistance = Vector2.Distance(firstTouch.screenPosition, secondTouch.screenPosition);

			if (Mathf.Approximately(_initialDistance, 0))
			{
				return;
			}

			float scaleFactor = currentDistance / _initialDistance;

			PlacedObjectHolder.PlacedObject.transform.localScale = _initialScale * scaleFactor;
		}
	}

	/// <summary>
	/// Rotate object based on two finger twist gesture.
	/// </summary>
	/// <param name="activeTouches">Active touches.</param>
	private void RotateModel(ReadOnlyArray<Touch> activeTouches)
	{
		_logger.WriteLog($"Try to Rotate object");

		if (activeTouches.Count != 2)
		{
			return;
		}

		var firstTouch = activeTouches[0];
		var secondTouch = activeTouches[1];

		if (firstTouch.phase == TouchPhase.Moved && secondTouch.phase == TouchPhase.Moved)
		{
			var firstDelat = firstTouch.delta;
			var secondDelta = secondTouch.delta;

			if (Vector2.Dot(firstDelat.normalized, secondDelta.normalized) > 0.9)
			{
				var averageDeltaX = (firstDelat.x + secondDelta.x) / 2;
				var averageDeltaY = (firstDelat.y + secondDelta.y) / 2;

				PlacedObjectHolder.PlacedObject.transform.Rotate(0, -averageDeltaX * _rotationSpeed, 0);
				PlacedObjectHolder.PlacedObject.transform.Rotate(averageDeltaY * _rotationSpeed, 0, 0);
			}
		}
	}

	#endregion
}
