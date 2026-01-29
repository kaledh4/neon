using UnityEngine;

namespace NeonSplash.V0_1
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Game Settings")]
        public float matchDuration = 300f; // 5 minutes (in seconds)
        public float timeToWin = 180f;    // 3 minutes holding time (in seconds)

        [Header("State")]
        public float currentMatchTime;
        public float redHoldTime;
        public float blueHoldTime;
        public Team currentZoneOwner = Team.None;
        public bool isGameOver = false;
        public string winnerText = "";

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            currentMatchTime = matchDuration;
            redHoldTime = 0;
            blueHoldTime = 0;
            isGameOver = false;
            winnerText = "";
        }

        void Update()
        {
            if (isGameOver) return;

            // 1. Match Timer
            if (currentMatchTime > 0)
            {
                currentMatchTime -= Time.deltaTime;
            }
            else
            {
                EndGameByTimeLimit();
            }

            // 2. Control/Hold Logic
            if (currentZoneOwner == Team.Red)
            {
                redHoldTime += Time.deltaTime;
                if (redHoldTime >= timeToWin) EndGame(Team.Red);
            }
            else if (currentZoneOwner == Team.Blue)
            {
                blueHoldTime += Time.deltaTime;
                if (blueHoldTime >= timeToWin) EndGame(Team.Blue);
            }
        }

        void EndGameByTimeLimit()
        {
            if (redHoldTime > blueHoldTime) EndGame(Team.Red);
            else if (blueHoldTime > redHoldTime) EndGame(Team.Blue);
            else EndGame(Team.None); // Draw
        }

        public void EndGame(Team winner)
        {
            isGameOver = true;
            if (winner == Team.None) winnerText = "DRAW!";
            else winnerText = winner.ToString().ToUpper() + " TEAM WINS!";
            
            Debug.Log("GAME OVER: " + winnerText);
        }

        public void SetZoneOwner(Team newOwner)
        {
            currentZoneOwner = newOwner;
        }

        // --- Simple UI for testing ---
        void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.white;

            // Timer
            string timeStr = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(currentMatchTime / 60), Mathf.FloorToInt(currentMatchTime % 60));
            GUI.Label(new Rect(Screen.width / 2 - 50, 20, 200, 50), timeStr, style);

            // Scores
            GUI.color = Color.red;
            GUI.Label(new Rect(20, 50, 300, 30), $"Red Control: {redHoldTime:F1}/{timeToWin}");
            
            GUI.color = Color.cyan;
            GUI.Label(new Rect(Screen.width - 250, 50, 300, 30), $"Blue Control: {blueHoldTime:F1}/{timeToWin}");

            // Center Status
            GUI.color = Color.white;
            if (currentZoneOwner != Team.None)
            {
                style.fontSize = 40;
                style.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(Screen.width/2 - 200, 100, 400, 100), $"CONTROLLED BY {currentZoneOwner}", style);
            }

            // Game Over
            if (isGameOver)
            {
                style.fontSize = 60;
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.yellow;
                GUI.Label(new Rect(0, Screen.height/2 - 50, Screen.width, 100), winnerText, style);

                // Play Again Button
                if (GUI.Button(new Rect(Screen.width/2 - 100, Screen.height/2 + 50, 200, 60), "PLAY AGAIN"))
                {
                    RestartGame();
                }
            }
        }

        private void RestartGame()
        {
            MapGeneratorV2 generator = FindObjectOfType<MapGeneratorV2>();
            if (generator != null)
            {
                // Pick new seed (1 to 100)
                generator.seed = Random.Range(1, 101);
                
                // Regenerate
                generator.GenerateWorld();
                
                // Reset Game Logic
                StartGame();
            }
        }
    }
}
