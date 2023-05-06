using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{

    public GameObject cameraTarget;
    public readonly float hitStrenghtMultiplier = .5f;
    public readonly float hitMaxStrenght = 1.0f;
    enum ControlState
    {
        idle,
        turn,
        hit
    };

    Rigidbody rb;
    Plane castPlane;
    Vector3 start, end, direction, power;
    bool turning;
    ControlState controlState = ControlState.idle;
    bool isDesktop;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        castPlane = new Plane(Vector3.up, transform.position);

        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.OSXPlayer ||
            Application.platform == RuntimePlatform.LinuxPlayer)
        {
            isDesktop = false;
        }
        else
        {
            isDesktop = true;
        }
    }

    void Update()
    {
        if (isDesktop) {
            HitHandler();
        } else {
           TouchHitHandler();
        }
    }

    private void ClearInput()
    {
        start = transform.position;
        end = transform.position;
        direction = Vector3.zero;
        power = Vector3.zero;
        controlState = ControlState.idle;
    }

    private void TurnCamera(int dir)
    {
        if (!turning)
        {
            StartCoroutine(RotateMe(new Vector3(0, 90 * dir, 0), 0.3f));
            ClearInput();
        }
    }

    IEnumerator RotateMe(Vector3 byAngles, float inTime)
    {
        turning = true;
        var fromAngle = cameraTarget.transform.rotation;
        var toAngle = Quaternion.Euler(cameraTarget.transform.eulerAngles + byAngles);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            cameraTarget.transform.rotation = Quaternion.Lerp(fromAngle, toAngle, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t)));
            yield return null;
        }
        cameraTarget.transform.rotation = toAngle;
        turning = false;
    }

    private void HitHandler()
    {
        switch (controlState)
        {
            case ControlState.idle:
                if (Input.GetMouseButtonDown(0))
                {
                    setStartPosition(Input.mousePosition);
                } else if (Input.GetMouseButton(0)) {
                    updateEndPosition(Input.mousePosition);
                } else if (Input.GetMouseButtonUp(0)) {
                    Hit();
                }
                break;
            case ControlState.hit:
            default:
                break;
        }
    }

    private void TouchHitHandler()
    {
        if (Input.touchCount == 0)
            return;
        Touch touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                setStartPosition(touch.position);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                updateEndPosition(touch.position);
                break;
            case TouchPhase.Ended:
                Hit();
                break;
            case TouchPhase.Canceled:
                ClearInput();
                break;
        }
    }

    private void setStartPosition(Vector3 screenStartPos)
    {
        Ray ray;
        float enter;
        castPlane = new Plane(Vector3.up, transform.position);
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        enter = 0.0f;
        if (castPlane.Raycast(ray, out enter))
            start = ray.GetPoint(enter);
    }

    private void updateEndPosition(Vector3 screenEndPosition)
    {
        Ray ray;
        float enter;
        ray = Camera.main.ScreenPointToRay(screenEndPosition);
        enter = 0.0f;
        if (castPlane.Raycast(ray, out enter))
            end = ray.GetPoint(enter);
        direction = start - end;
        direction.Normalize();
        power = direction * (end - start).magnitude * hitStrenghtMultiplier;
        power = Vector3.ClampMagnitude(power, hitMaxStrenght);
        predict();
    }

    private void Hit()
    {
        rb.AddForce(power, ForceMode.Impulse);
        ClearInput();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(start, .1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + power, .1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(end, .1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + direction, .1f);
    }

    void predict(){
        PredictionManager.instance.predict(gameObject, transform.position, power);
    }
}
