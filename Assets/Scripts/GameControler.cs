using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

public class GameControler : MonoBehaviour {
    public GameObject target;                               //gameobject target
    private bool alive;                                     //is app running

    public bool paused;                                     //is game paused
    public GameObject PlayerPrefab;                         //player prefab
    public GameObject AIShipPrefab;                         //AI prefab
    public List<Canvas> Menus;                              //UI menus
    public Text highscore1;                                 //Higscore text 1
    public Text highscore2;                                 //Higscore text 2                        
    public Text highscore3;                                 //Higscore text 3
    public Text lifesIndicator;                             //show life count
    public Text scoreIndicator;                             //show score
    public Text stageIndicator;                             //show current stage
    public Text enemyIndicator;                             //show remaining enemies
    public InputField name;                                 //new highscore text input
    
    public enum Menu                                        //collection of UI menues
    {
        Start,                                              //game start
        DifficultSelect,                                    //select difficulty
        Pause,                                              //pause game
        Play,                                               //gameplay UI
        Achievments,                                        //Achievments/highscore
        SetHighscore,                                       //Set new highscore
        Quit                                                //quit game
    }

    private Menu currentMenu;                               //current menu
    private int currentCanvas;                              //current canvas
    public List<GameObject> StarSystems;                    //list of star systems
    private GameObject currentMap;                          //current map

    private int score;                                      //player score
    private int lifes;                                      //player lifes
    private bool gameAlive;                                 //is game active
    private int difficulty;                                 //difficulty level
    private int currentStarSystem;                          //current star system
    private int stageNumber;                                //stage number
    private int enemycount;                                 //number of enemys
    private int totalKills;                                 //total kills                       NOT IMPLEMENTED
    private int sessionKills;                               //session kills                     NOT IMPLEMENTED
    private int powerupsPickedUp;                           //number of pickups                 NOT IMPLEMENTED
    private int systemsVisited;                             //number of maps seen               NOT IMPLEMENTED
    private string AIStatus;                                //status of AI ships
    private string PlayerOrientation;                       //player transform

    private string high1="";                                //highscore 1 value
    private string high2="";                                //highscore 2 value
    private string high3="";                                //highscore 3 value
    private int scorePosition;                              //which highscore

    // Use this for initialization
    void Start () {
        lifes = 1;                                          //set lifes to 1
        gameAlive = false;                                  //no game started
        paused = false;                                     //not paused
        currentCanvas = 1;                                  //select current UI
        currentMenu = Menu.Start;                           //current Menu is start
        Menus[1].enabled = true;                            //turn on current menu
        Menus[0].enabled = false;                           //turn of game UI
        loadHighscores();                                   //load highscores form save
        alive = true;                                       //set alive

        StartCoroutine(FSM());                              //Start state machine
	}
	
