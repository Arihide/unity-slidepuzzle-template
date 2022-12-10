using UnityEngine;

[DisallowMultipleComponent]
public class GestureManager : MonoBehaviour
{
    public AudioSource m_source;
    [SerializeField] private AudioClip m_flick;
    private Puzzle m_puzzle;

    public void SetPuzzle(Puzzle puzzle)
    {
        m_puzzle = puzzle;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TapPieceGestureUpdated(Input.mousePosition);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            TapPieceGestureUpdated(Input.GetTouch(0).position);
        }
    }

    private void TapPieceGestureUpdated(Vector2 pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(pos);

        if (m_puzzle != null && m_puzzle.plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3Int from = m_puzzle.grid.WorldToCell(hitPoint);

            // 範囲外タッチは無視
            if (!m_puzzle.pieceMap.ContainsKey(from)) return;

            TileObject hit = m_puzzle.pieceMap[from];
            Vector3Int to = m_puzzle.empty;

            if (m_puzzle.TrySlide(from, to))
            {
                m_source.PlayOneShot(m_flick);
                hit.Move(m_puzzle.grid.CellToLocalCenter(to), 0.1f);
            }
        }
    }
}