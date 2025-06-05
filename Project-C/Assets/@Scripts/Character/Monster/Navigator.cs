using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;
using UnityEngine.XR;
using static UnityEngine.Rendering.HableCurve;


class AStarNavNode : IComparable<AStarNavNode>
{
    public Vector2 Pos;
    public Vector2Int GridIndex;
    public AStarNavNode ParentNode = null;

    public float G = float.MaxValue;    // 이동 거리
    public float H = 0;                 // 목표 거리
   
    public bool IsBlocking;
    public bool IsOpen = false;

    // 테스트 용 
    public bool ISLoad = false;    

    public float F { get { return G + H; } }

    public AStarNavNode(Vector2Int index, Vector2 position, bool isBlocking) 
    {
        Pos = position;
        GridIndex = index;
        this.IsBlocking = isBlocking;
    }

    public int CompareTo(AStarNavNode node)
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
    
    [Header("Default Setting"), Space(10)]
    [SerializeField] private EPathType _pathType;        // 길 찾기 형식 종류
    [SerializeField] private bool _diagonalCheck;        // 대각선 여부 판단.
    [SerializeField] private string[] _blockLayers;      // 진출 불가 레이어 리스트

    [Header("Grid Setting"), Space(10)]
    [SerializeField] GameObject _gridObject;                // 맵 오브젝트
    [SerializeField] GameObject _tileMapObject;             // 타일맵 오브젝트

    [Header("Dynamic Setting"), Space(10)]
    [SerializeField] private int _maxDistance = 0;       // 감지 가능 최대 칸 수 (가로 세로 기준)
    [SerializeField] Vector2 _cellSize = Vector2.zero;   // 칸 사이즈 
    [SerializeField] private float _collisionBuffer = 0; // 맵핑 여백

    [Header("Test Setting"), Space(10)]
    
    private PriorityQueue_Custom<AStarNavNode> _searchCandidate; // 탐색 후보 노드 
    private Dictionary<Vector2Int, bool> _searchingNode;         // 큐에 들어 있는 노드
    private AStarNavNode _targetNode = null;                     // 타겟 노드
    private Stack<AStarNavNode> _finalPath = null;               // 최종 경로

    private string _blockStr;                                   // 진출 불가 레이어 문자열 
                                                        
    private AStarNavNode[,] _map;                            // 가상 맵
    private int _mapMaxSizeX;                                // 가상 맵 X축 최대 사이즈 
    private int _mapMaxSizeY;                                // 가상 맵 Y축 최대 사이즈


    public EPathType PathType { get { return _pathType; } }

    public Vector2? LoadPath()                                                                  // 다음 진출 경로
    {
        if (_finalPath == null || _finalPath.Count == 0) return null;
        return _finalPath.Pop().Pos;
    }

    public bool FindPath(Vector2 mover, Vector2 target)                                         // 경로 검색 
    {
        if (_cellSize == Vector2.zero || _maxDistance == 0 || _blockLayers == null) return false;
        if (_blockStr == null) makingBlockingLayerString();

        _searchCandidate = new PriorityQueue_Custom<AStarNavNode>();
        _searchingNode = new Dictionary<Vector2Int, bool>();
        _targetNode = null;

        switch (PathType)
        {
            case EPathType.Grid:
                settingMapForGrid(mover, target);

                break;
            case EPathType.Dynamic:
                settingMapForDynamic(mover, target);

                break;
        }

        if (_targetNode == null) return false;
        if (_searchCandidate.IsEmpty) return false;
        
        return aStar();
    }

    private void settingMapForGrid(Vector2 mover, Vector2 target)                               // Grid 기반 검색일 때의 설정 세팅
    {
        if(_gridObject == null || _tileMapObject == null) return;  
        
        Grid grid = _gridObject.GetComponent<Grid>();
        Tilemap tileMap = _tileMapObject.GetComponent<Tilemap>();

        if (grid == null ||  tileMap == null) return;
        
        BoundsInt bounds = tileMap.cellBounds;

        Vector3Int min = bounds.min;  
        Vector3Int max = bounds.max;  
        bool isMoverSearching = true;

        _mapMaxSizeX = max.x - min.x;
        _mapMaxSizeY = max.y - min.y;
        _cellSize = grid.cellSize;
        _map = new AStarNavNode[_mapMaxSizeY, _mapMaxSizeX];

        foreach (Vector3Int cellPos in tileMap.cellBounds.allPositionsWithin)
        {
            if (!tileMap.HasTile(cellPos)) continue;
            
            Vector2Int mapArrayPos = new Vector2Int(cellPos.x - min.x, max.y - cellPos.y - 1);
            Vector2 pos = (Vector2)tileMap.GetCellCenterWorld(cellPos);

            float radius = (Mathf.Min(_cellSize.x, _cellSize.y) / 2) - _collisionBuffer;
            bool isBlocking = Physics2D.OverlapCircle(pos, radius, LayerMask.GetMask(_blockLayers));

            _map[mapArrayPos.y, mapArrayPos.x] = new AStarNavNode(new Vector2Int(mapArrayPos.x, mapArrayPos.y), pos, isBlocking);

            if (_targetNode == null && isInsideGrid(_map[mapArrayPos.y, mapArrayPos.x].Pos, target))
                _targetNode = _map[mapArrayPos.y, mapArrayPos.x];

            if(isMoverSearching && isInsideGrid(_map[mapArrayPos.y, mapArrayPos.x].Pos, mover))
            {
                _map[mapArrayPos.y, mapArrayPos.x].G = 0; 
                inputNode(_map[mapArrayPos.y, mapArrayPos.x]);
                isMoverSearching = false;
            }

            //positions.Add(new Vector2(pos.x, pos.y)); // test
        }
    }