	// Update is called once per frame
	void Update () {
        #region inputsRead
        if (CrossPlatformInputManager.GetButtonDown("NewGame"))                                         //button new game read
        {
            currentMenu = Menu.DifficultSelect;                                                         //call difficulty menu
        }
        if (CrossPlatformInputManager.GetButtonDown("LoadGame"))                                        //button Load game read
        {
            loadGame();                                                                                 //load game
            currentMenu = Menu.Play;                                                                    //call difficulty menu
        }
        if (CrossPlatformInputManager.GetButtonDown("Achievments"))                                     //button achievments read
        {
            currentMenu = Menu.Achievments;                                                             //call difficulty menu
        }
        if (CrossPlatformInputManager.GetButtonDown("QuitApplication"))                                 //button quit read
        {
            currentMenu = Menu.Quit;                                                                    //call difficulty menu
        }

        if (CrossPlatformInputManager.GetButtonDown("easy"))                                            //button easy diff read
        {
            newGame(3);                                                                                 //create new game
            unPause();
            currentMenu = Menu.Play;                                                                    //call difficulty menu
        }
        if (CrossPlatformInputManager.GetButtonDown("medium"))                                          //button medium diff read
        {
            newGame(2);                                                                                 //create new game
            unPause();
            currentMenu = Menu.Play;                                                                    //call difficulty menu
        }
        if (CrossPlatformInputManager.GetButtonDown("hard"))                                            //button hard diff read
        {
            newGame(1);                                                                                 //create new game
            unPause();
            currentMenu = Menu.Play;                                                                    //call difficulty menu
        }
        if (CrossPlatformInputManager.GetButtonDown("QuitNewGame"))                                     //button back to main read
        {
            currentMenu = Menu.Start;                                                                   //call difficulty menu
        }

        if(CrossPlatformInputManager.GetButtonDown("pause"))                                            //button pause game read
        {
            Pause();                                                                          //pause/unpause game
            currentMenu = Menu.Pause;                                                                   //call difficulty menu
        }

        if (CrossPlatformInputManager.GetButtonDown("ResumeGame"))                                      //button resume read
        {
            unPause();                                                                          //pause/unpause game
            currentMenu = Menu.Play;                                                                    //call difficulty menu
        }
        if (CrossPlatformInputManager.GetButtonDown("SaveGame"))                                        //button save game read
        {
            saveGame();                                                                                 //save game

            unPause();                                                                          //pause/unpause game
            currentMenu = Menu.Play;                                                                    //call difficulty menu
        }
        if (CrossPlatformInputManager.GetButtonDown("LoadGamePause"))                                   //button load game read
        {
            loadGame();                                                                                 //load game
            unPause();
            currentMenu = Menu.Play;                                                                    //call difficulty menu
        }
        if (CrossPlatformInputManager.GetButtonDown("QuitGamePause"))                                   //button quit to main read
        {
            destroyMap();                                                                               //destroy current arena
            currentMenu = Menu.Start;                                                                   //call difficulty menu
        }

        if (CrossPlatformInputManager.GetButtonDown("CloseAchievments"))                                //button close achievments read
        {
            currentMenu = Menu.Start;                                                                   //call difficulty menu
        }

        if (CrossPlatformInputManager.GetButtonDown("SubmitHighscore"))                                 //button send highscore read
        {
            if (name.text != null && name.text != "")                                                   //if name text is not empty
            {
                setHighscore(name.text, score, scorePosition);                                          //set highscore values
                setHighscore(null, 0, 0);                                                               //set highscore texts
                saveHighScores();                                                                       //save highscores
                currentMenu = Menu.Start;                                                               //call start menu
            }
        }
        #endregion

        if (lifes<=0&&gameAlive)                            //if game is alive and player has no remaining lifes
        {
            destroyMap();                                   //destroy current map

            gameAlive = false;                              //no game played
            scorePosition = checkHighscore(score);          //check if new highscore                    

            if(scorePosition!=0)                            //if new highscore
            {
                currentMenu = Menu.SetHighscore;            //set highscore UI
            }
            else
            {
                currentMenu = Menu.Start;                   //call main menu
            }
        }

        if (gameAlive && enemycount <=0)                                //if game alive and no enemies left
        {
            Pause();                                          //toggle pause 
            destroyMap();                                               //destroy current map

            stageNumber += 1;                                           //increase stage number
            enemycount = stageNumber + 1;                               //increase enemy count
            currentStarSystem = Random.Range(0, StarSystems.Count);     //select new arena

            createNewStage(stageNumber, currentStarSystem);             //create new stage

           unPause();                                          //toogle pause
        }

        if (gameAlive)                                                  //if game alive
        {
            lifesIndicator.text = lifes.ToString();                     //display lifes
            scoreIndicator.text = score.ToString();                     //display score
            stageIndicator.text = stageNumber.ToString();               //display stage
            enemyIndicator.text = enemycount.ToString();                //display enemies
        }
    }

    //pause the game
    public void Pause()
    {
        paused = true;
    }

    //unpause the game
    public void unPause()
    {
        paused = false;
    }

