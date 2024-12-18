using UnityEngine;
using SuperMaze;

public class BombBehavior : MonoBehaviour
{
    public Transform player; // Reference to the player object
    public float detectionRadius = 10.0f; // Adjusted radius within which the bomb detects the player
    public float speed = 3.0f; // Adjusted speed at which the bomb follows the player
    public float catchDistance = 0.1f; // Distance at which the bomb is considered to have caught the player
    public AudioClip anxietySound; // Audio clip for the anxiety-inducing sound

    private Animator animator;
    private bool isFollowing = false;
    private MazeGenerator mazeGenerator; // Reference to the MazeGenerator script
    private AudioSource bombAudioSource; // AudioSource for the bomb

    void Start()
    {
        animator = GetComponent<Animator>();
        mazeGenerator = FindObjectOfType<MazeGenerator>(); // Find the MazeGenerator script in the scene
        bombAudioSource = GetComponent<AudioSource>(); // Get the AudioSource component
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius && !isFollowing)
        {
            isFollowing = true;
            animator.SetBool("isFollowing", true);

            // Play the anxiety-inducing sound when the bomb starts following the player
            if (bombAudioSource != null && anxietySound != null && !bombAudioSource.isPlaying)
            {
                bombAudioSource.clip = anxietySound;
                bombAudioSource.loop = true;
                bombAudioSource.Play();
            }
        }

        if (isFollowing)
        {
            if (distanceToPlayer > catchDistance)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;

                // Rotate bomb to face the player
                transform.LookAt(player);
            }
            else
            {
                // If the bomb catches the player, stop the sound and restart the game
                if (bombAudioSource != null && bombAudioSource.isPlaying)
                {
                    bombAudioSource.Stop();
                }
                mazeGenerator.RestartGame();
            }
        }
    }

    // Reset sound for bomb
    public void ResetSound()
    {
        if (bombAudioSource != null && bombAudioSource.isPlaying)
        {
            bombAudioSource.Stop();
            isFollowing = false;
            animator.SetBool("isFollowing", false);
        }
    }
}
