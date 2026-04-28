using System.Collections.Generic;
using UnityEngine;

public class PlayerKeys : MonoBehaviour
{
    private readonly HashSet<string> ownedKeys = new();

    public void AddKey(string keyName)
    {
        ownedKeys.Add(keyName);
        Debug.Log($"[PlayerKeys] Picked up '{keyName}'. Total keys: {ownedKeys.Count}");
    }

    public bool OwnKey(string keyName) => ownedKeys.Contains(keyName);
}
