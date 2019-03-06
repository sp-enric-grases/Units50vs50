using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//public delegate void UnitHasStopped(UnitMovement unit);

public class UnitMovement : MonoBehaviour
{
    //public event UnitHasStopped UnitHasStoppedRequest;
    private const float REMAINING_DISTANCE = 2f;
    private NavMeshAgent _navMeshAgent;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void SetDestination(Vector3 destination)
    {
        if (_navMeshAgent == null)
            _navMeshAgent = GetComponent<NavMeshAgent>();

        //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //go.transform.position = destination;
        _navMeshAgent.SetDestination(destination);
    }

    public bool UnitHasStopped()
    {
        return _navMeshAgent.remainingDistance > REMAINING_DISTANCE ? false : true;
    }
}
