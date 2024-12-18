using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    public ParticleSystem dustParticleSystem; // Reference to the dust particle system

    void Start()
    {
        if (dustParticleSystem == null)
        {
            Debug.LogError("Dust Particle System not assigned in PlayerCollisionHandler script!");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            EmitDustParticles(collision.contacts[0].point);
        }
    }

    void EmitDustParticles(Vector3 position)
    {
        if (dustParticleSystem != null)
        {
            dustParticleSystem.transform.position = position;
            dustParticleSystem.Play();
        }
    }
}
