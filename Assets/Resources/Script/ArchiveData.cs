using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AuthManager;
using static AudioManager;

[CreateAssetMenu(fileName = "ArchiveData", menuName = "Scriptable Ojbect/Archive")]
public class ArchiveData : ScriptableObject
{
    public ArchiveType archiveType;
    public string archiveDesc;
    public Sprite archiveIcon;
    //AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.Paper, true);
    public Sfx archiveSfx;
}
