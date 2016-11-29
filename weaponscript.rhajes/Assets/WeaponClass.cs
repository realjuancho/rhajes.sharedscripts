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
			InicializarArma ();
	}

	void LateUpdate()
	{

		RecibirInputArma ();


	}


	//tiempo desde disparo, se pone en 0 cada que se instancia una bala
	float f_tiempoDesdeDisparo;

	//Guarda las balas disparadas en el frame
	int i_balasDisparadasDeRazon = 0;

	//Guarda si la solicitud de disparo ya instanció todas las balas según la razon de disparo
	bool b_disparoTerminado = true;

	/// <summary>
	/// Controla la solicitud del usuario de disparar el arma
	/// </summary>
	void RecibirInputArma()
	{
		//Crea varialble local para no llamar Time.deltaTime mas de una vez en la función
		float deltaTime = Time.deltaTime;


		//guarda si el usuario está pidiendo un disparo en el frame actual
		bool inputDisparando = false;

		#region KeyboardAndMouse

		//Verifica si el usuario tiene que presionar varias veces (manual) o dejar presionado para efectuar solicitud de disparo)
		switch(preferenciasArma.TipoDisparo)
		{
			case (WeaponEnums.WeaponShootType.Automatico):
				inputDisparando = Input.GetMouseButton(0);
			break;
			case(WeaponEnums.WeaponShootType.Manual):
				inputDisparando = Input.GetMouseButtonDown(0);
			break;
		}
		

		#endregion

		//Si el usuario solicita disparar y la razon de tiro ha terminado se reinician los valores
		if(inputDisparando && b_disparoTerminado)
		{
			//Se ha iniciado un disparo
			b_disparoTerminado = false;

			//Aun no se instancian balas
			i_balasDisparadasDeRazon = 0;

			//El disparo se inicia en este frame
			f_tiempoDesdeDisparo = 0;

			//Se dispara el arma
			DispararArma(); 


		}

		//Si aún no termina de disparar la razon completa
		else if(!b_disparoTerminado)
		{

			
			//Cantidad de balas a disparar 
			int RazonDisparo = preferenciasArma.RazonDisparo;

			//Duracion total entre balas en la razon
			float DuracionRazonDividida = preferenciasArma.DuracionRazon / RazonDisparo;

			//Se agrega el deltatime al tiempo de disparo;
			f_tiempoDesdeDisparo += deltaTime;

			//Revisa si es tiempo de soltar la siguiente razon de bala
			if(f_tiempoDesdeDisparo > DuracionRazonDividida)
			{
				//Si aun no se acaban las balas de la razon
				if(i_balasDisparadasDeRazon < RazonDisparo)
				{
					//Se dispara el arma
					DispararArma();
				}

			}

			if(f_tiempoDesdeDisparo > preferenciasArma.TiempoEntreDisparos)
			{
				b_disparoTerminado = true;
			}
					


		}

	
		else if(inputDisparando)
		{
			Debug.Log("No puede disparar en éste frame, un disparo está siendo ejecutado aún");

		}


		
	}



	void DispararArma()
	{

		float balasRestantes = preferenciasArma.BalasRestantes();

		if(balasRestantes > 0)
		{
		//Instanciar la bala y pasar la velocidad desde las preferencias de la bala
		//pasar la rotación del barril a la bala
			BalaClass bala = (BalaClass)Instantiate (PrefabsArma.Bala, PrefabsArma.Barril.transform.position, 
				PrefabsArma.Barril.transform.rotation);
			bala.preferenciasBala.VelocidadBala = preferenciasArma.VelocidadDisparo;

			//Asignar el tiempo de vida de la bala directamente al gameobject
			Destroy (bala.gameObject, preferenciasArma.TiempoDeVidaBala);

			//Crear el casquillo
			GameObject casquillo = (GameObject)Instantiate (PrefabsArma.Casquillo, PrefabsArma.Escape.transform.position, 
				PrefabsArma.Escape.transform.rotation);

			//Destruir el casquillo igual que la bala;
			Destroy (casquillo.gameObject, preferenciasArma.TiempoDeVidaBala);


			preferenciasArma.UsarBala();	
			i_balasDisparadasDeRazon++;
		}
		else if(preferenciasArma.RecargaAutomatica)
		{
			preferenciasArma.IniciarCartucho();
		}



	}

	//Llamada para inicializar instancia de WeaponSettings;
	void InicializarArma()
	{
		switch (preferenciasArma.TipoDeArma) 
		{
		default:
			preferenciasArma.TipoDeArma = WeaponEnums.WeaponType.Rifle;

			preferenciasArma.RazonDisparo = 3;
			preferenciasArma.DuracionRazon = 0.2f;

			preferenciasArma.Rafaga = 1;
			preferenciasArma.SpreadRafaga = 0;

			preferenciasArma.MaximoCartuchos = 5;
			preferenciasArma.BalasPorCartucho = 18;
			preferenciasArma.RecargaCancelaDisparos = false;

			preferenciasArma.TiempoRecarga = 1.0f;
			preferenciasArma.TiempoEntreDisparos = 0.2f;
			preferenciasArma.VelocidadDisparo = 100.0f;
			preferenciasArma.AnguloRebote = 2.0f;

			preferenciasArma.TiempoDeVidaBala = 3.0f;
			preferenciasArma.RecargaAutomatica = true;


			preferenciasArma.IniciarArma();
			preferenciasArma.IniciarCartucho();

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
		//Tiempo de retardo entre la razon de disparos
		public float DuracionRazon;
		//El Máximo de cartuchos que puede cargar el jugador
		public int MaximoCartuchos;
		//El Máximo de balas en cada cartucho
		public int BalasPorCartucho;
		//Tiempo máx que existirá la bala en la escena
		public float TiempoDeVidaBala;
		//Velocidad en que se desplaza la bala instanciada (se pasa el valor a la clase bala)
		public float VelocidadDisparo;

		//El numero de balas por disparo
		public int Rafaga;
		//la distancia del spread entre bala y bala
		public float SpreadRafaga;
		//el ángulo de las balas una vez disparadas.
		public float AnguloRafaga;
		//Define si se carga el siguiente cartucho al disparar la ultima bala
		public bool RecargaAutomatica;
		//Tiempo que tarda entre cada bala disparada
		public float TiempoEntreDisparos;
		//Define si la recarga cancela la habilidad de disparar (cancelar animacion de recarga)
		public bool RecargaCancelaDisparos;


		//Cuanto tarda en recargar el arma completamente (idealmente se compara con la animación)
		public float TiempoRecarga;
		//Cuanto tarda en disparar la primera bala
		public float RetardoDisparoInicial;


		//La estabilidad del arma después de disparar;
		public float AnguloRebote;

		
		//Balas en el clip actual;
		[SerializeField]
		int i_balasRestantes;

		//Cartuchos Restantes;
		[SerializeField]
		int i_cartuchosRestantes;


		//El arma se está recargando
		[SerializeField]
		bool b_recargando;

		public void IniciarArma()
		{
			i_cartuchosRestantes = MaximoCartuchos;
		}

		public void IniciarCartucho()
		{	
			if(i_cartuchosRestantes > 0)
			{
				i_cartuchosRestantes--;
				i_balasRestantes = BalasPorCartucho;
			}
			else
			{
				Debug.Log("Se han terminado las municiones");
			}
		}

		public void UsarBala()
		{
			i_balasRestantes--;
		}

		public int BalasRestantes()
		{
			return i_balasRestantes;
		}

		public int CartuchosRestantes()
		{
			return i_cartuchosRestantes;
		}

		public void RecargarTodosCartuchos()
		{
			i_cartuchosRestantes = MaximoCartuchos;
		}

		public void RecargarCartuchos(int Cartuchos)
		{
			i_cartuchosRestantes = Cartuchos;
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
		public bool RecargarArma;
		public bool RecargarCartuchos;
	}






}
