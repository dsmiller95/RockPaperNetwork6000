using System;
using Cards;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class CardBinding : MonoBehaviour, IBoundCard
{
    public TMPro.TMP_Text displayText;
    public GameObject hiddenFace;
    public event Action OnClick;
    [SerializeField] private Transform moveToTarget;
    [SerializeField] private Transform originalParent;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotateSpeed = 90f;
    

    public void Initialize(CardId id, PlayerCardType ofType)
    {
        displayText.text = ofType.ToString();
        originalParent = transform.parent;
    }
    
    public void SetDisplay([CanBeNull] Transform slot, bool hidden)
    {
        if (slot != null)
        {
            this.transform.SetParent(slot, worldPositionStays: true);
        }
        else
        {
            this.transform.SetParent(originalParent, worldPositionStays: true);
        }
        moveToTarget = slot;
        hiddenFace.SetActive(hidden);
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