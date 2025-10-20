using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioSource source1;
    public AudioSource source2;
    public AudioClip theme;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        Theme();
    }

    public void Theme()
    {
        Play(source1 ,theme);
    }

    private void Play(AudioSource p, AudioClip pos)
    {
        p.clip = pos;
        p.Play();
    }
}
