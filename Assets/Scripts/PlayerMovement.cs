using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

	[SerializeField] private Player player;
	[SerializeField] private Vector3 camForward;
	[SerializeField] private GameObject camProxy;
	private Vector3 myVelocity;

	private void OnValidate()
	{
		camForward = Vector3.forward;
	}

	private void FixedUpdate()
	{
		SendTransform();
	}

	public void SetTransform(Vector3 camera, Vector3 position, Quaternion rotation, Vector3 velocity)
	{
		camForward = camera;
		camProxy.transform.forward = camera;
		transform.position = position;
		transform.rotation = rotation;
		myVelocity = velocity;
	}

	/* send transform data from clients to other clients
	 */
	private void SendTransform()
	{
		/* Client is expecting move data every second tick, so we
		 * won't send this message every tick.
		 */
		if (NetworkManager.Singleton.CurrentTick % 2 != 0)
			return;

		Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerTransform);
		message.AddUShort(player.Id);

		//servertick
		message.AddUShort(NetworkManager.Singleton.CurrentTick);

		//movement
		message.AddVector3(camForward);
		message.AddVector3(transform.position);
		message.AddQuaternion(transform.rotation);
		message.AddVector3(myVelocity);

		//send information about this player to every client
		NetworkManager.Singleton.Server.SendToAll(message);
	}
}