    //state machin ehandeling UI changes
    IEnumerator FSM()
    {
        while(alive)       //while app alive
        {
            switch(currentMenu)                                                     //select menu
            {
                case Menu.Play:                                                     //if play
                    if (gameAlive == false)                                         //if not already set game alive
                        gameAlive = true;

                    if (currentCanvas != 0)                                         //if calling canvas not this canvas
                    {
                        Menus[currentCanvas].enabled = false;                       //set calling canvas false
                        Menus[0].enabled = true;                                    //set called canvas true
                        currentCanvas = 0;                                          //set current canvas this canvas
                    }
                    break;
                case Menu.Start:                                                     //if start
                    if (currentCanvas != 1)                                         //if calling canvas not this canvas
                    {
                        Menus[currentCanvas].enabled = false;                       //set calling canvas false
                        Menus[1].enabled = true;                                    //set called canvas true
                        currentCanvas = 1;                                          //set current canvas this canvas
                    }
                    break;
                case Menu.Pause:                                                     //if pause
                    if (currentCanvas != 2)                                         //if calling canvas not this canvas
                    {
                        Menus[currentCanvas].enabled = false;                       //set calling canvas false
                        Menus[2].enabled = true;                                    //set called canvas true
                        currentCanvas = 2;                                          //set current canvas this canvas
                    }
                    break;
                case Menu.Achievments:                                              //if achievments
                    if (currentCanvas != 3)                                         //if calling canvas not this canvas
                    {
                        Menus[currentCanvas].enabled = false;                       //set calling canvas false
                        Menus[3].enabled = true;                                    //set called canvas true
                        currentCanvas = 3;                                          //set current canvas this canvas
                    }
                    break;
                case Menu.DifficultSelect:                                          //if select difficulty
                    if (currentCanvas != 4)                                         //if calling canvas not this canvas
                    {
                        Menus[currentCanvas].enabled = false;                       //set calling canvas false
                        Menus[4].enabled = true;                                    //set called canvas true
                        currentCanvas = 4;                                          //set current canvas this canvas
                    }
                    break;
                case Menu.SetHighscore:                                             //if set highscore
                    if (currentCanvas != 5)                                         //if calling canvas not this canvas
                    {
                        Menus[currentCanvas].enabled = false;                       //set calling canvas false
                        Menus[5].enabled = true;                                    //set called canvas true
                        currentCanvas = 5;                                          //set current canvas this canvas
                    }
                    break;
                case Menu.Quit:                                                     //if quit
                    Application.Quit();                                             //close application
                    break;
            }

            yield return null;
        }
    }

    //create new game
    public bool newGame(int chosendifficulty)
    {
        difficulty = chosendifficulty;                                              //set difficulty
        lifes = difficulty;                                                         //set lifes
        stageNumber = 1;                                                            //set stage number
        score = 0;                                                                  //reset score
        enemycount = stageNumber + 1;                                               //set enemy count
        currentStarSystem = Random.Range(0, StarSystems.Count);                     //choose map

        createNewStage(stageNumber, currentStarSystem);                             //create stage

        return true;
    }

    //save current game
    public bool saveGame()
    {
        PlayerPrefs.SetString("AIInformation", encodeAIInformation());              //save AI
        PlayerPrefs.SetString("PlayerInformation", encodePlayerPosition());         //save player

        saveHighScores();                                                           //save highscore

        PlayerPrefs.SetInt("Score", score);                                         //save score
        PlayerPrefs.SetInt("Lives", lifes);                                         //save lifes
        PlayerPrefs.SetInt("Diff", difficulty);                                     //save difficulty
        PlayerPrefs.SetInt("StarS", currentStarSystem);                             //save map

        PlayerPrefs.Save();                                                         //commit changes

        return true;
    }

