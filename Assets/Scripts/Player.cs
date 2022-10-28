using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

	public ushort Id { get; private set; }
	public string Username { get; private set; }
	public PlayerMovement Movement => movement;

	[SerializeField] private PlayerMovement movement;

	private void OnDestroy()
	{
		list.Remove(Id);
	}

	public static void Spawn(ushort id, string username)
	{
		//make sure we spawn on clients connecting after other clients
		foreach (Player otherPlayer in list.Values)
			otherPlayer.SendSpawned(id);

		Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity).GetComponent<Player>();
		player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
		player.Id = id;
		player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;

		player.SendSpawned();
		list.Add(id, player);
	}

	#region Messages
	private void SendSpawned()
	{
		NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)));
	}

	private void SendSpawned(ushort toClientId)
	{
		NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)), toClientId);
	}

	private Message AddSpawnData(Message message)
	{
		message.AddUShort(Id);
		message.AddString(Username);
		message.AddVector3(transform.position);
		return message;
	}

	[MessageHandler((ushort)ClientToServerId.name)]
	private static void Name(ushort fromClientId, Message message)
	{
		Spawn(fromClientId, message.GetString());
	}

	[MessageHandler((ushort)ClientToServerId.playerTransform)]
	private static void PlayerTransform(ushort fromClientId, Message message)
	{
		Debug.Log(message.GetUShort());
		if (list.TryGetValue(fromClientId, out Player player))
			//Debug.Log(message.GetVector3() + ", " + message.GetVector3() + ", " + message.GetQuaternion());
			player.Movement.SetTransform(message.GetVector3(), message.GetVector3(), message.GetQuaternion());
	}
	#endregion
}
