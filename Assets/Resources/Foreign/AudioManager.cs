using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Bgm")]
    public AudioClip bgmClip;
    public float bgmVolume;
    AudioSource bgmPlayer;
    AudioHighPassFilter bgmEffect;

    [Header("Sfx")]
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;//ä���� ����
    AudioSource[] sfxPlayers;
    int channelsIndex;

    public enum Sfx {Dead, Hit, LevelUp=3, Lose, Melee, Range = 7,Select, Win }//random���� Ȱ�� ������

    private void Awake()
    {
        instance = this;
        Init();
    }

    void Init() 
    {
        //����� �÷��̾� �ʱ�ȭ
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();//bgmPlayer�� �����ϸ鼭 ���ÿ� ������Ʈ ����
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;
        bgmEffect = Camera.main.GetComponent<AudioHighPassFilter>();//�����н� ������Ʈ ��������

        //ȿ���� �÷��̾� �ʱ�ȭ
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];//Audio Source �迭 �ʱ�ȭ
        for (int index = 0; index < channels; index++) 
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
            sfxPlayers[index].bypassListenerEffects = true;//Listner�� Ư���� ������
        }
    }

    public void PlayBgm(bool isPlay) 
    {
        if (isPlay) 
        {
            bgmPlayer.Play();
        }
        else 
        {
            bgmPlayer.Stop();
        }
    }

    public void EffectBgm(bool isPlay)
    {
        bgmEffect.enabled = isPlay;//Ư�� ������ �̻� �鸮�� �ؼ�, timeScale���� ��� ��� �ȵǵ��� �ϱ�
    }

    //ȿ���� ���
    public void PlaySfx(Sfx sfx) 
    {
        for (int index = 0; index < sfxPlayers.Length; index++) 
        {
            int loopIndex = (index + channelsIndex) % sfxPlayers.Length;//�ֱٿ� ����� �ε������� 0���� �����ذ��� ������ �� Ž��
            if (sfxPlayers[loopIndex].isPlaying) continue;//�������̶�� continue

            int ranIndex = 0;
            if (sfx == Sfx.Hit || sfx == Sfx.Melee) //Hit, Melee�� ����, ������ ���̳��� switch ���
                ranIndex = Random.Range(0, 2);//ȿ���� ������ ����

            channelsIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;
        } 
    }
}
