using UnityEngine;

namespace MSD.BasicSameGame.View
{
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
            if (size.y > size.x) {
                return size.y;
            } else {
                return ((float)Screen.height / (float)Screen.width) * 5f;
            }
        }
    }
}
