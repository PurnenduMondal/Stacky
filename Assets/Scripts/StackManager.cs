using UnityEngine;
using TMPro;
using System.Collections;
using UnityEditor.Rendering.LookDev;

public class StackManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject blockPrefab;
    public float blockHeight = 0.5f;
    public float moveSpeed = 3f;
    public float speedIncreasePerBlock = 0.1f;

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    private GameObject lastBlock;       // The block below (reference point)
    private GameObject currentBlock;    // The moving block
    private int score = 0;
    private float stackHeight = 0f;
    private bool moveOnX = true;
    private float hue = 0f;
    public float hueShiftPerBlock = 0.05f;
    public TextMeshProUGUI perfectText;
    private float perfectThreshold = 0.1f;
    public static StackManager Instance;

    private float inputDelay = 0.3f;
    private float inputTimer = 0f;
    private bool inputReady = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
    }

    public void StartGame()
    {
        score = 0;
        moveSpeed = 3f;
        hue = 0f;
        moveOnX = true;
        inputReady = false;
        inputTimer = 0f;

        if (scoreText != null) scoreText.text = "0";

        lastBlock = GameObject.Find("BaseBlock");
        SpawnNextBlock();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameStarted()) return;

        // Wait a short delay before accepting input
        if (!inputReady)
        {
            inputTimer += Time.deltaTime;
            if (inputTimer >= inputDelay)
                inputReady = true;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlaceBlock();
        }
    }

    public void ResumeAfterRevive()
    {
        inputReady = false;
        inputTimer = 0f;
        SpawnNextBlock();
    }

    IEnumerator ShowPerfect()
    {
        perfectText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        perfectText.gameObject.SetActive(false);
    }

    void SetBlockColour(GameObject block)
    {
        Color colour = Color.HSVToRGB(hue, 0.7f, 0.9f);
        hue = (hue + hueShiftPerBlock) % 1f;

        Renderer rend = block.GetComponent<Renderer>();
        if (rend != null)
        {
            // Create a unique material instance so each block has its own colour
            rend.material = new Material(rend.sharedMaterial);
            rend.material.color = colour;
        }
    }

    void SpawnNextBlock()
    {
        float lastY = lastBlock.transform.position.y;
        float lastH = lastBlock.transform.localScale.y;
        float spawnY = lastY + lastH / 2f + blockHeight / 2f;

        float lastWidth = lastBlock.transform.localScale.x;
        float lastDepth = lastBlock.transform.localScale.z;

        // Spawn position depends on which axis it will move on
        Vector3 spawnPos = moveOnX
            ? new Vector3(-3f, spawnY, lastBlock.transform.position.z)
            : new Vector3(lastBlock.transform.position.x, spawnY, -3f);

        currentBlock = Instantiate(blockPrefab, spawnPos, Quaternion.identity);
        SetBlockColour(currentBlock);
        currentBlock.transform.localScale = new Vector3(lastWidth, blockHeight, lastDepth);

        BlockController bc = currentBlock.AddComponent<BlockController>();
        bc.speed = moveSpeed;
        bc.moveOnX = moveOnX; // tell the block which axis to use
    }

    void PlaceBlock()
    {
        if (currentBlock == null) return;

        BlockController bc = currentBlock.GetComponent<BlockController>();
        bc.Stop();

        float lastX = lastBlock.transform.position.x;
        float lastZ = lastBlock.transform.position.z;
        float currX = currentBlock.transform.position.x;
        float currZ = currentBlock.transform.position.z;
        float lastWidth = lastBlock.transform.localScale.x;
        float lastDepth = lastBlock.transform.localScale.z;
        float currWidth = currentBlock.transform.localScale.x;
        float currDepth = currentBlock.transform.localScale.z;

        float overlap;
        float newX = currX;
        float newZ = currZ;

        if (moveOnX)
        {
            // Slicing on X axis
            overlap = (lastWidth / 2 + currWidth / 2) - Mathf.Abs(currX - lastX);

            if (overlap <= 0)
            {
                GameOver(); return;
            }

            newX = lastX + (currX - lastX) / 2f;
            newZ = lastZ;

            // Resize on X
            currentBlock.transform.localScale = new Vector3(overlap, blockHeight, currDepth);
            currentBlock.transform.position = new Vector3(newX, currentBlock.transform.position.y, newZ);

            SpawnChunk(currX, currZ, currWidth, currDepth, overlap, currDepth, newX, newZ, true);
        }
        else
        {
            // Slicing on Z axis
            overlap = (lastDepth / 2 + currDepth / 2) - Mathf.Abs(currZ - lastZ);

            if (overlap <= 0)
            {
                GameOver(); return;
            }

            newZ = lastZ + (currZ - lastZ) / 2f;
            newX = lastX;

            // Resize on Z
            currentBlock.transform.localScale = new Vector3(currWidth, blockHeight, overlap);
            currentBlock.transform.position = new Vector3(newX, currentBlock.transform.position.y, newZ);

            SpawnChunk(currX, currZ, currWidth, currDepth, currWidth, overlap, newX, newZ, false);
        }

        if (moveOnX && Mathf.Abs(currX - lastX) < perfectThreshold)
        {
            StartCoroutine(ShowPerfect());
        }
        else if (!moveOnX && Mathf.Abs(currZ - lastZ) < perfectThreshold)
        {
            StartCoroutine(ShowPerfect());
        }

        if (overlap < 0.5f)
        {
            StartCoroutine(CameraShake.Instance.Shake(0.15f, 0.08f));
        }

        // Update score
        score++;
        if (scoreText != null)
            scoreText.text = score.ToString();

        // Increase speed
        moveSpeed += speedIncreasePerBlock;

        // Alternate axis for next block
        moveOnX = !moveOnX;

        lastBlock = currentBlock;
        transform.position = new Vector3(0, lastBlock.transform.position.y, 0);
        SpawnNextBlock();
    }

    void SpawnChunk(float currX, float currZ, float currWidth, float currDepth,
                float overlapW, float overlapD, float newX, float newZ, bool slicedOnX)
    {
        float chunkWidth, chunkDepth, chunkX, chunkZ;

        if (slicedOnX)
        {
            chunkWidth = currWidth - overlapW;
            chunkDepth = currDepth;
            chunkX = (currX > newX)
                ? newX + overlapW / 2f + chunkWidth / 2f
                : newX - overlapW / 2f - chunkWidth / 2f;
            chunkZ = newZ;
        }
        else
        {
            chunkWidth = currWidth;
            chunkDepth = currDepth - overlapD;
            chunkX = newX;
            chunkZ = (currZ > newZ)
                ? newZ + overlapD / 2f + chunkDepth / 2f
                : newZ - overlapD / 2f - chunkDepth / 2f;
        }

        GameObject chunk = Instantiate(blockPrefab,
            new Vector3(chunkX, currentBlock.transform.position.y, chunkZ),
            Quaternion.identity);

        chunk.transform.localScale = new Vector3(chunkWidth, blockHeight, chunkDepth);

        Rigidbody rb = chunk.AddComponent<Rigidbody>();
        rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);

        // Random spin for visual flair
        rb.AddTorque(new Vector3(
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f)
        ), ForceMode.Impulse);

        Destroy(chunk, 2f);
    }
    void GameOver()
    {
        Debug.Log("GAME OVER");
        Rigidbody rb = currentBlock.AddComponent<Rigidbody>();
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        // Notify GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.ShowGameOver(score);
    }
}