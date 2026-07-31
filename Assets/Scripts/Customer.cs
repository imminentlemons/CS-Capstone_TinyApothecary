using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waypointDistance = 0.05f;


    [Header("Order Display")]
    [SerializeField] private GameObject orderBubble;
    [SerializeField] private Image requestedPotionIcon;
    [SerializeField] private Image patienceMoodImage;
    [SerializeField] private TMP_Text attentionText;

    [SerializeField] private Sprite greatMood;
    [SerializeField] private Sprite goodMood;
    [SerializeField] private Sprite neutralMood;
    [SerializeField] private Sprite sadMood;
    [SerializeField] private Sprite angryMood;
    [SerializeField] private Sprite skullMood;

    [SerializeField] private ShopFunds shopFunds;

    private Rigidbody2D customerBody;
    private Transform[] customerPath;
    private int pathIndex;

    private CustomerOutcome departureOutcome;
    private float departureSatisfaction;

    private Animator customerAnimator;
    private SpriteRenderer customerRenderer;
    private string currentWalkAnimation;

    public enum CustomerOutcome
    {
        Served,
        TimedOut
    }

    private enum CustomerState
    {
        Arriving,
        WaitingForAttention,
        WaitingForPotion,
        Leaving,
        Finished
    }

    private CustomerState currentState;

    public event Action<Customer> ArrivedAtCounter;
    
    public event Action<Customer, CustomerOutcome, float> Finished;

    public event Action<Customer, CustomerOutcome, float> Resolved;

    public CustomerOrder CurrentOrder { get; private set; }

    public bool HasOrder => CurrentOrder != null;

    private void Awake()
    {
        customerBody = GetComponent<Rigidbody2D>();
        customerAnimator = GetComponent<Animator>();
        customerRenderer = GetComponent<SpriteRenderer>();

        if (orderBubble != null)
        {
            orderBubble.SetActive(false);
        }
    }    

    private void Update()
    {
        if(CurrentOrder == null)
        {
            return;
        }

        CurrentOrder.remainingPatience -= Time.deltaTime;
        UpdateOrderDisplay();

        if(CurrentOrder.remainingPatience <= 0f)
        {
            OrderFailed();
        }
    }

    public void Initialize(ShopFunds funds, Transform[] path)
    {
        shopFunds = funds;
        customerPath = path;
        currentState = CustomerState.Arriving;

        if(customerPath == null || customerPath.Length == 0)
        {
            ArriveAtCounter();
            return;
        }

        Vector2 startingPosition = customerPath[0].position;

        if (customerBody != null)
        {
            customerBody.position = startingPosition;
        }
        else
        {
            transform.position = startingPosition;
        }

        //point 0 is the spawn point - begin walking to point 1
        pathIndex = 1;

        if(customerPath.Length == 1)
        {
            ArriveAtCounter();
        }
    }

    private void FixedUpdate()
    {
        bool isWalking =
            currentState == CustomerState.Arriving ||
            currentState == CustomerState.Leaving;

        if(!isWalking || customerBody == null || customerPath == null)
        {
            return;
        }

        if(pathIndex < 0 || pathIndex >= customerPath.Length)
        {
            CompleteCurrentPath();
            return;
        }

        Transform targetPoint = customerPath[pathIndex];

        if(targetPoint == null)
        {
            AdvanceToNextPoint();
            return;
        }

        Vector2 targetPositon = targetPoint.position;

        Vector2 travelDirection =
            targetPositon - customerBody.position;

        UpdateWalkingAnimation(travelDirection);

        Vector2 nextPosition = Vector2.MoveTowards(
            customerBody.position,
            targetPositon,
            moveSpeed * Time.fixedDeltaTime);

        customerBody.MovePosition(nextPosition);

        if(Vector2.Distance(nextPosition, targetPositon)
            <= waypointDistance)
        {
            AdvanceToNextPoint();
        }
    }

    private void AdvanceToNextPoint()
    {
        if(currentState == CustomerState.Arriving)
        {
            pathIndex++;

            if(pathIndex >= customerPath.Length)
            {
                ArriveAtCounter();
            }
        }
        else if(currentState == CustomerState.Leaving)
        {
            pathIndex--;

            if(pathIndex < 0)
            {
                CompleteDeparture();
            }
        }
    }

    private void CompleteCurrentPath()
    {
        if(currentState == CustomerState.Arriving)
        {
            ArriveAtCounter();
        }
        else if(currentState == CustomerState.Leaving)
        {
            CompleteDeparture();
        }
    }

    private void ArriveAtCounter()
    {
        currentState = CustomerState.WaitingForAttention;

        PlayCounterIdleAnimation();

        ArrivedAtCounter?.Invoke(this);
    }

    private void BeginLeaving(CustomerOutcome outcome, float satisfaction)
    {
        departureOutcome = outcome;
        departureSatisfaction = satisfaction;
        currentState = CustomerState.Leaving;
        Resolved?.Invoke(this, outcome, satisfaction);

        if(orderBubble != null)
        {
            orderBubble.SetActive(false);
        }

        if(customerPath == null || customerPath.Length <= 1)
        {
            CompleteDeparture();
            return;
        }

        //customer is already at final counter point
        //begin walking backward to previous point
        pathIndex = customerPath.Length - 2;
    }

    private void CompleteDeparture()
    {
        currentState = CustomerState.Finished;

        Finished?.Invoke(this, departureOutcome, departureSatisfaction);

        Destroy(gameObject);
    }

    public void SetOrder(CustomerOrder order)
    {
        CurrentOrder = order;

        if(CurrentOrder == null || CurrentOrder.RequestedPotion == null)
        {
            return;
        }

        currentState = CustomerState.WaitingForAttention;

        CurrentOrder.remainingPatience = CurrentOrder.patienceSeconds;

        if(orderBubble != null)
        {
            orderBubble.SetActive(true);
        }

        UpdateOrderDisplay();
    }

    public bool Interact(Player player)
    {
        if (CurrentOrder == null || player == null)
        {
            return false;
        }

        //first interaction reveals order
        if(currentState == CustomerState.WaitingForAttention)
        {
            RevealOrder();
            return true;
        } 
        
        if(currentState != CustomerState.WaitingForPotion)
        {
            return false;
        }

        //later interactions attempt to serve
        Inventory toolbar = player.inventoryManager.toolbar;

        int selectedIndex = toolbar.selectedSlotIndex;

        Inventory.Slot selectedSlot = toolbar.slots[selectedIndex];

        if(selectedSlot.IsEmpty)
        {
            NotificationPopup_UI.Show("Select the customer's potion.");
            return false;
        }

        Item selectedItem = GameManager.instance.itemManager.GetItemByName(selectedSlot.itemName);

        if(!CurrentOrder.isCorrectPotion(selectedItem))
        {
            NotificationPopup_UI.Show("That is not the correct potion.");
            return false;
        }

        toolbar.Remove(selectedIndex);
        player.toolbarUI.Refresh();

        OrderCompleted();
        return true;
    }

    private void RevealOrder()
    {
        currentState = CustomerState.WaitingForPotion;

        //reset patience/mood timer
        CurrentOrder.remainingPatience = CurrentOrder.patienceSeconds;

        UpdateOrderDisplay();
    }

    private void UpdateOrderDisplay()
    {
        if(CurrentOrder == null || CurrentOrder.RequestedPotion == null)
        {
            return;
        }

        bool isWaitingForAttention = currentState == CustomerState.WaitingForAttention;

        bool isWaitingForPotion = currentState == CustomerState.WaitingForPotion;

        if(attentionText != null)
        {
            attentionText.gameObject.SetActive(isWaitingForAttention);

            attentionText.text = "!";
        }

        if(requestedPotionIcon != null)
        {
            requestedPotionIcon.sprite = CurrentOrder.RequestedPotion.icon;
        }  

        if(requestedPotionIcon != null)
        {
            requestedPotionIcon.gameObject.SetActive(isWaitingForPotion);

            if(isWaitingForPotion)
            {
                requestedPotionIcon.sprite = CurrentOrder.RequestedPotion.icon;
            }
        }
        
        if(patienceMoodImage != null)
        {
            float patiencePercent =
                CurrentOrder.remainingPatience / CurrentOrder.patienceSeconds;

            if(patiencePercent > 0.90f)
            {
                patienceMoodImage.sprite = greatMood;
            }
            else if(patiencePercent > .70f)
            {
                patienceMoodImage.sprite = goodMood;
            }
            else if (patiencePercent > .50f)
            {
                patienceMoodImage.sprite = neutralMood;
            }
            else if (patiencePercent > .20f)
            {
                patienceMoodImage.sprite = sadMood;
            }
            else if (patiencePercent > .01f)
            {
                patienceMoodImage.sprite = angryMood;
            }
            else 
            {
                patienceMoodImage.sprite = skullMood;
            }
        }
    }

    private void OrderCompleted()
    {
        currentState = CustomerState.Finished;

        float satisfaction =
            Mathf.Clamp01(
                CurrentOrder.remainingPatience /
                CurrentOrder.patienceSeconds);

        if(shopFunds != null)
        {
            shopFunds.AddCoins(CurrentOrder.reward);
        }

        Debug.Log(
            "Order complete: " +
            CurrentOrder.RequestedPotion.itemName +
            " - earned " + CurrentOrder.reward);

        CurrentOrder = null;

        BeginLeaving(CustomerOutcome.Served, satisfaction);
    }

    private void OrderFailed()
    {      

        Debug.Log("Customer left because their patience ran out.");              

        CurrentOrder = null;

        BeginLeaving(CustomerOutcome.TimedOut, 0f);        
    }

    private void UpdateWalkingAnimation(Vector2 direction)
    {
        if(customerAnimator == null ||
            customerRenderer == null ||
            direction.sqrMagnitude <= 0.001f)
        {

        }

        string desiredAnimation;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) 
        {
            desiredAnimation = "WalkSide";

            //flip sprite for other walk direction
            customerRenderer.flipX = direction.x < 0f;
        }

        else if(direction.y > 0f)
        {
            desiredAnimation = "WalkUp";
            customerRenderer.flipX = false;
        }

        else
        {
            desiredAnimation = "WalkDown";
            customerRenderer.flipX = false;
        }

        customerAnimator.speed = 1f;

        if(desiredAnimation != currentWalkAnimation)
        {
            customerAnimator.Play(desiredAnimation, 0, 0f);

            currentWalkAnimation = desiredAnimation;
        }
        
    }

    private void PlayCounterIdleAnimation()
    {
        if (customerAnimator == null)
        {
            return;
        }

        if (customerRenderer != null)
        {
            customerRenderer.flipX = false;
        }

        customerAnimator.speed = 1f;
        customerAnimator.Play("IdleUp", 0, 0f);

        currentWalkAnimation = "IdleUp";
    }
}
