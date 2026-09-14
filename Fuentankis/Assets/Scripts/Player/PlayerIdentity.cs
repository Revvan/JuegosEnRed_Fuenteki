using System.Text;
using Photon.Pun;
using UnityEngine;

public static class PlayerIdentity
{
    public const int MaxNameLength = 20;
    private const string NameKey = "PlayerName";

    public static string LoadName()
    {
        return SaveName(PlayerPrefs.GetString(NameKey, string.Empty));
    }

    public static string SaveName(string value)
    {
        string name = NormalizeName(value);
        if (string.IsNullOrEmpty(name))
            name = "Jugador" + Random.Range(1000, 10000);

        PlayerPrefs.SetString(NameKey, name);
        PlayerPrefs.Save();
        // NickName is already synchronized by Photon. ActorNumber remains the room identity.
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
