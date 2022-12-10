using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class Puzzle : MonoBehaviour
{
    [SerializeField] private Transform m_pieceContainer;
    [SerializeField] private TileObject m_tilePrefab;

    public Grid grid => GetComponent<Grid>();
    public readonly Dictionary<Vector3Int, TileObject> pieceMap = new Dictionary<Vector3Int, TileObject>();
    public Plane plane
    {
        get
        {
            if (grid.cellSwizzle == GridLayout.CellSwizzle.XYZ)
                return new Plane(grid.transform.forward, transform.position);
            else
                return new Plane(grid.transform.up, transform.position);
        }
    }
    public Vector3Int empty { get; private set; }
    public readonly UnityEvent onCleared = new UnityEvent();
    public Bounds bounds = new Bounds();

    public int Width = 3;
    public int PieceCount => Width * Width - 1;

    public void CreateSquare(int width)
    {
        Width = width;

        for (var i = m_pieceContainer.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(m_pieceContainer.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < PieceCount; i++)
        {
#if UNITY_EDITOR
            TileObject piece = UnityEditor.PrefabUtility.InstantiatePrefab(m_tilePrefab, m_pieceContainer.transform) as TileObject;
#else
            TileObject piece = Instantiate<TileObject>(m_tilePrefab, m_pieceContainer.transform);
#endif
            piece.SetNumber(i + 1);
            piece.transform.localPosition = grid.CellToLocalCenter(IndexToVector3Int(i, Width));
        }

        empty = IndexToVector3Int(PieceCount, Width);

        pieceMap.Clear();

        Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        foreach (Transform piece in m_pieceContainer)
        {
            Vector3Int cell = grid.LocalToCell(piece.localPosition);
            pieceMap.Add(cell, piece.GetComponent<TileObject>());

            min = Vector3Int.Min(min, cell);
            max = Vector3Int.Max(max, cell);
        }

        Vector3 thickness = grid.CellToWorld(Vector3Int.forward);
        bounds.SetMinMax(grid.CellToWorld(min), grid.CellToWorld(max + new Vector3Int(1, 1, 0)));

        // スケール調整
        Fit();

        // 画面中央に移動
        Vector3 center = 0.5f * (grid.CellToLocal(min) + grid.CellToLocal(max + new Vector3Int(1, 1, 0)));
        transform.localPosition = new Vector3(-center.x * transform.localScale.x, -center.y * transform.localScale.y, 0);
    }

    private static Vector3Int IndexToVector3Int(int i, int size)
    {
        return new Vector3Int(i % size, (size - 1) - i / size, 0);
    }

    public bool Cleared()
    {
        for (int i = 0; i < PieceCount; i++)
        {
            Vector3Int cell = new Vector3Int(i % Width, (Width - 1) - i / Width, 0);

            if (!pieceMap.ContainsKey(cell) || pieceMap[cell].number != i + 1)
                return false;
        }

        return true;
    }

    public bool TrySlide(Vector3Int from, Vector3Int to, bool checkClear = true)
    {
        if (!pieceMap.ContainsKey(from))
            return false;

        // 上下左右のみ動かせる
        if (Vector3IntExtension.ManhattanDistance(from, to) != 1)
            return false;

        pieceMap[to] = pieceMap[from];
        pieceMap.Remove(from);
        empty = from;

        if (checkClear && Cleared())
            onCleared.Invoke();

        return true;
    }

    private static readonly List<TileObject> shuffleList = new List<TileObject>(); // shuffleList[i]のタイルはiに移動することを意味する
    private static readonly List<int> tempList = new List<int>();

    public static List<int> Shuffle(List<int> list, int l)
    {
        list.Clear();

        for (int src = 0; src < l; src++)
        {
            list.Add(src);
        }

        // Fisher–Yates Shuffle
        for (int src = 0; src < l; src++) // 前方から確定させていく
        {
            int dst = Random.Range(src, l - 1);
            int t = list[dst];
            list[dst] = list[src];
            list[src] = t;
        }

        if (!CheckSolutionExistance(list))
        {
            int src = 0;
            int dst = (src + 1) % l;

            int t = list[dst];
            list[dst] = list[src];
            list[src] = t;
        }

        return list;
    }

    public IEnumerator ShuffleSimple(UnityAction onComplete)
    {
        shuffleList.Clear();
        shuffleList.AddRange(pieceMap.Values);
        shuffleList.Add(null);
        tempList.Clear();
        pieceMap.Clear();

        int l = shuffleList.Count;

        Shuffle(tempList, l);

        // Move Tiles
        for (int i = 0; i < l; i++)
        {
            TileObject tile = shuffleList[i];
            Vector3Int dst = IndexToVector3Int(tempList[i], Width);

            if (tile == null)
            {
                empty = dst;
                continue;
            }

            tile.Move(grid.CellToLocalCenter(dst), 0.7f);
            pieceMap.Add(dst, tile);
        }

        yield return new WaitForSeconds(0.7f);
        onComplete.Invoke();
    }

    private static readonly List<int> tl = new List<int>();

    public static bool CheckSolutionExistance(List<int> list)
    {
        tl.Clear();
        tl.AddRange(list);

        int width = tl.Count / 3;
        int emptyDistance = -2; // tempList[0]==l-1(<=>左上が空白)の時は常に偶数なので計算しない
        int parity = 0;
        int l = tl.Count;

        for (int i = 0; i < l - 1; i++)
        {
            if (tl[i] == i) continue;

            for (int j = i + 1; j < l; j++)
            {
                if (tl[j] == l - 1 && emptyDistance == -2)
                {
                    emptyDistance = Vector3IntExtension.ManhattanDistance(IndexToVector3Int(j, width), IndexToVector3Int(l - 1, width));
                }

                if (tl[j] == i)
                {
                    tl[j] = tl[i];
                    tl[i] = i;

                    parity++;
                }
            }
        }

        return parity % 2 == emptyDistance % 2;
    }

    private RectTransform m_area = null;
    private Camera m_camera = null;

    public void Fit()
    {
        m_camera = Camera.main;
        m_area = GameObject.Find("GameArea").GetComponent<RectTransform>();

        m_camera.transform.position = -m_camera.transform.forward;
        var t = new Bounds(Vector3.zero, bounds.size);
        transform.localScale = 0.95f * Vector3.one / GetFittingScale(t);
    }

    // boundsがカメラに収まる時の、bounds.centerからカメラまでの距離を求める
    public float GetFittingScale(Bounds bounds)
    {
        Vector3 farthestPoint = GetFarthestPoint(bounds);

        Vector3 targetToCamera = (m_camera.transform.position - bounds.center);
        targetToCamera.Normalize();

        Vector3 farthestPointProjected = Vector3.Project(farthestPoint, targetToCamera);

        // farthestPointを原点を通るスクリーンに並行な面に移動する
        Vector3 viewportCenter = m_camera.WorldToViewportPoint(m_area.rect.center);
        Vector3 viewportPoint = m_camera.WorldToViewportPoint(farthestPoint - farthestPointProjected);
        Vector3 originPlane = viewportPoint - viewportCenter;

        float scale = 2 * Mathf.Max(originPlane.x, originPlane.y, -originPlane.x, -originPlane.y);
        float radius = (m_camera.transform.position - bounds.center).magnitude * scale + farthestPointProjected.magnitude;

        return radius / (m_camera.transform.position - bounds.center).magnitude;
    }

    private Vector3 GetFarthestPoint(Bounds bounds)
    {
        float max = float.MinValue;
        byte maxIndex = 0;

        for (byte i = 0; i < 8; i++)
        {
            var worldPoint = new Vector3(
                (i & 0b100) != 0 ? bounds.max.x : bounds.min.x,
                (i & 0b010) != 0 ? bounds.max.y : bounds.min.y,
                (i & 0b001) != 0 ? bounds.max.z : bounds.min.z
            ) - bounds.center;

            Vector3 viewportPoint = m_camera.WorldToScreenPoint(worldPoint) - m_camera.WorldToScreenPoint(m_area.rect.center);
            viewportPoint.x /= m_area.rect.width;
            viewportPoint.y /= m_area.rect.height;

            float pointMax = Mathf.Max(viewportPoint.x, viewportPoint.y, -viewportPoint.x, -viewportPoint.y);

            if (pointMax >= max)
            {
                max = pointMax;
                maxIndex = i;
            }
        }

        return new Vector3(
            (maxIndex & 0b100) != 0 ? bounds.max.x : bounds.min.x,
            (maxIndex & 0b010) != 0 ? bounds.max.y : bounds.min.y,
            (maxIndex & 0b001) != 0 ? bounds.max.z : bounds.min.z
        );
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Puzzle))]
    public class ExampleScriptEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Fit Camera"))
            {
                Puzzle puzzle = target as Puzzle;
                puzzle.Fit();

                // UnityEditor.Undo.RecordObject(puzzle.GetComponent<Camera>().transform, "Fit Camera");
                // UnityEditor.EditorUtility.SetDirty(target);
            }
        }
    }
#endif

}