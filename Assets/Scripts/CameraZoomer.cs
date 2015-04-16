using UnityEngine;
using System.Collections;

public class CameraZoomer : MonoBehaviour
{
    private const float MinZoom = 1;
    private const float MaxZoom = 90;
    private Camera _camera;
	// Use this for initialization
	void Start ()
	{

	    _camera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {

        float zoom = Input.GetAxis("Mouse ScrollWheel");

        float currentFOV = _camera.fieldOfView;
        if (zoom != 0)
        {
            if (zoom > 0)
            {
                currentFOV++;
            }
            if (zoom < 0)
            {
                currentFOV--;
            }
            currentFOV = Mathf.Clamp(currentFOV, MinZoom, MaxZoom);
            _camera.fieldOfView = currentFOV;
        }
	}
}
