using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BoidObstacleAvoid : MonoBehaviour
{

    public Boids boids;
    private Transform boid;

    private void Start()
    {
        
        boid = transform.parent;
        boids = boid.GetComponent<Boids>();
    }


    private void OnTriggerStay(Collider other)
    {
        // Bit shift the index of the layers to get a bit mask
        int groundLayerMask = 1 << 6; // ground
        int defaultLayerMask = 1 << 0; // default
        int layerMask = groundLayerMask | defaultLayerMask; // combine both layers

        bool didHit = false;
        RaycastHit hit;
        // Does the ray intersect any objects in the layer mask
        if (Physics.Raycast(boid.position, boid.forward, out hit, 10, layerMask))
        {
            Debug.DrawRay(boid.position, boid.forward * hit.distance, Color.red);

            boids.accumAvoid(hit.point);

            didHit = true;
        }
        if (Physics.Raycast(boid.position, -boid.up, out hit, 10, layerMask))
        {
            Debug.DrawRay(boid.position, -boid.up * hit.distance, Color.red);

            boids.accumAvoid(hit.point);

            didHit = true;
        }
        if (Physics.Raycast(boid.position, boid.up, out hit, 10, layerMask))
        {
            Debug.DrawRay(boid.position, boid.up * hit.distance, Color.red);

            boids.accumAvoid(hit.point);

            didHit = true;
        }
        if (Physics.Raycast(boid.position, boid.right, out hit, 10, layerMask))
        {
            Debug.DrawRay(boid.position, boid.right * hit.distance, Color.red);

            boids.accumAvoid(hit.point);

            didHit = true;
        }
        if (Physics.Raycast(boid.position, -boid.right, out hit, 10, layerMask))
        {
            Debug.DrawRay(boid.position, -boid.right * hit.distance, Color.red);

            boids.accumAvoid(hit.point);

            didHit = true;
        }

        if (!didHit)
            boids.resetAvoid();
    }
    private void OnTriggerExit(Collider other)
    {
        boids.resetAvoid();
    }
}

