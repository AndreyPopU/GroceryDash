using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    public List<Transform> targets;
    public float smoothness = .5f;
    public float zoomLimiter = 50;
    public float minZoom, maxZoom;
    public Vector3 offset;

    private Vector3 centerPoint;
    private float greatestDistance;
    private Vector3 velocity;
    private Bounds bounds;
    private float zoom;
    private Camera cam;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        if (targets.Count <= 0) return;

        UpdateBounds();

        centerPoint = targets.Count == 1 ? targets[0].position : bounds.center;
        greatestDistance = bounds.size.x;

        // Move
        transform.position = Vector3.SmoothDamp(transform.position, centerPoint + offset, ref velocity, smoothness);

        // Zoom
        zoom = Mathf.Lerp(minZoom, maxZoom, greatestDistance / zoomLimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, zoom, Time.deltaTime);
    }

    public void UpdateBounds()
    {
        bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++) bounds.Encapsulate(targets[i].position);
    }
}
