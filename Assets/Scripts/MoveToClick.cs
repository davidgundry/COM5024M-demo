using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerBehaviour : MonoBehaviour
{

    NavMeshAgent nma;

    public UnityEvent onMove;

    public DataObject dataObject;

    // Start is called before the first frame update
    void Start()
    {
        nma = GetComponent<NavMeshAgent>();
        nma.destination = new Vector3(100,10,10);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;
            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity))
            {
                nma.destination = hit.point;
            }
        }

        dataObject.globalValue = transform.position.y;
        onMove.Invoke();
    }
}

delegate void OnMove(); 