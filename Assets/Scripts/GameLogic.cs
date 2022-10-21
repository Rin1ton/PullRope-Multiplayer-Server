using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
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

	public GameObject PlayerPrefab => playerPrefab;

	[Header("Prefabs")]
	[SerializeField] private GameObject playerPrefab;

	private void Awake()
	{
		Singleton = this;
	}

}
