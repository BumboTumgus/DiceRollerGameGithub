using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class DiceFaceBehaviour : MonoBehaviour
{
    public DiceFaceData MyDiceFaceData { get => _myDiceFaceData; }
    public DiceFaceData MyTempDiceFaceData { get => _myTempDiceFaceData; }
    public bool TempFaceRemovedAfterCombat { get => _tempFaceRemovedAfterCombat; }

    [SerializeField] private string _startingDiceFaceState;

    private Animation _animation;
    private GameObject _particleSystemToSpawn;
    [SerializeField] private DiceFaceData _myDiceFaceData;
    [SerializeField] private DiceFaceData _myTempDiceFaceData;
    private Renderer _renderer;

    private bool _tempFaceRemovedAfterCombat = false;
    

    private void Start()
    {
        _animation = GetComponent<Animation>();
        _renderer = GetComponent<Renderer>();
        SwitchDiceFace(_startingDiceFaceState);
    }

    public void SwitchDiceFace(string diceFaceName)
    {
        _myDiceFaceData = DiceFaceDataSingleton.Instance.GetDiceFaceDataByName(diceFaceName);
        _particleSystemToSpawn = _myDiceFaceData.RolledDiceParticles;
        _renderer.material = _myDiceFaceData.DiceFaceMat;
    }

    public void TemporarySwitchDiceFace(DiceFaceData diceFaceData, bool tempFaceRevertsAfterCombat = false)
    {
        _myTempDiceFaceData = diceFaceData;
        _particleSystemToSpawn = _myTempDiceFaceData.RolledDiceParticles;
        _renderer.material = _myTempDiceFaceData.DiceFaceMat;
        _tempFaceRemovedAfterCombat = tempFaceRevertsAfterCombat;
    }

    public void RevertToOriginalDiceFace()
    {
        _myTempDiceFaceData = null;
        _particleSystemToSpawn = _myDiceFaceData.RolledDiceParticles;
        _renderer.material = _myDiceFaceData.DiceFaceMat;
    }

    public void SetTempAsNewOriginal()
    {
        _myDiceFaceData = _myTempDiceFaceData;
        _myTempDiceFaceData = null;
    }

    public void PopupDiceFace(float delayInSeconds)
    {
        Invoke(nameof(PlayDicePopUpAnimation), delayInSeconds);
    }

    public void SetStartingDieFace(string dieFaceState)
    {
        _startingDiceFaceState = dieFaceState;
    }

    private void PlayDicePopUpAnimation()
    {
        _animation.Play();
    }

    public void InstantiateParticleForRolledDiceFace()
    {
        Instantiate(_particleSystemToSpawn, transform.position, transform.rotation);
    }
}
