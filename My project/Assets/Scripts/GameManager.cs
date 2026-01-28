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
            ColorPalette palette = null;
            MapGeneratorV2 generator = FindFirstObjectByType<MapGeneratorV2>();
            
            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.alignment = TextAnchor.MiddleCenter;

            // Timer
            textStyle.fontSize = 28;
            textStyle.normal.textColor = Color.white;
            string timeStr = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(currentMatchTime / 60), Mathf.FloorToInt(currentMatchTime % 60));
            GUI.Label(new Rect(Screen.width / 2 - 100, 20, 200, 50), "TIME: " + timeStr, textStyle);

            // Scores
            textStyle.fontSize = 20;
            textStyle.alignment = TextAnchor.MiddleLeft;
            GUI.color = new Color(1, 0.2f, 0); // Red Team fixed color
            GUI.Label(new Rect(40, 40, 300, 30), $"RED STATUS: {redHoldTime:F1}s / {timeToWin}s", textStyle);
            
            textStyle.alignment = TextAnchor.MiddleRight;
            GUI.color = new Color(0, 0.6f, 1); // Blue Team fixed color
            GUI.Label(new Rect(Screen.width - 340, 40, 300, 30), $"BLUE STATUS: {blueHoldTime:F1}s / {timeToWin}s", textStyle);

            // Center Status
            if (currentZoneOwner != Team.None)
            {
                textStyle.fontSize = 44;
                textStyle.alignment = TextAnchor.MiddleCenter;
                GUI.color = (currentZoneOwner == Team.Blue) ? new Color(0, 0.6f, 1) : new Color(1, 0.2f, 0);
                GUI.Label(new Rect(Screen.width/2 - 300, 100, 600, 100), $">> {currentZoneOwner} DOMINATING <<", textStyle);
            }

            // Game Over
            if (isGameOver)
            {
                GUI.color = Color.white;
                textStyle.fontSize = 72;
                textStyle.alignment = TextAnchor.MiddleCenter;
                textStyle.normal.textColor = Color.yellow;
                
                // Draw a background box for the win text
                GUI.Box(new Rect(0, Screen.height/2 - 120, Screen.width, 240), "");
                GUI.Label(new Rect(0, Screen.height/2 - 80, Screen.width, 100), winnerText, textStyle);

                // Play Again Button
                GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
                btnStyle.fontSize = 24;
                btnStyle.fontStyle = FontStyle.Bold;
                if (GUI.Button(new Rect(Screen.width/2 - 120, Screen.height/2 + 60, 240, 70), "NEXT SEED MATCH", btnStyle))
                {
                    RestartGame();
                }
            }
        }

        private void RestartGame()
        {
            MapGeneratorV2 generator = FindFirstObjectByType<MapGeneratorV2>();
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
