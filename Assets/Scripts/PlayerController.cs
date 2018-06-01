using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PlayerController : SimpleGameStateObserver, IEventHandler{

	[Header("Spawn")]
	[SerializeField]
	private Transform m_SpawnPoint;

	#region Physics gravity
	[SerializeField] Vector3 m_LowGravity;
	[SerializeField] Vector3 m_HighGravity;
	//Vector3 m_Gravity;
	#endregion


	private Rigidbody m_Rigidbody;
	private Transform m_Transform;

	[SerializeField]
	private float m_TranslationSpeed;
	[SerializeField]
	private float m_JumpImpulsionMagnitude;

	private bool m_IsGrounded;


	protected override void Awake()
	{
		base.Awake();
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Transform = GetComponent<Transform>();
	}

	private void Reset()
	{
		m_Rigidbody.position = m_SpawnPoint.position;
		m_Rigidbody.velocity = Vector3.zero;
		m_Rigidbody.angularVelocity = Vector3.zero;
	}

	// Use this for initialization
	void Start () {
		m_IsGrounded = false;
		//m_Gravity = m_HighGravity;
	}

	private void Update()
	{
		//bool fire = Input.GetAxis("Fire1") > 0;
		////Debug.Log("Fire = " + fire);
		//m_Gravity = fire ? m_LowGravity : m_HighGravity;

	}

	// Update is called once per frame
	void FixedUpdate () {
		if (GameManager.Instance && !GameManager.Instance.IsPlaying) return;

		float hInput = Input.GetAxis("Horizontal");
		float vInput = Input.GetAxis("Vertical");
		bool jump = Input.GetAxis("Jump") > 0 || Input.GetKeyDown(KeyCode.Space);
		bool fire = Input.GetAxis("Fire1") > 0;

		//m_Rigidbody.rotation = Quaternion.AngleAxis(90 * Mathf.Sign(hInput), Vector3.up);

		m_Rigidbody.MovePosition(m_Rigidbody.position + m_Transform.forward * m_TranslationSpeed * hInput * Time.fixedDeltaTime);

		if (jump && m_IsGrounded)
		{
			Vector3 jumpForce = Vector3.up * m_JumpImpulsionMagnitude;
			m_Rigidbody.AddForce(jumpForce, ForceMode.Impulse);
		}

		if (m_IsGrounded)
		{
			m_Rigidbody.velocity = Vector3.zero;
		}

		m_Rigidbody.angularVelocity = Vector3.zero;

		Vector3 gravity = m_HighGravity;
		if (fire && m_Rigidbody.velocity.y < 0) gravity = m_LowGravity;

		m_Rigidbody.AddForce(gravity*m_Rigidbody.mass);

	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Ground")
			|| collision.gameObject.CompareTag("Platform"))
		{
			Vector3 colLocalPt = m_Transform.InverseTransformPoint(collision.contacts[0].point);

			//Debug.LogError(Time.frameCount+" colLocalPt = " + colLocalPt+ "   colLocalPt.magnitude = "+ colLocalPt.magnitude);

			if (colLocalPt.magnitude<.5f)
				m_IsGrounded = true;
		}

	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.CompareTag("Ground")
			|| collision.gameObject.CompareTag("Platform"))
		{
			m_IsGrounded = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(GameManager.Instance.IsPlaying
			&& other.gameObject.CompareTag("Enemy"))
		{
			if(other.GetComponent<Enemy>().IsEnemy)
				EventManager.Instance.Raise(new PlayerHasBeenHitEvent());
		}
	}

	//Game state events
	protected override void GameMenu(GameMenuEvent e)
	{
		Reset();
	}
}




