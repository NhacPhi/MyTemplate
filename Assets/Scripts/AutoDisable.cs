using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDisable : MonoBehaviour
{
    public float delay = 1f;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(DisableRoutine());
    }

    IEnumerator DisableRoutine()
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
