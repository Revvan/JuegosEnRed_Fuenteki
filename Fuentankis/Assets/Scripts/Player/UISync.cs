using UnityEngine;
using Photon.Pun;
public class UISync : MonoBehaviourPun
{
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject enemyBar;

    private PhotonView myView;

    private void Awake()
    {
        myView = GetComponent<PhotonView>();
    }
    void Start()
    {
        if(!myView.IsMine)
        {
            Destroy(ui);
        }
        else
        {
            enemyBar.SetActive(false);
        }
    }
}
