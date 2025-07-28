using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObjects : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up; // Set axis in Inspector
    public float rotationSpeed = 90f;         // Degrees per second

    void Update()
    {
        transform.Rotate(rotationAxis.normalized, rotationSpeed * Time.deltaTime);
    }
}
