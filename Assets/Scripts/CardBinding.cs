using System;
using Cards;
using UnityEngine;
using UnityEngine.Serialization;

public class CardBinding : MonoBehaviour, IBoundCard
{
    public TMPro.TMP_Text displayText;
    public CardId myId;
    public event Action OnClick;
    [SerializeField] private Transform moveToTarget;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotateSpeed = 90f;
    

    public void Initialize(CardId id, PlayerCardType ofType)
    {
        myId = id;
        displayText.text = ofType.ToString();
    }
    
    public void SetMoveTo(Transform target)
    {
        moveToTarget = target;
    }
    
    public void OnClicked()
    {
        OnClick?.Invoke();
    }
    
    private void Update()
    {
        if (moveToTarget == null) return;
        transform.position = Vector3.MoveTowards(transform.position, moveToTarget.position, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, moveToTarget.rotation, rotateSpeed * Time.deltaTime);
    }
}