using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class FleeTest : MonoBehaviour
{
    public NavMeshAgent navAgent;
    public GameObject fleeFrom;

    private void Start()
    {
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        navAgent.SetDestination(new Vector3(0, 0, 0));
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, fleeFrom.transform.position) < 4f)
        {
            Debug.Log(Vector3.Distance(transform.position, fleeFrom.transform.position) < 4f);

            Vector3 moveDirection = transform.position + (transform.position - fleeFrom.transform.position);
            navAgent.SetDestination(moveDirection.normalized * 2.5f);
        }
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.white;
        Handles.DrawWireDisc(transform.position, transform.up, 4f);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(navAgent.destination, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + (transform.position - fleeFrom.transform.position).normalized * 2.5f, 0.3f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, fleeFrom.transform.position);
        }
    }
}
