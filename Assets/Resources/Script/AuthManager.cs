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

    public bool IsFirebaseReady { get; private set; }//현재 파이어베이스에 접근 가능한 환경인지
    public bool IsSignInOnProgress { get; private set; }//로그인 진행중인지 확인용(로그인중 로그인 요청 방지)

    private void Awake() 
    {
        //해상도
        Screen.SetResolution(1280, 720, false);
        //초기화
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

        //준비가 안됐는데 사용하면 안되므로
        btnGroup.SetActive(false);
        //사용 가능한지 확인(ContinueWith이 원래 맞는데, 오류나서 MainThread 씀)
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>//ContinueWith: 사용 가능한지 검사가 끝났을 때 실행 됨
        {
            var result = task.Result;//여기서는 또 되네(결과를 받아옴)
            if (result != DependencyStatus.Available)//사용 가능한 상태가 아니라면
            {
                Debug.LogError(result.ToString());
                IsFirebaseReady = false;
            }
            else ////사용 가능한 상태임
            {
                IsFirebaseReady = true;
                //현재 firebaseApp은 아무것도 할당 안돼있음
                firebaseApp = FirebaseApp.DefaultInstance;//기본적인 관리 기능앱을 가져옴
                firebaseAuth = FirebaseAuth.DefaultInstance;//인증을 집중적으로 관리하는 오브젝트를 가져옴
            }
            btnGroup.SetActive(IsFirebaseReady);//버튼 입력 가능한 것 다시 조정
        });//async이므로 비동기, 여기서 안멈추고 계속 다음 진행
    }
    public FirebaseApp firebaseApp;//파이어베이스 전체 어플리케이션 관리
    public FirebaseAuth firebaseAuth;//파이어베이스 어플리케이션중에서 인증을 관리, 로그인(회원가입 등에 사용)
    public FirebaseUser User;// 인증이 완료된 유저 정보

    public GameObject btnGroup;
    public InputField emailField;
    public InputField passwordField;
    public string playerEmail = "";
    public string playerId = "";

    #region 회원가입
    public void Create() //회원가입
    {
        firebaseAuth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task =>
        {
            if (task.IsCanceled) //회원가입 취소
            {
                Debug.LogError("회원가입 취소");
                return;
            }
            if (task.IsFaulted) //회원가입 실패(이메일 오류, 비밀번호가 간단함, 이미 가입됨)
            {
                Debug.LogError("회원가입 실패");
                return;
            }
            //취소나 실패가 아니면 회원가입
            AuthResult authResult = task.Result;
            FirebaseUser newUser = authResult.User;
            Debug.Log("회원가입 완료");
        });
    }
    #endregion

    #region 로그인
    public void Login()//로그인
    {
         if (!IsFirebaseReady || IsSignInOnProgress || User != null) //1 = 대기중이 아닐 경우, 2 = 이미 진행 중 일 경우 3 = 이미 로그인 한 경우
            return;
        //로그인 진행 
        IsSignInOnProgress = true;
        btnGroup.SetActive(false);

        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"Sign in status: {task.Status}");//현재 로그인의 상태
            IsSignInOnProgress = false;//여기까지 왔다는 것 자체가, 로그인 처리 자체는 완료라는 뜻
            btnGroup.SetActive(true);

            if (task.IsFaulted) //어떤 오류가 발생했는지
            {
                Debug.LogError("Sign-in canceled(오류)");
                Debug.LogError(task.Exception);
            }
            else if (task.IsCanceled) Debug.LogError("Sign-in canceled(취소)");//취소
            else// 정상적인 경우 task에 이메일과 비밀번호에 대응되는 유저 정보가 저장됨
            {
                AuthResult authResult = task.Result;
                User = authResult.User;
                Gone();
            }
        }
        );
    }
    #endregion
    public void Gone() //게임 시작
    {
        playerEmail = User.Email;
        playerId = User.UserId;

        //불러오기
        LoadData();

        if (User != null) //SceneManager.LoadScene("FakeScene");//
            SceneManager.LoadScene("LobbyScene");
        else Debug.Log("Game Start Error");
    }
    public void LogOut()//로그아웃
    {
        firebaseAuth.SignOut();
        Debug.Log("로그아웃");
    }

    public void LoadData()//데이터 불러오기
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
                Debug.LogError("Failed to save JSON data to the database: " + task.Exception.Flatten().InnerExceptions);//160번째 줄
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
