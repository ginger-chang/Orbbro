using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Orb : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public enum Suits { none, red, blue, green, yellow, purple, pink};
    public Suits suit = Suits.none;

    public Grid parentGrid; // will change!
    private float gameAreaMinX;
    private float gameAreaMaxX;
    private float gameAreaMinY;
    private float gameAreaMaxY;

    private RectTransform rect;
    public RectTransform gameAreaRect;
    public RectTransform dragLayerRect;

    private AudioManager audioManager;
    private GameController gameController;

    public Image image;

    //------------------------STARTING---------------------------

    void Awake()
    {
        this.image = GetComponent<Image>();
        this.rect = GetComponent<RectTransform>();
        this.audioManager = GameObject.FindGameObjectWithTag("audio").GetComponent<AudioManager>();
        this.gameController = GameObject.FindGameObjectWithTag("game controller").GetComponent<GameController>();
        ChangeSuitRandom();
    }

    public void ChangeSuitRandom()
    {
        int rand = UnityEngine.Random.Range(1, 7);
        var suits = Enum.GetValues(typeof(Suits));
        Suits randSuit = (Suits)suits.GetValue(rand);
        ChangeSuit(randSuit);
    }

    // Change the color
    void ChangeSuit(Suits suit)
    {
        this.suit = suit;
        switch (suit)
        {
            case Suits.none:
                break;
            case Suits.red:
                this.name = "red orb";
                image.color = new Color(0.8980392156862745f, 0.29411764705882354f, 0.29411764705882354f, 1f);
                break;
            case Suits.blue:
                this.name = "blue orb";
                image.color = new Color(0.7058823529411765f, 0.8823529411764706f, 1f, 1f);
                break;
            case Suits.green:
                this.name = "green orb";
                image.color = new Color(0.7843137254901961f, 1f, 0.7450980392156863f, 1f);
                break;
            case Suits.yellow:
                this.name = "yellow orb";
                image.color = new Color(0.9607843137254902f, 0.8313725490196079f, 0.5686274509803921f, 1f);
                break;
            case Suits.purple:
                this.name = "purple orb";
                image.color = new Color(0.6705882352941176f, 0.5294117647058824f, 1f, 1f);
                break;
            case Suits.pink:
                this.name = "pink orb";
                image.color = new Color(1f, 0.6745098039215687f, 0.8941176470588236f, 1f);
                break;
            default:
                break;
        }
    }

    private Vector2 startDragPos;
    public Grid curGrid;

    // Setting up Orb's Rect transform, for draggable game area and drag layer, called by game controller
    public void SetRects(RectTransform gameArea, RectTransform dragLayer)
    {
        this.gameAreaRect = gameArea;
        Vector3[] corners = new Vector3[4];
        gameAreaRect.GetWorldCorners(corners);

        // Get game area (for dragging) and the size of orb (relative to ui scalar)
        Vector2 scaledSize = Vector2.Scale(rect.rect.size, rect.lossyScale);
        float offset = scaledSize.x;
        this.gameAreaMinX = corners[0].x + offset / 2;
        this.gameAreaMaxX = corners[2].x - offset / 2;
        this.gameAreaMinY = corners[0].y + offset / 10 * 2;
        this.gameAreaMaxY = corners[2].y - offset / 10 * 8;

        // Set drag layer rect
        this.dragLayerRect = dragLayer;
    }


    //------------------------DRAGGING-----------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gameController.state != GameController.GameState.PlayMode) return;
        validDrag = true;
        gameController.setOrbInSwap(this);
        // Drag layer 
        transform.SetParent(dragLayerRect);
        startDragPos = rect.position;

        curGrid = parentGrid;
    }

    public bool validDrag = false;

    [Obsolete]
    public void OnDrag(PointerEventData eventData)
    {
        if (gameController.state != GameController.GameState.PlayMode || !validDrag) return;
        // Set the dragging area to be within game area.
        float x = eventData.position.x;
        x = Math.Max(x, this.gameAreaMinX);
        x = Math.Min(x, this.gameAreaMaxX);
        float y = eventData.position.y;
        y = Math.Max(y, this.gameAreaMinY);
        y = Math.Min(y, this.gameAreaMaxY);
        rect.position = new Vector3(x, y, 0f);

        // Detect swap
        foreach(Grid grid in FindObjectsOfType<Grid>())
        {
            if (grid == curGrid)
            {
                continue;
            }
            RectTransform gridRect = grid.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(gridRect, rect.position, null))
            {
                audioManager.PlaySwapSFX();
                Orb otherOrb = grid.orb;
                curGrid.assignOrb(otherOrb);
                curGrid = grid;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (gameController.state != GameController.GameState.PlayMode || !validDrag) return;
        validDrag = false;
        gameController.setOrbInSwap(null);
        rect.position = startDragPos;
        // Remove from drag layer

        // Finish drag
        curGrid.assignOrb(this, "just go there");
        // later: add trigger coroutine for resolving
        //gameController.GetMatches();
        gameController.Resolve();
        // 1. disable moving in game area (show with hint like darker background)
        // 2. (slow down timer)
        // 3. loop of (disappear -> new orbs fall down)
        // 4. when no match detected: add score, do level change if needed
        // 5. allow moving in game area again
    }


    //-------------------------HELPERS------------------------------

    // HELPERS, Called by Grid I believe
    public void setParent(Grid grid)
    {
        transform.SetParent(grid.transform, true);
    }

    // When a new orb is created
    public void SetToTransparent()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
    }
}
