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

    public bool IsFirebaseReady { get; private set; }//현재 파이어베이스에 접근 가능한 환경인지
    public bool IsSignInOnProgress { get; private set; }//로그인 진행중인지 확인용(로그인중 로그인 요청 방지)

    private void Awake() 
    {
        //해상도
        Screen.SetResolution(1280, 720, false);
        //배경 음악 초기화
        audioManager = GetComponent<AudioManager>();
    } 


    void Start()
    {
        audioManager.PlayBgm(AudioManager.Bgm.Auth);

        DontDestroyOnLoad(this.gameObject);

        //emailField.text = "222@222.2";
        //passwordField.text = "222@222.2";

        //준비가 안됐는데 사용하면 안되므로
        btnGroup.SetActive(false);
        //사용 가능한지 확인(ContinueWith이 원래 맞는데, 오류나서 MainThread 씀)
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>//ContinueWith: 사용 가능한지 검사가 끝났을 때 실행 됨
        {
            var result = task.Result;//여기서는 또 되네(결과를 받아옴)
            if (result != DependencyStatus.Available)//사용 가능한 상태가 아니라면
            {
                stateText.text = result.ToString();
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
            fireImage.SetActive(!IsFirebaseReady);
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
        fireImage.SetActive(true);
        stateText.text = "회원가입 중...";
        Debug.Log("회원가입 중...");
        firebaseAuth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task =>
        {
            if (task.IsCanceled) //회원가입 취소
            {
                stateText.text = "회원가입 취소";
                Debug.LogError("회원가입 취소");
                fireImage.SetActive(false);
                return;
            }
            if (task.IsFaulted) //회원가입 실패(이메일 오류, 비밀번호가 간단함, 이미 가입됨)
            {
                stateText.text = "회원가입 실패";
                Debug.LogError("회원가입 실패");
                fireImage.SetActive(false);
                return;
            }
            //취소나 실패가 아니면 회원가입
            AuthResult authResult = task.Result;
            FirebaseUser newUser = authResult.User;
            
        });
        stateText.text = "회원가입 완료";
        Debug.Log("회원가입 완료");
        fireImage.SetActive(false);
        //성공 효과음
        audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
    }
    #endregion

    #region 로그인
    public void Login()//로그인
    {
        if (!IsFirebaseReady || IsSignInOnProgress || User != null) //1 = 대기중이 아닐 경우, 2 = 이미 진행 중 일 경우 3 = 이미 로그인 한 경우
        {
            stateText.text = "로그인 오류";
            Debug.LogError("로그인 오류");
            return;
        }
        //로그인 진행 
        IsSignInOnProgress = true;
        btnGroup.SetActive(false);
        fireImage.SetActive(true);

        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task =>
        {
            stateText.text = $"Sign in status: {task.Status}";
            Debug.Log($"Sign in status: {task.Status}");//현재 로그인의 상태
            IsSignInOnProgress = false;//여기까지 왔다는 것 자체가, 로그인 처리 자체는 완료라는 뜻
            btnGroup.SetActive(true);
            fireImage.SetActive(false);

            if (task.IsFaulted) //어떤 오류가 발생했는지
            {
                stateText.text = "Sign-in canceled(오류)" + task.Exception;
                Debug.LogError("Sign-in canceled(오류)");
                Debug.LogError(task.Exception);
            }
            else if (task.IsCanceled) 
            {
                stateText.text = "Sign-in canceled(취소)";
                Debug.LogError("Sign-in canceled(취소)");//취소
            } 
            else// 정상적인 경우 task에 이메일과 비밀번호에 대응되는 유저 정보가 저장됨
            {
                AuthResult authResult = task.Result;
                User = authResult.User;
                stateText.text = "로그인";
                Gone();
                //성공 효과음
                audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
            }
        }
        );
    }
    #endregion
    public void Gone() //게임 시작
    {
        if (User != null)
        {
            stateText.text = "입장 중...";
            audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
            //미리 설정
            playerEmail = User.Email;
            playerId = User.UserId;

            //불러오기
            LoadData();
            originAchievements.classEmail = playerEmail;
            Invoke("RealGone", 2.5f);
        }
        else 
        {
            audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
            stateText.text = "게임 시작 오류";
            Debug.Log("Game Start Error");
        }
    }

    void RealGone() 
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void LogOut()//로그아웃
    {
        audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
        firebaseAuth.SignOut();
        stateText.text = "로그아웃";
        Debug.Log("로그아웃");
    }

    public void LoadData()//데이터 불러오기
    {
        if (User == null)
        {
            Debug.Log("유저가 로그인 하지 않아, 데이터를 불러 올 수 없습니다");
            return;
        }

        // 데이터 베이스에 대한 참조 생성
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        //유저 ID를 통해 업적 데이터에 대한 참조 획득
        DatabaseReference achievementsRef = databaseReference.Child("achievements").Child(playerId);//playerId

        //데이터베이스로부터 데이터를 읽어옴
        achievementsRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("데이터베이스로부터 데이터를 읽는데 실패함: " + task.Exception.Flatten().InnerExceptions);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot != null && dataSnapshot.Exists)
                {
                    // 스냅샷으로부터 JSON 데이터를 가져옴
                    string json = dataSnapshot.GetRawJsonValue();

                    //JSON을 업적 데이터로 전환
                    Achievements achievements = JsonUtility.FromJson<Achievements>(json);

                    int arrSize = System.Enum.GetValues(typeof(ArchiveType)).Length;
                    for (int index = 0; index < arrSize; index++) 
                    {
                        originAchievements.Arr[index] = achievements.Arr[index];
                    }
                }
                else
                {
                    Debug.Log("해당 유저에 대한 정보가 없음");
                }
            }
        });
    }

    public void SaveJson() 
    {
        if (User == null)
        {
            Debug.Log("유저가 로그인 하지 않아, 데이터를 저장 할 수 없습니다");
            return;
        }

        // 데이터베이스에 대한 참조 생성
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        //업적 정보를 JSON으로 전환
        string json = JsonUtility.ToJson(originAchievements);

        //유저 ID를 통해 JSON 데이터를 데이터베이스에 저장
        databaseReference.Child("achievements").Child(playerId).SetRawJsonValueAsync(json).ContinueWith(task =>//User.UserId
        {
            if (task.IsFaulted)
            {//task.Exception
                Debug.LogError("JSON 데이터를 데이터베이스에 저장하는데 실패함: " + task.Exception.Flatten().InnerExceptions);//160번째 줄
            }
            else if (task.IsCompleted)
            {
                Debug.Log("JSON 데이터가 성공적으로 저장됨");
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
