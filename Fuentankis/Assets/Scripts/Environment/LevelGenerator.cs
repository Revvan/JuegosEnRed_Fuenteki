using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class LevelGenerator : MonoBehaviourPunCallbacks
{
    [SerializeField, Range(10000, 99999)]int seed = 0;
    [SerializeField] Vector3 worldSpace = Vector3.one;
    [SerializeField] Vector3 origin = Vector3.zero;
    [SerializeField] int obstaclesMinAmount;
    [SerializeField] int obstaclesMaxAmount;
    [SerializeField] GameObject wallPrefab = null;

    public override void OnJoinedRoom()
    {
        //seed = Random.Range(10000, 99999);
        StartCoroutine(SetUpLevel(seed));
    }

    private IEnumerator SetUpLevel(int seed)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            seed = Random.Range(10000, 99999);
            Random.InitState(seed);

            int obstaclesAmount = Random.Range(obstaclesMinAmount, obstaclesMaxAmount);

            for (int i = 0; i < obstaclesAmount; i++)
            {
                int x = (int)(Random.value * (worldSpace.x));
                int y = (int)(Random.value * (worldSpace.y));

                GameObject go = PhotonNetwork.InstantiateRoomObject(wallPrefab.name, new Vector3(x + origin.x, y + origin.y, worldSpace.z), Quaternion.identity);
                go.transform.localScale = new Vector3(Random.value + 1 + go.transform.localScale.x, Random.value + 0.8f + go.transform.localScale.y, go.transform.localScale.z);
                go.transform.rotation *= Quaternion.Euler(0, 0, (Random.Range(-100, 100)));
                //Debug.Log(go.transform.localScale);
                go.transform.SetParent(gameObject.transform);
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
