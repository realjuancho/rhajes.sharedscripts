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
		//Recibe las solicitudes del usuario de disparar el arma
		RecibirInputArma ();


	}

	void Update()
	{

		#region Debug
		if(DebugArma.InicializarArma) {
			InicializarArma();
			DebugArma.InicializarArma = false;
		}
		#endregion

		if(DebugArma.RecargarCartuchos)
		{
			preferenciasArma.RecargarTodosCartuchos();
			DebugArma.RecargarCartuchos = false;
		}



		//Recibe la solicitud del usuario de recargar el arma
		RecargarArma();

	}


	//tiempo desde disparo, se pone en 0 cada que se instancia una bala
	//tiempo desde disparo entre razon;
	//Guarda las balas disparadas en el frame
	//Guarda si la solicitud de disparo ya instanció todas las balas según la razon de disparo
	float f_tiempoDesdeDisparo;
	float f_tiempoDesdeDisparoRazon;
	int i_balasDisparadasDeRazon = 0;
	bool b_disparoTerminado = true;

	bool b_solicitudDisparo;
	float f_tiempoDesdeSolicitudDisparo;
	bool b_disparar;

	//Registra si el usuario ha solicitado recargar el arma;
	bool b_solicitudRecarga;

	/// <summary>
	/// Controla la solicitud del usuario de disparar el arma
	/// </summary>
	void RecibirInputArma()
	{
		//Crea varialble local para no llamar Time.deltaTime mas de una vez en la función
		float deltaTime = Time.deltaTime;


		//guarda si el usuario está pidiendo un disparo en el frame actual
		bool inputDisparando = false;
		bool inputRecargando = false;

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

		inputRecargando = Input.GetKeyDown(KeyCode.R);
		

		#endregion


		if(!b_solicitudRecarga) 
			b_solicitudRecarga = inputRecargando;

		
		if(b_solicitudRecarga && preferenciasArma.RecargaCancelaDisparos && inputDisparando)
		{
			Debug.Log("No puede disparar con esta arma mientras recarga");

		}
		else if(preferenciasArma.BalasRestantes() > 0) 
		{

			if(inputDisparando && preferenciasArma.ParqueRestante() > 0 && b_solicitudRecarga)
			{
				Debug.Log("Se canceló la recarga");
				b_solicitudRecarga = false;
			}

			//Si el usuario solicita disparar y la razon de tiro ha terminado se reinician los valores
			if((inputDisparando && b_disparoTerminado) || b_solicitudDisparo)
			{
				b_solicitudDisparo = true;

				f_tiempoDesdeSolicitudDisparo += deltaTime;

				if(f_tiempoDesdeSolicitudDisparo > preferenciasArma.RetardoDisparoInicial)
				{
					b_disparar = true;
				}

				else {
					b_disparar = false;
				}

				if(b_disparar)
				{
					//Se ha iniciado un disparo
					b_disparoTerminado = false;
					b_solicitudDisparo = false;

					f_tiempoDesdeSolicitudDisparo = 0;

					//El disparo se inicia en este frame
					f_tiempoDesdeDisparo = 0;
					f_tiempoDesdeDisparoRazon = 0;

					//Aun no se instancian balas
					i_balasDisparadasDeRazon = 0;

					//Se dispara el arma
					DispararArma(preferenciasArma.Rafaga); 
				}
			
			}
			//Si aún no termina de disparar la razon completa
			else if(!b_disparoTerminado)
			{

				
				//Cantidad de balas a disparar 
				int RazonDisparo = preferenciasArma.RazonDisparo;

				//Duracion total entre balas en la razon
				float DuracionRazonDividida = preferenciasArma.DuracionRazon / RazonDisparo;

				//Se agrega el deltatime al tiempo de razon;
				f_tiempoDesdeDisparoRazon += deltaTime;

				//Revisa si es tiempo de soltar la siguiente razon de bala
				if(f_tiempoDesdeDisparoRazon > DuracionRazonDividida)
				{
					//Si aun no se acaban las balas de la razon
					if(i_balasDisparadasDeRazon < RazonDisparo)
					{
						//Se dispara el arma
						//Se pasa el parametro de rafaga
						DispararArma(preferenciasArma.Rafaga);
						
						f_tiempoDesdeDisparoRazon = 0;
					}
				}

				f_tiempoDesdeDisparo += deltaTime;
				if(f_tiempoDesdeDisparo > preferenciasArma.TiempoEntreDisparos)
				{
					b_disparoTerminado = true;
				}
			}
			//Si el usuario sigue disparando pero no ha terminado el anterior disparo
			else if(inputDisparando)
			{
				Debug.Log("No puede disparar en éste frame, un disparo está siendo ejecutado aún");

			}
		}
		else if(preferenciasArma.RecargaAutomatica)
		{
			b_solicitudRecarga = true;
		}
	}


	void DispararArma(int Rafaga)
	{

		int balasRestantes = preferenciasArma.BalasRestantes();


		if(balasRestantes > 0)
		{

			if(Rafaga == 1)
			{

				BalaClass bala = (BalaClass)Instantiate (PrefabsArma.Bala, PrefabsArma.Barril.transform.position, 
					PrefabsArma.Barril.transform.rotation);

				bala.preferenciasBala.VelocidadBala = preferenciasArma.VelocidadDisparo;

				//Asignar el tiempo de vida de la bala directamente al gameobject
				Destroy (bala.gameObject, preferenciasArma.TiempoDeVidaBala);

			}

			else if (Rafaga > 1)
			{
				float spreadRafaga = preferenciasArma.SpreadRafaga;
				float anguloRafaga = preferenciasArma.AnguloRafaga;

				for(int i=0; i < Rafaga; i++)
				{
				//Instanciar la bala y pasar la velocidad desde las preferencias de la bala
				//pasar la rotación del barril a la bala


					Vector3 randomPosition = PrefabsArma.Barril.transform.position;
					randomPosition.x += Random.Range(-spreadRafaga,spreadRafaga);
					randomPosition.y += Random.Range(-spreadRafaga,spreadRafaga);


					Quaternion bulletRotation = PrefabsArma.Barril.transform.rotation;

					float randomX = Random.Range(-anguloRafaga, anguloRafaga);
					float randomY = Random.Range(-anguloRafaga, anguloRafaga);

					bulletRotation.eulerAngles += new Vector3(randomX,randomY,0);



					BalaClass bala = (BalaClass)Instantiate (PrefabsArma.Bala, randomPosition, 
						bulletRotation);

					bala.preferenciasBala.VelocidadBala = preferenciasArma.VelocidadDisparo;

					//Asignar el tiempo de vida de la bala directamente al gameobject
					Destroy (bala.gameObject, preferenciasArma.TiempoDeVidaBala);
				}
			}

			//Crear el casquillo
			GameObject casquillo = (GameObject)Instantiate (PrefabsArma.Casquillo, PrefabsArma.Escape.transform.position, 
				PrefabsArma.Escape.transform.rotation);

			//Destruir el casquillo igual que la bala;
			Destroy (casquillo.gameObject, preferenciasArma.TiempoDeVidaBala);


			preferenciasArma.UsarBala();

			i_balasDisparadasDeRazon++;
		}


	}

	float f_tiempoDesdeRecarga;
	float f_tiempoDesdeRecargaRazon;

	void RecargarArma()
	{
		float deltaTime = Time.deltaTime;

		if(b_solicitudRecarga){

			f_tiempoDesdeRecarga += deltaTime;

			if(preferenciasArma.TipoRecarga == WeaponEnums.WeaponReloadType.Cartuchos)
			{
				
				if(f_tiempoDesdeRecarga > preferenciasArma.TiempoRecarga)
				{
					preferenciasArma.IniciarCartucho();
				}
			}

			else if(preferenciasArma.TipoRecarga == WeaponEnums.WeaponReloadType.Parque)
				{

					int balasRestantes = preferenciasArma.BalasRestantes();

					if(preferenciasArma.RazonRecarga == 0)
					{

						if(f_tiempoDesdeRecarga > preferenciasArma.TiempoRecarga)
						{
							int BalasTomadas = preferenciasArma.BalasRecamara - balasRestantes;
							preferenciasArma.UsarParque(BalasTomadas);
						}
					}

					else
					{
						f_tiempoDesdeRecargaRazon += deltaTime;
						float tiempoRecargaRazonDividida = preferenciasArma.TiempoRecarga / preferenciasArma.RazonRecarga;

						if(f_tiempoDesdeRecargaRazon > tiempoRecargaRazonDividida)
						{
							preferenciasArma.UsarParque(preferenciasArma.RazonRecarga);
							f_tiempoDesdeRecargaRazon = 0;
						}

					}
				}

			if(f_tiempoDesdeRecarga > preferenciasArma.TiempoRecarga)
			{
				b_solicitudRecarga = false;
				f_tiempoDesdeRecarga = 0;
			}
		}
		else
		{
			f_tiempoDesdeRecarga = 0;
		}


	}

	//Llamada para inicializar instancia de WeaponSettings;
	void InicializarArma()
	{
		switch (preferenciasArma.TipoDeArma) 
		{

		case WeaponEnums.WeaponType.Escopeta :
			
			preferenciasArma.RazonDisparo = 2;
			preferenciasArma.DuracionRazon = 0.2f;

			preferenciasArma.Rafaga = 4;
			preferenciasArma.SpreadRafaga = 0.5f;

			preferenciasArma.BalasRecamara = 18;
			preferenciasArma.MaximoCartuchos = 5;

			preferenciasArma.RecargaCancelaDisparos = false;
			preferenciasArma.TiempoRecarga = 1.0f;
			preferenciasArma.RazonRecarga = 2;

			preferenciasArma.TiempoEntreDisparos = 0.2f;
			preferenciasArma.VelocidadDisparo = 100.0f;
			preferenciasArma.AnguloRebote = 2.0f;

			preferenciasArma.TiempoDeVidaBala = 3.0f;
			preferenciasArma.RecargaAutomatica = true;


			preferenciasArma.IniciarArma();
			preferenciasArma.IniciarCartucho();

			break;

		default:
			preferenciasArma.TipoDeArma = WeaponEnums.WeaponType.Rifle;

			preferenciasArma.RazonDisparo = 3;
			preferenciasArma.DuracionRazon = 0.2f;

			preferenciasArma.Rafaga = 1;
			preferenciasArma.SpreadRafaga = 0;

			preferenciasArma.BalasRecamara = 18;
			preferenciasArma.MaximoCartuchos = 5;

			preferenciasArma.RecargaCancelaDisparos = false;
			preferenciasArma.TiempoRecarga = 1.0f;
			preferenciasArma.RazonRecarga = 0;

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

		//Define el tipo de recarga que usará el arma
		public WeaponEnums.WeaponReloadType TipoRecarga;

		//Define si se carga el siguiente cartucho/se extrae del parque al disparar la ultima bala
		public bool RecargaAutomatica;

		//El maximo de disparos antes de sentir retardo
		public int RazonDisparo;
		//Tiempo de retardo entre la razon de disparos
		public float DuracionRazon;


		//El Máximo de cartuchos que puede cargar el jugador
		public int MaximoCartuchos;

		//El Máximo de balas en cada cartucho
		public int BalasRecamara;


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

		//Tiempo que tarda entre cada bala disparada
		public float TiempoEntreDisparos;
		//Define si la recarga cancela la habilidad de disparar (cancelar animacion de recarga)
		public bool RecargaCancelaDisparos;


		//Cuanto tarda en recargar el arma completamente (idealmente se compara con la animación)
		public float TiempoRecarga;
	
		/// <summary>
		/// Cuantas balas recarga a la vez (usar 0 para recargar totalmente)
		/// </summary>
		public int RazonRecarga;

		//Cuanto tarda en disparar la primera bala
		public float RetardoDisparoInicial;

		//La estabilidad del arma después de disparar;
		public float AnguloRebote;
		
		//Balas en el clip actual;
		int i_balasRestantes;

		//Cartuchos/Parque Restantes;
		int i_cartuchosRestantes;
		int i_parqueRestante;


		public void IniciarArma()
		{

			//Se ajusta el tiempo de disparos al minimo de lo que dura una razon de disparo
			if(DuracionRazon < TiempoEntreDisparos)
				TiempoEntreDisparos = DuracionRazon;

			
				i_cartuchosRestantes = MaximoCartuchos;

				i_parqueRestante = MaximoCartuchos * BalasRecamara;

		}

		public void IniciarCartucho()
		{	
			if(i_cartuchosRestantes > 0)
			{
				i_cartuchosRestantes--;

				i_parqueRestante = i_cartuchosRestantes * BalasRecamara;

				i_balasRestantes = BalasRecamara;;
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

		public void UsarParque(int BalasTomadas)
		{
			if(BalasTomadas <= i_parqueRestante)
			{
				i_parqueRestante -= BalasTomadas;

			}
			else 
				{
					BalasTomadas = i_parqueRestante;
					i_parqueRestante = 0;
				}

			if(i_balasRestantes + BalasTomadas < BalasRecamara)
				i_balasRestantes += BalasTomadas;
			else {
				i_balasRestantes = BalasRecamara;
			}
		}


		public int BalasRestantes()
		{
			return i_balasRestantes;
		}

		public int CartuchosRestantes()
		{
			return i_cartuchosRestantes;
		}

		public int ParqueRestante()
		{
			return i_parqueRestante;
		}

		public void RecargarTodosCartuchos()
		{
			i_cartuchosRestantes = MaximoCartuchos;
			i_parqueRestante = i_cartuchosRestantes * BalasRecamara;
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
		public enum WeaponReloadType { Cartuchos, Parque }

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