    private void settingMapForDynamic(Vector2 mover, Vector2 target)                        // Dynamic 기반 검색일 때의 설정 세팅 
    {
        _mapMaxSizeX = _maxDistance * 2 + 1;
        _mapMaxSizeY = _maxDistance * 2 + 1;

        _map = new AStarNavNode[_mapMaxSizeY, _mapMaxSizeX];

        for (int y = 0; y < _mapMaxSizeY; y++)
        {
            for (int x = 0; x < _mapMaxSizeX; x++)
            {
                float posX = ((_maxDistance - x) * _cellSize.x) + mover.x;
                float posY = ((_maxDistance - y) * _cellSize.y) + mover.y;
                float radius = (Mathf.Min(_cellSize.x, _cellSize.y) / 2) - _collisionBuffer;
                bool isBlocking = Physics2D.OverlapCircle(new Vector2(posX, posY), radius, LayerMask.GetMask(_blockLayers));

                _map[y, x] = new AStarNavNode(new Vector2Int(x, y), new Vector2(posX, posY), isBlocking);

                if (_targetNode == null && isInsideGrid(_map[y, x].Pos, target))
                    _targetNode = _map[y, x];

                //positions.Add(new Vector2(posX, posY)); // test
            }
        }

        _map[_maxDistance, _maxDistance].G = 0; //_maxDistance는 계산상 전체 가상 맵 중 중앙을 의미 
        inputNode(_map[_maxDistance, _maxDistance]);
    }

    private bool aStar()                                                                         // A*
    {
        while (!_searchCandidate.IsEmpty)
        {
            AStarNavNode curNode = _searchCandidate.Dequeue();
            curNode.IsOpen = true;

            if (curNode == _targetNode)
            {
                _finalPath = new Stack<AStarNavNode>();
                
                while (curNode != null)
                {
                    curNode.ISLoad = true;
                    _finalPath.Push(curNode);
                    curNode = curNode.ParentNode;
                }

                return true;
            }

            straightLineCheck(curNode);
            if (_diagonalCheck) diagonalLineCheck(curNode);
        }
        
        return false;
    }

    private bool isInsideGrid(Vector2 pos, Vector2 target)                                          // AABB기반 포함 여부 확인 
    {
        float halfSizeX = _cellSize.x / 2;
        float halfSizeY = _cellSize.y / 2;

        return (
            target.x >= pos.x - halfSizeX &&
            target.x <= pos.x + halfSizeX &&
            target.y >= pos.y - halfSizeY &&
            target.y <= pos.y + halfSizeY
            );
    }

    private void straightLineCheck(AStarNavNode curNode)                                            // 직선 방향 진출
    {
        inputCandidateNode(curNode, curNode.GridIndex.x + 1, curNode.GridIndex.y, false);
        inputCandidateNode(curNode, curNode.GridIndex.x - 1, curNode.GridIndex.y, false);
        inputCandidateNode(curNode, curNode.GridIndex.x, curNode.GridIndex.y + 1, false);
        inputCandidateNode(curNode, curNode.GridIndex.x, curNode.GridIndex.y - 1, false);
    }

    private void diagonalLineCheck(AStarNavNode curNode)                                            // 대각선 방향 진출 
    {
        inputCandidateNode(curNode, curNode.GridIndex.x + 1, curNode.GridIndex.y + 1, true);
        inputCandidateNode(curNode, curNode.GridIndex.x + 1, curNode.GridIndex.y - 1, true);
        inputCandidateNode(curNode, curNode.GridIndex.x - 1, curNode.GridIndex.y + 1, true);
        inputCandidateNode(curNode, curNode.GridIndex.x - 1, curNode.GridIndex.y - 1, true);
    }

    private void inputCandidateNode(AStarNavNode curNode, int indexX, int indexY, bool _isDiagonal) // 현 노드 진출 가능성 검토 
    {
        if (indexX < 0 || _mapMaxSizeX <= indexX ||
            indexY < 0 || _mapMaxSizeY <= indexY ||
            _map[indexY, indexX] == null ||
            _map[indexY, indexX].IsBlocking ||
            _map[indexY, indexX].IsOpen) 
                return;
        
        if (_isDiagonal && (_map[curNode.GridIndex.y, indexX].IsBlocking || _map[indexY, curNode.GridIndex.x].IsBlocking)) 
            return;

        AStarNavNode neighborNode = _map[indexY, indexX];
        float moveCost = curNode.G + (_isDiagonal ? 1.4f : 1f);

        if(moveCost < neighborNode.G)
        {
            neighborNode.G = moveCost;
            neighborNode.H = (Mathf.Abs(indexX - _targetNode.GridIndex.x) + Mathf.Abs(indexY - _targetNode.GridIndex.y));
            neighborNode.ParentNode = curNode;
            
            if (!_searchingNode.ContainsKey(neighborNode.GridIndex))
                inputNode(neighborNode);  
        }
    }

    private void inputNode(AStarNavNode node)       // 우선 순위큐에 노드 삽입
    {
        if(_searchingNode == null || _searchCandidate == null) return;
        _searchCandidate.Enqueue(node);
        _searchingNode.TryAdd(node.GridIndex, true);
    }

    private void makingBlockingLayerString()        //필터 문자열 생성
    {
        _blockStr = string.Join(",", _blockLayers);
    }


    /*
     *
     *테스트 용
     
    public List<Vector2> positions = new List<Vector2>();  // 여러 개의 원의 중심 좌표
    

    public void Update()
    {
        if(_map == null) return;    
        foreach (AStarNavNode node in _map)
        {
            Color col = Color.green;
            if (node.IsBlocking) col = Color.red;
            if (node.ISLoad) col = Color.blue;

            DrawCircle(node.Pos, _cellSize.x / 2 - (_collisionBuffer), col);
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

    */
}
