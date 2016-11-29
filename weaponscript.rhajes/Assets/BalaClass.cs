using UnityEngine;
using System.Collections;

public class BalaClass : MonoBehaviour {


	public PreferenciasBala preferenciasBala;

	Rigidbody rigidBodyBala;


	void Awake()
	{
		rigidBodyBala = GetComponent<Rigidbody> ();
	}

	void Start()
	{

		//rigidBodyBala.AddForce (Vector3.forward * preferenciasBala.VelocidadBala);

		rigidBodyBala.velocity = (transform.forward * preferenciasBala.VelocidadBala);
	}



	[System.Serializable]
	public class  PreferenciasBala
	{
		/// <summary>
		/// Recibe la velocidad de la bala del arma
		/// </summary>
		public float VelocidadBala;

	}
}
