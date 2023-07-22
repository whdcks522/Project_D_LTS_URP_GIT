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
    public int channels;//채널의 개수
    AudioSource[] sfxPlayers;
    int channelsIndex;

    public enum Sfx {Dead, Hit, LevelUp=3, Lose, Melee, Range = 7,Select, Win }//random으로 활용 가능함

    private void Awake()
    {
        instance = this;
        Init();
    }

    void Init() 
    {
        //배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();//bgmPlayer에 저장하면서 동시에 컴포넌트 삽입
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;
        bgmEffect = Camera.main.GetComponent<AudioHighPassFilter>();//하이패스 컴포넌트 가져오기

        //효과음 플레이어 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];//Audio Source 배열 초기화
        for (int index = 0; index < channels; index++) 
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
            sfxPlayers[index].bypassListenerEffects = true;//Listner의 특성을 무시함
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
        bgmEffect.enabled = isPlay;//특정 음역대 이상만 들리게 해서, timeScale동안 브금 재생 안되도록 하기
    }

    //효과음 재생
    public void PlaySfx(Sfx sfx) 
    {
        for (int index = 0; index < sfxPlayers.Length; index++) 
        {
            int loopIndex = (index + channelsIndex) % sfxPlayers.Length;//최근에 사용한 인덱스에서 0부터 증가해가며 가능한 것 탐색
            if (sfxPlayers[loopIndex].isPlaying) continue;//실행중이라면 continue

            int ranIndex = 0;
            if (sfx == Sfx.Hit || sfx == Sfx.Melee) //Hit, Melee를 위함, 갯수가 차이나면 switch 사용
                ranIndex = Random.Range(0, 2);//효과음 랜덤을 위함

            channelsIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;
        } 
    }
}
