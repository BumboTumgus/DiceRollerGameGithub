using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiceFaceViewerController : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    private const float ROTATIONAL_SPEED_MULTIPLIER = 0.1f;
    private const float ROTATIONAL_SNAP_TO_TARGET_ANGLE_THRESHOLD = 1f;
    private const float SNAP_TO_FACE_ROTATION_SPEED = 0.04f;
    public DiceRollingBehaviour ConnectedDie { get => _connectedDie; }

    [SerializeField] private Transform _dieViewerSpawnPoint;
    [SerializeField] private Transform _dieViewerParent;
    [SerializeField] private Image[] _connectedDieFaceButtonImages;
    private Rigidbody _rotationalDieRigidbody;
    private DiceRollingBehaviour _connectedDie;
    private Vector2 _previousMousePosition;
    private Vector3 _angularVelocity;
    private bool _grabAndMoveLock = false;


    public void LoadDiceIntoViewer(DiceRollingBehaviour connectedDie)
    {
        UnloadDiceFromViewer();

        _connectedDie = connectedDie;
        _connectedDie.transform.parent = _dieViewerParent;
        _connectedDie.transform.position = _dieViewerSpawnPoint.position;

        for (int faceIndex = 0; faceIndex < 6; faceIndex++) 
        {
            if (_connectedDie.DiceFaces[faceIndex].MyTempDiceFaceData != null)
            {
                _connectedDieFaceButtonImages[faceIndex].sprite = _connectedDie.DiceFaces[faceIndex].MyTempDiceFaceData.DiceFaceUiSprite;
                _connectedDieFaceButtonImages[faceIndex].color = _connectedDie.DiceFaces[faceIndex].MyTempDiceFaceData.DiceFaceUiColor;
            }
            else
            {
                _connectedDieFaceButtonImages[faceIndex].sprite = _connectedDie.DiceFaces[faceIndex].MyDiceFaceData.DiceFaceUiSprite;
                _connectedDieFaceButtonImages[faceIndex].color = _connectedDie.DiceFaces[faceIndex].MyDiceFaceData.DiceFaceUiColor;
            }
        }
        _rotationalDieRigidbody = _connectedDie.GetComponent<Rigidbody>();
        _rotationalDieRigidbody.isKinematic = false;
        _rotationalDieRigidbody.useGravity = false;
    }

    public void UnloadDiceFromViewer()
    {
        if (_connectedDie == null)
            return;

        Debug.Log("DIE UNLOADED");
        _rotationalDieRigidbody.isKinematic = true;
        _rotationalDieRigidbody.useGravity = true;
        _connectedDie.transform.parent = null;
        _connectedDie.transform.position = Vector3.one * 999;

        if (!DiceRollerSingleton.Instance.DieArsenalContainsDie(_connectedDie))
            Destroy(_connectedDie.gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _previousMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_grabAndMoveLock)
            return;

        Vector2 rotationalMovement = _previousMousePosition - eventData.position;
        _angularVelocity.x = rotationalMovement.y * -1;
        _angularVelocity.y = rotationalMovement.x;
        _angularVelocity.z = 0;
        _rotationalDieRigidbody.angularVelocity = _angularVelocity * ROTATIONAL_SPEED_MULTIPLIER;
        _previousMousePosition = eventData.position;
    }

    public void RotateDieToTargetRotation(int faceIndexClickedOn)
    {
        StopAllCoroutines();

        _rotationalDieRigidbody.angularVelocity = Vector3.zero;
        switch (faceIndexClickedOn)
        {
            case 0:
                StartCoroutine(RotateDieToTargetRotation(Quaternion.Euler(0, 180, 90)));
                break;
            case 1:
                StartCoroutine(RotateDieToTargetRotation(Quaternion.Euler(0, 0, 0)));
                break;
            case 2:
                StartCoroutine(RotateDieToTargetRotation(Quaternion.Euler(-90, 0, 0)));
                break;
            case 3:
                StartCoroutine(RotateDieToTargetRotation(Quaternion.Euler(90, 90, 90)));
                break;
            case 4:
                StartCoroutine(RotateDieToTargetRotation(Quaternion.Euler(0, 90, 0)));
                break;
            case 5:
                StartCoroutine(RotateDieToTargetRotation(Quaternion.Euler(0, -90, 0)));
                break;
            default:
            break;
        }
    }

    public IEnumerator RotateDieToTargetRotation(Quaternion target)
    {
        _grabAndMoveLock = true;

        while (Quaternion.Angle(_rotationalDieRigidbody.rotation, target) > ROTATIONAL_SNAP_TO_TARGET_ANGLE_THRESHOLD)
        {
            _rotationalDieRigidbody.rotation = Quaternion.Slerp(_rotationalDieRigidbody.rotation, target, SNAP_TO_FACE_ROTATION_SPEED);
            yield return null;
        }

        _rotationalDieRigidbody.rotation = target;

        _grabAndMoveLock = false;
    }
}
