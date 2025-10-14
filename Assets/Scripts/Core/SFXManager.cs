using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioSource source1;
    public AudioSource source2;
    public AudioClip slash;
    public AudioClip damage;

    private void Awake()
    {
        instance = this;
    }

    public void SwordSlash()
    {
        Play(source1 ,slash);
    }
    public void Damage()
    {
        Play(source2 , damage);
    }

    private void Play(AudioSource p, AudioClip pos)
    {
        p.clip = pos;
        p.Play();
    }
}
