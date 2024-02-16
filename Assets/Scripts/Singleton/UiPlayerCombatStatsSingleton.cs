using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiPlayerCombatStatsSingleton : MonoBehaviour
{
    private const float PARTICLE_TARGET_DEPTH = 2f;

    public static UiPlayerCombatStatsSingleton Instance;

    [SerializeField] private RectTransform _attackImageRectTransform;
    [SerializeField] private RectTransform _defenseImageRectTransform;
    [SerializeField] Transform testCube;

    private Vector3 _attackParticleEndPoint;
    private Vector3 _defenseParticleEndPoint;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        // InitStatParticleEndPoints();
    }

    // private void InitStatParticleEndPoints()
    // {
    //     Vector3 rayTarget = _attackImageRectTransform.position;
    //     rayTarget.x += _attackImageRectTransform.sizeDelta.x / 2;
    //     rayTarget.y += _attackImageRectTransform.sizeDelta.y / 2;
    //     rayTarget.z = PARTICLE_TARGET_DEPTH;
    //     Ray rayFromCamera = Camera.main.ScreenPointToRay(rayTarget);
    //     _attackParticleEndPoint = rayFromCamera.GetPoint(PARTICLE_TARGET_DEPTH);
    //     testCube.transform.position = _attackParticleEndPoint;
    // }
}
