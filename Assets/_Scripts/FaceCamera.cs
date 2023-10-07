using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    // [SerializeField] private bool _rotateX;
    // [SerializeField] private bool _rotateY;
    // [SerializeField] private bool _rotateZ;

    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(Vector3.right * _cam.transform.rotation.eulerAngles.x);
    }
}
