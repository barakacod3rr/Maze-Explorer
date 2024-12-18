using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

namespace SuperMaze
{
    public class MazeGenerator : MonoBehaviour
    {
        // Public variables
        public int cellsWide = 5;
        public int cellsDeep = 5;
        public float cellSize = 1.0f;
        public float wallThickness = 0.2f;
        public float wallHeight = 1.0f;
        public bool floorOn = true;
        public bool ceilingOn = false;
        public Material wallMaterial;
        public Material floorMaterial;
        public Material ceilingMaterial;
        public Material goldMaterial; // Gold material for destination
        public float textureScale = 1.0f;

        public GameObject Player;
        public GameObject bombPrefab; // Bomb prefab

        public AudioClip gameStartSound; // Audio clip for the game start sound
        public TextMeshProUGUI countdownText; // Reference to the TMP Text object
        public Light sceneLight; // Reference to the Light object
        public Color detectionColor = Color.red; // Color when the player is within detection radius
        public Color normalColor = Color.white; // Color when the player is outside detection radius
        public float detectionRadius = 25.0f; // Radius within which the light color changes
        public float blinkInterval = 0.2f; // Interval at which the light blinks

        // Private variables
        private int[,] grid;
        private bool[,] visited;
        private int width;
        private int depth;
        private Vector2 destination;
        private GameObject destinationTile;

        private float timer = 60f; // 60 seconds timer
        private bool gameWon = false;
        private bool isBlinking = false; // Flag to check if the light is currently blinking

        private int complexity = 1;

        private AudioSource playerAudioSource; // AudioSource for the player
        private Coroutine blinkCoroutine; // Reference to the blink coroutine

        // Initialize
        void Start()
        {
            // Find the TMP Text object in the scene
            countdownText = GameObject.Find("CountdownTimer").GetComponent<TextMeshProUGUI>();

            // Get the AudioSource component from the player
            playerAudioSource = Player.GetComponent<AudioSource>();

            // Play the game start sound
            PlayGameStartSound();

            // Optional seed value for the random generation
            // If seed == 0, then the seed will be randomized
            int seed = 0;

            // Calling main Maze Generation Function
            GenerateMaze(seed);
            SetDestination();
            InstantiateBomb(); // Instantiate the bomb
        }

        void PlayGameStartSound()
        {
            if (playerAudioSource != null && gameStartSound != null)
            {
                playerAudioSource.clip = gameStartSound;
                playerAudioSource.Play();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!gameWon)
            {
                timer -= Time.deltaTime;

                // Update the countdown text
                if (countdownText != null)
                {
                    countdownText.text = "Time Left: " + Mathf.Ceil(timer).ToString();
                }

                if (timer <= 0)
                {
                    RestartGame();
                }

                CheckWinCondition();
                CheckLightChangeCondition();
            }
        }

        void CheckLightChangeCondition()
        {
            float distanceToDestination = Vector3.Distance(Player.transform.position, destinationTile.transform.position);
            if (distanceToDestination <= detectionRadius)
            {
                if (!isBlinking)
                {
                    blinkCoroutine = StartCoroutine(BlinkLight());
                    isBlinking = true;
                }
            }
            else
            {
                if (isBlinking)
                {
                    StopCoroutine(blinkCoroutine);
                    ChangeLightColor(normalColor);
                    isBlinking = false;
                }
            }
        }

        IEnumerator BlinkLight()
        {
            while (true)
            {
                ChangeLightColor(detectionColor);
                yield return new WaitForSeconds(blinkInterval);
                ChangeLightColor(normalColor);
                yield return new WaitForSeconds(blinkInterval);
            }
        }

        void ChangeLightColor(Color newColor)
        {
            if (sceneLight != null)
            {
                sceneLight.color = newColor;
            }
        }

