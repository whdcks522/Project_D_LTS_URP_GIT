using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;
using Firebase.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class AuthManager : MonoBehaviour//MonoBehaviour
{
    private static AuthManager instance;// = null
    public static AuthManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AuthManager>();//new AuthManager();
            }
            return instance;
        }
    }
    public Text stateText;
    public GameObject fireImage;
    public AudioManager audioManager;

    public bool IsFirebaseReady { get; private set; }//���� ���̾�̽��� ���� ������ ȯ������
    public bool IsSignInOnProgress { get; private set; }//�α��� ���������� Ȯ�ο�(�α����� �α��� ��û ����)

    private void Awake() 
    {
        //�ػ�
        Screen.SetResolution(1280, 720, false);
        //��� ���� �ʱ�ȭ
        audioManager = GetComponent<AudioManager>();
    } 


    void Start()
    {
        audioManager.PlayBgm(AudioManager.Bgm.Auth);

        DontDestroyOnLoad(this.gameObject);

        //emailField.text = "222@222.2";
        //passwordField.text = "222@222.2";

        //�غ� �ȵƴµ� ����ϸ� �ȵǹǷ�
        btnGroup.SetActive(false);
        //��� �������� Ȯ��(ContinueWith�� ���� �´µ�, �������� MainThread ��)
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>//ContinueWith: ��� �������� �˻簡 ������ �� ���� ��
        {
            var result = task.Result;//���⼭�� �� �ǳ�(����� �޾ƿ�)
            if (result != DependencyStatus.Available)//��� ������ ���°� �ƴ϶��
            {
                stateText.text = result.ToString();
                Debug.LogError(result.ToString());
                IsFirebaseReady = false;
            }
            else ////��� ������ ������
            {
                IsFirebaseReady = true;
                //���� firebaseApp�� �ƹ��͵� �Ҵ� �ȵ�����
                firebaseApp = FirebaseApp.DefaultInstance;//�⺻���� ���� ��ɾ��� ������
                firebaseAuth = FirebaseAuth.DefaultInstance;//������ ���������� �����ϴ� ������Ʈ�� ������
            }
            btnGroup.SetActive(IsFirebaseReady);//��ư �Է� ������ �� �ٽ� ����
            fireImage.SetActive(!IsFirebaseReady);
        });//async�̹Ƿ� �񵿱�, ���⼭ �ȸ��߰� ��� ���� ����
    }
    public FirebaseApp firebaseApp;//���̾�̽� ��ü ���ø����̼� ����
    public FirebaseAuth firebaseAuth;//���̾�̽� ���ø����̼��߿��� ������ ����, �α���(ȸ������ � ���)
    public FirebaseUser User;// ������ �Ϸ�� ���� ����

    public GameObject btnGroup;
    public InputField emailField;
    public InputField passwordField;
    public string playerEmail = "";
    public string playerId = "";

    #region ȸ������
    public void Create() //ȸ������
    {
        fireImage.SetActive(true);
        stateText.text = "ȸ������ ��...";
        Debug.Log("ȸ������ ��...");
        firebaseAuth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task =>
        {
            if (task.IsCanceled) //ȸ������ ���
            {
                stateText.text = "ȸ������ ���";
                Debug.LogError("ȸ������ ���");
                fireImage.SetActive(false);
                return;
            }
            if (task.IsFaulted) //ȸ������ ����(�̸��� ����, ��й�ȣ�� ������, �̹� ���Ե�)
            {
                stateText.text = "ȸ������ ����";
                Debug.LogError("ȸ������ ����");
                fireImage.SetActive(false);
                return;
            }
            //��ҳ� ���а� �ƴϸ� ȸ������
            AuthResult authResult = task.Result;
            FirebaseUser newUser = authResult.User;
            
        });
        stateText.text = "ȸ������ �Ϸ�";
        Debug.Log("ȸ������ �Ϸ�");
        fireImage.SetActive(false);
        //���� ȿ����
        audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
    }
    #endregion

    #region �α���
    public void Login()//�α���
    {
        if (!IsFirebaseReady || IsSignInOnProgress || User != null) //1 = ������� �ƴ� ���, 2 = �̹� ���� �� �� ��� 3 = �̹� �α��� �� ���
        {
            stateText.text = "�α��� ����";
            Debug.LogError("�α��� ����");
            return;
        }
        //�α��� ���� 
        IsSignInOnProgress = true;
        btnGroup.SetActive(false);
        fireImage.SetActive(true);

        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task =>
        {
            stateText.text = $"Sign in status: {task.Status}";
            Debug.Log($"Sign in status: {task.Status}");//���� �α����� ����
            IsSignInOnProgress = false;//������� �Դٴ� �� ��ü��, �α��� ó�� ��ü�� �Ϸ��� ��
            btnGroup.SetActive(true);
            fireImage.SetActive(false);

            if (task.IsFaulted) //� ������ �߻��ߴ���
            {
                stateText.text = "Sign-in canceled(����)" + task.Exception;
                Debug.LogError("Sign-in canceled(����)");
                Debug.LogError(task.Exception);
            }
            else if (task.IsCanceled) 
            {
                stateText.text = "Sign-in canceled(���)";
                Debug.LogError("Sign-in canceled(���)");//���
            } 
            else// �������� ��� task�� �̸��ϰ� ��й�ȣ�� �����Ǵ� ���� ������ �����
            {
                AuthResult authResult = task.Result;
                User = authResult.User;
                stateText.text = "�α���";
                Gone();
                //���� ȿ����
                audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
            }
        }
        );
    }
    #endregion
    public void Gone() //���� ����
    {
        if (User != null)
        {
            stateText.text = "���� ��...";
            audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
            //�̸� ����
            playerEmail = User.Email;
            playerId = User.UserId;

            //�ҷ�����
            LoadData();
            originAchievements.classEmail = playerEmail;
            Invoke("RealGone", 2.5f);
        }
        else 
        {
            audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
            stateText.text = "���� ���� ����";
            Debug.Log("Game Start Error");
        }
    }

    void RealGone() 
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void LogOut()//�α׾ƿ�
    {
        audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
        firebaseAuth.SignOut();
        stateText.text = "�α׾ƿ�";
        Debug.Log("�α׾ƿ�");
    }

    public void LoadData()//������ �ҷ�����
    {
        if (User == null)
        {
            Debug.Log("������ �α��� ���� �ʾ�, �����͸� �ҷ� �� �� �����ϴ�");
            return;
        }

        // ������ ���̽��� ���� ���� ����
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        //���� ID�� ���� ���� �����Ϳ� ���� ���� ȹ��
        DatabaseReference achievementsRef = databaseReference.Child("achievements").Child(playerId);//playerId

        //�����ͺ��̽��κ��� �����͸� �о��
        achievementsRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("�����ͺ��̽��κ��� �����͸� �дµ� ������: " + task.Exception.Flatten().InnerExceptions);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot != null && dataSnapshot.Exists)
                {
                    // ���������κ��� JSON �����͸� ������
                    string json = dataSnapshot.GetRawJsonValue();

                    //JSON�� ���� �����ͷ� ��ȯ
                    Achievements achievements = JsonUtility.FromJson<Achievements>(json);

                    int arrSize = System.Enum.GetValues(typeof(ArchiveType)).Length;
                    for (int index = 0; index < arrSize; index++) 
                    {
                        originAchievements.Arr[index] = achievements.Arr[index];
                    }
                }
                else
                {
                    Debug.Log("�ش� ������ ���� ������ ����");
                }
            }
        });
    }

    public void SaveJson() 
    {
        if (User == null)
        {
            Debug.Log("������ �α��� ���� �ʾ�, �����͸� ���� �� �� �����ϴ�");
            return;
        }

        // �����ͺ��̽��� ���� ���� ����
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        //���� ������ JSON���� ��ȯ
        string json = JsonUtility.ToJson(originAchievements);

        //���� ID�� ���� JSON �����͸� �����ͺ��̽��� ����
        databaseReference.Child("achievements").Child(playerId).SetRawJsonValueAsync(json).ContinueWith(task =>//User.UserId
        {
            if (task.IsFaulted)
            {//task.Exception
                Debug.LogError("JSON �����͸� �����ͺ��̽��� �����ϴµ� ������: " + task.Exception.Flatten().InnerExceptions);//160��° ��
            }
            else if (task.IsCompleted)
            {
                Debug.Log("JSON �����Ͱ� ���������� �����");
            }
        });
    }
    //#endregion

    public Achievements originAchievements = new Achievements();
    public enum ArchiveType { Undead, NoShot, Chapter1}
    [System.Serializable]
    public class Achievements 
    {
        public string classEmail = "";
        public int[] Arr = new int[3];
    }
}
