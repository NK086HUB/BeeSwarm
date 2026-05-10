using UnityEngine;

namespace BeeSwarm.Core
{
    /// <summary>
    /// Главный менеджер игры
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Настройки игры")]
        [SerializeField] private bool isPaused = false;
        [SerializeField] private float gameSpeed = 1f;
        
        [Header("Ссылки")]
        [SerializeField] private HiveManager hiveManager;
        
        // Синглтон
        public static GameManager Instance { get; private set; }
        
        // Свойства
        public bool IsPaused => isPaused;
        public float GameSpeed => gameSpeed;
        public HiveManager HiveManager => hiveManager;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            InitializeGame();
        }
        
        /// <summary>
        /// Инициализация игры
        /// </summary>
        private void InitializeGame()
        {
            Debug.Log("Игра Bee Swarm инициализирована");
            Debug.Log($"Максимальное количество пчёл: {hiveManager?.BeeCount ?? 0}");
            
            // Установка скорости игры
            Time.timeScale = gameSpeed;
        }
        
        /// <summary>
        /// Пауза/продолжение игры
        /// </summary>
        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : gameSpeed;
            
            Debug.Log($"Игра {(isPaused ? "на паузе" : "продолжена")}");
        }
        
        /// <summary>
        /// Установить скорость игры
        /// </summary>
        public void SetGameSpeed(float speed)
        {
            gameSpeed = Mathf.Clamp(speed, 0.1f, 5f);
            
            if (!isPaused)
            {
                Time.timeScale = gameSpeed;
            }
            
            Debug.Log($"Скорость игры установлена: {gameSpeed}x");
        }
        
        /// <summary>
        /// Сохранить игру
        /// </summary>
        public void SaveGame()
        {
            // TODO: Реализация сохранения
            Debug.Log("Игра сохранена");
        }
        
        /// <summary>
        /// Загрузить игру
        /// </summary>
        public void LoadGame()
        {
            // TODO: Реализация загрузки
            Debug.Log("Игра загружена");
        }
        
        /// <summary>
        /// Перезапустить игру
        /// </summary>
        public void RestartGame()
        {
            // TODO: Реализация перезапуска
            Debug.Log("Игра перезапущена");
        }
        
        /// <summary>
        /// Выйти из игры
        /// </summary>
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        void OnGUI()
        {
            // Простой UI для отладки
            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 200));
            
            GUILayout.Label("УПРАВЛЕНИЕ ИГРОЙ");
            
            if (GUILayout.Button(isPaused ? "ПРОДОЛЖИТЬ" : "ПАУЗА"))
            {
                TogglePause();
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Скорость:");
            if (GUILayout.Button("0.5x")) SetGameSpeed(0.5f);
            if (GUILayout.Button("1x")) SetGameSpeed(1f);
            if (GUILayout.Button("2x")) SetGameSpeed(2f);
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("СОХРАНИТЬ")) SaveGame();
            if (GUILayout.Button("ЗАГРУЗИТЬ")) LoadGame();
            if (GUILayout.Button("ПЕРЕЗАПУСТИТЬ")) RestartGame();
            if (GUILayout.Button("ВЫЙТИ")) QuitGame();
            
            GUILayout.EndArea();
        }
        
        void OnApplicationQuit()
        {
            Debug.Log("Bee Swarm - игра завершена");
        }
    }
}
