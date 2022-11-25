using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoopRange : MonoBehaviour
{

	[SerializeField] private Player myPlayer;
	[SerializeField] private Collider myPlayerCollider;

/*	private void OnCollisionEnter(Collision collision)
	{
		if ((!myPlayer.playersInBoopRange.Contains(collision.collider)) && collision.collider != myPlayerCollider)
		{
			myPlayer.playersInBoopRange.Add(collision.collider);
		}
	}*/

	private void OnTriggerEnter(Collider other)
	{
		if ((!myPlayer.playersInBoopRange.Contains(other)) && other != myPlayerCollider)
		{
			myPlayer.playersInBoopRange.Add(other);
		}
	}

/*	private void OnCollisionExit(Collision collision)
	{
		if (myPlayer.playersInBoopRange.Contains(collision.collider))
			myPlayer.playersInBoopRange.Remove(collision.collider);
	}*/

	private void OnTriggerExit(Collider other)
	{
		if (myPlayer.playersInBoopRange.Contains(other))
			myPlayer.playersInBoopRange.Remove(other);
	}
}
