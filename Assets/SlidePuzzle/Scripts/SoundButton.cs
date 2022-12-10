using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SoundButton : MonoBehaviour
{
    public AudioClip clickSound = null;

    public AudioSource soundSource = null;

    [Range(0, 1)] public float volumeScale = 1.0f;

    private void Start()
    {
        if (clickSound == null) return;

        if (soundSource == null)
        {
            soundSource = Camera.main.GetComponent<AudioSource>();
        }

        if (soundSource != null)
        {
            Button buttonVar = gameObject.GetComponent<Button>();

            buttonVar.onClick.AddListener(delegate
            {
                if (clickSound != null)
                    soundSource.PlayOneShot(clickSound, volumeScale);
            });
        }

    }

}