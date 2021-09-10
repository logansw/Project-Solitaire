using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : Singleton<BoardManager>
{
    // Global Variables
    [SerializeField] public Card[] entireDeck;
    public Stack<Card> handPile = new Stack<Card>();
    public Stack<Card> wastePile = new Stack<Card>();
    public HashSet<GameObject> clearPile = new HashSet<GameObject>();

    [SerializeField] private Transform wasteLocation;
    [SerializeField] private Transform handLocation;
    [SerializeField] private Transform tableauStartLocation;
    [SerializeField] private float tableauGap;
    [SerializeField] private GameObject singleCard;
    [SerializeField] private int[] deckOrder;
    [SerializeField] private GameObject[] placeholdersArr;

    private GameObject[,] tableauColumns = new GameObject[7, 7];

    // Start is called before the first frame update
    void Start()
    {
        GenerateDeck();
        PlaceCards();
        StartCoroutine(WaitOneFrame());
        GameManager.Instance.gameStatus = GameStatus.live;
    }

    #region Deck Tracking Functions

    // Adds the specified card to the clearPile list.
    public void AddCardToClear(GameObject card)
    {
        clearPile.Add(card);
    }

    // Returns true if the game has been won by the player, false otherwise.
    public bool CheckGameWon()
    {
        return (clearPile.Count == 52);
    }

    #endregion

    #region Hand Functionality

    public void DrawFromHand()
    {
        int numDrawn = 0;
        while (numDrawn < 3 && handPile.Count > 0)
        {
            Debug.Log("DrawFromHand()");
            Card card = handPile.Pop();
            wastePile.Push(card);
            card.location = Location.waste;
            card.gameObject.transform.position = wasteLocation.position + new Vector3(0, 0, wastePile.Count * -0.1f);
            card.RevealCard();
            numDrawn++;
            FindObjectOfType<AudioManager>().Play("Pong");
            AudioManager.instance.pitchCounter = -1;
        }
    }

    public void PopOneFromWaste()
    {
        wastePile.Pop();
    }

    public void RefillHand()
    {
        while (wastePile.Count > 0)
        {
            Card card = wastePile.Pop();
            handPile.Push(card);
            card.location = Location.hand;
            card.gameObject.transform.position = handLocation.position + new Vector3(0, 0, handPile.Count * -0.1f);
            card.HideCard();
            FindObjectOfType<AudioManager>().Play("Shuffle");
            AudioManager.instance.pitchCounter = -1;
        }
    }

    public void LogicalHand()
    {
        if (handPile.Count == 0)
        {
            RefillHand();
        }
        else
        {
            DrawFromHand();
        }
    }

    #endregion

    #region Board Setup

    // Instantiates 52 card objects into the scene and tracks each in the entireDeck array.
    private void GenerateDeck()
    {
        for (int i = 0; i < 52; i++)
        {
            GameObject cardGameObject = Instantiate(singleCard, Vector3.zero, Quaternion.identity);
            Card card = cardGameObject.GetComponent<Card>();
            if (i < 13)
                card.suit = Suit.clubs;
            else if (i < 26)
                card.suit = Suit.diamonds;
            else if (i < 39)
                card.suit = Suit.hearts;
            else
                card.suit = Suit.spades;

            card.value = (i % 13) + 1;

            entireDeck[i] = card;
        }
    }

    // Randomizes the order of the cards, then positions them across the playing field.
    private void PlaceCards()
    {
        deckOrder = RandomOrder(52);
        float stackDistance = entireDeck[0].gameObject.GetComponent<Card>().cardSize.y / 4;
        for (int i = 0; i < deckOrder.Length; i++)
        {
            GameObject thisCard = entireDeck[deckOrder[i]].gameObject;
            Card thisCardCard = thisCard.GetComponent<Card>();
            if (i <= 0)
            {
                thisCard.transform.position = tableauStartLocation.position + new Vector3(0, 0, -1 * stackDistance);
                tableauColumns[0, 0] = thisCard;
                thisCardCard.location = Location.tableau;
                thisCard.transform.parent = placeholdersArr[0].transform;
            } else if (i <= 2)
            {
                thisCard.transform.position = tableauStartLocation.position + new Vector3(tableauGap, -(i-1) * stackDistance, -(i - 0) * stackDistance);
                tableauColumns[1, i - 1] = thisCard;
                thisCardCard.location = Location.tableau;
                if (i == 1)
                    thisCard.transform.parent = placeholdersArr[1].transform;
            } else if (i <= 5)
            {
                thisCard.transform.position = tableauStartLocation.position + new Vector3(tableauGap * 2, -(i-3) * stackDistance, -(i - 2) * stackDistance);
                tableauColumns[2, i - 3] = thisCard;
                thisCardCard.location = Location.tableau;
                if (i == 3)
                    thisCard.transform.parent = placeholdersArr[2].transform;
            } else if (i <= 9)
            {
                thisCard.transform.position = tableauStartLocation.position + new Vector3(tableauGap * 3, -(i-6) * stackDistance, -(i - 5) * stackDistance);
                tableauColumns[3, i - 6] = thisCard;
                thisCardCard.location = Location.tableau;
                if (i == 6)
                    thisCard.transform.parent = placeholdersArr[3].transform;
            } else if (i <= 14)
            {
                thisCard.transform.position = tableauStartLocation.position + new Vector3(tableauGap * 4, -(i-10) * stackDistance, -(i - 9) * stackDistance);
                tableauColumns[4, i - 10] = thisCard;
                thisCardCard.location = Location.tableau;
                if (i == 10)
                    thisCard.transform.parent = placeholdersArr[4].transform;
            } else if (i <= 20)
            {
                thisCard.transform.position = tableauStartLocation.position + new Vector3(tableauGap * 5, -(i-15) * stackDistance, -(i - 14) * stackDistance);
                tableauColumns[5, i - 15] = thisCard;
                thisCardCard.location = Location.tableau;
                if (i == 15)
                    thisCard.transform.parent = placeholdersArr[5].transform;
            } else if (i <= 27)
            {
                thisCard.transform.position = tableauStartLocation.position + new Vector3(tableauGap * 6, -(i-21) * stackDistance, -(i - 20) * stackDistance);
                tableauColumns[6, i - 21] = thisCard;
                thisCardCard.location = Location.tableau;
                if (i == 21)
                    thisCard.transform.parent = placeholdersArr[6].transform;
            } else
            {
                thisCard.transform.position = handLocation.position + new Vector3(0, 0, -((i - 26) * 0.1f));
                thisCardCard.location = Location.hand;
                handPile.Push(thisCardCard);
            }
        }
    }

    // Organizes the cards in the tableau, parenting higher cards to lower cards.
    // Also reveals the lowest cards in every column.
    private void ParentAndRevealCards()
    {
        for (int i = 0; i < 7; i++)
        {
            for (int j = i; j > 0; j--)
            {
                if (i == j)
                {
                    tableauColumns[i, j].GetComponent<Card>().RevealCard();
                }
                tableauColumns[i, j].transform.parent = tableauColumns[i, j - 1].transform;
            }
        }
        tableauColumns[0, 0].GetComponent<Card>().RevealCard();
    }

    private IEnumerator WaitOneFrame()
    {
        yield return 0;
        ParentAndRevealCards();
    }

    #endregion

    #region Helper Functions

    // Randomly rearranges the cards and returns an array of cards in a new order.
    private int[] RandomOrder(int count)
    {
        int[] newOrder = new int[count];
        HashSet<int> pool = new HashSet<int>();
        for (int i = 0; i < count; i++)
        {
            pool.Add(i);
            newOrder[i] = -1;
        }

        for (int i = 0; i < count; i++)
        {
            while (pool.Count == count - i)
            {
                int insertHere = Random.Range(0, count);
                if (newOrder[insertHere] == -1)
                {
                    newOrder[insertHere] = i;
                    pool.Remove(i);
                }
            }
        }

        return newOrder;
    }

    #endregion
}