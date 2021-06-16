using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static LunarSurface terrainInstance;
    public static TargetSelector targetSelector;

    private float backgroundWidth = 1600f;
    private float backgroundHeight = 900f;
    private float startTime;
    public bool debuggingIsEnabled = false;
    public bool triggerDomeResize = false;
    [SerializeField] private Vector3 gravityMode1, gravityMode2;

    public enum GameState
    {
        Menu,
        Play,
        Pause,
        Spectate,
        End,
        Close
    }

    public static GameState currentState;

    [SerializeField] private GameObject mainMenuInterface;
    [SerializeField] private GameObject pauseMenuInterface;
    [SerializeField] private EndMenuInterface endMenuInterface;
    public GameObject editorInterface;
    public GameObject activeInterface; // MUST BE SET EXTERNALLY!!!

    [SerializeField] private Button mainMenuPlay;
    [SerializeField] private Button mainMenuQuit;

    [SerializeField] private Button pauseMenuResume;
    [SerializeField] private Button pauseMenuSpectate;
    [SerializeField] private Button pauseMenuQuit;

    [SerializeField] private Button endMenuPlayAgain;
    [SerializeField] private Button endMenuQuit;

    [SerializeField] private EnemyCar enemyPrefab;

    private bool spawnIsScheduled = false;
    private bool reloadIsScheduled = false;

    public Slider[] sliders;
    public Text[] sliderTexts;

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    // Ensure the background image always fills the screen:
    private void ScaleInterface(GameObject userInterface)
    {
        if (userInterface != null)
        {
            float targetScaleX = Screen.width / backgroundWidth;
            float targetScaleY = Screen.height / backgroundHeight;

            /*
            // Preserve the aspect ratio:
            if (targetScaleX > targetScaleY)
            {
                targetScaleX = targetScaleY;
            }
            else
            {
                targetScaleY = targetScaleX;
            }*/

            userInterface.transform.localScale = new Vector3(targetScaleX, targetScaleY, 1f);
        }
    }

    public void ToggleGravityMode() {
        if (Physics.gravity == gravityMode1) Physics.gravity = gravityMode2;
        else Physics.gravity = gravityMode1;
    }

    public int NumberOfEnemiesAlive() {
        return targetSelector.returnQuantity() - 1;
    }

    // Listener for the "Play Again" button:
    public void ScheduleReload()
    {
        endMenuInterface.gameObject.SetActive(false);
        reloadIsScheduled = true;
    }

    // Spawn 7 enemies around the crater in a ring:
    public void SpawnEnemies()
    {
        float spawnRadius = terrainInstance.GetCraterRadius() * 0.8f;
        float centerOffset = terrainInstance.GetCenterOffset();

        for (int i = 8; i < 18; i++)
        {
            float spawnAngle = 2f * Mathf.PI * (float)i / 11f;
            float spawnX = spawnRadius * Mathf.Cos(spawnAngle) + centerOffset;
            float spawnZ = spawnRadius * Mathf.Sin(spawnAngle) + centerOffset;
            float spawnY = terrainInstance.GetExactHeight(spawnX, spawnZ) + 2f;

            Vector3 spawnLocation = new Vector3(spawnX, spawnY, spawnZ);
            Quaternion spawnDirection = Quaternion.Euler(0f, -spawnAngle * Mathf.Rad2Deg - 90f, 0f);

            Instantiate(enemyPrefab, spawnLocation, spawnDirection);
        }

        spawnIsScheduled = false;
    }

    public void Halt()
    {
        // If the game has not already ended
        if (currentState == GameState.Play)
        {
            currentState = GameState.End;

            mainMenuInterface.SetActive(false);
            pauseMenuInterface.SetActive(false);
            editorInterface.SetActive(false);
            endMenuInterface.gameObject.SetActive(true);

            PlayerCar playerVehicle = targetSelector.GetPlayer();
            int playerHealth;

            if (playerVehicle == null)
            {
                playerHealth = 0;
            }
            else
            {
                playerHealth = playerVehicle.currentHealth;
            }

            // Set the win/lose condition based on whether the player is still alive:
            endMenuInterface.SetCondition(playerHealth > 0);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void BeginMenu()
    {
        targetSelector.ClearAll();
        currentState = GameState.Menu;
        SceneManager.LoadScene(0);
        Time.timeScale = 1;

        mainMenuInterface.SetActive(true);
        pauseMenuInterface.SetActive(false);
        editorInterface.SetActive(false);
        endMenuInterface.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Begin a new game by loading the LunarDerby scene:
    private void BeginPlay()
    {
        Physics.gravity = gravityMode2;
        mainMenuInterface.SetActive(false);
        pauseMenuInterface.SetActive(false);
        editorInterface.SetActive(false);
        endMenuInterface.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentState = GameState.Play;
        SceneManager.LoadScene(1);
        Time.timeScale = 1;

        spawnIsScheduled = true;
        reloadIsScheduled = false;
        triggerDomeResize = true;
    }

    // Exit from a pause without reloading the scene:
    private void ResumePlay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentState = GameState.Play;
        Time.timeScale = 1;

        pauseMenuInterface.SetActive(false);
        editorInterface.SetActive(false);
    }

    private void BeginPause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        currentState = GameState.Pause;
        Time.timeScale = 0;

        pauseMenuInterface.SetActive(true);
    }

    private void BeginSpectate()
    {
        // Keep the game paused, but remove visible pause menu interfaces.
        // In this state, the camera script will enable mouse control to look around.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentState = GameState.Spectate;
        pauseMenuInterface.SetActive(false);
        editorInterface.SetActive(false);
    }

    private void BeginClose()
    {
        targetSelector.ClearAll();

        currentState = GameState.Close;
        SceneManager.LoadScene(2);
        Time.timeScale = 1;

        mainMenuInterface.SetActive(false);
        pauseMenuInterface.SetActive(false);
        editorInterface.SetActive(false);
        endMenuInterface.gameObject.SetActive(false);

        startTime = Time.time;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void PresetSlider(int index, float minValue, float maxValue, float preset)
    {
        sliders[index].minValue = minValue;
        sliders[index].maxValue = maxValue;
        sliders[index].value = preset;
    }

    // Ensure that only one object of this class can exist:
    private void Awake()
    {
        if (instance == null || instance == this)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        Physics.gravity = gravityMode2;
        PresetSlider(0, 8f, 32f, 16f);    // Crater radius
        PresetSlider(1, 0f, 16f, 6.05f);  // Crater depth
        PresetSlider(2, 1f, 16f, 8f);     // Crater edge decay rate
        PresetSlider(3, 0f, 1f, 0.42f);   // Crater edge ratio above ground
        PresetSlider(4, 1f, 32f, 16f);    // Coarse noise coefficient
        PresetSlider(5, 0f, 16f, 2.9f);   // Coarse noise maximum magnitude
        PresetSlider(6, 32f, 128f, 64f);  // Fine noise coefficient
        PresetSlider(7, 0f, 4f, 0.8f);    // Fine noise maximum magnitude

        // Main menu UI buttons:
        mainMenuPlay.onClick.AddListener(BeginPlay);
        mainMenuQuit.onClick.AddListener(BeginClose);

        // Pause menu UI buttons:
        pauseMenuResume.onClick.AddListener(ResumePlay);
        pauseMenuSpectate.onClick.AddListener(BeginSpectate);
        pauseMenuQuit.onClick.AddListener(BeginMenu);

        // End menu UI buttons:
        endMenuPlayAgain.onClick.AddListener(ScheduleReload);
        endMenuQuit.onClick.AddListener(BeginMenu);

        targetSelector = new TargetSelector(16);
        BeginMenu();

        startTime = Time.time;
    }

    // Handling the main menu inputs
    private void UpdateMenu()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            BeginClose();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            BeginPlay();
        }
        else
        {
            ScaleInterface(mainMenuInterface);
        }
    }

    // Global gameplay input handler:
    private void UpdatePlay()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKey(KeyCode.Escape))
        {
            // Enter the normal pause menu:
            BeginPause();
            editorInterface.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            BeginPause();
            editorInterface.SetActive(false);
            BeginSpectate();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            // Enter the pause menu with terrain editing options:
            BeginPause();
            editorInterface.SetActive(true);
            debuggingIsEnabled = true;
        }
        else
        {
            ScaleInterface(activeInterface);

            if (spawnIsScheduled && terrainInstance != null)
            {
                SpawnEnemies();
            }
        }
    }

    // Pause menu input handler:
    private void UpdatePause()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            BeginMenu(); // Quit to main menu
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            ResumePlay(); // Unpause the game
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {

            BeginSpectate();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            // Toggle visibility of terrain editor controls, but stay in the pause menu:
            editorInterface.SetActive(!editorInterface.gameObject.activeSelf);
            debuggingIsEnabled = true;
        }
        else
        {
            ScaleInterface(pauseMenuInterface);

            if (editorInterface.gameObject.activeSelf)
            {
                ScaleInterface(editorInterface);
            }
        }
    }

    // Spectate has similar keyboard funtionality as the pause state, but without visible menus:
    private void UpdateSpectate()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            BeginMenu(); // Quit to main menu
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            ResumePlay();
        }
        else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKey(KeyCode.Escape))
        {
            BeginPause(); // Stay paused, but restore the interfaces
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            // Enter the pause menu with terrain editing options:
            BeginPause();
            editorInterface.SetActive(true);
            debuggingIsEnabled = true;
        }
    }

    // Handling the end menu inputs
    private void UpdateEnd()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            BeginMenu();
        }
        else if (Input.GetKeyDown(KeyCode.P) || reloadIsScheduled)
        {
            reloadIsScheduled = true;
            BeginClose();
        }
        else
        {
            ScaleInterface(endMenuInterface.gameObject);
        }
    }

    // Exit the program:
    private void UpdateClose()
    {
        if (reloadIsScheduled)
        {
            BeginPlay();
        }
        else if (Time.time - startTime > 0.8f)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    // Select the applicable Update function based on the current state:
    private void Update()
    {
        // **********************************************************
        // FOR TESTING PURPOSES ONLY; TO BE REMOVED
        // if (Input.GetKeyDown(KeyCode.H))
        // {
        //     Halt();
        // }
        // **********************************************************

        switch (currentState)
        {
            case GameState.Menu:
                {
                    UpdateMenu();
                    break;
                }
            case GameState.Play:
                {
                    UpdatePlay();
                    break;
                }
            case GameState.Pause:
                {
                    UpdatePause();
                    break;
                }
            case GameState.Spectate:
                {
                    UpdateSpectate();
                    break;
                }
            case GameState.End:
                {
                    UpdateEnd();
                    break;
                }
            default: // Closing state:
                {
                    UpdateClose();
                    break;
                }
        }
    }
}
