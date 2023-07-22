using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainManager : MonoBehaviour
{
    GameManager gameManager;
    public Dropdown dropdown;
    Vector3 enemiesPos = new Vector3(0, 0.5f, 0);
    private void Awake()
    {
        gameManager = GameManager.Instance;
        
    }
    private void Start()
    {
        //���� ����
        dropdown.value = 1;
    }

    public void GenerateBtn() //�ӽ� �� ��ȯ
    {
        string str = "";
        if (dropdown.value == 0)
            str = "Dummy";
        else if (dropdown.value == 1)
            str = "EnemyA";
        else if(dropdown.value == 2)
            str = "EnemyB";
        else if (dropdown.value == 3)
            str = "EnemyC";
        GameManager.Instance.SpawnEnemy(str, 5);
        
    }
}
