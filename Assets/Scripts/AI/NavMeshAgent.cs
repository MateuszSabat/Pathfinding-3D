using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding3D;

public class NavMeshAgent : MonoBehaviour
{
    protected bool pathRequestSet;
    public float speed;
    protected Vector3[] path = new Vector3[0];
    public float turningDistance;
    public float sqrTurningDistance;

    public bool drawPath;

    protected void Start()
    {
        sqrTurningDistance = turningDistance * turningDistance;
    }


    public void UpdatePath(Vector3 target)
    {
        NavMesh.RequestPath(transform.position, target, turningDistance, SetPath);
    }

    void SetPath(bool existPath, List<Vector3> _path)
    {
        if (existPath)
        {
            path = _path.ToArray();

            OnSetPath();
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
        else
        {
            //path.Clear();
            Debug.LogWarning("Path does not exist");
        }
    }

    IEnumerator FollowPath()
    {
        int index = 0;

        while (true)
        {
            if(index < path.Length-1)
                if (path[index] == path[index + 1])
                {
                    index++;
                }
            if(transform.position == path[index])
            {
                index++;
                if (index >= path.Length)
                    yield break;
            }
            else if(index < path.Length -1 && (transform.position-path[index]).sqrMagnitude < sqrTurningDistance)
            {
                path[index] = Vector3.MoveTowards(path[index], path[index + 1], speed * 0.5f * Time.deltaTime);
            }
            transform.forward = path[index] - transform.position;
            transform.position = Vector3.MoveTowards(transform.position, path[index], speed * Time.deltaTime);
            yield return null;
        }
    }

    public virtual void OnSetPath()
    {
        Debug.Log("Path Set");
    }


    private void OnDrawGizmos()
    {
        if(drawPath)
            for (int i = 0; i < path.Length - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
                Gizmos.DrawSphere(path[i + 1], 1f);
            }
    }

}
