using UnityEngine;

namespace SuperMaze {
    public class BallController : MonoBehaviour {
        public float bounceForce = 0.5f; // Reduced force applied when the ball hits the wall
        public float maxVelocity = 3f; // Maximum allowed velocity

        private Rigidbody rb;

        void Start() {
            rb = GetComponent<Rigidbody>();
        }

        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.CompareTag("Wall")) {
                // Calculate bounce direction
                Vector3 bounceDirection = Vector3.Reflect(rb.velocity.normalized, collision.contacts[0].normal);

                // Apply a smaller force to prevent the ball from bouncing too far
                rb.velocity = bounceDirection * bounceForce;

                // Optionally, to just stop the ball on collision, uncomment the next two lines
                // rb.velocity = Vector3.zero;
                // rb.angularVelocity = Vector3.zero;
            }
        }

        void Update() {
            // Limit the velocity to prevent the ball from going too fast
            if (rb.velocity.magnitude > maxVelocity) {
                rb.velocity = rb.velocity.normalized * maxVelocity;
            }
        }
    }
}
