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
        public List<string> unlockedJerseys;
        public List<string> unlockedShoes;
        public List<string> unlockedBalls;
        public string currentJersey;
        public string currentShoe;
        public string currentBall;
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
            Debug.Log("SaveManager: Loaded, Coins = " + _data.coins);
        }
        else
        {
            CreateNewSave();
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(_data);
        File.WriteAllText(_savePath, json);
    }

    [ContextMenu("Reset Save Data")]
    public void ResetSave()
    {
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
        }
        CreateNewSave();
        Debug.Log("SaveManager: Save reset!");
    }

    private void CreateNewSave()
    {
        _data = new SaveData();
        _data.coins = 0;
        _data.unlockedJerseys = new List<string>();
        _data.unlockedShoes = new List<string>();
        _data.unlockedBalls = new List<string>();
        _data.currentJersey = "jersey_1";
        _data.currentShoe = "shoe_1";
        _data.currentBall = "ball_1";
        Save();
        Debug.Log("SaveManager: New save created");
    }

    public int Coins
    {
        get { return _data.coins; }
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

    public bool UnlockJersey(string jerseyId)
    {
        if (!_data.unlockedJerseys.Contains(jerseyId))
        {
            _data.unlockedJerseys.Add(jerseyId);
            Save();
            return true;
        }
        return false;
    }

    public bool UnlockShoe(string shoeId)
    {
        if (!_data.unlockedShoes.Contains(shoeId))
        {
            _data.unlockedShoes.Add(shoeId);
            Save();
            return true;
        }
        return false;
    }

    public bool UnlockBall(string ballId)
    {
        if (!_data.unlockedBalls.Contains(ballId))
        {
            _data.unlockedBalls.Add(ballId);
            Save();
            return true;
        }
        return false;
    }

    public bool OwnsJersey(string jerseyId)
    {
        return _data.unlockedJerseys.Contains(jerseyId);
    }

    public bool OwnsShoe(string shoeId)
    {
        return _data.unlockedShoes.Contains(shoeId);
    }

    public bool OwnsBall(string ballId)
    {
        return _data.unlockedBalls.Contains(ballId);
    }

    public string CurrentJersey
    {
        get { return _data.currentJersey; }
        set { _data.currentJersey = value; Save(); }
    }

    public string CurrentShoe
    {
        get { return _data.currentShoe; }
        set { _data.currentShoe = value; Save(); }
    }

    public string CurrentBall
    {
        get { return _data.currentBall; }
        set { _data.currentBall = value; Save(); }
    }

    public List<string> GetUnlockedJerseys()
    {
        return new List<string>(_data.unlockedJerseys);
    }

    public List<string> GetUnlockedShoes()
    {
        return new List<string>(_data.unlockedShoes);
    }

    public List<string> GetUnlockedBalls()
    {
        return new List<string>(_data.unlockedBalls);
    }
}