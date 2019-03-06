using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.AI;

public class AIManager : MonoBehaviour
{
    public float timeToCheck = 1;
    public GameObject navMesh;
    public GameObject controlPoints;
    private List<UnitMovement> _units = new List<UnitMovement>();
    private List<Vector3> _controlPoints = new List<Vector3>();
    private Mesh _mesh;

	void Awake ()
    {
        NavMesh.pathfindingIterationsPerFrame = 500;

        _units = GetComponentsInChildren<UnitMovement>().ToList();
        _mesh = navMesh.GetComponent<MeshFilter>().mesh;

        CreateControlPoints();
        SetInitDestinations();
    }

    private void OnEnable()
    {
        InvokeRepeating("CheckUnits", 3, timeToCheck);
    }

    private void CreateControlPoints()
    {
        for (int i = 0; i < _mesh.vertexCount; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = _mesh.vertices[i];
            go.transform.parent = controlPoints.transform;
        }

        controlPoints.transform.position = navMesh.transform.position;
        controlPoints.transform.rotation = navMesh.transform.rotation;

        for (int i = controlPoints.transform.childCount-1; i > 0; i--)
            _controlPoints.Add(controlPoints.transform.GetChild(i).transform.position);

        Destroy(controlPoints);
    }

    private void SetInitDestinations()
    {
        foreach (var unit in _units)
        {
            unit.SetDestination(GetRandomDestination());
            //unit.UnitHasStoppedRequest += SetNewDestination;
        }
    }

    private Vector3 GetRandomDestination()
    {
        return _controlPoints[Random.Range(0, _controlPoints.Count)];
    }
    
    private void CheckUnits()
    {
        foreach (var unit in _units)
        {
            if (unit.UnitHasStopped())
                unit.SetDestination(GetRandomDestination());
        }
    }

    private void SetNewDestination(UnitMovement unit)
    {
        unit.SetDestination(GetRandomDestination());
    }

    private void OnDisable()
    {
        CancelInvoke("CheckUnits");
    }
}
