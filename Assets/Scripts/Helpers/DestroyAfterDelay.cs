using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] private float _timeUntilDestruction;

    private void Awake()
    {
        Destroy(gameObject, _timeUntilDestruction);
    }
}
