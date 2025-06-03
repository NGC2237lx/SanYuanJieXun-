using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("基本设置")]
    public int gridSize = 5;
    public float columnSpacing = 40f;
    public float rowSpacing = 20f;
    public Transform mapParent;

    [Header("主角设置")]
    public Transform playerTransform;

    [Header("房间预制体")]
    public GameObject startRoom;
    public GameObject endRoom;
    public List<GameObject> roomPrefabs = new List<GameObject>();

    [Header("补全房间预制体")]
    public GameObject fillUp;
    public GameObject fillDown;
    public GameObject fillLeft;
    public GameObject fillRight;

    private GameObject[,] roomGrid;
    private enum RoomType { None, Start, End, Normal }
    private class RoomData
    {
        public RoomType type;
        public GameObject prefab;
    }

    private RoomData[,] dungeonMap;

    void Start()
    {
        GenerateDungeon();
        PositionPlayerAtStart();
    }

    public void GenerateDungeon()
    {
        dungeonMap = new RoomData[gridSize, gridSize];
        roomGrid = new GameObject[gridSize, gridSize];

        Vector2Int startPos = Vector2Int.zero;
        Vector2Int endPos = Vector2Int.zero;
        int maxDistance = -1;

        for (int i = 0; i < 20; i++)
        {
            Vector2Int a = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            Vector2Int b = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            int dist = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
            if (a != b && dist > maxDistance)
            {
                startPos = a;
                endPos = b;
                maxDistance = dist;
            }
        }

        List<Vector2Int> mainPath = GeneratePath(startPos, endPos);

        dungeonMap[startPos.x, startPos.y] = new RoomData { type = RoomType.Start, prefab = startRoom };
        dungeonMap[endPos.x, endPos.y] = new RoomData { type = RoomType.End, prefab = endRoom };

        foreach (var pos in mainPath)
        {
            if (dungeonMap[pos.x, pos.y] == null)
            {
                dungeonMap[pos.x, pos.y] = new RoomData
                {
                    type = RoomType.Normal,
                    prefab = GetRandomRoom()
                };
            }
        }

        int sideRooms = Random.Range(1, 3);
        int attempts = 0;

        while (sideRooms > 0 && attempts < 100)
        {
            var basePos = mainPath[Random.Range(0, mainPath.Count)];
            var offset = GetRandomDirection();
            var newPos = basePos + offset;

            if (IsInBounds(newPos) && dungeonMap[newPos.x, newPos.y] == null)
            {
                dungeonMap[newPos.x, newPos.y] = new RoomData
                {
                    type = RoomType.Normal,
                    prefab = GetRandomRoom()
                };
                sideRooms--;
            }
            attempts++;
        }

        // 实例化主地图房间
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var room = dungeonMap[x, y];
                if (room != null && room.prefab != null)
                {
                    Vector3 pos = new Vector3(x * columnSpacing, y * rowSpacing, 0);
                    GameObject roomObj = Instantiate(room.prefab, pos, Quaternion.identity, mapParent);
                    roomGrid[x, y] = roomObj;
                    roomObj.name = $"Room_{x}_{y}_{room.type}";
                }
            }
        }

        // 生成边界填补房间
        FillVoidEdges();

        Debug.Log("地图生成完成");
    }

    private void FillVoidEdges()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (dungeonMap[x, y] != null)
                {
                    Vector3 basePos = new Vector3(x * columnSpacing, y * rowSpacing, 0);

                    // 上
                    if (!IsValidRoom(x, y + 1) && fillUp != null)
                        Instantiate(fillUp, basePos + new Vector3(0, rowSpacing, 0), Quaternion.identity, mapParent);

                    // 下
                    if (!IsValidRoom(x, y - 1) && fillDown != null)
                        Instantiate(fillDown, basePos + new Vector3(0, -rowSpacing, 0), Quaternion.identity, mapParent);

                    // 左
                    if (!IsValidRoom(x - 1, y) && fillLeft != null)
                        Instantiate(fillLeft, basePos + new Vector3(-columnSpacing, 0, 0), Quaternion.identity, mapParent);

                    // 右
                    if (!IsValidRoom(x + 1, y) && fillRight != null)
                        Instantiate(fillRight, basePos + new Vector3(columnSpacing, 0, 0), Quaternion.identity, mapParent);
                }
            }
        }
    }

    // 判断是否为合法已生成房间
    private bool IsValidRoom(int x, int y)
    {
        if (x < 0 || x >= gridSize || y < 0 || y >= gridSize)
            return false;
        return dungeonMap[x, y] != null;
    }


    private List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        path.Add(start);
        DFS(start, end, path, visited);
        return path;
    }

    private bool DFS(Vector2Int current, Vector2Int end, List<Vector2Int> path, HashSet<Vector2Int> visited)
    {
        if (current == end) return true;
        visited.Add(current);

        var directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Shuffle(directions);

        foreach (var dir in directions)
        {
            Vector2Int next = current + dir;
            if (IsInBounds(next) && !visited.Contains(next))
            {
                path.Add(next);
                if (DFS(next, end, path, visited))
                    return true;
                path.RemoveAt(path.Count - 1);
            }
        }
        return false;
    }

    private GameObject GetRandomRoom()
    {
        if (roomPrefabs == null || roomPrefabs.Count == 0)
        {
            Debug.LogWarning("未设置房间Prefab");
            return null;
        }
        return roomPrefabs[Random.Range(0, roomPrefabs.Count)];
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize;
    }

    private Vector2Int GetRandomDirection()
    {
        var dirs = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        return dirs[Random.Range(0, dirs.Count)];
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    private void PositionPlayerAtStart()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (dungeonMap[x, y] != null && dungeonMap[x, y].type == RoomType.Start)
                {
                    GameObject startRoomObj = roomGrid[x, y];
                    if (startRoomObj == null)
                    {
                        Debug.LogWarning("Start 房间对象未正确生成！");
                        return;
                    }

                    Transform birthplace = startRoomObj.transform.Find("birthplace");
                    if (birthplace != null && playerTransform != null)
                    {
                        playerTransform.position = birthplace.position;
                        Debug.Log("主角已移动到出生点：" + birthplace.position);
                    }
                    else
                    {
                        Debug.LogWarning("未找到 'birthplace' 子对象，或未设置 playerTransform！");
                    }
                    return;
                }
            }
        }
        Debug.LogWarning("未找到 Start 房间！");
    }
}
