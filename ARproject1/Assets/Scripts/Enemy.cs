using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Shooter player;
    public int scoreGiven = 10;
    void Start()
    {
        player = FindAnyObjectByType<Shooter>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Destroy when bullet
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            Debug.Log($"Enemy Destroyed");

            player.score += scoreGiven;
            player.scoreText.text = $"Score = {player.score}";
            gameObject.SetActive(false);
        }
    }
}