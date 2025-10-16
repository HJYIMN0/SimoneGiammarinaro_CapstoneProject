using UnityEngine;

public class InteractableBed : AbstractInteractable
{
    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            //gameManager.Sleep();
        }
        else
        {
            Debug.LogError("GameManager instance not found.");
        }
    }
}
