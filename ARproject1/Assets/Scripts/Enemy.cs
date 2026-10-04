using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Shooter player;
    public int scoreGiven = 10;

    public enum Types { Fire, Water, Plant };
    public Types type;
    string Weakness = "null";

    bool isDead = false;
    float deadTimer = 1f;

    Animator anim;
    BoxCollider bc;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        bc = GetComponent<BoxCollider>();

        player = FindAnyObjectByType<Shooter>();
        switch (type)
        {
            case Types.Fire: 
                Weakness = "BulletWater";
                break;
            case Types.Water:
                Weakness = "BulletPlant";
                break;
            case Types.Plant:
                Weakness = "BulletFire";
                break;
            default:
                break;
        }
    }

    void Update()
    {
        //hola soy el update
    }

    IEnumerator Die()
    {
        isDead = true;
        anim.SetBool("isDead", true);
        bc.enabled = false;

        yield return new WaitForSeconds(deadTimer);
        gameObject.SetActive(false);
    }

    //Destroy when bullet
    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        if (collision.gameObject.tag == Weakness)
        {
            Debug.Log($"Enemy Destroyed");

            player.score += scoreGiven;
            player.scoreText.text = $"Score = {player.score}";

            StartCoroutine(Die());
        }
    }
}