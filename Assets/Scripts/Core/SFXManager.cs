using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public Slider volumeSlider1;
    public AudioSource source1;
    public AudioClip theme;
    public Slider volumeSlider2;
    public AudioSource source2;
    public AudioClip goOnWar;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        volumeSlider1.value = source1.volume; // Start slider at current volume
        volumeSlider1.onValueChanged.AddListener(SetVolume1);
        volumeSlider2.value = source2.volume; // Start slider at current volume
        volumeSlider2.onValueChanged.AddListener(SetVolume2);

        Theme();
    }
    void SetVolume1(float v)
    {
        source1.volume = v;
    }
    void SetVolume2(float v)
    {
        source2.volume = v;
    }

    public void Theme()
    {
        Play(source1, theme);
    }
    public void GoOnWar()
    {
        Play(source2, goOnWar);
    }

    private void Play(AudioSource p, AudioClip pos)
    {
        p.clip = pos;
        p.Play();
    }
}
