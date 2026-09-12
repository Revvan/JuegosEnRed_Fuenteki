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
        Random.InitState(seed);

        int obstaclesAmount = Random.Range(obstaclesMinAmount, obstaclesMaxAmount);

        for (int i = 0; i < obstaclesAmount; i++)
        {
            //Debug.Log("mix x luncanon");
            int x = (int)(Random.value * (worldSpace.x));
            int y = (int)(Random.value * (worldSpace.y));

            GameObject go = PhotonNetwork.Instantiate(wallPrefab.name, new Vector3(x + origin.x, y + origin.y, worldSpace.z), Quaternion.identity);
            go.transform.localScale *= Random.value + 1;
            //Debug.Log(go.transform.localScale);
            go.transform.SetParent(gameObject.transform);
        }

        yield return new WaitForEndOfFrame();
    }
}
