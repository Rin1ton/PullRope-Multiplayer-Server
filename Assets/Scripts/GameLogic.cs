using Riptide;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class GameLogic : MonoBehaviour
{

	public static readonly float roundLength = 120;

	public bool gameCommenced { get; private set; } = false;

	float gameState = 0;

	private static GameLogic _singleton;
	public static GameLogic Singleton
	{
		get => _singleton;
		private set
		{
			if (_singleton == null) _singleton = value;
			else
			{
				Debug.LogWarning($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
				Destroy(value);
			}
		}
	}

	private static void StartGame()
	{
		Singleton.gameCommenced = true;
		Singleton.gameState = roundLength;
		foreach (Player player in Player.list.Values)
			player.Score = 0;

		Singleton.SendGameState();
	}

	public GameObject PlayerPrefab => playerPrefab;

	[Header("Prefabs")]
	[SerializeField] private GameObject playerPrefab;

	private void Awake()
	{
		Singleton = this;
	}

	private void SendGameState()
	{
		Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.gameState);
		message.AddFloat(gameState);
		NetworkManager.Singleton.Server.SendToAll(message);
	}

	private void FixedUpdate()
	{
		if (gameCommenced && gameState >= 0)
		{
			gameState -= Time.fixedDeltaTime;
			SendGameState();
		} else if (gameCommenced && gameState <= 0)
		{
			gameCommenced = false;
			foreach (Player player in Player.list.Values)
			{
				player.isReady = false;
			}
		}
	}

	[MessageHandler((ushort)ClientToServerId.playerReady)]
	private static void PlayerReady(ushort fromClientID, Message message)
	{
		Player readiedPlayer;
		Player.list.TryGetValue(fromClientID, out readiedPlayer);
		readiedPlayer.isReady = true;
		int readyPlayers = 0;
		foreach (Player player in Player.list.Values)
			if (player.isReady) readyPlayers++;

		if (readyPlayers > (Player.list.Count / 2))
			StartGame();
	}

}
