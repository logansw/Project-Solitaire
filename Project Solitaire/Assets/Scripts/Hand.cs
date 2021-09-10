using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite refreshSprite;
    [SerializeField] private Sprite drawSprite;

    private void Start()
    {
        sr.sprite = drawSprite;
        sr.color = ColorMan.Instance.neutralOff;
    }

    private void OnMouseDown()
    {
        BoardManager.Instance.LogicalHand();
        if (BoardManager.Instance.handPile.Count == 0)
        {
            sr.sprite = refreshSprite;
        } else
        {
            sr.sprite = drawSprite;
        }
    }

    private void OnMouseOver()
    {
        sr.color = ColorMan.Instance.neutralOn;
    }

    private void OnMouseExit()
    {
        sr.color = ColorMan.Instance.neutralOff;
    }
}
