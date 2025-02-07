using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;


[SerializeField]
class Node : IComparable<Node>
{
    public int x, y, G, H;
    public int F { get { return G + H; } }

    public int CompareTo(Node node)
    {
        if (node == null) return 1; 
        return this.F.CompareTo(node.F);
    }
}

public class PriorityQueue_Custom<T> where T : IComparable<T>
{
    private List<T> _heap = new List<T>();
    public int Count => _heap.Count;
    public bool IsEmpty => _heap.Count == 0;

    public void Clear()
    {
        _heap.Clear();
    }

    public void Enqueue(T newData)
    {
        _heap.Add(newData);
        heapifyUp(_heap.Count - 1);
    }

    public T Dequeue()
    {
        if (_heap.Count == 0) return default(T);
        T returnData = _heap[0];

        int lastIndex = _heap.Count - 1;
        _heap[0] = _heap[lastIndex];
        _heap.RemoveAt(lastIndex);

        if(_heap.Count > 0 ) heapifyDown(0);

        return returnData;
    }

    private void heapifyUp(int index)
    {
        while(index > 0)
        {
            int pIndex = (index - 1) / 2;
            if (_heap[index].CompareTo(_heap[pIndex]) >= 0) break;

            swap(index, pIndex);
            index = pIndex;
        }
    }

    private void heapifyDown(int index)
    {
        int lastIndex = _heap.Count - 1;

        while((index * 2 + 1) <= lastIndex)
        {
            int left = index * 2 + 1;
            int right = index * 2 + 2;
            int selectIndex = left;

            if(right <= lastIndex && _heap[left].CompareTo(_heap[right]) > 0 )
                selectIndex = right;

            if (_heap[index].CompareTo(_heap[selectIndex]) <= 0)
                break;

            swap(index, selectIndex);
            index = selectIndex;
        }
    }

    private void swap(int index1, int index2)
    {
        T temp = _heap[index1];
        _heap[index1] = _heap[index2];
        _heap[index2] = temp;
    }
}

public class Navigator : MonoBehaviour
{
    public enum EPathType{
        Grid,
        Dynamic
    }
    
    [SerializeField] private EPathType _pathType;
    [SerializeField] private int _nodeSize;
    [SerializeField] private int _maxDistance;
    [SerializeField] private bool _diagonalCheck;


    private List<Node> confirmedPath;

    public EPathType PathType { get { return _pathType; } }


    public bool FindPath()
    {
        
        
        return false;
        
    }
}
