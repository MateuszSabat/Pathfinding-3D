using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding3D
{
    [System.Serializable]
    public class Node
    {
        public Vector3 center;
        public Vector3 size;
        public float weight = 1f;

        [HideInInspector]
        public List<int> connected;
        [HideInInspector]
        public bool closed;
        [HideInInspector]
        public int heapIndex;
        [HideInInspector]
        public float hCost;
        [HideInInspector]
        public float gCost;
        [HideInInspector]
        public float fCost {
            get {
                return gCost + hCost;
            }
        }
        
        [HideInInspector]
        public Node parent;

        public bool walkable;

        public Node(Vector3 _center, Vector3 _size)
        {
            center = _center;
            size = _size;
            connected = new List<int>();
            closed = false;
            heapIndex = 0;
            hCost = 0;
            gCost = 0;
            parent = null;
            walkable = true;
        }

        public bool ContainsPoint(Vector3 pos)
        {
            if (pos.x >= center.x - size.x && pos.x < center.x + size.x && pos.y >= center.y - size.y && pos.y < center.y + size.y && pos.z >= center.z - size.z && pos.z < center.z + size.z)
                return true;
            else return false;
        }

        public void DrawNodeGizmo()
        {
            Color c = Gizmos.color;
            if (walkable)
            {
               // Gizmos.color = Color.white;
              //  Gizmos.DrawWireCube(center, 2 * size);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(center, 2 * size);
            }
            Gizmos.color = c;
        }

        public int CompareTo(Node nodeToCompare)
        {
            int c = fCost.CompareTo(nodeToCompare.fCost);
            if (c == 0)
                c = hCost.CompareTo(nodeToCompare.hCost);
            return -c;
        }

        public Vector3 ClosestPointOnSurface(Vector3 to)
        {
            return new Vector3(
                Mathf.Clamp(to.x, center.x - size.x, center.x + size.x),
                Mathf.Clamp(to.y, center.y - size.y, center.y + size.y),
                Mathf.Clamp(to.z, center.z - size.z, center.z + size.z));
        }
    }
}
