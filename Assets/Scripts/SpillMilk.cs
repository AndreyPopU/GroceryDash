using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpillMilk : MonoBehaviour
{
    public ParticleSystem spillEffect;
    public Transform spill;

    private void OnCollisionEnter(Collision collision)
    {
        // Strip the gameObject from it's components
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<BoxCollider>());
        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());

        // Effects
        spillEffect.Play();
        StartCoroutine(MilkSpill());
    }

    IEnumerator MilkSpill() // Scale up milk for a smooth effect
    {
        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();

        if (spill.localScale.x < 1)
        {
            while (spill.localScale.x < 1)
            {
                spill.localScale += Vector3.one * .1f;
                yield return waitForFixedUpdate;
            }
        }

        spill.GetComponent<MeshCollider>().enabled = true;
        Destroy(this, 1);
    }
}
