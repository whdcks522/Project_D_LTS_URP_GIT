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
    public int channels;//ä���� ����
    AudioSource[] sfxPlayers;
    int channelsIndex;//���� ���� �� �� �÷��̾� ��ȣ

    public enum Bgm {Auth, Lobby, Entrance, Chapter1, Chapter1_BossA}//random���� Ȱ�� ������
    public enum Sfx { DoorOpen, DoorDrag = 6, Impact = 9 , Step = 15, PlayerBulletA = 18, Paper = 24}//29

    private void Awake()
    {
        //����� �÷��̾� �ʱ�ȭ
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();//bgmPlayer�� �����ϸ鼭 ���ÿ� ������Ʈ ����
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;

        //ȿ���� �÷��̾� �ʱ�ȭ
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];//Audio Source �迭 �ʱ�ȭ
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

    

    //ȿ���� ���
    public void PlaySfx(Sfx sfx, bool isUseRan) 
    {
        for (int index = 0; index < sfxPlayers.Length; index++) 
        {
            int loopIndex = (index + channelsIndex) % sfxPlayers.Length;//�ֱٿ� ����� �ε������� 0���� �����ذ��� ������ �� Ž��
            if (sfxPlayers[loopIndex].isPlaying) continue;//�������̶�� continue

            int ranIndex = 0;
            if (isUseRan) 
            {
                int maxRanIndex = 1;//���� �ִ�ġ
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
                ranIndex = Random.Range(0, maxRanIndex);//ȿ���� ������ ����
            }

            channelsIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;
        } 
    }
}
