using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSoundLinker : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    void Start()
    {
        Button button = GetComponent<Button>();
        
        button.onClick.AddListener(() => audioManager.AudioPlay("ButtonSound"));
    }
}