using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractInteractable : MonoBehaviour, IInteractable
{

    [SerializeField] private float _interactionRadius = 1.5f;

    protected GameManager _gameManager => GameManager.Instance;
    public virtual void OnClick(PlayerController player, RaycastHit hit)
    {
        player.MoveToPoint(this.transform.position);
        StartCoroutine(WaitInteract(player));
    }

    public virtual IEnumerator WaitInteract(PlayerController player)
    {
        while (Vector3.Distance(player.transform.position, this.transform.position) > _interactionRadius
            || player.CurrentState == PlayerController.PlayerControlState.Walking)
        {
            yield return null;
        }
        Interact(player);
    }

    public virtual void Interact(PlayerController player)
    {
        Debug.Log("Interacted with " + this.name);
        player.SetState(PlayerController.PlayerControlState.Interacting);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);
    }
}
