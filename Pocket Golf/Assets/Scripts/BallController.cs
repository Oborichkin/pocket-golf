using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{

    public GameObject cameraTarget;
    public readonly float hitStrenghtMultiplier = 2.5f;
    public readonly float hitMaxStrenght = 10.0f;
    enum ControlState
    {
        idle,
        turn,
        hit
    };

    Rigidbody rb;
    Plane castPlane;
    Vector3 start, end, direction, power;
    ControlState controlState = ControlState.idle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        castPlane = new Plane(Vector3.up, transform.position);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (controlState)
            {
                case ControlState.hit:
                    HitStateHandler(touch);
                    break;
                case ControlState.turn:
                    TurnStateHandler(touch);
                    break;
                case ControlState.idle:
                    if (touch.position.y / Screen.height < 0.15)
                    {
                        controlState = ControlState.turn;
                        TurnStateHandler(touch);
                    }
                    else
                    {
                        controlState = ControlState.hit;
                        HitStateHandler(touch);
                    }
                    break;
                default:
                    break;
            }
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
        StartCoroutine(RotateMe(new Vector3(0, 90 * dir, 0), 0.3f));
        ClearInput();
    }

    IEnumerator RotateMe(Vector3 byAngles, float inTime)
    {
        var fromAngle = cameraTarget.transform.rotation;
        var toAngle = Quaternion.Euler(cameraTarget.transform.eulerAngles + byAngles);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            cameraTarget.transform.rotation = Quaternion.Lerp(fromAngle, toAngle, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t)));
            yield return null;
        }
        cameraTarget.transform.rotation = toAngle;
    }

    private void TurnStateHandler(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Ended:
                if (touch.position.x / Screen.width > 0.5)
                    TurnCamera(1);
                else
                    TurnCamera(-1);
                break;
            case TouchPhase.Canceled:
                ClearInput();
                break;
            default:
                break;
        }
    }

    private void HitStateHandler(Touch touch)
    {
        Ray ray;
        float enter;
        switch (touch.phase)
        {
            case TouchPhase.Began:
                castPlane = new Plane(Vector3.up, transform.position);
                ray = Camera.main.ScreenPointToRay(touch.position);
                enter = 0.0f;
                if (castPlane.Raycast(ray, out enter))
                    start = ray.GetPoint(enter);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                ray = Camera.main.ScreenPointToRay(touch.position);
                enter = 0.0f;
                if (castPlane.Raycast(ray, out enter))
                    end = ray.GetPoint(enter);
                direction = end - start;
                direction.Normalize();
                power = direction * (end - start).magnitude * hitStrenghtMultiplier;
                power = Vector3.ClampMagnitude(power, hitMaxStrenght);
                break;
            case TouchPhase.Ended:
                Hit();
                break;
            case TouchPhase.Canceled:
                ClearInput();
                break;
        }
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
}
