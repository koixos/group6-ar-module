using System;

[Serializable]
public class GameUpdateData
{
    public PlayerData player1;
    public PlayerData player2;
    public string gameState; // waiting, playing, finished
    public AttackData lastAttack;
}

[Serializable]
public class PlayerData
{
    public int id;
    public string username;
    public string modelType;
    public float health;
}

[Serializable]
public class AttackData
{
    public int attackerId;
    public int targetId;
    public string attackType;
    public float damage;
}