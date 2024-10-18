using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public TextMeshProUGUI TextMesh;

    float animLength = 0.5f;
    float targetY;

    // make a simple animation here and destroy it
    IEnumerator Start()
    {
        targetY = Random.Range(25f, 50f);
        Destroy(gameObject, 1.5f);
        
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / animLength;
            float sqT = 1 - Mathf.Pow(1 - t, 5);

            TextMesh.transform.localPosition = new Vector3(0, Mathf.Lerp(0, targetY, sqT), 0);
            yield return new WaitForEndOfFrame();
        }

        TextMesh.transform.localPosition = new Vector3(0, targetY, 0);
    }
}
