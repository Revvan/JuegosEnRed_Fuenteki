using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Fixed scene rows with pagination; no runtime UI instantiation.
public class UI_RoomLIst : MonoBehaviour
{
    [SerializeField] private PhotonRoomSearcher Searcher;
    [SerializeField] private UI_RoomButtom[] rows;
    [SerializeField] private TMP_Text pageText, emptyText;
    [SerializeField] private Button previousButton, nextButton;
    private int page;
    private void Awake()
    {
        foreach (var row in rows) row.gameObject.SetActive(false);
        emptyText.gameObject.SetActive(true);
    }
    public void PreviousPage() { page = Mathf.Max(0, page - 1); Refresh_List(); }
    public void NextPage() { page++; Refresh_List(); }
    public void Refresh_List()
    {
        if (rows == null || rows.Length == 0) return;
        var rooms = Searcher.local_roomList.OrderBy(room => room.Name).ToArray();
        int pages = Mathf.Max(1, Mathf.CeilToInt((float)rooms.Length / rows.Length));
        page = Mathf.Clamp(page, 0, pages - 1);
        for (int i = 0; i < rows.Length; i++)
        {
            int index = page * rows.Length + i;
            rows[i].gameObject.SetActive(index < rooms.Length);
            if (index < rooms.Length) rows[i].Setup(rooms[index]);
        }
        pageText.text = (page + 1) + " / " + pages;
        emptyText.gameObject.SetActive(rooms.Length == 0);
        previousButton.interactable = page > 0;
        nextButton.interactable = page + 1 < pages;
    }
}