    //load safed game
    public bool loadGame()
    {
        destroyMap();                                                                   //setroy current map

        score = PlayerPrefs.GetInt("Score");                                            //read score from save
        lifes = PlayerPrefs.GetInt("Lives");                                            //read life from save
        difficulty = PlayerPrefs.GetInt("Diff");                                        //read difficulty from save

        currentStarSystem = PlayerPrefs.GetInt("StarS");                                //read star system
        currentMap = loadStarSystem(currentStarSystem);                                 //load star system

        GameObject[] PatrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoints");  //load patrol points

        LoadAIInformation(PlayerPrefs.GetString("AIInformation"), PatrolPoints);        //load AI
        loadPlayerInformation(PlayerPrefs.GetString("PlayerInformation"));              //Load Player

        loadHighscores();                                                               //load highscore
        
        return true;
    }

    //load highscores
    public void loadHighscores()
    {
        high1 = PlayerPrefs.GetString("High1");                                         //load first highscore
        high2 = PlayerPrefs.GetString("High2");                                         //load second highscore
        high3 = PlayerPrefs.GetString("High3");                                         //load third highscore

        highscore1.text = high1;                                                        //set highscore
        highscore2.text = high2;                                                        //set highscore
        highscore3.text = high3;                                                        //set highscore
    }

    //save highscores
    public void saveHighScores()
    {
        PlayerPrefs.SetString("High1", high1);      //set highscore 1
        PlayerPrefs.SetString("High2", high2);      //set highscore 2
        PlayerPrefs.SetString("High3", high3);      //set highscore 3
        PlayerPrefs.Save();                         //commit changes
    }

    //set highscores
    public void setHighscore(string name, int score, int position)
    {
        if (position == 1)                          //if position 1
        {
            high1 = name +":"+ score;               //set text value
            highscore1.text = high1;                //write in object
        }
        else if (position == 2)                     //if position 2
        {
            high2 = name + ":" + score;             //set text value
            highscore2.text = high2;                //write in object
        }
        else if (position == 3)                     //if position 3
        {
            high3 = name + ":" + score;             //set text value
            highscore3.text = high3;                //write in object
        }
        else if(position==0)                        //if 0 write all
        {
            highscore1.text = high1;
            highscore2.text = high2;
            highscore3.text = high3;
        }
    }