/*
 * Bomb Jack est un jeu vidéo de plates-formes développé et édité par Tehkan en 1984 sur borne d'arcade. Le jeu a été adapté sur divers supports familiaux et a fait l'objet de suites (Bomb Jack II, Mighty Bomb Jack et Bomb Jack Twin).

Le joueur incarne Bomb Jack, une sorte de super-héros masqué pouvant voler. Il doit capturer les bombes placées sur des décors fixes de sites touristiques célèbres : le Sphinx de Gizeh et les Pyramides, l'Acropole, le château de Neuschwanstein en Bavière, et deux paysages ressemblant à la plage de Miami et Hollywood. En utilisant les plates-formes disposés sur le décors, Bomb Jack doit courir et sauter jusqu'à l'ensemble des bombes tout en évitant les ennemis dans le but de finir le plateau. Le moindre contact avec un ennemi est fatal, mais il est possible de les éviter en les survolant en appuyant à plusieurs reprises sur le bouton de tir (ou la touche de saut), ce qui a pour effet de faire léviter quelques instants le héros. Une fois les cinq plateaux terminés, la difficulté augmente, les monstres devenant plus rapides, mais les décors restent les mêmes.


	Sinon au niveau des contrôles, c’est bien simple, à part gauche/droite, on a un seul bouton pour faire sauter Jack. On peut également stopper sa chute en appuyant de manière rapide et répétée sur ce même bouton. Lors d’un saut, si on pousse le joystick vers le haut, on sautera plus haut. De la même manière, abaisser le joystick lors d’une chute vous permettra de regagner le sol plus rapidement.


Bomb Jack doit ramasser sur un tableau toutes les bombes qui s'y trouvent en évitant les ennemis et en les capturant si possible dans l'ordre de l'allumage de leur mèche. La capture d'une bombe allumée entraîne l'allumage de la suivante. Chaque tableau contient exactement 24 bombes. Selon le nombre de bombes allumées que Bomb Jack aura récolté dans l'ordre, un bonus sera ajouté au score du joueur une fois le niveau terminé : pour 23 bombes allumées capturés, 50 000 points, pour 22 bombes 30 000 points, 21 bombes 20 000 points, 20 bombes 10 000 points.

Tous les 5 000 points marqués, un bonus(symbolisé par les lettres B, E, ou S) fait son apparition à un endroit aléatoire du tableau :

la lettre "B" augmentera un multiplicateur de score(1x au départ du tableau, puis 2x, 3x et enfin 5x, multipliant tous les points marqués par le joueur). À chaque début de niveau, le multiplicateur revient à 1x.
la lettre "E" fera gagner une vie supplémentaire(E comme Extra Live)
la lettre "S", bonus suprême, fera gagner au joueur une partie gratuite.
À l'instar de Pac-Man, il est également possible de "manger" les monstres du tableau en accédant au bonus "P". Ce bonus s'obtient par la capture des bombes, la progression étant symbolisée par des "ailes" autour de l'afficheur du multiplicateur de bonus en haut de l'écran.Chaque bombe capturée allumée augmente la progression de deux unités, tandis que chaque bombe éteinte capturée ne le fera augmenter que d'une demie unité. Il faut que 10 unités soient atteintes pour activer l'affichage du bonus "P" sur le plateau.Lorsque Bomb Jack parvient à le capturer, les monstres se transforment alors en pièces d'or et peuvent à leur tour être capturés pendant environ 10 secondes.

L'algorithme de mouvement des monstres ennemis de Bomb Jack est invariable :

L'oiseau suit une grille qui à chaque intersection de lignes et de colonnes imaginaires permettra à celui-ci de se rapprocher du joueur (sauf quelques cas particuliers, par exemple lorsque l'oiseau est collé à une paroi qui se trouve entre lui et le joueur).
La boule grise suit un mouvement vertical de vitesse constante, et un mouvement horizontal de vitesse variable la faisant se rapprocher du joueur en accélérant horizontalement.Son mouvement vertical est inversé lorsque la boule grise touche une paroi.
La boule noire suit un mouvement horizontal de vitesse constante, et un mouvement vertical de vitesse variable la faisant accélérer verticalement vers le niveau où se trouve le joueur.Son mouvement horizontal est inversé lorsque la boule noire touche une paroi.
La soucoupe volante suit une direction invariable jusqu'à ce qu'elle touche une paroi, elle se dirige alors vers Bomb Jack à une vitesse en rapport avec la distance qui sépare le point d'impact sur la paroi et la position où se trouve Bomb Jack au moment de cet impact.
L'"escargot" (une tête avec une sorte de chapeau pointu ressemblant à une coquille d'escargot ou de coquillage) ira toujours en direction du joueur, mais son accélération vers le joueur sera limité par une inertie.
Alors que la plupart des jeux de cette époque se basaient principalement sur les réflexes du joueur, qui n'avaient qu'à éliminer les ennemis pour passer de niveau en niveau, Bomb Jack y ajoutait une forte dose de stratégie et de timing dans l'anticipation des mouvements des monstres comme du "calcul" du moment auquel les bonus pouvaient apparaître. Alors que le joueur novice se contentera de terminer le niveau aussi vite que possible en évitant les monstres, un joueur expérimenté saura de façon précise à quel moment le bonus P se présentera. Le score d'un joueur novice atteindra généralement 20 000 à 30 000 points pour le premier niveau, celui d'un fort joueur dépassera lui les 100 000 points pour ce même niveau.
*/
