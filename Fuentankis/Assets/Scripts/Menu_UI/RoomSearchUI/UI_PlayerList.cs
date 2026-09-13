using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerList : MonoBehaviour
{
    [SerializeField] PhotonRoomSearcher Searcher;
    [SerializeField] GameObject PlayerButtomPrefab;
    public List<GameObject> PlayerButtomList;
}
