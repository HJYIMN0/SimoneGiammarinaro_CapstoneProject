using UnityEngine;

public class InteractableGround : MonoBehaviour, IInteractable
{
    public void OnClick(PlayerController player, RaycastHit hit)
    {
        player.MoveToPoint(hit.point);
    }
}
