using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using UnityEngine;
using UnityEngine.XR;

public class Card : MonoBehaviour
{
    #region GlobalVariablesAndSoForth
    // Global variables
    public Suit suit;
    public int value;
    public bool isFaceUp;
    public Location location;
    public UnityEngine.Vector2 cardSize;
    public bool isFresh;

    private UnityEngine.Vector3 offset;
    private UnityEngine.Vector3 pickUpLocation;
    private float cardWidth;
    private float cardHeight;
    private SpriteRenderer suitSpriteRenderer;
    private SpriteRenderer valueSpriteRenderer;
    private Sprite faceSprite;
    private Sprite valueSprite;
    private Color cardHoverColor;
    private Color cardExitColor;
    private bool isClickable = true;

    [SerializeField] private Sprite[] suitsArray;
    [SerializeField] private Sprite[] valuesArray;

    [SerializeField] private Sprite backSprite;
    #endregion

    private void Awake()
    {
        Physics2D.queriesStartInColliders = false; // Disables objects from raycasting themselves
        cardSize = GetComponent<Collider2D>().bounds.size; // Get dimensions of card
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = GenerateSpriteName();
        cardWidth = cardSize.x;
        cardHeight = cardSize.y;
        suitSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        GameObject valueObject = gameObject.transform.GetChild(0).gameObject;
        valueSpriteRenderer = valueObject.GetComponentInChildren<SpriteRenderer>();

        LoadSprites();
        ShowSprite();
        HideSprite();
    }

    private void OnMouseEnter()
    {
        FindObjectOfType<AudioManager>().Play("Pong");
    }
    private void OnMouseOver()
    {
        if (isFaceUp)
        {
            suitSpriteRenderer.color = cardHoverColor;
            valueSpriteRenderer.color = cardHoverColor;
        } else
        {
            suitSpriteRenderer.color = ColorMan.Instance.neutralOn;
            valueSpriteRenderer.color = ColorMan.Instance.neutralOn;
        }
    }

    private void OnMouseExit()
    {
        if (isFaceUp)
        {
            suitSpriteRenderer.color = cardExitColor;
            valueSpriteRenderer.color = cardExitColor;
        } else
        {
            suitSpriteRenderer.color = ColorMan.Instance.neutralOff;
            valueSpriteRenderer.color = ColorMan.Instance.neutralOff;
        }
    }

    #region Reveal/Hide
    public void RevealCard()
    {
        isFaceUp = true;
        ShowSprite();
    }

    public void HideCard()
    {
        isFaceUp = false;
        HideSprite();
    }
    #endregion

