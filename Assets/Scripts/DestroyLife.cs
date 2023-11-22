using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyLife : MonoBehaviour
{
    public Rigidbody rigidbody;

    private void Start() {
        Destroy(gameObject, 5f);
    }

    public void deathPhysics(Vector3 dir) {
        rigidbody.AddForce(dir * 10000);
    }
}
