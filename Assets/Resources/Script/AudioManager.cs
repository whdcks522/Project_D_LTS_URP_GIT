using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    [Header("Bgm")]
    public AudioClip[] bgmClips;
    public float bgmVolume;
    AudioSource bgmPlayer;

    [Header("Sfx")]
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;//채널의 개수
    AudioSource[] sfxPlayers;
    int channelsIndex;//현재 실행 중 인 플레이어 번호

    public enum Bgm {Auth, Lobby, Entrance, Chapter1, Chapter1_BossA}//random으로 활용 가능함
    public enum Sfx { DoorOpen, DoorDrag = 6, Impact = 9 , Step = 15, PlayerBulletA = 18, Paper = 24}//29

    private void Awake()
    {
        //배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();//bgmPlayer에 저장하면서 동시에 컴포넌트 삽입
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;

        //효과음 플레이어 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];//Audio Source 배열 초기화
        for (int index = 0; index < channels; index++)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
        }
    }

    public void PlayBgm(Bgm bgm) 
    {
        bgmPlayer.Stop();
        switch (bgm) 
        {
            case Bgm.Auth:
                bgmPlayer.clip = bgmClips[0];
                break;
            case Bgm.Lobby:
                bgmPlayer.clip = bgmClips[1];
                break;
            case Bgm.Entrance:
                bgmPlayer.clip = bgmClips[2];
                break;
            case Bgm.Chapter1:
                bgmPlayer.clip = bgmClips[3];
                break;
            case Bgm.Chapter1_BossA:
                bgmPlayer.clip = bgmClips[4];
                break;
        }
        bgmPlayer.Play();
    }

    

    //효과음 재생
    public void PlaySfx(Sfx sfx, bool isUseRan) 
    {
        for (int index = 0; index < sfxPlayers.Length; index++) 
        {
            int loopIndex = (index + channelsIndex) % sfxPlayers.Length;//최근에 사용한 인덱스에서 0부터 증가해가며 가능한 것 탐색
            if (sfxPlayers[loopIndex].isPlaying) continue;//실행중이라면 continue

            int ranIndex = 0;
            if (isUseRan) 
            {
                int maxRanIndex = 1;//랜덤 최대치
                switch (sfx)
                {
                    case Sfx.DoorOpen:
                    case Sfx.Impact:
                    case Sfx.PlayerBulletA:
                        maxRanIndex = 6;
                        break;

                    case Sfx.Paper:
                        maxRanIndex = 5;
                        break;

                    case Sfx.DoorDrag:
                    case Sfx.Step:
                        maxRanIndex = 3;
                        break;
                    
                }
                ranIndex = Random.Range(0, maxRanIndex);//효과음 랜덤을 위함
            }

            channelsIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;
        } 
    }
}
