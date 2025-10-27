using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip foodCollect, preciousFoodCollect,rockHit, explode, speedUp, gameOver, speedDown, buttonPressed, gameMusic;

    private void OnEnable()
    {
        Player.AudioPlay += AudioPlay;
        // GameUI.GameStart += GameStart;
    }

    private void OnDisable()
    {
        Player.AudioPlay -= AudioPlay;
        // GameUI.GameStart -= GameStart;
    }

    private void GameStart()
    {
        audioSource.clip = gameMusic;
        audioSource.Play();
    }

    public void AudioPlay(string soundName)
    {
        switch (soundName)
        {
            case "foodCollect": 
                audioSource.PlayOneShot(foodCollect);
                break;
            case "preciousFoodCollect":
                audioSource.PlayOneShot(preciousFoodCollect);
                break;
            case "rockHit":
                audioSource.PlayOneShot(rockHit);
                break;
            case "explode":
                audioSource.PlayOneShot(explode);
                break;
            case "speedUp":
                audioSource.PlayOneShot(speedUp);
                break;
            case "speedDown":
                audioSource.PlayOneShot(speedDown);
                break;
            case "gameOver":
                audioSource.PlayOneShot(gameOver);
                break;
            case "ButtonSound":
                audioSource.PlayOneShot(buttonPressed);
                break;
        }
    }
}
