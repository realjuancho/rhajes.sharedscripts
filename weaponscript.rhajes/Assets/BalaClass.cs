using UnityEngine;
using System.Collections;

public class BalaClass : MonoBehaviour {


	public PreferenciasBala preferenciasBala;
	Rigidbody rigidBodyBala;
	float distanciaRay = 1.0f;

	void Awake()
	{
		rigidBodyBala = GetComponent<Rigidbody> ();
	}

	void Start()
	{

		//rigidBodyBala.AddForce (Vector3.forward * preferenciasBala.VelocidadBala);

		rigidBodyBala.velocity = (transform.forward * preferenciasBala.VelocidadBala);


	}



	void LateUpdate()
	{


		//transform.Translate(transform.forward * preferenciasBala.VelocidadBala);

		Ray rayBala = new Ray(transform.position,transform.forward * distanciaRay);
		RaycastHit hit_Bala = new RaycastHit();

		bool deteccionBala = Physics.Raycast(rayBala, out hit_Bala, distanciaRay);



		if(deteccionBala)
		{

			Vector3 incomingVec = hit_Bala.point - transform.position;
                Vector3 reflectVec = Vector3.Reflect(incomingVec, hit_Bala.normal);
                Debug.DrawLine(transform.position, hit_Bala.point, Color.red,3);
                Debug.DrawRay(hit_Bala.point, reflectVec, Color.green,6);

            Destroy(gameObject);
		}
	}




	[System.Serializable]
	public class  PreferenciasBala
	{
		/// <summary>
		/// Recibe la velocidad de la bala del arma
		/// </summary>
		public float VelocidadBala = 100.0f;

	}
}
