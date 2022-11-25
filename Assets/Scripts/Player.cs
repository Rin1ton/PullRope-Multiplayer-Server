using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
	public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

	public List<Collider> playersInBoopRange = new List<Collider>();

	public ushort Id { get; private set; }
	public string Username { get; private set; }
	public string mySkin { get; private set; }
	public PlayerMovement Movement => movement;

	[SerializeField] private PlayerMovement movement;

	public Vector3 camForward => movement.camForward;

	private void OnDestroy()
	{
		list.Remove(Id);
	}

	public static void Spawn(ushort id, string username, string skin)
	{
		//make sure we spawn on clients connecting after other clients
		foreach (Player otherPlayer in list.Values)
			otherPlayer.SendSpawned(id);

		Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity).GetComponent<Player>();
		player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
		player.Id = id;
		player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
		player.mySkin = skin;

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
		message.AddString(mySkin);
		return message;
	}

	[MessageHandler((ushort)ClientToServerId.name)]
	private static void Name(ushort fromClientId, Message message)
	{
							//Get name			 Get skin
		Spawn(fromClientId, message.GetString(), message.GetString());
	}

	[MessageHandler((ushort)ClientToServerId.playerTransform)]
	private static void PlayerTransform(ushort fromClientId, Message message)
	{
		ushort playerID = message.GetUShort();
		if (list.TryGetValue(fromClientId, out Player player))
			//								camera forward				position			rotation				velocity
			player.Movement.SetTransform(message.GetVector3(), message.GetVector3(), message.GetQuaternion(), message.GetVector3());

			//Debug.Log(message.GetVector3() + ", " + message.GetVector3() + ", " + message.GetQuaternion());
	}
	#endregion
}