        // Main Maze Generation Function
        public void GenerateMaze(int seed)
        {
            if (seed != 0) Random.InitState(seed);
            width = Mathf.Max(width, 1);
            depth = Mathf.Max(depth, 1);
            width = cellsWide * 2 + 1;
            depth = cellsDeep * 2 + 1;
            grid = new int[width, depth];
            visited = new bool[width, depth];

            ArrayList validCells = new ArrayList();
            int currentX = 1;
            int currentZ = 1;
            grid[currentX, currentZ] = 1;

            int its = 0;
            while (!AllVisited())
            {
                its++;
                visited[currentX, currentZ] = true;

                if (UnvisitedNeighbours(currentX, currentZ) > 0)
                {
                    validCells.Add(new Vector2(currentX, currentZ));
                }
                else
                {
                    for (int i = validCells.Count - 1; i >= 0; i--)
                    {
                        Vector2 cell = (Vector2)validCells[i];
                        if (UnvisitedNeighbours(Mathf.RoundToInt(cell.x), Mathf.RoundToInt(cell.y)) > 0)
                        {
                            currentX = Mathf.RoundToInt(cell.x);
                            currentZ = Mathf.RoundToInt(cell.y);
                        }
                        else
                        {
                            validCells.RemoveAt(i);
                        }
                    }
                }

                int oldX = currentX;
                int oldZ = currentZ;

                if (Random.value < 0.5f)
                {
                    int stepX = 2;
                    if (Random.value < 0.5f) stepX = -2;
                    currentX += stepX;
                }
                else
                {
                    int stepZ = 2;
                    if (Random.value < 0.5f) stepZ = -2;
                    currentZ += stepZ;
                }
                currentX = Mathf.Clamp(currentX, 1, width - 2);
                currentZ = Mathf.Clamp(currentZ, 1, depth - 2);

                grid[currentX, currentZ] = 1;

                if (!visited[currentX, currentZ])
                {
                    int wallX = (oldX + currentX) / 2;
                    int wallZ = (oldZ + currentZ) / 2;
                    grid[wallX, wallZ] = 1;
                }
            }

            if (floorOn)
            {
                GameObject floor = MakeFloor();
            }
            if (ceilingOn)
            {
                GameObject ceiling = MakeFloor();
                ceiling.transform.position += new Vector3(0, wallHeight + 0.2f, 0);
                if (ceilingMaterial != null) ceiling.GetComponent<Renderer>().material = ceilingMaterial;
            }
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    float xPos = x * 0.5f * cellSize;
                    float zPos = z * 0.5f * cellSize;
                    float blockWidth = cellSize - wallThickness;
                    if (x % 2 == 0) blockWidth = wallThickness;
                    float blockDepth = cellSize - wallThickness;
                    if (z % 2 == 0) blockDepth = wallThickness;

                    if (grid[x, z] == 0)
                    {
                        MakeBlock(new Vector3(xPos, wallHeight * 0.5f, zPos), new Vector3(blockWidth, wallHeight, blockDepth));
                    }
                }
            }
        }

        void SetDestination()
        {
            if (destinationTile != null)
            {
                Destroy(destinationTile);
            }

            destination = new Vector2(width - 2, depth - 2);
            Vector3 destinationPosition = new Vector3(destination.x * 0.5f * cellSize, 0.05f, destination.y * 0.5f * cellSize);
            destinationTile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            destinationTile.transform.position = destinationPosition;
            destinationTile.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
            destinationTile.GetComponent<Renderer>().material = goldMaterial;
        }

        void CheckWinCondition()
        {
            Vector2 PlayerPosition = new Vector2(Mathf.Floor(Player.transform.position.x / (cellSize * 0.5f)), Mathf.Floor(Player.transform.position.z / (cellSize * 0.5f)));
            if (PlayerPosition == destination)
            {
                gameWon = true;
                StartCoroutine(NextLevel());
            }
        }

        IEnumerator NextLevel()
        {
            yield return new WaitForSeconds(2);
            complexity++;
            DestroyMaze();
            GenerateMaze(0);
            SetDestination();
            timer = 60f;
            gameWon = false;

            Player.transform.position = new Vector3(0.5f * cellSize, Player.transform.position.y, 0.5f * cellSize);

            PlayGameStartSound();

            ResetBombSounds();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        bool AllVisited()
        {
            int visitedCells = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (x % 2 == 0 || z % 2 == 0) visited[x, z] = true;
                    if (!visited[x, z])
                    {
                        return false;
                    }
                    visitedCells++;
                }
            }
            return true;
        }

        int UnvisitedNeighbours(int x, int z)
        {
            int unvisitedNeighbours = 0;
            if (x > 1 && !visited[x - 2, z]) unvisitedNeighbours++;
            if (x < width - 2 && !visited[x + 2, z]) unvisitedNeighbours++;
            if (z > 1 && !visited[x, z - 2]) unvisitedNeighbours++;
            if (z < depth - 2 && !visited[x, z + 2]) unvisitedNeighbours++;
            return unvisitedNeighbours;
        }

        GameObject MakeFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.parent = transform;
            float sizeX = cellSize * (width - 1);
            float sizeZ = cellSize * (depth - 1);
            float posY = -0.1f;
            floor.transform.localScale = new Vector3(sizeX * 0.5f, 0.2f, sizeZ * 0.5f);
            if (floorMaterial != null) floor.GetComponent<Renderer>().material = floorMaterial;
            floor.transform.localPosition = new Vector3(sizeX * 0.25f, posY, sizeZ * 0.25f);
            WorldUvs(floor);
            return floor;
        }

        void MakeBlock(Vector3 pos, Vector3 size)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.parent = transform;
            block.transform.localPosition = pos;
            block.transform.localScale = size;
            if (wallMaterial != null) block.GetComponent<Renderer>().material = wallMaterial;
            WorldUvs(block);
            block.AddComponent<BoxCollider>();
            block.tag = "Wall";
        }

        void WorldUvs(GameObject go)
        {
            Mesh mesh = go.transform.GetComponent<MeshFilter>().mesh;
            Vector2[] uvs = new Vector2[mesh.uv.Length];
            uvs = mesh.uv;
            Vector2[] newsUv = new Vector2[mesh.uv.Length];
            Vector3 size = go.transform.localScale;
            Vector3 pos = go.transform.position;

            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                if (Mathf.Abs(mesh.normals[i].y) > 0.5f)
                {
                    newsUv[i] = new Vector2(textureScale * (mesh.vertices[i].x * size.x + pos.x), textureScale * (mesh.vertices[i].z * size.z + pos.z));
                }
                if (Mathf.Abs(mesh.normals[i].x) > 0.5f)
                {
                    newsUv[i] = new Vector2(textureScale * (mesh.vertices[i].z * size.z + pos.z), textureScale * (mesh.vertices[i].y * size.y + pos.y));
                }
                if (Mathf.Abs(mesh.normals[i].z) > 0.5f)
                {
                    newsUv[i] = new Vector2(textureScale * (mesh.vertices[i].x * size.x + pos.z), textureScale * (mesh.vertices[i].y * size.y + pos.y));
                }
            }

            mesh.uv = newsUv;
        }

        void DestroyMaze()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        void InstantiateBomb()
        {
            Vector3 bombPosition = new Vector3(Random.Range(1, cellsWide) * cellSize, 0.5f, Random.Range(1, cellsDeep) * cellSize);
            GameObject bombInstance = Instantiate(bombPrefab, bombPosition, Quaternion.identity);
            bombInstance.GetComponent<BombBehavior>().player = Player.transform;
        }

        void ResetBombSounds()
        {
            BombBehavior[] bombs = FindObjectsOfType<BombBehavior>();
            foreach (BombBehavior bomb in bombs)
            {
                bomb.ResetSound();
            }
        }
    }
}
