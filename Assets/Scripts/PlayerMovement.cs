using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private Player player;
    [SerializeField] private Vector3 camForward;

    private void OnValidate()
    {
        camForward = Vector3.forward;
    }

    private void FixedUpdate()
    {
        SendTransform();
    }

    public void SetTransform(Vector3 camera, Vector3 position, Quaternion rotation)
    {
        camForward = camera;
        transform.position = position;
        transform.rotation = rotation;
    }

    private void SendTransform()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerTransform);
        message.AddUShort(player.Id);

        //movement
        message.AddVector3(camForward);
        message.AddVector3(transform.position);
        message.AddQuaternion(transform.rotation);

        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
