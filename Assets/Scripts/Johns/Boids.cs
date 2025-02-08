using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : MonoBehaviour
{
    [Tooltip("The flock that this boid belongs to.")]
    [SerializeField] Transform flock;

    [Tooltip("Factor that influences the boid's tendency to move towards the center of the flock.")]
    public float cohesionFactor = 0.2f;
    [Tooltip("Factor that influences the boid's tendency to avoid other boids.")]
    public float separationFactor = 6.0f;
    [Tooltip("Factor that influences the boid's tendency to align its velocity with the flock.")]
    public float allignFactor = 1.0f;
    [Tooltip("Factor that influences the boid's tendency to stay within a certain area.")]
    public float constrainFactor = 2.0f;
    [Tooltip("Factor that influences the boid's tendency to avoid obstacles.")]
    public float avoidFactor = 20.0f;
    [Tooltip("The distance within which the boid will try to avoid collisions with other boids.")]
    public float collisionDistance = 6.0f;
    [Tooltip("The speed at which the boid moves.")]
    public float speed = 6.0f;
    [Tooltip("The point that the boid will try to stay close to.")]
    public Vector3 constrainPoint;
    [Tooltip("The vector representing the boid's avoidance direction.")]
    public Vector3 avoidObst;
    [Tooltip("The rate at which the boid integrates its velocity changes.")]
    public float integrationRate = 3.0f;
    
    //final velocity
    [Tooltip("The current velocity of the boid.")]
    public Vector3 velocity;
    
    //states
     [Tooltip("Indicates whether the boid is flocking or targeting something.")]
    public bool isFlocking = true;
    [Tooltip("The target that the boid will move towards when not flocking.")]
    public Transform target;
    
   
    float avoidCount;


    // Start is called before the first frame update
    void Start()
    {
        flock = transform.parent;
        

        Vector3 pos = new Vector3(Random.Range(0f, 80), Random.Range(0f, 20f), Random.Range(0f, 80));
        Vector3 look = new Vector3(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
        float speed = Random.Range(0f, 3f);


        transform.localPosition = pos;
        transform.LookAt(look);
        velocity = (look - pos) * speed;

        

    }

    // Update is called once per frame
    void Update()
    {
        

        if (isFlocking)
        {
            constrainPoint = flock.position;  //flock folows player

            Vector3 newVelocity = new Vector3(0, 0, 0);
            // rule 1 all boids steer towards center of mass - cohesion
            newVelocity += cohesion() * cohesionFactor;
    
            // rule 2 all boids steer away from each other - avoidance        
            newVelocity += separation() * separationFactor;
    
            // rule 3 all boids match velocity - allignment
            newVelocity += align() * allignFactor;
    
            newVelocity += constrain() * constrainFactor;
    
            newVelocity += avoid() * avoidFactor;
           
            Vector3 slerpVelo = Vector3.Slerp(velocity, newVelocity, Time.deltaTime * integrationRate);

            velocity = slerpVelo.normalized;
    
            transform.position += velocity * Time.deltaTime * speed;
            transform.LookAt(transform.position + velocity);

        }
        else if(target)  
        {

            Debug.Log("Attacking");

            //if not flocking, its going for a target, usually attacking
            Vector3 newVelocity = target.position - transform.position;

            Vector3 slerpVelo = Vector3.Slerp(newVelocity, velocity, Time.deltaTime * integrationRate);

            velocity = slerpVelo.normalized;

            transform.position += velocity * Time.deltaTime * speed;
            transform.LookAt(transform.position + velocity);

            if(Vector3.Distance(transform.position, target.position) < 0.3f)
            {
                //Attack successfull, do damage, fly away
                Debug.Log("Hit Target");
                isFlocking = true;
                
            }
        }
    }

    Vector3 avoid()
    {

        if (avoidCount > 0)
        {
            return (avoidObst / avoidCount).normalized ;
        }

        return Vector3.zero;
    }
    
    Vector3 constrain()
    {
        Vector3 steer = new Vector3(0, 0, 0);

        steer += (constrainPoint - transform.position);

        steer.Normalize();

        return steer;
    }

    Vector3 cohesion()
    {
        Vector3 steer = new Vector3(0, 0, 0);

        int sibs = 0;           //count the boids, it might change

        foreach (Transform boid in flock)
        {
            if (boid != transform)
            {
                steer += boid.transform.position;
                sibs++;
            }

        }

        steer /= sibs; //center of mass is the average position of all        

        steer -= transform.position;

        steer.Normalize();


        return steer;
    }

    Vector3 separation()
    {
        Vector3 steer = new Vector3(0, 0, 0);

        int sibs = 0;


        foreach (Transform boid in flock)
        {
            // if boid is not itself
            if (boid != transform)
            {
                // if this boids position is within the collision distance of a neighbouring boid
                if ((transform.position - boid.transform.position).magnitude < collisionDistance)
                {
                    // our vector becomes this boids pos - neighbouring boids pos
                    steer += (transform.position - boid.transform.position);
                    sibs++;
                }

            }

        }
        steer /= sibs;
        steer.Normalize();        //unit, just direction
        return steer;

    }

    Vector3 align()
    {
        Vector3 steer = new Vector3(0, 0, 0);
        int sibs = 0;

        foreach (Transform boid in flock)
        {
            if (boid != transform)
            {
                steer += boid.GetComponent<Boids>().velocity;
                sibs++;
            }

        }
        steer /= sibs;

        steer.Normalize();

        return steer;
    }

    public void accumAvoid(Vector3 avoid)
    {
        avoidObst += transform.position - avoid;
        avoidCount++;

    }
    public void resetAvoid()
    {
        avoidCount = 0;
        avoidObst *= 0;
    }
}
