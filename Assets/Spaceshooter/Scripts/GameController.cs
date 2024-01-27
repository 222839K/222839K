using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour {

    public Vector3 positionAsteroid;
    public GameObject asteroid;
    public GameObject asteroid2;
    public GameObject asteroid3;
    public int hazardCount;
    public float startWait;
    public float spawnWait;
    public float waitForWaves;
    public TMP_Text scoreText;
    public TMP_Text lvlText;
    public TMP_Text xpText;
    public TMP_Text dtText;
    public Text gameOverText;
    public Text restartText;
    public Text mainMenuText;
    public GameObject gleaderboard;
    public GameObject nleaderboard;

    private bool restart;
    private bool gameOver;
    private int score;
    private int level;
    private int xp;
    private string dt;
    private List<GameObject> asteroids;

    float timePassed = 0f;

    [SerializeField] TMP_Text currentScore;
    [SerializeField] TextMeshProUGUI Msg;

    private void Start()
    {
        asteroids = new List<GameObject> {
            asteroid,
            asteroid2,
            asteroid3
        };
        gameOverText.text = "";
        restartText.text = "";
        mainMenuText.text = "";
        restart = false;
        gameOver = false;
        score = 0;
        xp = 0;
        level = 0;
        dt = System.DateTime.Now.ToString();
        gleaderboard.SetActive(false);
        nleaderboard.SetActive(false);
        StartCoroutine(spawnWaves());
        updateDt();
        updateScore();
        updateXP();
        updateLevel();

        //LoadJSON();
    }

    private void Update()
    {
        if (restart)
        {
            if (Input.GetKey(KeyCode.R))
            {
                gleaderboard.SetActive(false);
                nleaderboard.SetActive(false);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                SceneManager.LoadScene("Menu");
            }
        }
        if (gameOver)
        {
            restartText.text = "Press R to restart game";
            mainMenuText.text = "Press Q to go back to main menu";
            restart = true;
        }

        if (!gameOver)
        {
            timePassed += Time.deltaTime;
            if (timePassed > 5f)
            {
                this.xp += 10;
                updateXP();
                timePassed = 0f;
            }
        }

        if (this.xp >= 50)
        {
            this.level += 1;
            this.xp = 0;
            updateXP();
            updateLevel();
        }

        //this.xp = this.score / 10;
        //updateXP();

        //if (this.xp > 50)
        //{
        //    this.level += 1;
        //    this.xp = 0;
        //    updateXP();
        //    updateLevel();
        //}
    }

    private IEnumerator GiveXP()
    {
        yield return new WaitForSeconds(5f);
        while (true)
        {
            this.xp += 10;
            updateXP();
            if (gameOver)
            {
                break;
            }
        }
        //code here will execute after 5 seconds
    }

    private IEnumerator spawnWaves()
    {
        yield return new WaitForSeconds(startWait);
        while (true)
        {
            for (int i = 0; i < hazardCount; i++)
            {
                Vector3 position = new Vector3(Random.Range(-positionAsteroid.x, positionAsteroid.x), positionAsteroid.y, positionAsteroid.z);
                Quaternion rotation = Quaternion.identity;
                Instantiate(asteroids[Random.Range(0, 3)], position, rotation);
                yield return new WaitForSeconds(spawnWait);
            }
            yield return new WaitForSeconds(waitForWaves);
            if (gameOver)
            {
                break;
            }
        }
    }

    public void gameIsOver()
    {
        gameOverText.text = "Game Over";
        gleaderboard.SetActive(true);
        nleaderboard.SetActive(true);
        dt = System.DateTime.Now.ToString();
        updateDt();
        gameOver = true;
        SendJSON();
        OnButtonSendLeaderboard();
    }

    public void addScore(int score)
    {
        this.score += score;
        updateScore();
    }

    void updateScore()
    {
        scoreText.text = "Score:" + score;
    }

    void updateLevel()
    {
        lvlText.text = level.ToString();
    }

    void updateXP()
    {
        xpText.text = xp.ToString();
    }

    void updateDt()
    {
        dtText.text = dt;
    }

    void UpdateMsg(string msg) //to display in console and messagebox
    {
        Debug.Log(msg);
        Msg.text = msg + '\n';
    }
    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        UpdateMsg("display name updated!" + r.DisplayName);
    }

    public void OnButtonSendLeaderboard()
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>{ //playfab leaderboard statistic name
                new StatisticUpdate{
                    StatisticName="highscore",
                    Value = score
                    //Value=int.Parse(currentScore.text)
                }
            }
        };
        UpdateMsg("Submitting score:" + score);
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnError);
    }
    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    {
        UpdateMsg("Successful leaderboard sent:" + r.ToString());
    }

    public void GotoScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void ClientGetTitleData()
    { //Slide 33: MOTD
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result => {
                if (result.Data == null || !result.Data.ContainsKey("MOTD")) UpdateMsg("No MOTD");
                else UpdateMsg("MOTD: " + result.Data["MOTD"]);
            },
            error => {
                UpdateMsg("Got error getting titleData:");
                UpdateMsg(error.GenerateErrorReport());
            }
        );
    }

    [SerializeField] Playerbox[] PlayerBoxes;
    public void SendJSON()
    {
        List<Player> playerList = new List<Player>();
        foreach (var item in PlayerBoxes) playerList.Add(item.ReturnClass());
        string stringListAsJson = JsonUtility.ToJson(new JSListWrapper<Player>(playerList));
        Debug.Log("JSON data prepared:" + stringListAsJson);
        var req = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"Player", stringListAsJson }
            }
        };
        PlayFabClientAPI.UpdateUserData(req, result => Debug.Log("Data sent success"), OnError);
    }

    public void LoadJSON()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJSONDataReceived, OnError);
    }

    void OnJSONDataReceived(GetUserDataResult r)
    {
        
        Debug.Log("received JSON data");
        if (r.Data != null && r.Data.ContainsKey("Player"))
        {
                JSListWrapper<Player> jlw = JsonUtility.FromJson<JSListWrapper<Player>>(r.Data["Player"].Value);
                for (int i = 0; i < PlayerBoxes.Length; i++)
                {
                    PlayerBoxes[i].SetUI(jlw.list[i]);
                }
        }
    }

    public void BacktoMaininScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }

    [System.Serializable]
    public class JSListWrapper<T>
    {
        public List<T> list;
        public JSListWrapper(List<T> list) => this.list = list;
    }

}
