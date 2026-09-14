using System.Text;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public static class PlayerIdentity
{
    public const int MaxNameLength = 20;
    private const string NameKey = "PlayerName";
    private const string IdKey = "PlayerId";

    // ActorNumber changes on a fresh join. UserId identifies this local profile across joins.
    public static string LocalId
    {
        get { PrepareNetworkIdentity(); return PhotonNetwork.AuthValues.UserId; }
    }

    public static void PrepareNetworkIdentity()
    {
        if (PhotonNetwork.AuthValues == null) PhotonNetwork.AuthValues = new AuthenticationValues();
        if (!string.IsNullOrEmpty(PhotonNetwork.AuthValues.UserId)) return;
        string id = PlayerPrefs.GetString(IdKey, "");
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(IdKey, id);
            PlayerPrefs.Save();
        }
        PhotonNetwork.AuthValues.UserId = id;
    }

    public static string LoadName()
    {
        return SaveName(PlayerPrefs.GetString(NameKey, string.Empty));
    }

    public static string SaveName(string value)
    {
        PrepareNetworkIdentity();
        string name = NormalizeName(value);
        if (string.IsNullOrEmpty(name))
            name = "Jugador" + Random.Range(1000, 10000);

        PlayerPrefs.SetString(NameKey, name);
        PlayerPrefs.Save();
        // NickName is display-only; changing it does not create another score profile.
        PhotonNetwork.NickName = name;
        return name;
    }

    public static string NormalizeName(string value)
    {
        var result = new StringBuilder();
        foreach (char character in (value ?? string.Empty).Trim())
        {
            if (char.IsControl(character) || char.IsSurrogate(character) ||
                character == '<' || character == '>')
                continue;
            if (result.Length == MaxNameLength)
                break;
            result.Append(character);
        }
        return result.ToString().Trim();
    }
}
