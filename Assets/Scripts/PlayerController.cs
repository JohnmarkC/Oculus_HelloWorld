using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // Rigidbody of the player.
    private Rigidbody rb;

    // Variable to keep track of collected "PickUp" objects.
    private int count;

    // Variable to keep track of enemy health
    private int health;

    // Movement along X and Y axes.
    private float movementX;
    private float movementY;

    // Speed at which the player moves.
    public float speed = 0;

    // Reference to the XR Camera (HMD) — drag your Main Camera here in the Inspector.
    public Camera xrCamera;

    // UI text component to display count of "PickUp" objects collected.
    public TextMeshProUGUI countText;

    // UI text component to display enemy health remaining
    public TextMeshProUGUI healthText;

    // UI object to display winning text.
    public GameObject winTextObject;

    // Internal reference to the left Meta Quest controller.
    private InputDevice leftController;

    // Start is called before the first frame update.
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        health = 4;
        SetCountText();
        SetHealthDisplay();

        // Try to find the left controller on startup.
        TryInitializeController();
    }

    // Attempt to locate the left XR controller.
    void TryInitializeController()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            devices
        );

        if (devices.Count > 0)
        {
            leftController = devices[0];
        }
    }

    // FixedUpdate is called once per fixed frame-rate frame.
    private void FixedUpdate()
    {
        if (!leftController.isValid)
        {
            TryInitializeController();
            return;
        }

        // Read the left thumbstick value.
        Vector2 thumbstick = Vector2.zero;
        leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstick);

        movementX = thumbstick.x;
        movementY = thumbstick.y;

        // Get the controller's rotation and derive forward/right from it.
        Quaternion controllerRotation = Quaternion.identity;
        leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out controllerRotation);

        Vector3 controllerForward = controllerRotation * Vector3.forward;
        Vector3 controllerRight = controllerRotation * Vector3.right;

        // Flatten onto the horizontal plane so vertical tilt doesn't affect movement.
        controllerForward.y = 0f;
        controllerRight.y = 0f;
        controllerForward.Normalize();
        controllerRight.Normalize();

        Vector3 movement = (controllerForward * movementY + controllerRight * movementX);

        rb.AddForce(movement * speed);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("pickup"))
        {
            other.gameObject.SetActive(false);
            count += 1;
            SetCountText();
        }

        if (other.gameObject.CompareTag("button"))
        {
            other.gameObject.SetActive(false);
            health -= 1;
            SetHealthDisplay();
        }
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();

        if (count >= 12)
        {
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Win!";

            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                Destroy(enemy);
        }
    }

    void SetHealthDisplay()
    {
        healthText.text = "Enemy Health: " + health.ToString();

        if (health <= 0)
        {
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                Destroy(enemy);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
        }
    }
}