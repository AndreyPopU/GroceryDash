using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpillMilk : MonoBehaviour
{
    public ParticleSystem spillEffect;
    public Transform spill;

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<BoxCollider>());
        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());
        spillEffect.Play();
        StartCoroutine(MilkSpill());
    }

    IEnumerator MilkSpill()
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
