using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ceiling : MonoBehaviour
{
    public List<Transform> inRange;
    public bool invisible;

    private float alpha;
    private Material mat;

    private IEnumerator Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        yield return null;
        inRange.Clear();
    }

        void Update()
    {
        // Animate
        if (inRange.Count > 0 && !invisible) invisible = true;

        if (inRange.Count <= 0 && invisible) invisible = false;

        if (invisible && alpha > 0) alpha -= 2 * Time.deltaTime;
        else if (!invisible && alpha < 1) alpha += 2 * Time.deltaTime;
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!inRange.Contains(other.transform) && other.GetComponent<Player>()) inRange.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (inRange.Contains(other.transform)) inRange.Remove(other.transform);
    }
}
