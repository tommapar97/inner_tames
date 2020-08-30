﻿using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    // Components
    private new Rigidbody rigidbody;

    public new string name;

    // Movement
    public Vector3 initialVelocity;
    public Vector3 Position => rigidbody.position;
    public float Mass => rigidbody.mass;

    // I want the Sun to always be at 0, 0, 0. I can do it with moving sun, but it will ease the numbers
    public bool isStationary;

    // Nested objects
    private Orbit orbit;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        orbit = CreateOrbit();

        rigidbody.velocity = initialVelocity;
    }

    private Orbit CreateOrbit()
    {
        return new Orbit(rigidbody.position, Color.white);
    }

    private void FixedUpdate()
    {
        if (!isStationary)
        {
            orbit.Draw();
            orbit.Update(this);
        }
    }

    public void ApplyGravity(Vector3 gravityForce)
    {
        rigidbody.AddForce(gravityForce);
    }
}