using UnityEngine;

public interface IInteractable
{
    public void OnClick(PlayerController player, RaycastHit hit);

}