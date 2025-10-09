using System.Collections;
using UnityEngine;

public class InteractableDoor : AbstractInteractable
{
    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        // StartCoroutine(DoorRoutine(player));
    }

    //private IEnumerator DoorRoutine(PlayerController player)
    //{
    //    player.gameObject.TryGetComponent<PlayerAnimationController>();
    //    player.SetState(PlayerController.PlayerControlState.Idle);
    //}
}
