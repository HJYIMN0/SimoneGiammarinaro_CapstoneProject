using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    //[SerializeField] private LayerMask _groundLayer;
    //[SerializeField] private LayerMask _interactableLayer;

    private PlayerControlState _currentState = PlayerControlState.Idle;
    private NavMeshAgent _agent;

    private float h;
    private float v;
    public PlayerControlState CurrentState => _currentState;

    public void SetState(PlayerControlState newState)
    {
        if (_currentState != newState)
        {
            _currentState = newState;
        }
    }

    public enum PlayerControlState
    {
        Idle,
        Walking,
        Interacting
    }


    private void Awake()
    {
        if (_cam == null)
        {
            Debug.Log("No camera assigned, using main camera.");
            _cam = Camera.main;
        }

        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null) Debug.LogError("NavMeshAgent component is missing on PlayerController.");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && _currentState != PlayerControlState.Interacting)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit) &&
                hit.collider.TryGetComponent(out IInteractable clickable))
            {

                clickable.OnClick(this, hit);
            }
        }

        if (_currentState == PlayerControlState.Walking && !_agent.pathPending)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                {
                    _currentState = PlayerControlState.Idle;
                }
            }
        }
    }

    public void MoveToPoint(Vector3 point)
    {
        _agent.SetDestination(point);
        _currentState = PlayerControlState.Walking;
    }
}

