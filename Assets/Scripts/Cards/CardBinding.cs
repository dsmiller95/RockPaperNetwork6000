﻿using System;
using Cards;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class CardBinding : MonoBehaviour, IBoundCard
{
    public TMPro.TMP_Text displayText;
    public GameObject hiddenFace;
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
    
    public void SetDisplay([CanBeNull] Transform slot, bool hidden)
    {
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