    //check highscores
    public int checkHighscore(int score)
    {
        if (high1 != null && high1 != "" && high1.Contains(":"))                //if highscore 1 exits and contains seperator 
        {
            if (score > int.Parse(high1.Split(':')[1]))                         //if the score is higher than the saved highscore
            {
                high3 = high2;                                                  //move highscore 2 to spott 3
                high2 = high1;                                                  //move highscore 1 to spott 2
                return 1;
            }   
            else if(high2!=null && high2 != "" && high2.Contains(":"))          //if highscore 2 exits and contains seperator 
            {
                if (score > int.Parse(high2.Split(':')[1]))                     //if the score is higher than the saved highscore
                {
                    high3 = high2;                                              //move highscore 2 to spott 3
                    return 2;
                }
                else if (high3 != null && high3 != "" && high3.Contains(":"))   //if highscore 3 exits and contains seperator 
                {
                    if (score > int.Parse(high3.Split(':')[1]))                 //if the score is higher than the saved highscore
                    {
                        return 3;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 3;
                }
            }
            else
            {
                return 2;
            }
        }
        else
        {
            return 1;
        }
    }

    //on player death
    public void deathPlayer(Vector3 location, Quaternion rotation)
    {
        lifes -= 1;                                                 //remove 1 life
        if(lifes>0)                                                 //if lifes remaining respawn player
        {
            StartCoroutine(respawnPlayer(location, rotation));
        }
    }

    //respawn player
    private IEnumerator respawnPlayer(Vector3 location, Quaternion rotation)
    {
        yield return new WaitForSeconds(3);     //wait 3 seconds

        GameObject player=Instantiate(PlayerPrefab, location, rotation) as GameObject; //spawn new player prefab
    }

    //increase lifes
    public void addLifes()
    {
        lifes += 1;     //increase lifes by 1
    }

    //on killed enemy
    public void killedEnemy()
    {
        enemycount = enemycount - 1;        //decrease enemy number
    }

    //increase player score
    public void addToScore(int scoreToAdd)
    {
        score += scoreToAdd/difficulty;         //add to player score
    }

    //get AI information for save
    private string encodeAIInformation()
    {
        GameObject[] AI = GameObject.FindGameObjectsWithTag("AIShips");             //get all AI Gameobjects
        string encode = "";                                                         //enciding string
        int count = 0;                                                              //count variables
        foreach(GameObject ship in AI)                                              //for all game objects
        {
            string position = ship.transform.position.x + ":" + ship.transform.position.y +":"+ ship.transform.position.z;                                          //position of game object
            string rotation = ship.transform.rotation.w + ":" + ship.transform.rotation.x + ":" + ship.transform.rotation.y + ":" + ship.transform.rotation.z;      //rotation of game object

            string orientation = position + ";" + rotation;                                                                                                         //transform of game object

            float[] info = ship.GetComponent<ShipEventController>().getShipInformationToSave();                                                                     //get event script
            string status = info[0]+":"+info[1];                                                                                                                    //get ship status

            string AIShip=orientation+";"+status;                                                                                                                   //AI ship

            encode = encode + AIShip;                                                                                                                               //add to encode

            if(count<AI.Length-1)
            {
                encode = encode + "!";                                                                                                                              //if more AI add seperator
            }
            count++;                                                    
        }

        return encode;
    }

    //load AI information from save
    private void LoadAIInformation(string AIShip, GameObject[] pPoints)
    {
        enemycount = 0;                                                                                                                 //enemy count
        string[] ships = AIShip.Split('!');                                                                                             //read AI
        foreach (string information in ships)                                                                                           //for each ship
        {
            Vector3 position = new Vector3();                                                                                           //vector position
            Quaternion rotation = new Quaternion();                                                                                     //quaternion orientation

            string[] pos = information.Split(';')[0].Split(':');                                                                        //seperate string
                position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));                                  //put position

            string[] rot = information.Split(';')[1].Split(':');                                                                        //seperate string
                rotation = new Quaternion(float.Parse(rot[1]), float.Parse(rot[2]), float.Parse(rot[3]), float.Parse(rot[0]));          //put rotation

            GameObject currentShip=Instantiate(AIShipPrefab, position, rotation) as GameObject;                                         //spawn ship
            enemycount += 1;                                                                                                            //increase count

            string[] status= information.Split(';')[2].Split(':');                                                                      //split string

            currentShip.GetComponent<ShipEventController>().setShipInformation(status[0], status[1]);                                   //set ship information
            currentShip.GetComponent<AIShips>().setPatrolPoints(pPoints);                                                               //set patrol points
        }
    }

    //encode player for save
    private string encodePlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");                                                                                                 //find player
        string encode = "";                                                                                                                                             //encode player

        string position = player.transform.position.x + ":" + player.transform.position.y + ":" + player.transform.position.z;                                          //player position
        string rotation = player.transform.rotation.w + ":" + player.transform.rotation.x + ":" + player.transform.rotation.y + ":" + player.transform.rotation.z;      //player rotation

        string orientation = position + ";" + rotation;                                                                                                                 //player tranform

        float[] info = player.GetComponent<ShipEventController>().getShipInformationToSave();                                                                           //get player information
        string status = info[0] + ":" + info[1];                                                                                                                        //set player information

        string PlayerShip = orientation + ";" + status;                                                                                                                 //player gameobject

        encode = encode + PlayerShip;                                                                                                                                   //encode player

        return encode;
    }

    //load player from save
    private void loadPlayerInformation(string information)
    {
        Vector3 position = new Vector3();                                                                                   //player position
        Quaternion rotation = new Quaternion();                                                                             //player rotation    

        string[] pos = information.Split(';')[0].Split(':');                                                                //split string
        position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));                              //set postition

        string[] rot = information.Split(';')[1].Split(':');                                                                //split string
        rotation = new Quaternion(float.Parse(rot[1]), float.Parse(rot[2]), float.Parse(rot[3]), float.Parse(rot[0]));      //set rotation

        GameObject currentShip = Instantiate(PlayerPrefab, position, rotation) as GameObject;                               //create player

        string[] status = information.Split(';')[2].Split(':');                                                             //split string

        currentShip.GetComponent<ShipEventController>().setShipInformation(status[0], status[1]);                           //set player information
    }

    //read difficulty
    public int readDifficulty()
    {
        return difficulty;
    }
    
    //create new stage
    private void createNewStage(int stage, int StarSystem)
    {
        currentMap = loadStarSystem(StarSystem);                                                                            //set current map

        GameObject[] PatrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoints");                                      //get all patrol points

        spawnEnemy(stage, PatrolPoints, currentMap);                                                                        //spawn enemies

        GameObject player = Instantiate(PlayerPrefab, new Vector3(0, 10, 0), new Quaternion(0, 0, 0, 0)) as GameObject;     //create player
    }

    //load star system
    private GameObject loadStarSystem(int StarSystem)
    {
        Vector3 position = new Vector3(0,0,0);                                                      //spawn position
        Quaternion rotation = new Quaternion(0, 0, 0, 1);                                           //spawn rotation
        GameObject map = Instantiate(StarSystems[StarSystem], position, rotation) as GameObject;    //generate new map

        return map;
    }

    //spawn enemies
    private void spawnEnemy(int stage, GameObject[] pPoints, GameObject Map)
    {
        for (int x = 0; x < stage + 1; x++)                                                             //until all enemys spawned
        {
            float[] positionCalc=new float[3];                                                          //new position
            for(int y=0; y<3; y++)
            {
                switch(Random.Range(0,2))
                {
                    case 0:
                        positionCalc[y] = Random.Range(5000f, 6000f);                                   //set position element
                        break;
                    case 1:
                        positionCalc[y] = Random.Range(-5000f, -6000f);                                 //set position element
                        break;
                }
            }
            Vector3 position = new Vector3(positionCalc[0], positionCalc[1], positionCalc[2]);          //set position
            Quaternion rotation = new Quaternion(0, 0, 0, 1);                                           //create rotation

            GameObject currentShip = Instantiate(AIShipPrefab, position, rotation) as GameObject;       //create player

            currentShip.GetComponent<AIShips>().setPatrolPoints(pPoints);                               //set patrol points
        }
    }

    //destroy current arena
    private void destroyMap()
    {
        GameObject[] AI = GameObject.FindGameObjectsWithTag("AIShips");                             //find all AI
        GameObject[] Explosions = GameObject.FindGameObjectsWithTag("explosions");                  //find all explosions
        GameObject Player = GameObject.FindGameObjectWithTag("Player");                             //find player
        GameObject Map = GameObject.FindGameObjectWithTag("Map");                                   //find map
        GameObject[] HPUps = GameObject.FindGameObjectsWithTag("healthPuP");                        //find all health pUps
        GameObject[] LPUps = GameObject.FindGameObjectsWithTag("livesPuP");                         //find all lifes pUps
        GameObject[] SPUps = GameObject.FindGameObjectsWithTag("shieldsPuP");                       //find all shields pUps

        Destroy(Map);                                                                               //destroy map
        Destroy(Player);                                                                            //destroy player
        foreach(GameObject ship in AI)                                                              //destroy all AI
        {
            Destroy(ship);
        }
        foreach (GameObject PuPs in HPUps)                                                          //destroy all Health pUps
        {
            Destroy(PuPs);
        }
        foreach (GameObject PuPs in LPUps)                                                          //destroy all life pUps
        {
            Destroy(PuPs);
        }
        foreach (GameObject PuPs in SPUps)                                                          //destroy all shield pUps
        {
            Destroy(PuPs);
        }
        foreach (GameObject exp in Explosions)                                                      //destroy all explosions
        {
            Destroy(exp);
        }
    }
}

