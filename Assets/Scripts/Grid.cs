using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class Grid : MonoBehaviour
{
    public Orb orb;

    private GameController gameController;
    private RectTransform rect;
    public Vector2 posOfGridAbove;

    public AnimationCurve dropAnimationCurve;

    void Awake()
    {
        this.gameController = GameController.Instance;

        AssignOrb(GetComponentInChildren<Orb>());

        this.rect = this.GetComponent<RectTransform>();

        Vector2 scaledSize = Vector2.Scale(rect.rect.size, rect.lossyScale);
        this.posOfGridAbove = new Vector2(0, scaledSize.y - 45);
    }

    // Assign and orb to this grid, includes animation: swap (in play mode), drop (existing orbs) (new orbs)
    public void AssignOrb(Orb orb, string flag = "", int dropGridCount = 1, bool newOrb = false)
    {
        if (orb == null)
        {
            this.orb = null;
        }
        else
        {
            orb.SetParent(this);
            this.orb = orb;
            orb.parentGrid = this;
            RectTransform orbRect = orb.GetComponent<RectTransform>();

            orbRect.sizeDelta = new Vector2(150, 150);
            Vector2 targetPos = new Vector2(0, -45);
            
            // just go there~
            if (flag == "just go there")
            {
                orbRect.anchoredPosition = targetPos;
            } // swap animation
            else if (gameController.StateMachine.IsInState<PlayModeState>())
            {
                orbRect.DOAnchorPos(targetPos, 0.1f).SetEase(Ease.OutQuad);
            } // gravity drop animation
            else
            {
                float dropTime = gameController.BoardManager.dropTime;
                if (newOrb)
                {
                    orb.transform.localScale = Vector3.one;
                    orb.SetToTransparent();
                    Vector2 scaledSize = Vector2.Scale(rect.rect.size, rect.lossyScale);
                    orbRect.anchoredPosition = new Vector2(0, (scaledSize.y * dropGridCount) - 45);
                    orb.image.DOFade(1f, dropTime);
                }

                // Render above all static board elements while falling so dropping orbs
                // never get buried behind orbs that are children of lower (later-sibling) grid cells.
                var sortCanvas = orb.gameObject.AddComponent<Canvas>();
                sortCanvas.overrideSorting = true;
                sortCanvas.sortingOrder = 10;

                Sequence seq = DOTween.Sequence();
                if (dropGridCount > 1)
                {
                    float dropTimeLinear = dropTime * (dropGridCount - 1);
                    seq.Append(orbRect.DOAnchorPos(posOfGridAbove, dropTimeLinear).SetEase(Ease.Linear));
                }
                seq.Append(orbRect.DOAnchorPos(targetPos, dropTime).SetEase(dropAnimationCurve));
                seq.OnComplete(() => Destroy(sortCanvas));
                seq.Play();
            }
            
        }      
    }

}
