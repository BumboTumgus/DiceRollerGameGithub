using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class DiceFaceBehaviour : MonoBehaviour
{
    public DiceFaceData MyDiceFaceData { get { return _myDiceFaceData; }}
    
    [SerializeField] private string _startingDiceFaceState;

    private Animation _animation;
    private GameObject _particleSystemToSpawn;
    private DiceFaceData _myDiceFaceData;
    private Renderer _renderer;

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

    public void PopupDiceFace(float delayInSeconds)
    {
        Invoke(nameof(PlayDicePopUpAnimation), delayInSeconds);
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
