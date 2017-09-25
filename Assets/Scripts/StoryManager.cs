using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour {

    public MovementController actor_1;
    public MovementController actor_2;
    public MovementController actor_3;

    public GameObject redRing1;
    public GameObject redRing2;

    public GameObject house;

    public float scene_1_duration = 5.0f;
    public float scene_2_duration = 4.0f;
    public float scene_3_duration = 8.0f;

    /*
     * STORYBOARD:
     * actor 1: hunter
     * actor 2: wolf
     * actor 3: Red
     * 
     * scene 1:
     * hunter & wolf appear and wanders in woods
     * 
     * scene 2:
     * wolf flees hunter, hunter persues (slowly)
     * 
     * scene 3: 
     * Red appears and follows path to house
     * 
     * scene 4:
     * wolf appears and arrives at house
     */


    // Use this for initialization
    void Start () {
        actor_1.gameObject.SetActive(true);
        actor_2.gameObject.SetActive(true);
        actor_3.gameObject.SetActive(false);

        actor_1.m_state = MovementController.MovementState.Wander;
        actor_2.m_state = MovementController.MovementState.Wander;
        Invoke("start_scene_2", scene_1_duration);
	}
	
    void start_scene_2()
    {
        redRing1.SetActive(false);
        redRing2.SetActive(false);
        actor_1.m_state = MovementController.MovementState.Persue;
        actor_1.persue_target = actor_2.gameObject;
        actor_2.m_state = MovementController.MovementState.Evade;
        actor_2.enemy = actor_1.gameObject;

        Invoke("scene_3", scene_2_duration);
    }

    void scene_3()
    {
        actor_1.gameObject.SetActive(false);
        actor_2.gameObject.SetActive(false);
        actor_3.gameObject.SetActive(true);

        actor_3.m_state = MovementController.MovementState.PathFollow;

        Invoke("scene_4", scene_3_duration);
    }

    void scene_4()
    {
        actor_3.gameObject.SetActive(false);
        actor_2.gameObject.SetActive(true);

        actor_2.m_state = MovementController.MovementState.Persue;
        actor_2.persue_target = house;

        redRing1.SetActive(true);
        redRing1.transform.position = house.transform.position;
        redRing1.transform.localScale = redRing1.transform.localScale * 2.0f;

        // move the wolf from his offscreen position
        actor_2.transform.position = transform.position;
        // give him a bit of RNG in position
        Vector2 rng_displace;
        rng_displace.x = Random.Range(-10.0f, 10.0f);
        rng_displace.y = Random.Range(-10.0f, 10.0f);
        actor_2.transform.position = (Vector2)transform.position + rng_displace;
    }

	// Update is called once per frame
	void Update () {
		
	}
}
