using UnityEngine;

public class Enemy : MonoBehaviour
{
    Shooter player;
    public int scoreGiven = 10;

    public enum Types { Fire, Water, Plant };
    public Types type;
    string Weakness = "null";

    void Start()
    {
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

    //Destroy when bullet
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == Weakness)
        {
            Debug.Log($"Enemy Destroyed");

            player.score += scoreGiven;
            player.scoreText.text = $"Score = {player.score}";
            gameObject.SetActive(false);
        }
    }
}