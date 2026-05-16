using System.Collections.Generic;
using UnityEngine;

public enum DoorDirection
{
    Up,
    Down,
    Left,
    Right
}

public class DoorData
{
    public Vector2Int position;
    public DoorDirection direction;

    public DoorData(Vector2Int position, DoorDirection direction)
    {
        this.position = position;
        this.direction = direction;
    }
}

public class RoomData
{
    public RectInt area;
    public int type;

    public RoomData(RectInt rect, int type)
    {
        this.area = rect;
        this.type = type;
    }
}

public class RoomEdge
{
    public int from;
    public int to;
    public float distance;

    public RoomEdge(int from, int to, float distance)
    {
        this.from = from;
        this.to = to;
        this.distance = distance;
    }
}

public class DungeonGenerator
{
    public Dictionary<Vector2Int, int> floorTileData =
        new Dictionary<Vector2Int, int>();

    public Dictionary<Vector2Int, int> wallTileData =
        new Dictionary<Vector2Int, int>();

    public List<DoorData> doorTileData =
        new List<DoorData>();

    public List<RoomData> rooms =
        new List<RoomData>();

    public DungeonGenerator(
        int roomCount,
        int minSize,
        int maxSize,
        int maxTypes
    )
    {
        float currentRange = 5f;
        int attempts = 0;

        while (rooms.Count < roomCount && attempts < 10000)
        {
            attempts++;

            // 원형 범위 안 랜덤 위치 생성
            float angle =
                Random.Range(0, Mathf.PI * 2);

            float radius =
                Random.Range(0, currentRange);

            int x =
                Mathf.RoundToInt(
                    Mathf.Cos(angle) * radius
                );

            int y =
                Mathf.RoundToInt(
                    Mathf.Sin(angle) * radius
                );

            // 방 크기 생성
            int w =
                Random.Range(minSize, maxSize + 1);

            int h =
                Random.Range(minSize, maxSize + 1);

            RectInt rect =
                new RectInt(x, y, w, h);

            bool overlap = false;

            foreach (var r in rooms)
            {
                // 방끼리 너무 가까워지지 않게 여유 공간 포함 검사
                if (rect.Overlaps(
                    new RectInt(
                        r.area.x - 8,
                        r.area.y - 8,
                        r.area.width + 16,
                        r.area.height + 16
                    )))
                {
                    overlap = true;
                    break;
                }
            }

            // 겹치지 않을 때만 방 추가
            if (!overlap)
            {
                int type =
                    (maxTypes > 1)
                    ? Random.Range(1, maxTypes)
                    : 0;

                rooms.Add(
                    new RoomData(rect, type)
                );
            }

            currentRange += 0.2f;
        }

        // Prim 기반 MST 연결
        ConnectRoomsPrim();

        // 방 바닥 생성
        foreach (var room in rooms)
        {
            for (
                int rx = room.area.xMin;
                rx < room.area.xMax;
                rx++
            )
            {
                for (
                    int ry = room.area.yMin;
                    ry < room.area.yMax;
                    ry++
                )
                {
                    floorTileData[
                        new Vector2Int(rx, ry)
                    ] = room.type;
                }
            }
        }

        // 벽 생성
        BuildWallsWithTypes();
    }

    void ConnectRoomsPrim()
    {
        if (rooms.Count < 2)
            return;

        // 연결된 방 체크
        bool[] visited =
            new bool[rooms.Count];

        // 후보 간선 저장
        List<RoomEdge> candidateEdges =
            new List<RoomEdge>();

        // 시작 방
        visited[0] = true;

        int connectedCount = 1;

        // 시작 방 간선 추가
        AddCandidateEdges(
            0,
            visited,
            candidateEdges
        );

        while (
            connectedCount < rooms.Count &&
            candidateEdges.Count > 0
        )
        {
            // 가장 짧은 간선 찾기
            RoomEdge bestEdge =
                candidateEdges[0];

            foreach (var edge in candidateEdges)
            {
                if (
                    edge.distance <
                    bestEdge.distance
                )
                {
                    bestEdge = edge;
                }
            }

            // 사용한 간선 제거
            candidateEdges.Remove(bestEdge);

            // 이미 연결된 방이면 무시
            if (visited[bestEdge.to])
                continue;

            // 복도 생성
            CreateCorridor(
                rooms[bestEdge.from].area,
                rooms[bestEdge.to].area,
                0
            );

            // 연결 처리
            visited[bestEdge.to] = true;

            connectedCount++;

            // 새 방 기준 후보 간선 추가
            AddCandidateEdges(
                bestEdge.to,
                visited,
                candidateEdges
            );
        }
    }

    void AddCandidateEdges(
        int roomIndex,
        bool[] visited,
        List<RoomEdge> candidateEdges
    )
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (visited[i])
                continue;

            // 일자 복도 가능 여부 검사
            if (!CanMakeStraightCorridor(
                rooms[roomIndex].area,
                rooms[i].area
            ))
            {
                continue;
            }

