using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlickererBehaviour : MonoBehaviour
{
    [SerializeField] private Vector2 _lightIntensityRange;
    [SerializeField] private Vector2 _lightFlickerTimerRange;
    
    private Light _light;
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    private void Awake()
    {
        _light = GetComponent<Light>();
        StartCoroutine(FlickerLight());
    }

    private IEnumerator FlickerLight()
    {
        float currentTimer = 0f;
        float targetTimer;
        float originalIntensity;
        float targetIntensity;

        while(true)
        {  
            currentTimer = 0;
            targetTimer = Random.Range(_lightFlickerTimerRange.x,_lightFlickerTimerRange.y);
            originalIntensity = _light.intensity;
            targetIntensity = Random.Range(_lightIntensityRange.x, _lightIntensityRange.y); 

            while(currentTimer < targetTimer)
            {
                currentTimer += Time.deltaTime;
                _light.intensity = Mathf.Lerp(originalIntensity, targetIntensity, currentTimer / targetTimer);
                yield return waitForEndOfFrame;
            }
        }
    }

}
