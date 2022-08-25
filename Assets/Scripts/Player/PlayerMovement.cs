using UnityEngine;

using Core;

namespace Player {

    public class PlayerMovement : MonoBehaviour {

        [SerializeField][Range(0f, 100f)] float maxSpeed = 5f;
        [SerializeField][Tooltip("How quickly the player accelerates to max speed after receiving input")][Range(0.001f, 2f)] float throttleUpTime = 0.1f;
        [SerializeField][Tooltip("How quickly the player comes to a stop after no input")][Range(0.001f, 2f)] float throttleDownTime = 0.05f;
        [SerializeField][Tooltip("How fast the player can change directions")][Range(0.001f, 2f)] float speedDelta = 0.1f;
        [SerializeField][Tooltip("How fast the player can rotate (degrees / sec)")][Range(0f, 1080f)] float rotateSpeed = 720f;

        // cached
        PlayerController controller;
        Rigidbody2D rb;

        // props
        float initialDrag;

        // state
        float throttle; // 0.0 to 1.0 --> what percentage of maxSpeed to move the player
        Vector2 desiredVelocity;
        Vector2 prevVelocity;
        Vector2 currentForces;
        Quaternion desiredHeading;

        void Awake() {
            controller = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody2D>();
            initialDrag = rb.drag;
        }

        void Update() {
            SetThrottle();
            SetDrag();
        }

        void FixedUpdate() {
            HandleRotate();
            HandleMove();
        }

        void SetDrag() {
            rb.drag = HasMoveInput() ? 0f : initialDrag;
        }

        void SetThrottle() {
            throttle = HasMoveInput() ? throttle + Time.deltaTime / throttleUpTime : throttle - Time.deltaTime / throttleDownTime;
            throttle = Mathf.Clamp01(throttle);
        }

        void HandleMove() {
            currentForces = rb.velocity - prevVelocity;
            desiredVelocity = controller.Move * maxSpeed * throttle;
            rb.velocity = Vector2.MoveTowards(rb.velocity, desiredVelocity, speedDelta);
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
            rb.velocity += currentForces;
            prevVelocity = rb.velocity;
        }

        void HandleRotate() {
            if (!HasMoveInput()) return;
            desiredHeading = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.down, Utils.GetNearestCardinal(controller.Move)));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredHeading, rotateSpeed * Time.deltaTime);
        }

        bool HasMoveInput() {
            return controller.Move.magnitude > float.Epsilon;
        }
    }
}
