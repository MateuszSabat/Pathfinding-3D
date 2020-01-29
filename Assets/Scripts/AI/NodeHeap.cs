using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding3D
{
    public class NodeHeap
    {
        private Node[] items;
        private int currentItemCount;

        public NodeHeap(int maxSize)
        {
            items = new Node[maxSize];
            currentItemCount = 0;
        }

        /// <summary>
        /// Add node to heap
        /// </summary>
        /// <param name="node"></param>
        public void Push(Node node)
        {
            node.heapIndex = currentItemCount;
            items[currentItemCount] = node;
            currentItemCount++;
            SortUp(node);
        }
        /// <summary>
        /// Get smallest node and remove it from heap
        /// </summary>
        /// <returns>smallest node in heap</returns>
        public Node Pop()
        {
            Node f = items[0];
            currentItemCount--;
            items[0] = items[currentItemCount];
            items[0].heapIndex = 0;
            SortDown(items[0]);
            return f;
        }
        /// <summary>
        /// need to be called after updating fCost of node n
        /// </summary>
        /// <param name="n"></param>
        public void UpdateHeap(Node n)
        {
            SortUp(n);
        }
        public int Count {
            get {
                return currentItemCount;
            }
        }
        public bool Contains(Node n)
        {
            return items[n.heapIndex] == n;
        }

        void SortDown(Node node)
        {
            while (true)
            {
                int indexL = node.heapIndex * 2 + 1;
                int indexR = node.heapIndex * 2 + 2;
                int swapIndex = 0;
                if (indexL < currentItemCount)
                {
                    swapIndex = indexL;
                    if (indexR < currentItemCount)
                        if (items[indexL].CompareTo(items[indexR]) < 0)
                            swapIndex = indexR;

                    if (node.CompareTo(items[swapIndex]) < 0)
                        Swap(node, items[swapIndex]);
                    else
                        return;
                }
                else
                    return;
            }
        }
        void SortUp(Node node)
        {
            while (true)
            {
                int parentIndex = (node.heapIndex - 1) / 2;
                Node parentNode = items[parentIndex];
                if (node.CompareTo(parentNode) > 0)
                    Swap(node, parentNode);
                else
                    return;
            }
        }

        void Swap(Node a, Node b)
        {
            items[a.heapIndex] = b;
            items[b.heapIndex] = a;
            int i = a.heapIndex;
            a.heapIndex = b.heapIndex;
            b.heapIndex = i;
        }
    }
}
