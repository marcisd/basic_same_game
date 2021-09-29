using UnityEngine;

namespace MSD.BasicSameGame.View
{
	/// <summary>
	/// Ensures that the grid fits within the camera.
	/// </summary>
	[RequireComponent(typeof(SameGameView))]
	public class SameGameViewCameraInitializer : MonoBehaviour
	{
		[SerializeField]
		private Camera _camera;

		private SameGameView _mySameGameView;

		private void Awake()
		{
			_mySameGameView = GetComponent<SameGameView>();
		}

		private void Start()
		{
			_camera.orthographic = true;

			Vector2Int size = _mySameGameView.GridSize;
			_camera.orthographicSize = CalculateOrtographicSize(size);

			Vector3 origCamPos = _camera.transform.position;
			Vector3 position = new Vector3((size.x * 0.5f) - 0.5f, (size.y * 0.5f) - 0.5f, origCamPos.z);
			_camera.transform.position = position;
		}

		private float CalculateOrtographicSize(Vector2Int size)
		{
			float screenRatio = (float)Screen.width / Screen.height;
			float gridRatio = (float)size.x / size.y;

			if (screenRatio > gridRatio) {
				return size.y / 2f;
			} else {
				float scale = gridRatio / screenRatio;
				return size.y / 2f * scale;
			}
		}
	}
}
