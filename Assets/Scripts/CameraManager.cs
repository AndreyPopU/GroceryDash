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
    public float yLimiter;
    public float minZoom, maxZoom;
    public Vector3 offset;

    [HideInInspector] public Vector3 centerPoint;
    private float greatestDistance;
    private Vector3 velocity;
    private Bounds bounds;
    private float zoom;
    private Camera cam;
    private float maxY = 20;
    private float currentY;
    private float currentZ;

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
        greatestDistance = bounds.size.x + bounds.size.z;

        // Limit camera zoom and movement
        if (bounds.size.z > 30) bounds.size = new Vector3(bounds.size.x, bounds.size.y, 30);
        currentY = offset.y + bounds.size.z / 6;
        currentZ = offset.z - (bounds.size.z / yLimiter);

        // Move
        transform.position = Vector3.SmoothDamp(transform.position, centerPoint + new Vector3(offset.x, currentY, currentZ), ref velocity, smoothness);

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
