using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStateTest : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private float stateDuration = 2f;

    private GameState[] gameStates;

    private void Awake()
    {
        InitializeGameStates();
    }

    private void Start()
    {
        StartCoroutine(SimulateGameStates());
    }

    private void InitializeGameStates()
    {
        gameStates = new GameState[]
        {
            // Initial state
            new GameState
            {
                status = "start",
                currentTurnPlayerId = "",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 85, maxhealth = 115, state = "idle" },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 102, maxhealth = 115, state = "idle" }
                }
            },
            // WarriorQueen attacks with fireball
            new GameState
            {
                status = "ongoing",
                currentTurnPlayerId = "p1",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 85, maxhealth = 115, state = "attack", attackType = "fireball", attackDamage = 15 },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 102, maxhealth = 115, state = "hurt" }
                }
            },
            // Voldemort counter-attacks with lightning
            new GameState
            {
                status = "ongoing",
                currentTurnPlayerId = "p2",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 85, maxhealth = 115, state = "hurt" },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 87, maxhealth = 115, state = "attack", attackType = "lightening", attackDamage = 25 }
                }
            },
            // WarriorQueen attacks with lightning
            new GameState
            {
                status = "ongoing",
                currentTurnPlayerId = "p1",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 60, maxhealth = 115, state = "attack", attackType = "lightening", attackDamage = 25 },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 87, maxhealth = 115, state = "hurt" }
                }
            },
            // Voldemort attacks with fireball
            new GameState
            {
                status = "ongoing",
                currentTurnPlayerId = "p2",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 60, maxhealth = 115, state = "hurt" },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 62, maxhealth = 115, state = "attack", attackType = "fireball", attackDamage = 15 }
                }
            },
            // WarriorQueen attacks with lightning
            new GameState
            {
                status = "ongoing",
                currentTurnPlayerId = "p1",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 45, maxhealth = 115, state = "attack", attackType = "lightening", attackDamage = 25 },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 62, maxhealth = 115, state = "hurt" }
                }
            },
            // Voldemort attacks with fireball
            new GameState
            {
                status = "ongoing",
                currentTurnPlayerId = "p2",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 45, maxhealth = 115, state = "hurt" },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 37, maxhealth = 115, state = "attack", attackType = "fireball", attackDamage = 15 }
                }
            },
            // WarriorQueen attacks with lightning
            new GameState
            {
                status = "ongoing",
                currentTurnPlayerId = "p1",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 30, maxhealth = 115, state = "attack", attackType = "lightening", attackDamage = 25 },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 37, maxhealth = 115, state = "hurt" }
                }
            },
            // Voldemort attacks with lightning
            new GameState
            {
                status = "ongoing",
                currentTurnPlayerId = "p2",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 30, maxhealth = 115, state = "hurt" },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 12, maxhealth = 115, state = "attack", attackType = "lightening", attackDamage = 25 }
                }
            },
            // WarriorQueen's final attack with fireball
            new GameState
            {
                status = "ongoing",
                currentTurnPlayerId = "p1",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 5, maxhealth = 115, state = "attack", attackType = "fireball", attackDamage = 15 },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = 12, maxhealth = 115, state = "hurt" }
                }
            },
            // Game ends with Voldemort's defeat
            new GameState
            {
                status = "finished",
                currentTurnPlayerId = "p2",
                players = new[]
                {
                    new PlayerStatus { id = "p1", username = "WarriorQueen", avatar = "GreatSword", health = 5, maxhealth = 115, state = "victory" },
                    new PlayerStatus { id = "p2", username = "Voldemort", avatar = "Archer", health = -3, maxhealth = 115, state = "defeat" }
                }
            }
        };
    }

    private IEnumerator SimulateGameStates()
    {
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < gameStates.Length; i++)
        {
            Debug.Log($"Processing game state {i + 1}/{gameStates.Length}");
            gameController.ProcessGameState(gameStates[i], i == 0);
            yield return new WaitForSeconds(stateDuration);
        }

        Debug.Log("Game simulation completed!");
    }
} 