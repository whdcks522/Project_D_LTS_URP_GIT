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
    private static AuthManager instance = null;
    public static AuthManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AuthManager();
            }
            return instance;
        }
    }

    AudioManager audioManager;

    public bool IsFirebaseReady { get; private set; }//���� ���̾�̽��� ���� ������ ȯ������
    public bool IsSignInOnProgress { get; private set; }//�α��� ���������� Ȯ�ο�(�α����� �α��� ��û ����)

    private void Awake() 
    {
        //�ػ�
        Screen.SetResolution(1280, 720, false);
        //�ʱ�ȭ
        audioManager = GetComponent<AudioManager>();
    } 


    void Start()
    {
        if (instance == null)
                            instance = this;
        else if (instance != this)
                            Destroy(instance.gameObject);
        DontDestroyOnLoad(this.gameObject);

        emailField.text = "222@222.2";
        passwordField.text = "222@222.2";

        //�غ� �ȵƴµ� ����ϸ� �ȵǹǷ�
        btnGroup.SetActive(false);
        //��� �������� Ȯ��(ContinueWith�� ���� �´µ�, �������� MainThread ��)
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>//ContinueWith: ��� �������� �˻簡 ������ �� ���� ��
        {
            var result = task.Result;//���⼭�� �� �ǳ�(����� �޾ƿ�)
            if (result != DependencyStatus.Available)//��� ������ ���°� �ƴ϶��
            {
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
        firebaseAuth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task =>
        {
            if (task.IsCanceled) //ȸ������ ���
            {
                Debug.LogError("ȸ������ ���");
                return;
            }
            if (task.IsFaulted) //ȸ������ ����(�̸��� ����, ��й�ȣ�� ������, �̹� ���Ե�)
            {
                Debug.LogError("ȸ������ ����");
                return;
            }
            //��ҳ� ���а� �ƴϸ� ȸ������
            AuthResult authResult = task.Result;
            FirebaseUser newUser = authResult.User;
            Debug.Log("ȸ������ �Ϸ�");
        });
    }
    #endregion

    #region �α���
    public void Login()//�α���
    {
         if (!IsFirebaseReady || IsSignInOnProgress || User != null) //1 = ������� �ƴ� ���, 2 = �̹� ���� �� �� ��� 3 = �̹� �α��� �� ���
            return;
        //�α��� ���� 
        IsSignInOnProgress = true;
        btnGroup.SetActive(false);

        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"Sign in status: {task.Status}");//���� �α����� ����
            IsSignInOnProgress = false;//������� �Դٴ� �� ��ü��, �α��� ó�� ��ü�� �Ϸ��� ��
            btnGroup.SetActive(true);

            if (task.IsFaulted) //� ������ �߻��ߴ���
            {
                Debug.LogError("Sign-in canceled(����)");
                Debug.LogError(task.Exception);
            }
            else if (task.IsCanceled) Debug.LogError("Sign-in canceled(���)");//���
            else// �������� ��� task�� �̸��ϰ� ��й�ȣ�� �����Ǵ� ���� ������ �����
            {
                AuthResult authResult = task.Result;
                User = authResult.User;
                Gone();
            }
        }
        );
    }
    #endregion
    public void Gone() //���� ����
    {
        playerEmail = User.Email;
        playerId = User.UserId;

        //�ҷ�����
        LoadData();

        if (User != null) //SceneManager.LoadScene("FakeScene");//
            SceneManager.LoadScene("LobbyScene");
        else Debug.Log("Game Start Error");
    }
    public void LogOut()//�α׾ƿ�
    {
        firebaseAuth.SignOut();
        Debug.Log("�α׾ƿ�");
    }

    public void LoadData()//������ �ҷ�����
    {
        if (User == null)
        {
            Debug.Log("User is not logged in. Unable to load data.");
            return;
        }

        // Create a reference to the database
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Get the reference to the achievements data under the user's ID
        DatabaseReference achievementsRef = databaseReference.Child("achievements").Child(playerId);

        // Read the data from the database
        achievementsRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to load data from the database: " + task.Exception.Flatten().InnerExceptions);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot != null && dataSnapshot.Exists)
                {
                    // Get the JSON data from the snapshot
                    string json = dataSnapshot.GetRawJsonValue();

                    // Convert the JSON data back to the Achievements object
                    Achievements achievements = JsonUtility.FromJson<Achievements>(json);

                    // Use the loaded data as needed
                    Debug.Log("Achievements loaded successfully. x: " + achievements.x);
                    Debug.Log("Achievements loaded successfully. x: " + achievements.y);
                }
                else
                {
                    Debug.Log("No data found for the specified user.");
                }
            }
        });
    }

    public void SaveJson() 
    {
        if (User == null)
        {
            Debug.Log("User is not logged in. Unable to save data.");
            return;
        }

        // Create a reference to the database
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Convert the Achievements object to JSON
        string json = JsonUtility.ToJson(new Achievements());

        // Set the JSON data in the database under the user's ID
        databaseReference.Child("achievements").Child(playerId).SetRawJsonValueAsync(json).ContinueWith(task =>//User.UserId
        {
            if (task.IsFaulted)
            {//task.Exception
                Debug.LogError("Failed to save JSON data to the database: " + task.Exception.Flatten().InnerExceptions);//160��° ��
            }
            else if (task.IsCompleted)
            {
                Debug.Log("JSON data saved successfully.");
            }
        });
    }
    //#endregion

    class Achievements 
    {
        //int[] a = { 0, 0, 0 };
        public int x = 5;
        public int y = 5;
    }
}