    #region Dragging Functionality
    // Enables cards to be revealed, dragged around, and released
    private void OnMouseDown()
    {
        if (isClickable)
        {
            pickUpLocation = transform.position;
            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }
    private void OnMouseDrag()
    {
        DragCard();
    }
    private void OnMouseUp()
    {
        if (isFaceUp)
        {
            ReleaseCard();
        }
        if (transform.childCount == 1 && location != Location.hand && !isFaceUp)
        {
            AudioManager.instance.pitchCounter++;
            FindObjectOfType<AudioManager>().Play("TetrisPong");
            RevealCard();
        }
        // CHEAT CODES
        if (!isFaceUp)
        {
            RevealCard();
        }
    }
    private void DragCard()
    { /* The card you click on is brought to the foreground and follows mouse coordinates.
       * The card's parent becomes enabled */
        if (isFaceUp)
        {
            UnityEngine.Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = -150;
            transform.position = mousePosition + offset;
            if (transform.parent != null)
            {
                transform.parent.GetComponent<Collider2D>().enabled = true;
            }
        }
    }
    private void ReleaseCard()
    { /* The card shoots a raycast and calls SnapToCard() to decide where to place the card */
        RaycastHit2D hit = GetRaycastHit();
        SnapToCard(hit);
    }

    private IEnumerator MoveOneFrame(UnityEngine.Vector3 endPos, UnityEngine.Vector3 startPos, float time)
    {
        float stopwatch = 0f;
        UnityEngine.Vector3 changeVector = endPos - startPos;
        while (stopwatch < time)
        {
            transform.position = startPos + changeVector * stopwatch / time;
            stopwatch += Time.deltaTime;
            yield return 0;
        }
        transform.position = endPos;
        isClickable = true;
    }
    #endregion

    #region Raycasting Behavior
    // Shoots raycast and returns the thing we hit
    public RaycastHit2D GetRaycastHit()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + UnityEngine.Vector3.down * cardHeight / 2.01f, UnityEngine.Vector3.up, cardHeight);
        return hit;
    }

    // Decides where to place a card after release
    private void SnapToCard(RaycastHit2D hit)
    {
        if (hit.collider == null) // We missed
        {
            Debug.Log("We missed completely");
            ReturnToPickup();
        }
        else if (hit.collider.gameObject.tag == "Placeholder") // We hit a childless placeholder
        {
            Debug.Log("We hit a placeholder");
            if (hit.collider.gameObject.transform.childCount == 0)
                HitPlaceholder(hit);
            else
                ReturnToPickup();
        }
        else if (hit.collider.name == "Waste" || hit.collider.name == "Hand") // We hit the waste or hand
        {
            Debug.Log("We hit the waste / hand");
            ReturnToPickup();
        }
        else // We hit a card
        {
            HitCard(hit);
        }
    }
    private void HitPlaceholder(RaycastHit2D hit)
    {
        Placeholder placeholderHit = hit.collider.gameObject.GetComponent<Placeholder>();
        if (placeholderHit.GetLocation() == Location.tableau && value == 13) // Tableau placeholder logic
        {
            PlaceOn(hit);
            location = Location.tableau;
        }
        else if (placeholderHit.GetLocation() == Location.foundation && value == 1) // Foundation placeholder logic
        {
            PlaceOn(hit);
            location = Location.foundation;
            BoardManager.Instance.AddCardToClear(this.gameObject);
            Debug.Log(BoardManager.Instance.clearPile.Count);
        }
        else
        {
            ReturnToPickup();
        }
    }
    private void HitCard(RaycastHit2D hit)
    {
        Card cardHit = hit.collider.gameObject.GetComponent<Card>();
        Debug.Log("We hit the " + cardHit.name + " in the " + cardHit.location);
        // Debug.Log("This card has " + hit.transform.childCount + " children");
        if (cardHit.location == Location.tableau) // Card in tableau
        {
            if ((((suit == Suit.clubs || suit == Suit.spades) && (cardHit.suit == Suit.hearts || cardHit.suit == Suit.diamonds))
             || ((suit == Suit.hearts || suit == Suit.diamonds) && (cardHit.suit == Suit.clubs || cardHit.suit == Suit.spades)))
             && value == cardHit.value - 1 && hit.transform.childCount == 1)
            {
                PlaceBelow(hit);
                location = Location.tableau;
            }
            else
            {
                ReturnToPickup();
            }
        }
        else if (cardHit.location == Location.foundation) // Card in foundation
        {
            if ((suit == cardHit.suit) && (value == cardHit.value + 1) && transform.childCount == 1)
            {
                PlaceOn(hit);
                location = Location.foundation;
            }
            else
            {
                ReturnToPickup();
            }
        }
        else // Card in hand or waste
        {
            ReturnToPickup();
        }
    }

    // Methods to place cards
    private void PlaceBelow(RaycastHit2D hit)
    {
        if (isFresh)
        {
            consumeFreshness();
        }
        FindObjectOfType<AudioManager>().Play("TetrisPong");
        transform.parent = hit.transform; // Makes parent child relation
        transform.position = hit.transform.position + UnityEngine.Vector3.down * cardHeight / 4 + UnityEngine.Vector3.back * 0.1f;
        CheckRemoveFromWaste();
    }
    private void PlaceOn(RaycastHit2D hit)
    {
        if (isFresh)
        {
            consumeFreshness();
        }
        FindObjectOfType<AudioManager>().Play("TetrisPong");
        transform.parent = hit.transform; // Makes parent child relation
        transform.position = hit.transform.position + UnityEngine.Vector3.back * 0.1f;
        hit.collider.enabled = false;
        CheckRemoveFromWaste();
    }
    private void ReturnToPickup()
    {
        isClickable = false;
        FindObjectOfType<AudioManager>().Play("MissedSound");
        StartCoroutine(MoveOneFrame(pickUpLocation, transform.position, 0.1f));
    }
    #endregion

    #region Helper Functions

    private string GenerateSpriteName()
    { // Returns the proper name of the card's corresponding image file (e.g. "AceOfSpades.png", "7OfClubs.png").
        CheckValidCard();
        string spriteName = "";

        if (value == 1)
            spriteName += "Ace";
        else if (value > 1 && value < 11)
            spriteName += value;
        else if (value == 11)
            spriteName += "Jack";
        else if (value == 12)
            spriteName += "Queen";
        else
            spriteName += "King";

        spriteName += "Of";

        if (suit == Suit.clubs)
            spriteName += "Clubs";
        else if (suit == Suit.diamonds)
            spriteName += "Diamonds";
        else if (suit == Suit.hearts)
            spriteName += "Hearts";
        else
            spriteName += "Spades";

        return spriteName;
    }
    private void CheckValidCard()
    { // Ensures that the current card's suit and value properties are valid. Sanity check.
        Debug.Assert(suit == Suit.clubs || suit == Suit.diamonds || suit == Suit.hearts || suit == Suit.spades);
        Debug.Assert(value >= 1 && value <= 13);
    }
    private void LoadSprites()
    { // Loads the appropriate suit, color, and value text for the card.
        if (suit == Suit.clubs)
        {
            cardExitColor = ColorMan.Instance.blackOff;
            cardHoverColor = ColorMan.Instance.blackOn;
            faceSprite = suitsArray[0];
        }
        else if (suit == Suit.diamonds)
        {
            cardExitColor = ColorMan.Instance.redOff;
            cardHoverColor = ColorMan.Instance.redOn;
            faceSprite = suitsArray[1];
        }
        else if (suit == Suit.hearts)
        {
            cardExitColor = ColorMan.Instance.redOff;
            cardHoverColor = ColorMan.Instance.redOn;
            faceSprite = suitsArray[2];
        }
        else
        {
            cardExitColor = ColorMan.Instance.blackOff;
            cardHoverColor = ColorMan.Instance.blackOn;
            faceSprite = suitsArray[3];
        }

        valueSprite = valuesArray[value - 1];
        valueSpriteRenderer.sprite = valueSprite;
    }
    private void ShowSprite()
    {
        suitSpriteRenderer.sprite = faceSprite;
        suitSpriteRenderer.color = cardExitColor;
        valueSpriteRenderer.enabled = true;
        valueSpriteRenderer.sprite = valueSprite;
        valueSpriteRenderer.color = cardExitColor;
    }
    private void HideSprite()
    {
        suitSpriteRenderer.sprite = backSprite;
        suitSpriteRenderer.color = ColorMan.Instance.neutralOff;
        valueSpriteRenderer.enabled = false;
    }

    private void CheckRemoveFromWaste()
    {
        if (location == Location.waste)
        {
            BoardManager.Instance.PopOneFromWaste();
        }
    }

    private void consumeFreshness()
    {
        AudioManager.instance.pitchCounter++;
        isFresh = false;
    }
    #endregion
}
