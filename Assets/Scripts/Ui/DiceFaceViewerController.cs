using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiceFaceViewerController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private const float ROTATIONAL_SPEED_MULTIPLIER = 0.1f;

    [SerializeField] private GameObject _dieToSpawn;
    [SerializeField] private Transform _dieViewerSpawnPoint;
    [SerializeField] private Transform _dieViewerParent;
    [SerializeField] private Rigidbody _rotationalDieRigidbody;
    [SerializeField] private DiceRollingBehaviour _connectedDie;
    [SerializeField] private Image[] _connectedDieFaceButtonImages;
    private Vector2 _previousMousePosition;
    private Vector3 _angularVelocity;

    private void Start()
    {
        CreateNewRandomDie();
    }

    public void CreateNewRandomDie()
    {
        GameObject die = Instantiate(_dieToSpawn, _dieViewerSpawnPoint.position, _dieViewerSpawnPoint.rotation, _dieViewerParent);
        StartCoroutine(GiveDieFrameToInit(die));
    }

    IEnumerator GiveDieFrameToInit(GameObject die)
    {
        yield return new WaitForEndOfFrame();
        LoadDiceIntoViewer(die.GetComponent<DiceRollingBehaviour>());
    }

    public void LoadDiceIntoViewer(DiceRollingBehaviour connectedDie)
    {
        _connectedDie = connectedDie;
        for(int faceIndex = 0; faceIndex < 6; faceIndex++) 
        {
            _connectedDieFaceButtonImages[faceIndex].sprite = _connectedDie.DiceFaces[faceIndex].MyDiceFaceData.DiceFaceUiSprite;
            _connectedDieFaceButtonImages[faceIndex].color = _connectedDie.DiceFaces[faceIndex].MyDiceFaceData.DiceFaceUiColor;
        }
        _rotationalDieRigidbody = _connectedDie.GetComponent<Rigidbody>();
        _rotationalDieRigidbody.isKinematic = false;
        _rotationalDieRigidbody.useGravity = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _previousMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 rotationalMovement = _previousMousePosition - eventData.position;
        _angularVelocity.x = rotationalMovement.y * -1;
        _angularVelocity.y = rotationalMovement.x;
        _angularVelocity.z = 0;
        _rotationalDieRigidbody.angularVelocity = _angularVelocity * ROTATIONAL_SPEED_MULTIPLIER;
        _previousMousePosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("DRAG ENDED");
    }
}
