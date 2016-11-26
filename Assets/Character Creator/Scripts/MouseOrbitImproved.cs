using UnityEngine;

public class MouseOrbitImproved : MonoBehaviour {
  public Transform Target;
  public float Distance = 5.0f;
  public float XSpeed = 120.0f;
  public float YSpeed = 120.0f;

  public float YMinLimit = -20f;
  public float YMaxLimit = 80f;

  public float DistanceMin = .5f;
  public float DistanceMax = 15f;

  private Rigidbody m_Rigidbody;

  private float m_X;
  private float m_Y;

  public MouseOrbitImproved() {
    m_Y = 0.0f;
  }

  private void Start() {
    Vector3 angles = transform.eulerAngles;
    m_X = angles.y;
    m_Y = angles.x;

    m_Rigidbody = GetComponent<Rigidbody>();

    if (m_Rigidbody != null) {
      m_Rigidbody.freezeRotation = true;
    }
  }

  private void LateUpdate() {
    if (!Target || !Input.GetMouseButton(0)) {
      return;
    }

    m_X += Input.GetAxis("Mouse X")*XSpeed*Distance*0.02f;
    m_Y -= Input.GetAxis("Mouse Y")*YSpeed*0.02f;

    m_Y = ClampAngle(m_Y, YMinLimit, YMaxLimit);

    Quaternion rotation = Quaternion.Euler(m_Y, m_X, 0);

    Distance = Mathf.Clamp(Distance - Input.GetAxis("Mouse ScrollWheel")*5, DistanceMin, DistanceMax);

    RaycastHit hit;
    if (Physics.Linecast(Target.position, transform.position, out hit)) {
      Distance -= hit.distance;
    }

    Vector3 negDistance = new Vector3(0.0f, 0.0f, -Distance);
    Vector3 position = rotation*negDistance + Target.position;

    transform.rotation = rotation;
    transform.position = position;
  }

  private static float ClampAngle(float angle, float min, float max) {
    if (angle < -360F) {
      angle += 360F;
    }
    if (angle > 360F) {
      angle -= 360F;
    }
    return Mathf.Clamp(angle, min, max);
  }
}