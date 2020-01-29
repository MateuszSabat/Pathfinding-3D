using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding3D;

public class HomingMissile : NavMeshAgent
{

    public Transform missileTarget;
    private Vector3 aimingPos;
    public float updateInterval;
    private float timeToUpdate;

    private Node targetNode;

    protected void Start()
    {
        base.Start();
        UpdatePath(missileTarget.position);
        aimingPos = missileTarget.position;
        timeToUpdate = updateInterval;
        targetNode = NavMesh.PositionToNode(aimingPos);
    }

    private void Update()
    {
        if (timeToUpdate < 0)
        {
            if (aimingPos != missileTarget.position)
            {
                if (targetNode != null && targetNode.ContainsPoint(missileTarget.position))
                {
                    aimingPos = missileTarget.position;
                    path[path.Length - 1] = aimingPos;
                }
                else
                {
                    timeToUpdate = updateInterval;
                    UpdatePath(missileTarget.position);
                    aimingPos = missileTarget.position;
                    targetNode = NavMesh.PositionToNode(aimingPos);
                }
            }
        }
        else
            timeToUpdate -= Time.deltaTime;
    }
}
