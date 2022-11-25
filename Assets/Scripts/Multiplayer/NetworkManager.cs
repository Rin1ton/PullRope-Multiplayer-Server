using Riptide;
using Riptide.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ServerToClientId : ushort
{
	playerSpawned = 1,
	playerTransform,
	sync,
	playerBooped,
}

public enum ClientToServerId : ushort
{
	name = 1,
	playerTransform,
	playerBooped,
}

public class NetworkManager : MonoBehaviour
{
	private static NetworkManager _singleton;
	public static NetworkManager Singleton
	{
		get => _singleton;
		private set
		{
			if (_singleton == null) _singleton = value;
			else
			{
				Debug.LogWarning($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
				Destroy(value);
			}
		}
	}

	public Server Server { get; private set; }
	public ushort CurrentTick { get; private set; } = 0;

	[SerializeField] private ushort port;
	[SerializeField] private ushort maxClientCount;

	private void Awake()
	{
		Singleton = this;
	}

	private void Start()
	{
		Application.targetFrameRate = 60;

		RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

		Server = new Server();
		Server.Start(port, maxClientCount);
		//PlayerLeft should be called when a client disconnects;
		Server.ClientDisconnected += PlayerLeft;
	}

	private void FixedUpdate()
	{
		Server.Update();

		if (CurrentTick % 200 == 0)
			SendSync();

		CurrentTick++;
	}

	private void OnApplicationQuit()
	{
		Server.Stop();
	}

	private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
	{
		if (Player.list.TryGetValue(e.Client.Id, out Player player))
			Destroy(player.gameObject);
	}

	/* every so often (5 seconds) a message is sent out to synchronize the client tick
	 * to the server tick
	 */
	private void SendSync()
	{
		Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToClientId.sync);

		message.Add(CurrentTick);
		Server.SendToAll(message);
	}

}
