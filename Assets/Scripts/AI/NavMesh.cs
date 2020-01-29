using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

namespace Pathfinding3D{
    public class NavMesh : MonoBehaviour
    {
        public LayerMask obstacleLayer;

        private Queue<PathResult> results = new Queue<PathResult>();

        public static NavMesh instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);

            SetNodeGrid();
        }

        public Vector3 gridZeroCorner;
        public Vector3 gridSize;
        public float nodeSize;

        private List<Node> nodes = new List<Node>();

        public bool drawNodeGizmos;
        public bool drawGridGizmo;

        #region Start
        private void Start()
        {

        }

        void SetNodeGrid()
        {
            Vector3Int s = new Vector3Int((int)(gridSize.x / nodeSize), (int)(gridSize.y / nodeSize), (int)(gridSize.z / nodeSize));
            int index(int x, int y, int z) // get index of node which position in grid is (x, y, z)
            {
                if (x >= 0 && x < s.x && y >= 0 && y < s.y && z >= 0 && z < s.z)
                    return z + y * s.z + x * s.y * s.z;
                else
                    return -1;
            }
            nodes = new List<Node>();
            for (int x = 0; x < s.x; x++)
                for (int y = 0; y < s.y; y++)
                    for (int z = 0; z < s.z; z++)
                    {
                        Vector3 pos = new Vector3(x, y, z) * nodeSize;
                        Vector3 size = new Vector3(nodeSize, nodeSize, nodeSize) * .5f;
                        Node n = new Node(pos, size);
                        if (Physics.CheckBox(pos, size, Quaternion.identity, obstacleLayer))
                            n.walkable = false;
                        for (int i=-1; i<2; i++)
                            for(int j=-1; j<2; j++)
                                for(int k = -1; k<2; k++)
                                    if(i!=0 || j!= 0 || k != 0)
                                    {
                                        int ind = index(x + i, y + j, z + k);
                                        if (ind != -1)
                                            n.connected.Add(ind);
                                    }
                        nodes.Add(n);
                    }
            
        }
        #endregion
        #region Update
        private void Update()
        {
            if(Time.time > .3f)
                if(results.Count > 0)
                {
                    int itemsInQueue = results.Count;
                    lock (results)
                    {
                        for(int i=0; i<itemsInQueue; i++)
                        {
                            PathResult r = results.Dequeue();
                            r.callback(r.exist, r.vertices);
                        }
                    }
                }
        }
        #endregion
        #region pathfinding
        /// <summary>
        /// Gets the shortest path beetween two points
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private Path GetPath(Vector3 from, Vector3 to)
        {
            Node start = PositionToNode(from);
            Node end = PositionToNode(to);

            List<Vector3> path = new List<Vector3>();
            bool pathSuccess = false;

            if (start != null && end != null) //if from and to is inside the navMesh
            {
                if (start.walkable == true && end.walkable == true)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        nodes[i].closed = false;
                    }

                    NodeHeap open = new NodeHeap(nodes.Count);
                    open.Push(start);

                    while (open.Count > 0)
                    {
                        Node cNode = open.Pop();
                        cNode.closed = true;
                        if (cNode == end)
                        {
                            pathSuccess = true;
                            break;
                        }

                        for (int i = 0; i < cNode.connected.Count; i++)
                        {
                            Node n = nodes[cNode.connected[i]];
                            if (n.closed || !n.walkable)
                                continue;
                            float newGCost;
                            if (cNode.parent == null)
                                newGCost = cNode.gCost + GetRelativeGCost(cNode, n);
                            else
                                newGCost = cNode.gCost + GetRelativeGCost(cNode, n, cNode.center - cNode.parent.center, 0.005556f);   //0.005556 = 1/180
                            if (!open.Contains(n))
                            {
                                n.gCost = newGCost;
                                n.hCost = HeuristiceDistance(n, end);
                                n.parent = cNode;
                                open.Push(n);
                            }
                            else if (newGCost < n.gCost)
                            {
                                n.gCost = newGCost;
                                n.hCost = HeuristiceDistance(n, end);
                                n.parent = cNode;
                                open.UpdateHeap(n);
                            }
                        }
                    }
                    if (pathSuccess)
                    {
                        Node cNode = end;
                        path.Add(to);
                        while (cNode != start)
                        {
                            cNode = cNode.parent;
                            path.Add(cNode.center);
                        }
                        path.Add(from);
                        path.Reverse();
                        path = SimplifyPath(path);
                    }
                }
            }
            return new Path(pathSuccess, path);
        }

        /// <summary>
        /// Index of the node form NavMesh.nodes that contains pos
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>-1 if pos is beyond navMesh, else index of node</returns>
        public static Node PositionToNode(Vector3 pos)
        {
            /*   foreach (Node n in instance.nodes)
                   if (n.ContainsPoint(pos))
                       return n;
            */
            Vector3Int s = new Vector3Int((int)(instance.gridSize.x / instance.nodeSize), (int)(instance.gridSize.y / instance.nodeSize), (int)(instance.gridSize.z / instance.nodeSize));
            int index(int x, int y, int z) // get index of node which position in grid is (x, y, z)
            {
                if (x >= 0 && x < s.x && y >= 0 && y < s.y && z >= 0 && z < s.z)
                    return z + y * s.z + x * s.y * s.z;
                else
                    return -1;
            }
            Vector3Int gridPos = Vector3Int.RoundToInt(pos / instance.nodeSize);
            int ind = index(gridPos.x, gridPos.y, gridPos.z);
            if (ind != -1 && instance.nodes[ind].ContainsPoint(pos))
                {
                    return instance.nodes[ind];
                }
            for (int i=-1; i<2; i++)
                for(int j=-1; j<2; j++)
                    for(int k=-1; k<2; k++)
                    {
                        if (i != 0 || j != 0 || k != 0)
                        {
                            ind = index(gridPos.x + i, gridPos.y + j, gridPos.z + k);
                            if (ind != -1 && instance.nodes[ind].ContainsPoint(pos))
                                return instance.nodes[ind];
                        }
                    }
            return null;
        }
        /// <summary>
        /// calculates gCost of moving between nodes a and b, given rotation in node a;
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="aForward">forward axis of agent in node a</param>
        /// <param name="angularPenalty">penalty for turning</param>
        /// <returns></returns>
        float GetRelativeGCost(Node a, Node b, Vector3 aForward, float angularPenalty)
        {
            Vector3 dir = b.center - a.center;
            float penalty = 1 + Vector3.Angle(aForward, dir) * angularPenalty;
            return dir.magnitude * (a.weight + b.weight) * penalty;

        }
        float GetRelativeGCost(Node a, Node b)
        {
            return Vector3.Magnitude(a.center - b.center) * (a.weight + b.weight);
        }
        float HeuristiceDistance(Node a, Node b)
        {
            return Vector3.Magnitude(a.center - b.center) * (a.weight + b.weight);
        }


        List<Vector3> SimplifyPath(List<Vector3> path)
        {
            List<Vector3> sPath = new List<Vector3>();
            sPath.Add(path[0]);
            for (int i = 2; i < path.Count-1; i++)
                if (path[i] - path[i - 1] != path[i + 1] - path[i])
                    sPath.Add(path[i]);
            sPath.Add(path[path.Count - 1]);
            return sPath;
        }

        #endregion
        #region RequestingPath
        public static void RequestPath(Vector3 from, Vector3 to, float turningDistance, Action<bool, List<Vector3>> callback)
        {
            ThreadStart threadStart = delegate
            {
                Path p = instance.GetPath(from, to);
                
                instance.FinishedProcessingPath(new PathResult(p, callback));
            };
            threadStart.Invoke();
        }

        void FinishedProcessingPath(PathResult result)
        {
            lock (results)
            {
                results.Enqueue(result);
            }
        }
        #endregion
        #region Gizmos
        private void OnDrawGizmos()
        {

            if (drawNodeGizmos)
            {
                Gizmos.color = Color.white;
                for (int i = 0; i < nodes.Count; i++)
                    nodes[i].DrawNodeGizmo();
            }
            if (drawGridGizmo)
            {
                Gizmos.DrawWireCube(gridZeroCorner + (gridSize - new Vector3(nodeSize, nodeSize, nodeSize)) * .5f, gridSize);
            }
        }
        #endregion
    }
    public struct Path
    {
        public bool exist;
        public List<Vector3> vertices;
        public Path(bool _exist, List<Vector3> _vertices)
        {
            exist = _exist;
            vertices = _vertices;
        }
    }
    public struct PathResult
    {
        public bool exist;
        public List<Vector3> vertices;
        public Action<bool, List<Vector3>> callback;
        public PathResult(bool _exist, List<Vector3> _vertices, Action<bool, List<Vector3>> _callback)
        {
            exist = _exist;
            vertices = _vertices;
            callback = _callback;
        }
        public PathResult(Path _path, Action<bool, List<Vector3>> _callback)
        {
            exist = _path.exist;
            vertices = _path.vertices;
            callback = _callback;
        }
    }
}




