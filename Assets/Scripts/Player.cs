using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [Header("Water Effect")]
    [SerializeField] private WaterSplashEffect waterSplashPrefab;

    [Header("Combat")]
    [SerializeField, Min(1)] private int axeDamage = 1;
    [SerializeField, Min(0.1f)] private float axeRange = 0.55f;
    [SerializeField, Min(0)] private float axeForwardOffset = 0.65f;
    [SerializeField] private LayerMask enemyLayer;

    private Vector3Int pendingWaterPosition;
    private Vector2 pendingWaterDirection;
    private bool hasPendingWater;

    public Inventory_UI inventoryUI;
    public InventoryManager inventoryManager;
    private TileManager tileManager;
    public Toolbar_UI toolbarUI;
    public Animator animator;
    public IngredientStorage ingredientStorage;    
    public Storage_UI storageUI;
    public PotionBook_UI potionBookUI;

    private PlayerMovement movement;

    private Vector2 pendingAxeDirection;
    private bool hasPendingAxeAttack;

    private Vector2 facingDirection = Vector2.down;
    private Vector3Int pendingPlowPosition;
    private bool hasPendingPlow;
    private bool isUsingTool;


    private void Start()
    {
        tileManager = GameManager.instance.tileManager;
    }

    private void Awake()
    {
        inventoryManager = GetComponent<InventoryManager>();
        movement = GetComponent<PlayerMovement>();
    }

    public void OnInventory()
    {
        if (storageUI != null && storageUI.IsOpen)
        {
            storageUI.Close();
            return;
        }

        inventoryUI.ToggleInventory();
    }

    private void Update()
    {
        if (tileManager != null)
        {
            if(toolbarUI != null &&
                toolbarUI.inputType == Toolbar_UI.InputType.Gamepad)
            {
                UpdateFarmHighlight();
            }
            if(isUsingTool)
            {
                return;
            }

            bool interactPressed =
                toolbarUI.inputType == Toolbar_UI.InputType.Keyboard
                ? Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame
                : Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;

            if (interactPressed)
            {
                Vector2 direction = movement.GetFacingDirection();

                Vector3 targetPosition = transform.position + (Vector3)direction;

                Vector3Int position = tileManager.WorldToCell(targetPosition);

                Collider2D hit = Physics2D.OverlapCircle(targetPosition, 0.35f);
                IngredientStorage storage = hit != null
                    ? hit.GetComponent<IngredientStorage>()
                    : null;                                

                if (storage != null)
                {
                    if (storage != null)
                    {
                        storageUI.Open(storage, this);
                        return;
                    }

                    return;
                }

                BrewingStation brewingStation = hit != null
                    ? hit.GetComponent<BrewingStation>()
                    : null;

                if (brewingStation != null)
                {
                   //p2 has no potionbookUI, only p1 uses cauldrons
                   if(potionBookUI == null)
                    {
                        return;
                    }

                   if(brewingStation.IsReady)
                    {
                        brewingStation.Collect(this);
                    }
                   else if(!brewingStation.IsBrewing)
                    {
                        potionBookUI.Open(brewingStation, this);
                    }
                    return;
                }

                Customer customer = hit != null ? hit.GetComponent<Customer>() : null;

                if(customer != null)
                {
                    customer.Interact(this);
                    return;
                }

                SeedShop seedShop = hit != null
                    ? hit.GetComponent<SeedShop>()
                    : null;

                if(seedShop != null)
                {
                    seedShop.Open(this);
                    return;
                }

                Inventory.Slot selectedSlot =
                    inventoryManager.toolbar.selectedSlot;

                Item selectedItem = null;

                if(selectedSlot != null &&
                    !string.IsNullOrEmpty(selectedSlot.itemName))
                {
                    selectedItem =
                        GameManager.instance.itemManager.GetItemByName(
                            selectedSlot.itemName);
                }

                if(selectedItem != null &&
                    selectedItem.data != null &&
                    selectedItem.data.toolType == ItemData.ToolType.Axe)
                {
                    BeginAxeAttack(direction);
                    return;
                }


                if (!tileManager.IsFarmTile(position))
                {
                    return;
                }

                TileManager.FarmState state = tileManager.GetFarmState(position);

                if(state == TileManager.FarmState.Ready)
                {
                    if(tileManager.Harvest(position, out ItemData harvestedData))
                    {
                        Item ingredientPrefab =
                            GameManager.instance.itemManager.GetItemByName(harvestedData.itemName);

                        if(ingredientPrefab != null)
                        {
                            DropItem(ingredientPrefab);
                        }
                        else
                        {
                            Debug.Log("No item prefab found  for: " + harvestedData.itemName);
                        }
                    }

                    return;
                }                

                if(selectedItem ==  null)
                {
                    return;
                }                

                switch (state)
                {
                    case TileManager.FarmState.Empty:
                        
                        if(selectedItem.data.toolType == ItemData.ToolType.Hoe)
                        {

                            Vector2 facing = movement.GetFacingDirection();

                            animator.SetFloat("LastMoveX", facing.x);
                            animator.SetFloat("LastMoveY", facing.y);

                            pendingPlowPosition = position;
                            hasPendingPlow = true;
                            isUsingTool = true;

                            movement.SetMovementLocked(true);
                            tileManager.ClearHighlight();

                            animator.SetTrigger("Hoe");
                        }

                        break;

                    case TileManager.FarmState.Plowed:

                        if(selectedItem.data.itemType == ItemData.ItemType.Seed)
                        {
                            if(selectedItem.data.cropToGrow == null)
                            {
                                Debug.Log("This seed has no crop assigned");
                                return;
                            }

                            bool planted = tileManager.Plant(position,
                                selectedItem.data.cropToGrow
                                );

                            if(planted)
                            {
                                inventoryManager.toolbar.Remove(
                                inventoryManager.toolbar.selectedSlotIndex
                                );
                                toolbarUI.Refresh();
                            }

                            
                        }

                        break;

                    case TileManager.FarmState.Growing:

                        if(selectedItem.data.toolType == ItemData.ToolType.WateringCan)
                        {
                            Vector2 facing = movement.GetFacingDirection();

                            animator.SetFloat("LastMoveX", facing.x);
                            animator.SetFloat("LastMoveY", facing.y);

                            pendingWaterPosition = position;
                            pendingWaterDirection = facing;
                            hasPendingWater = true;
                            isUsingTool = true;

                            movement.SetMovementLocked(true);
                            tileManager.ClearHighlight();

                            animator.SetTrigger("Water");
                        }                        

                        break;                    
                }
            }              

        }
    }

    private void BeginAxeAttack(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            direction = Vector2.down;
        }

        direction.Normalize();

        animator.SetFloat("LastMoveX", direction.x);
        animator.SetFloat("LastMoveY", direction.y);

        pendingAxeDirection = direction;
        hasPendingAxeAttack = true;
        isUsingTool = true;

        movement.SetMovementLocked(true);
        tileManager.ClearHighlight();

        animator.SetTrigger("Axe");
    }
       
    public void DropItem(Item item)
    {
        Vector2 spawnLocation = transform.position;

        Vector2 spawnOffset = Random.insideUnitCircle * 1.25f;

        Item droppedItem = Instantiate(item, spawnLocation + spawnOffset, Quaternion.identity);

        droppedItem.rb2d.AddForce(spawnOffset * .2f, ForceMode2D.Impulse);
    }

    public void DropItem(Item item, int numToDrop)
    {
        for (int i = 0; i < numToDrop; i++)
        {
            DropItem(item);
        }
    }
    private void UpdateFarmHighlight()
    {
        if (isUsingTool ||
            inventoryManager == null ||
            inventoryManager.toolbar == null)
        {
            tileManager.ClearHighlight();
            return;
        }

        Inventory.Slot selectedSlot =
            inventoryManager.toolbar.selectedSlot;

        if (selectedSlot == null ||
            string.IsNullOrEmpty(selectedSlot.itemName))
        {
            tileManager.ClearHighlight();
            return;
        }

        Item selectedItem =
            GameManager.instance.itemManager.GetItemByName(
                selectedSlot.itemName);

        if (selectedItem == null || selectedItem.data == null)
        {
            tileManager.ClearHighlight();
            return;
        }

        Vector2 direction = movement.GetFacingDirection();

        Vector3 targetWorldPosition =
            transform.position + (Vector3)direction;

        Vector3Int targetCell =
            tileManager.WorldToCell(targetWorldPosition);

        if(!tileManager.IsFarmTile(targetCell))
        {
            tileManager.ClearHighlight();
            return;
        }

        TileManager.FarmState state = tileManager.GetFarmState(targetCell);

        bool canPlow =
             selectedItem.data.toolType == ItemData.ToolType.Hoe && 
             state == TileManager.FarmState.Empty;

        bool canWater =
            selectedItem.data.toolType ==
            ItemData.ToolType.WateringCan &&
            state == TileManager.FarmState.Growing;

        if(canPlow || canWater)
        {
            tileManager.HighlightTile(targetCell);
        }
        else
        {
            tileManager.ClearHighlight();
        }
    }

    public void ApplyPendingPlow()
    {
        if (!hasPendingPlow || tileManager == null)
        {
            return;
        }

        bool canStillPlow =
            tileManager.IsFarmTile(pendingPlowPosition) &&
            tileManager.GetFarmState(pendingPlowPosition) ==
            TileManager.FarmState.Empty;

        if (canStillPlow)
        {
            tileManager.Plow(pendingPlowPosition);
        }

        hasPendingPlow = false;
    }

    public void ApplyPendingWater()
    {
        if(!hasPendingWater || tileManager == null)
        {
            return;
        }

        bool canStillWater =
            tileManager.IsFarmTile(pendingWaterPosition) &&
            tileManager.GetFarmState(pendingWaterPosition) ==
            TileManager.FarmState.Growing;

        if(canStillWater && tileManager.Water(pendingWaterPosition))
        {
            if(waterSplashPrefab != null)
            {
                Vector3 splashPostion = tileManager.GetCellCenterWorld(pendingWaterPosition);

                WaterSplashEffect splash = Instantiate(waterSplashPrefab, 
                    splashPostion, Quaternion.identity);

                splash.Play(pendingWaterDirection);
            }
        }

        hasPendingWater = false;
    }

    public void ApplyPendingAxeDamage()
    {
        if(!hasPendingAxeAttack)
        {
            return;
        }

        Vector2 attackCenter = (Vector2)transform.position +
            pendingAxeDirection * axeForwardOffset;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, axeRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy =
                hit.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(axeDamage);
                break;
            }
        }

        hasPendingAxeAttack = false;
    }

    public void FinishToolAnimation()
    {
        hasPendingPlow = false;
        hasPendingWater = false;
        hasPendingAxeAttack = false;
        isUsingTool = false;

        if (movement != null)
        {
            movement.SetMovementLocked(false);
        }
    }
}
