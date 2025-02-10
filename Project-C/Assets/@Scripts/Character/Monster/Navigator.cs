using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.HableCurve;


[SerializeField]
class Node : IComparable<Node>
{
    public float x, y, G, H;
    public bool _isBlocking;
    public float F { get { return G + H; } }

    public Node(float x, float y, bool isBlocking) 
    {
        this.x = x;
        this.y = y;
        this._isBlocking = isBlocking;
    }

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
    [SerializeField] private float _nodeSize = 0;
    [SerializeField] private float _collisionBuffer = 0;
    [SerializeField] private int _maxDistance = 0;
    [SerializeField] private bool _diagonalCheck;
    [SerializeField] private string[] _blockLayers;
    

    [SerializeField] GameObject target;

    private List<Node> _confirmedPath;
    private Node[,] _map;
    private string _blockStr;

    public EPathType PathType { get { return _pathType; } }

    private void makingBlockingLayerString()
    {
        _blockStr = string.Join(",", _blockLayers);
        Debug.Log(_blockStr);
    }

    void Start()
    {
        FindPath(this.transform.position, target.transform.position);
    }


    public bool FindPath(Vector2 mover, Vector2 target)
    {
        
        if (_nodeSize == 0 || _maxDistance == 0 || _blockLayers == null) return false;
        if(Vector2.Distance(mover, target) > _nodeSize * _maxDistance) return false;
        
        if(_blockStr == null) makingBlockingLayerString();
        
        int mapMaxSizeX = _maxDistance * 2 + 1;
        int mapMaxSizeY = _maxDistance * 2 + 1;

        _map = new Node[mapMaxSizeY, mapMaxSizeX];


        for(int y = 0; y < mapMaxSizeY; y++)
        {
            for(int x = 0; x < mapMaxSizeX; x++)
            {
                float posX = ((_maxDistance - x) * _nodeSize) + mover.x;
                float posY = ((_maxDistance - y) * _nodeSize) + mover.y;
                bool isBlocking = Physics2D.OverlapCircle(new Vector2(posX, posY), _nodeSize / 2 - (_collisionBuffer), LayerMask.GetMask(_blockLayers));

                _map[y, x] = new Node(posX, posY, isBlocking);
                positions.Add(new Vector2(posX, posY)); // test
            }
        }

        




        return false;
        
    }




    /*
     * 테스트 용
     */
    public List<Vector2> positions = new List<Vector2>();  // 여러 개의 원의 중심 좌표
    

    public void Update()
    {
        foreach (Node node in _map)
        {
            Color col = Color.green;
            if (node._isBlocking) col = Color.red;

            DrawCircle(new Vector2(node.x, node.y), _nodeSize / 2 - (_collisionBuffer), col);
        }
    }

    private void DrawCircle(Vector2 center, float radius, Color color, int segments = 36)
    {
        
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector2(radius, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

            Debug.DrawLine(prevPoint, newPoint, color, 0.02f);
            prevPoint = newPoint;
        }
    }



}
