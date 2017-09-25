using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {

    public GameObject dest_visualizer;
    public enum MovementState {Persue, Evade, Wander, PathFollow, Wait};
    public MovementState m_state = MovementState.Wander;

    [Header("Movement Stats")]
    public float max_speed = 5.0f;
    public float acceleration = 100.0f;
    public float rot_accel = 5.0f;
    public float max_rot_speed = 5.0f;

    [Header("Wander Stats")]
    public float circe_center_dist = 10.0f;
    public float circle_radius = 2.0f;
    public GameObject wander_circle;
    float cur_wander_angle = 0.0f;
    public float wander_angle_rot = 1.0f;

    [Header("Evade Stats")]
    public GameObject enemy;

    [Header("Persue Stats")]
    public GameObject persue_target;
    public float arrive_slow_circle_radius = 5.0f;
    public float time_to_arrive = 1.5f;
    float time_since_arrvial_start = 0.0f;

    [Header("Pathfollowing")]
    public GameObject pt1;
    public GameObject pt2;
    public GameObject pt3;

    public GameObject tmp_dot_path_thing;

    float cur_time_arrive;
    float cur_rot_speed = 0.0f;

    Rigidbody2D rb;
    bool path_been_drawn = false;


    GameObject[] path_particles;
    List<KeyValuePair<Vector2, float> > path_particles_t = new List<KeyValuePair<Vector2, float> >();

    Vector2 dest;

	// Use this for initialization
	void Start () {
        dest = transform.position;
        rb = GetComponent<Rigidbody2D>();
        cur_time_arrive = 0.0f;
        
    }

	// Update is called once per frame
	void FixedUpdate () {
        if(m_state == MovementState.Wait)
        {
            rb.velocity = Vector2.zero;
        }
        // dynamic persue with dynamic arrive
        if(m_state == MovementState.Persue)
        {
            // see if we are in the circle
            // maybe make the circle into a trigger so that we have a state for arrive?
            dest_visualizer.transform.position = dest;

            float dist_to_arrive_center = (transform.position - persue_target.transform.position).magnitude;
            if (dist_to_arrive_center < arrive_slow_circle_radius)
            {
                // ARRIVE PROCEDURE:
                float target_speed = max_speed * (dist_to_arrive_center / arrive_slow_circle_radius);
                float target_velocity = target_speed;

                float m_accel = target_velocity - rb.velocity.magnitude;
                time_since_arrvial_start += Time.deltaTime;
                m_accel = m_accel / (time_to_arrive - time_since_arrvial_start);

                m_accel = Mathf.Clamp(m_accel, -acceleration, acceleration);

                rb.velocity = transform.right * (rb.velocity.magnitude + m_accel * Time.deltaTime);

                if (rb.velocity.magnitude < 0.2f)
                {
                    rb.velocity = Vector2.zero;
                    m_state = MovementState.Wait;
                    return;
                }
                    

                // rotate
                Vector3 vectorToTarget = dest - (Vector2)transform.position;
                float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
                Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * max_rot_speed);
                
            }
            else
            {
                dest = persue_target.transform.position;
                Dynamic_Seek();
            }
        }

        // dynamic evade
        if(m_state == MovementState.Evade)
        {
            // turn towards the dest that is gonna be away from the enemy
            dest = transform.position + transform.position - enemy.transform.position;
            dest_visualizer.transform.position = dest;
            Dynamic_Seek();
        }

        // dynamic wander
        if(m_state == MovementState.Wander)
        {
            Vector2 tmp = transform.position + transform.right * circe_center_dist;
            wander_circle.transform.position = tmp;

            // Random 
            dest = tmp + (Random.insideUnitCircle.normalized * circle_radius);

            // Rotate around
            /*
            dest.x = Mathf.Cos(cur_wander_angle) * circle_radius + tmp.x;
            dest.y = Mathf.Sin(cur_wander_angle) * circle_radius + tmp.y;
            cur_wander_angle += wander_angle_rot * Time.deltaTime;
            */


            dest_visualizer.transform.position = dest;

            Dynamic_Seek();
        }
        
        // path following
        if(m_state == MovementState.PathFollow)
        {
            // draw the path if it has not been drawn
            if(!path_been_drawn)
            {
                for (int i = 1; i < 20; i++)
                {
                    float t = (float)i / 20;
                    Vector2 cur_pos = Vector2.Lerp(
                        Vector2.Lerp(pt1.transform.position, pt2.transform.position, t),
                        Vector2.Lerp(pt2.transform.position, pt3.transform.position, t), t);
                    Instantiate(tmp_dot_path_thing, cur_pos, Quaternion.identity);

                    path_particles_t.Add(new KeyValuePair<Vector2, float>(cur_pos, t));
                }
                path_been_drawn = true;
            }

            // find the nearest dot on the path
            float min_dist = float.MaxValue;
            Vector2 closest_point = Vector2.zero;
            float closest_t = 0;
            foreach(KeyValuePair<Vector2, float> particle in path_particles_t)
            {
                float dist = (particle.Key - (Vector2)transform.position).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    closest_point = particle.Key;
                    closest_t = particle.Value;
                }
            }
            

            float add_t = 0.2f;
            float dest_t = closest_t + add_t;
            if (dest_t > 1.0f) dest_t = 1.0f;

            dest = Vector2.Lerp(
                    Vector2.Lerp(pt1.transform.position, pt2.transform.position, dest_t),
                    Vector2.Lerp(pt2.transform.position, pt3.transform.position, dest_t), dest_t);

            dest_visualizer.transform.position = dest;

            Dynamic_Seek();
        }        
	}

    void Dynamic_Seek()
    {
        // vf = vi + a * t
        // accelerate forward
        float cur_speed = rb.velocity.magnitude;
        cur_speed += acceleration * Time.deltaTime;

        // cap max speed
        if(cur_speed > max_speed)
        {
            cur_speed = max_speed;
        }

        rb.velocity = cur_speed * transform.right;

        // accelerate rotational speed
        cur_rot_speed = Mathf.Min(cur_rot_speed + rot_accel * Time.deltaTime, max_rot_speed);

        // rotate
        Vector3 vectorToTarget = dest - (Vector2)transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * max_rot_speed);

    }
}