            // 방 중심 거리 계산
            float distance =
                Vector2.Distance(
                    rooms[roomIndex].area.center,
                    rooms[i].area.center
                );

            RoomEdge edge =
                new RoomEdge(
                    roomIndex,
                    i,
                    distance
                );

            candidateEdges.Add(edge);
        }
    }

    void CreateCorridor(
        RectInt roomA,
        RectInt roomB,
        int type
    )
    {
        // 좌우 복도 생성
        if (RangesOverlap(
            roomA.yMin + 1,
            roomA.yMax - 2,
            roomB.yMin + 1,
            roomB.yMax - 2
        ))
        {
            int y =
                GetOverlapMiddle(
                    roomA.yMin + 1,
                    roomA.yMax - 2,
                    roomB.yMin + 1,
                    roomB.yMax - 2
                );

            int startX;
            int endX;

            if (roomA.center.x < roomB.center.x)
            {
                startX = roomA.xMax;
                endX = roomB.xMin - 1;

                doorTileData.Add(
                    new DoorData(
                        new Vector2Int(roomA.xMax, y),
                        DoorDirection.Right
                    )
                );

                doorTileData.Add(
                    new DoorData(
                        new Vector2Int(roomB.xMin - 1, y),
                        DoorDirection.Left
                    )
                );
            }
            else
            {
                startX = roomB.xMax;
                endX = roomA.xMin - 1;

                doorTileData.Add(
                    new DoorData(
                        new Vector2Int(roomB.xMax, y),
                        DoorDirection.Right
                    )
                );

                doorTileData.Add(
                    new DoorData(
                        new Vector2Int(roomA.xMin - 1, y),
                        DoorDirection.Left
                    )
                );
            }

            for (
                int x = Mathf.Min(startX, endX);
                x <= Mathf.Max(startX, endX);
                x++
            )
            {
                floorTileData[
                    new Vector2Int(x, y)
                ] = type;
            }

            return;
        }

        // 상하 복도 생성
        if (RangesOverlap(
            roomA.xMin + 1,
            roomA.xMax - 2,
            roomB.xMin + 1,
            roomB.xMax - 2
        ))
        {
            int x =
                GetOverlapMiddle(
                    roomA.xMin + 1,
                    roomA.xMax - 2,
                    roomB.xMin + 1,
                    roomB.xMax - 2
                );

            int startY;
            int endY;

            if (roomA.center.y < roomB.center.y)
            {
                startY = roomA.yMax;
                endY = roomB.yMin - 1;

                doorTileData.Add(
                    new DoorData(
                        new Vector2Int(x, roomA.yMax),
                        DoorDirection.Up
                    )
                );

                doorTileData.Add(
                    new DoorData(
                        new Vector2Int(x, roomB.yMin - 1),
                        DoorDirection.Down
                    )
                );
            }
            else
            {
                startY = roomB.yMax;
                endY = roomA.yMin - 1;

                doorTileData.Add(
                    new DoorData(
                        new Vector2Int(x, roomB.yMax),
                        DoorDirection.Up
                    )
                );

                doorTileData.Add(
                    new DoorData(
                        new Vector2Int(x, roomA.yMin - 1),
                        DoorDirection.Down
                    )
                );
            }

            for (
                int y = Mathf.Min(startY, endY);
                y <= Mathf.Max(startY, endY);
                y++
            )
            {
                floorTileData[
                    new Vector2Int(x, y)
                ] = type;
            }

            return;
        }
    }

    bool CanMakeStraightCorridor(
        RectInt roomA,
        RectInt roomB
    )
    {
        bool verticalOverlap =
            RangesOverlap(
                roomA.yMin + 1,
                roomA.yMax - 2,
                roomB.yMin + 1,
                roomB.yMax - 2
            );

        bool horizontalOverlap =
            RangesOverlap(
                roomA.xMin + 1,
                roomA.xMax - 2,
                roomB.xMin + 1,
                roomB.xMax - 2
            );

        return
            verticalOverlap ||
            horizontalOverlap;
    }

    bool RangesOverlap(
        int minA,
        int maxA,
        int minB,
        int maxB
    )
    {
        return
            minA <= maxB &&
            minB <= maxA;
    }

    int GetOverlapMiddle(
        int minA,
        int maxA,
        int minB,
        int maxB
    )
    {
        int overlapMin =
            Mathf.Max(minA, minB);

        int overlapMax =
            Mathf.Min(maxA, maxB);

        return
            (overlapMin + overlapMax) / 2;
    }

    void BuildWallsWithTypes()
    {
        foreach (var entry in floorTileData)
        {
            Vector2Int pos = entry.Key;

            int type = entry.Value;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int neighbor =
                        new Vector2Int(
                            pos.x + x,
                            pos.y + y
                        );

                    if (
                        !floorTileData.ContainsKey(
                            neighbor
                        )
                    )
                    {
                        wallTileData[
                            neighbor
                        ] = type;
                    }
                }
            }
        }
    }
}