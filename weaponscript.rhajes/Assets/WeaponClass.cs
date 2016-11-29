using UnityEngine;
using System.Collections;

/// <summary>
/// Clase para instanciar armas
/// </summary>
public class WeaponClass : MonoBehaviour {


	public bool ArmaHabilitada;

	public ColocarPrefabs PrefabsArma;
	public PreferenciasArma preferenciasArma;
	public DebugWeapon DebugArma;

	void Awake()
	{
		if (DebugArma.InicializarArma)
			InicializarSettings ();
	}

	void Update()
	{

		RecibirInputArma ();


	}


	//tiempo desde disparo, se pone en 0 cada que se instancia una bala
	float f_tiempoDesdeDisparo;
	bool inputDisparando;

	void RecibirInputArma()
	{

		//guarda si el usuario está pidiendo un disparo en el frame actual
		 inputDisparando = false;

		#region KeyboardAndMouse
		if(ArmaHabilitada)
		{
			switch(preferenciasArma.TipoDisparo)
			{
				case (WeaponEnums.WeaponShootType.Automatico):
					inputDisparando = Input.GetMouseButton(0);
				break;
				case(WeaponEnums.WeaponShootType.Manual):
					inputDisparando = Input.GetMouseButtonDown(0);
				break;
			}
		}

		#endregion

		if(inputDisparando)
		{

			float deltaTime = Time.deltaTime;

			InstanciarBala ();
		}
	}

	void InstanciarBala()
	{
		//Instanciar la bala y pasar la velocidad desde las preferencias de la bala
		//pasar la rotación del barril a la bala
		BalaClass bala = (BalaClass)Instantiate (PrefabsArma.Bala, PrefabsArma.Barril.transform.position, 
			PrefabsArma.Barril.transform.rotation);
		bala.preferenciasBala.VelocidadBala = preferenciasArma.VelocidadDisparo;
		f_tiempoDesdeDisparo = 0.0f;
		//Asignar el tiempo de vida de la bala directamente al gameobject
		Destroy (bala.gameObject, preferenciasArma.TiempoDeVidaBala);

		//Crear el casquillo
		GameObject casquillo = (GameObject)Instantiate (PrefabsArma.Casquillo, PrefabsArma.Barril.transform.position, 
			PrefabsArma.Barril.transform.rotation);
		//Destruir el casquillo igual que la bala;
		Destroy (casquillo.gameObject, preferenciasArma.TiempoDeVidaBala);
	}

	//Llamada para inicializar instancia de WeaponSettings;
	void InicializarSettings()
	{
		switch (preferenciasArma.TipoDeArma) 
		{
		default:
			preferenciasArma.TipoDeArma = WeaponEnums.WeaponType.Rifle;

			preferenciasArma.Rafaga = 1;
			preferenciasArma.SpreadRafaga = 0;

			preferenciasArma.RazonDisparo = 3;
			preferenciasArma.RetardoDisparo = 0.2f;
			preferenciasArma.MaximoCartuchos = 5;
			preferenciasArma.BalasPorCartucho = 18;
			preferenciasArma.RecargaCancelaDisparos = false;
			preferenciasArma.RetardoDisparo = 0.3f;
			preferenciasArma.TiempoRecarga = 1.0f;
			preferenciasArma.TiempoEntreDisparos = 0.2f;
			preferenciasArma.VelocidadDisparo = 100.0f;
			preferenciasArma.AnguloRebote = 2.0f;

			preferenciasArma.TiempoDeVidaBala = 3.0f;
			break;
		}

	}

	[System.Serializable]
	public class ColocarPrefabs
	{

		//TODO: Remplazar GAMEOBJECT por las clases especificas
		public BalaClass Bala;
		public GameObject Casquillo;
		public GameObject Barril;
		public GameObject Escape;
		public GameObject Flash;
		public GameObject SonidoDisparo;
		public GameObject SonidoCasquillo;

	}


	[System.Serializable]
	/// <summary>
	/// Clase que guarda las preferencias del arma.
	/// </summary>
	public class PreferenciasArma{

		//Define el tipo de arma de la instancia
		public WeaponEnums.WeaponType TipoDeArma;

		//Define si el usuario tendrá que usar el disparo automáticamente
		public WeaponEnums.WeaponShootType TipoDisparo;

		//El maximo de disparos antes de sentir retardo
		public int RazonDisparo;
		//El numero de balas por disparo
		public int Rafaga;
		//la distancia del spread entre bala y bala
		public float SpreadRafaga;
		//el ángulo de las balas una vez disparadas.
		public float AnguloRagaga;
		//Tiempo de retardo entre la razon de disparos
		public float RetardoDisparo;
		//El Máximo de cartuchos que puede cargar el jugador
		public int MaximoCartuchos;
		//El Máximo de balas en cada cartucho
		public int BalasPorCartucho;
		//Define si la recarga cancela la habilidad de disparar (cancelar animacion de recarga)
		public bool RecargaCancelaDisparos;
		//Cuanto tarda en recargar el arma completamente (idealmente se compara con la animación)
		public float TiempoRecarga;
		//Cuanto tarda en disparar la primera bala
		public float RetardoDisparoInicial;
		//Tiempo que tarda entre cada bala disparada
		public float TiempoEntreDisparos;
		//Velocidad en que se desplaza la bala instanciada (se pasa el valor a la clase bala)
		public float VelocidadDisparo;
		//La estabilidad del arma después de disparar;
		public float AnguloRebote;
		//Tiempo máx que existirá la bala en la escena
		public float TiempoDeVidaBala;
		
		//Balas en el clip actual;
		[SerializeField]
		int BalasActuales;

		//El arma se está recargando
		[SerializeField]
		bool recargando;

		PreferenciasArma()
		{
			BalasActuales = BalasPorCartucho;
		}

	}

	/// <summary>
	/// Clase que guarda los  diferentes enumeradores del arma
	/// </summary>
	public class WeaponEnums
	{
		public enum WeaponType { Rifle, RifleAsalto, Escopeta, HandGun, Knife }; 
		public enum WeaponShootType { Automatico, Manual }; 
	}

	[System.Serializable]
	public class DebugWeapon
	{
		public bool InicializarArma;
		public bool DibujarRayoTrayecto;
	}






}
