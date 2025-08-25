using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HouseCreator : MonoBehaviour
{
    [SerializeField]
    GameObject _prefab;

    [SerializeField]
    GameObject _floorPrefab;

    [SerializeField]
    GameObject _innerWallPrefab;

    [SerializeField]
    GameObject _innerDoorPrefab;

    [SerializeField]
    GameObject _outerWallPrefab;

    [SerializeField]
    GameObject _parent;

    [SerializeField]
    HouseObjectDataBase _houseObjectDataBase;

    const float _interval = 2.32f;

    Vector2Int _houseAreaSize;

    List<List<HouseData>> _houseDataList;
    List<List<Vector2Int>> _roomPositionList;


    public IEnumerator CreateHouse()
    {
        _houseAreaSize = new Vector2Int(20, 10);
        _houseDataList = new List<List<HouseData>>();
        _roomPositionList = new List<List<Vector2Int>>();

        // リストの初期化
        for (int z = 0; z < _houseAreaSize.y; z++)
        {
            _houseDataList.Add(new List<HouseData>());
            for (int x = 0; x < _houseAreaSize.x; x++)
            {
                _houseDataList[z].Add(new HouseData());
            }
        }

        // 要素の全削除
        int childNum = _parent.transform.childCount;
        for (int i = 0; i < childNum; i++)
        {
            DestroyImmediate(_parent.transform.GetChild(0).gameObject);
        }


        // 部屋の配置
        int id = 0;
        float roomDensity = 0;
        while (roomDensity < 0.50f)
        {
            // 部屋密度
            int areaCount = 0;
            int roomAreaCount = 0;
            for (int y = 0; y < _houseAreaSize.y; y++)
            {
                for (int x = 0; x < _houseAreaSize.x; x++)
                {
                    if (_houseDataList[y][x].id != -1)
                    {
                        roomAreaCount++;
                    }
                    areaCount++;
                }
            }
            roomDensity = (float)roomAreaCount / areaCount;

            Vector2Int roomPosition = new Vector2Int(Random.Range(0, _houseAreaSize.x - 1), Random.Range(0, _houseAreaSize.y - 1));
            int roomSizeX = Random.Range(2, _houseAreaSize.x - roomPosition.x);
            int roomSizeY;
            int roomSizeY_min = roomSizeX / 2;
            if (roomSizeY_min < 2)
            {
                roomSizeY_min = 2;
            }
            if (roomSizeY_min >= Mathf.Min(roomSizeX * 2, _houseAreaSize.y - roomPosition.y))
            {
                roomSizeY = Mathf.Min(roomSizeX * 2, _houseAreaSize.y - roomPosition.y);
            }
            else
            {
                roomSizeY = Random.Range(roomSizeY_min, Mathf.Min(roomSizeX * 2, _houseAreaSize.y - roomPosition.y));
            }
            Vector2Int roomSize = new Vector2Int(roomSizeX, roomSizeY);
            for (int y = roomPosition.y; y < roomPosition.y + roomSize.y; y++)
            {
                for (int x = roomPosition.x; x < roomPosition.x + roomSize.x; x++)
                {
                    if (_houseDataList[y][x].type == RoomType.Corridor)
                    {
                        _houseDataList[y][x].id = id;
                        _houseDataList[y][x].wallType = WallType.Wall;
                        _houseDataList[y][x].type = RoomType.None;
                    }
                    else
                    {
                        continue;
                    }

                    if (x == roomPosition.x || x == roomPosition.x + roomSize.x - 1 ||
                        y == roomPosition.y || y == roomPosition.y + roomSize.y - 1)
                    {
                        //Instantiate(_prefab, new Vector3(x, 0, y) * _interval, Quaternion.identity, _parent.transform);
                        yield return null;
                    }
                }
            }
            id++;
            
            yield return null;
        }

        // 部屋の分類
        ClassifyRoom();

        // 扉の生成
        foreach (List<Vector2Int> vectorList in _roomPositionList)
        {
            List<int> indexList = new List<int>();
            for (int i = 0; i < vectorList.Count; i++)
            {
                Vector2Int vector = vectorList[i];

                if (_houseDataList.GetValue(vector + new Vector2Int(-1, 0), null)?.type != _houseDataList[vector.y][vector.x].type ||
                    _houseDataList.GetValue(vector + new Vector2Int(1, 0), null)?.type != _houseDataList[vector.y][vector.x].type ||
                    _houseDataList.GetValue(vector + new Vector2Int(0, -1), null)?.type != _houseDataList[vector.y][vector.x].type ||
                    _houseDataList.GetValue(vector + new Vector2Int(0, 1), null)?.type != _houseDataList[vector.y][vector.x].type)
                {
                    indexList.Add(i);
                }
            }

            int index = indexList[Random.Range(0, indexList.Count)];
            _houseDataList[vectorList[index].y][vectorList[index].x].wallType = WallType.Door;
        }

        // 内壁の生成
        for (int y = 0; y < _houseAreaSize.y; y++)
        {
            for (int x = 0; x < _houseAreaSize.x; x++)
            {
                CreateInnerWall(x, y);

                yield return null;
            }
        }

        // 外壁
        for (int y = 0; y < _houseAreaSize.y; y++)
        {
            for (int x = 0; x < _houseAreaSize.x; x++)
            {
                HouseData houseData = _houseDataList[y][x];

                if (x == 0 || x == _houseAreaSize.x - 1 ||
                    y == 0 || y == _houseAreaSize.y - 1)
                {
                    //Instantiate(_prefab, new Vector3(x, 0, y) * _interval, Quaternion.identity, _parent.transform);

                    // 壁の配置

                    // 外壁
                    if (y == 0)
                    {
                        Instantiate(_outerWallPrefab, new Vector3(x, 0, y - 0.5f) * _interval, Quaternion.AngleAxis(180, Vector3.up), _parent.transform);
                    }
                    else if (y == _houseAreaSize.y - 1)
                    {
                        Instantiate(_outerWallPrefab, new Vector3(x, 0, y + 0.5f) * _interval, Quaternion.identity, _parent.transform);
                    }

                    if (x == 0)
                    {
                        Instantiate(_outerWallPrefab, new Vector3(x - 0.5f, 0, y) * _interval, Quaternion.AngleAxis(-90, Vector3.up), _parent.transform);
                    }
                    else if (x == _houseAreaSize.x - 1)
                    {
                        Instantiate(_outerWallPrefab, new Vector3(x + 0.5f, 0, y) * _interval, Quaternion.AngleAxis(90, Vector3.up), _parent.transform);
                    }

                    yield return null;
                }
            }
        }


        List<(GameObject houseObject, HouseObjectData houseObjectData)> putHouseObjectDataList = new List<(GameObject houseObject, HouseObjectData houseObjectData)>();

        // 家具の配置
        for (int i = 0; i < _houseObjectDataBase.HouseObjectDataList.Count; i++)
        {
            HouseObjectData houseObjectData = _houseObjectDataBase.HouseObjectDataList[i];

            // ランダムな位置に配置
            Vector2 position = _roomPositionList[0][Random.Range(0, _roomPositionList[0].Count)];
            GameObject houseObject = Instantiate(houseObjectData.Prefab, position.ToVector3XZ() * _interval, Quaternion.identity, _parent.transform);

            putHouseObjectDataList.Add((houseObject, houseObjectData));

            // 壁との距離評価
            float currentEvaluation = 0;
            while (true)
            {
                currentEvaluation = CalculateWallEvaluation(houseObject, houseObject.transform.position, houseObjectData.Wall);
                currentEvaluation += CalculateRelationshipEvaluation(houseObject.transform.position);

                // 移動後の評価を計算し、高くなる所へ移動
                const float MOVING_VALUE = 0.1f;
                float maxEvaluation = float.NegativeInfinity;
                Vector3 direction = Vector3.zero;
                for (float x = -MOVING_VALUE; x <= MOVING_VALUE; x += MOVING_VALUE)
                {
                    for (float y = -MOVING_VALUE; y <= MOVING_VALUE; y += MOVING_VALUE)
                    {
                        if (!(x == 0 && y == 0))
                        {
                            Vector3 additionalPosition = new Vector3(x, 0, y);
                            float evaluation = 0;
                            evaluation = CalculateWallEvaluation(houseObject, houseObject.transform.position + additionalPosition, houseObjectData.Wall);
                            evaluation += CalculateRelationshipEvaluation(houseObject.transform.position + additionalPosition);
                            //Debug.Log($"Dir: {additionalPosition}, Eval: {evaluation}");
                            if (evaluation > maxEvaluation)
                            {
                                maxEvaluation = evaluation;
                                direction = additionalPosition.normalized;
                            }
                        }
                    }
                }

                //Debug.Log($"Current: {currentEvaluation}, Max: {maxEvaluation}");
                if (currentEvaluation >= maxEvaluation)
                {
                    break;
                }

                // 実際に移動
                houseObject.transform.position += direction * MOVING_VALUE;

                yield return new WaitForSecondsRealtime(0.01f);
            }

            float CalculateRelationshipEvaluation(Vector3 position)
            {
                float _evaluation = 0;

                // 対象のオブジェクトとの距離を利用
                HouseObjectData putHouseObjectData = putHouseObjectDataList[0].houseObjectData;
                if (houseObjectData.RelationshipList.Select(x => x.houseObjectData).Contains(putHouseObjectData))
                {
                    Vector3 putHouseObjectPosition = putHouseObjectDataList[0].houseObject.transform.position;
                    float distance = Vector3.Distance(position, putHouseObjectPosition);
                    _evaluation = 1 / distance;

                    Debug.DrawRay(position, (putHouseObjectPosition - position).normalized * distance, Color.green, Time.deltaTime);
                }


                // 他のオブジェクトにめり込んでいる場合、めり込んでいるほど評価を下げる
                houseObject.SetLayer(LayerMask.NameToLayer("Ignore Raycast"));
                Vector3 scale = houseObject.transform.localScale;
                BoxCollider boxCollider = houseObject.GetComponentInChildren<BoxCollider>();
                Collider[] colliders;
                if ((colliders = Physics.OverlapBox(position + boxCollider.center.Times(scale), boxCollider.size.Times(scale) / 2, Quaternion.identity, ~(1 << LayerMask.NameToLayer("Ignore Raycast")))).Length > 0)
                {
                    foreach (Collider collider in colliders)
                    {
                        _evaluation -= 100 / Vector3.Distance(position, collider.transform.position);
                    }
                }
                houseObject.SetLayer(0);

                return _evaluation;
            }
        }


        // 床の生成
        for (int y = 0; y < _houseAreaSize.y; y++)
        {
            for (int x = 0; x < _houseAreaSize.x; x++)
            {
                Instantiate(_floorPrefab, new Vector3(x, 0, y) * _interval, Quaternion.identity, _parent.transform);
            }
        }

        Debug.Log("生成完了！");
    }

    void CreateInnerWall(int x, int y)
    {
        HouseData houseData = _houseDataList[y][x];

        Vector2Int position = new Vector2Int(y, x);
        for (int i = 0; i <= 1; i++)
        {
            for (int j = 0; j <= 1; j++)
            {
                if (!((i == 0 || j == 0) && i != j))
                {
                    continue;
                }

                HouseData h = _houseDataList.GetValue(position + new Vector2Int(j, i), null);
                if (h == null) continue;
                
                if (h.id == houseData.id)
                {
                    continue;
                }

                GameObject prefab = _innerWallPrefab;
                if (houseData.wallType == WallType.Door || h.wallType == WallType.Door)
                {
                    prefab = _innerDoorPrefab;
                }
                Instantiate(prefab, (new Vector3(position.y, 0, position.x) + new Vector3(i, 0, j) * 0.5f) * _interval, Quaternion.AngleAxis(i * 90, Vector3.up), _parent.transform);
            }
        }
    }

    void ClassifyRoom()
    {
        // スタート位置の検索
        Vector2Int startPosition = Vector2Int.zero;

        while (true)
        {
            for (int y = 0; y < _houseAreaSize.y; y++)
            {
                for (int x = 0; x < _houseAreaSize.x; x++)
                {
                    if (_houseDataList[y][x].type == RoomType.None)
                    {
                        startPosition = new Vector2Int(x, y);
                        goto CHECK;
                    }
                }
            }

            break;

        CHECK:
            List<Vector2Int> openedPositionList = new List<Vector2Int>();
            List<Vector2Int> closedPositionList = new List<Vector2Int>();
            closedPositionList.Add(startPosition);

            while (closedPositionList.Count > 0)
            //int j = 0;
            //while (j < 7)
            {
                Vector2Int checkingPosition = closedPositionList[0];
                closedPositionList.RemoveAt(0);
                openedPositionList.Add(checkingPosition);

                // 周囲のマスを調べる
                int id = _houseDataList[checkingPosition.y][checkingPosition.x].id;
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if ((x == 0 || y == 0) && x != y)
                        {
                            Vector2Int aroundPosition = new Vector2Int(checkingPosition.x + x, checkingPosition.y + y);
                            if (aroundPosition.x < 0 || aroundPosition.x >= _houseAreaSize.x ||
                                aroundPosition.y < 0 || aroundPosition.y >= _houseAreaSize.y)
                            {
                                continue;
                            }
                            if (_houseDataList[aroundPosition.y][aroundPosition.x].id == id &&
                                !openedPositionList.Contains(aroundPosition) &&
                                !closedPositionList.Contains(aroundPosition))
                            {
                                closedPositionList.Add(aroundPosition);
                            }
                        }
                    }
                }
                //Debug.Log(closedPositionList.ToStringWithCommma());
                //j++;
            }

            // 部屋の種類の決定
            RoomType roomType = (RoomType)Random.Range(2, 16);

            Color color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f), 1);

            //Debug.Log(openedPositionList.Count +", " + roomType);
            foreach (Vector2Int position in openedPositionList)
            {
                _houseDataList[position.y][position.x].type = roomType;

                //GameObject cube = Instantiate(_prefab, new Vector3(position.x, 0, position.y) * _interval, Quaternion.identity, _parent.transform);
                /*MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();
                Material material = new Material(meshRenderer.material);
                material.color = color;
                meshRenderer.material = material;*/
            }

            _roomPositionList.Add(new List<Vector2Int>(openedPositionList));
        }
    }

    float CalculateWallEvaluation(GameObject houseObject, Vector3 position, float weight)
    {
        float _evaluation = 0;

        // 四方にRayを飛ばし、距離を計測
        houseObject.SetLayer(LayerMask.NameToLayer("Ignore Raycast"));
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((x == 0 || y == 0) && x != y)
                {
                    Ray ray = new Ray(position.AddY(0.1f), new Vector3(x, 0, y));
                    if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, ~(1 << LayerMask.NameToLayer("Ignore Raycast"))))
                    {
                        float distance = Vector3.Distance(position, hitInfo.transform.position);
                        _evaluation += weight * 1 / distance;
                        //Debug.Log($"Dir: {new Vector3(x, 0, y)}, Distance: {distance}, Eval: {_evaluation}");
                        Debug.DrawRay(position, ray.direction * distance, Color.blue, Time.deltaTime);
                    }
                }
            }
        }

        // 他のオブジェクトにめり込んでいる場合、めり込んでいるほど評価を下げる
        Vector3 scale = houseObject.transform.localScale;
        BoxCollider boxCollider = houseObject.GetComponentInChildren<BoxCollider>();
        Collider[] colliders;
        if ((colliders = Physics.OverlapBox(position + boxCollider.center.Times(scale), boxCollider.size.Times(scale) / 2, Quaternion.identity, ~(1 << LayerMask.NameToLayer("Ignore Raycast")))).Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                _evaluation -= 100 / Vector3.Distance(position, collider.transform.position);
            }
        }

        houseObject.SetLayer(0);

        return _evaluation;
    }

    enum WallType
    {
        None,
        Wall,
        Door
    }

    enum RoomType
    {
        None,
        DeadSpace,
        Corridor,
        LivingRoom,
        DiningRoom,
        Kitchen,
        Pantry,
        Sunroom,
        Bedroom,
        GuestRoom,
        Office,
        Library,
        DressingRoom,
        Bathroom,
        Restroom,
        LaundryRoom,
    }

    class HouseData
    {
        public int id = -1;
        public WallType wallType = WallType.None;
        public RoomType type = RoomType.Corridor;
    }
}
