//using UnityEngine;

//public class ClickLocation : MonoBehaviour
//{
//    public Camera Camera;

//    [Header("Click Locator")]
//    public bool ShowClickLocation = false;

//    //private CameraFollow _cam;
//    private RaycastHit _hit;
//    private ParticleSystem _particle;

//    void Start()
//    {
//        //_cam = Camera.GetComponent<CameraFollow>();
//        _particle = GetComponent<ParticleSystem>();

//        _particle.Stop();
//        _particle.Clear();
//    }

//    void LateUpdate()
//    {
//        UpdateClickLocation();
//    }

//    public void UpdateClickLocation()
//    {
//        if (ShowClickLocation)
//            Debug.DrawLine(transform.localPosition, _hit.point, Color.red);

//        if (!Input.GetMouseButtonDown(1)) return;
//        if (!Physics.Raycast(_cam.MouseRay, out _hit)) return;
//        _particle.Play();
//        var adjustedHit = new Vector3(_hit.point.x, 0.01f, _hit.point.z);
//        transform.position = adjustedHit;
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject.tag != "Player") return;
//        _particle.Stop();
//        _particle.Clear();
//    }
//}
