using UnityEngine;

namespace AFramework
{
	public class DontDestroyOnLoad : MonoBehaviour 
	{
		void Awake()
		{
			DontDestroyOnLoad (this);
		}
	}
}
