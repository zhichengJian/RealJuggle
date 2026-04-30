using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Serializable]
    public class SaveData
    {
        public int coins;
        public List<string> unlockedSkins;
        public string currentSkin;
    }

    private SaveData _data;
    private string _savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _savePath = Path.Combine(Application.persistentDataPath, "save.json");
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Load()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            _data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("SaveManager: Save file loaded, Coins = " + _data.coins);
        }
        else
        {
            _data = new SaveData();
            _data.coins = 0;
            _data.unlockedSkins = new List<string>();
            _data.currentSkin = "";
            Save();
            Debug.Log("SaveManager: New save file created");
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(_data);
        File.WriteAllText(_savePath, json);
        Debug.Log("SaveManager: Saved, Coins = " + _data.coins);
    }

    public int Coins
    {
        get { return _data.coins; }
    }

    public List<string> UnlockedSkins
    {
        get { return _data.unlockedSkins; }
    }

    public string CurrentSkin
    {
        get { return _data.currentSkin; }
        set { _data.currentSkin = value; Save(); }
    }

    public void AddCoins(int amount)
    {
        _data.coins += amount;
        Save();
    }

    public bool SpendCoins(int amount)
    {
        if (_data.coins >= amount)
        {
            _data.coins -= amount;
            Save();
            return true;
        }
        return false;
    }

    public void UnlockSkin(string skinId)
    {
        if (!_data.unlockedSkins.Contains(skinId))
        {
            _data.unlockedSkins.Add(skinId);
            Save();
        }
    }

    public bool OwnsSkin(string skinId)
    {
        return _data.unlockedSkins.Contains(skinId);
    }
}