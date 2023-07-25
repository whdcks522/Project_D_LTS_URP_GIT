using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AuthManager;

[CreateAssetMenu(fileName = "ArchiveData", menuName = "Scriptable Ojbect/Archive")]
public class ArchiveData : ScriptableObject
{
    

    public ArchiveType archiveType;
    public string archiveTitle;
    public string archiveDesc;
    public Sprite archiveIcon;
}
