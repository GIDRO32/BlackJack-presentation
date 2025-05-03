using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SFB;  // Include the StandaloneFileBrowser namespace

public class Login : MonoBehaviour
{
    public InputField nickname_field_login;
        public InputField nickname_field_register;
    private string nickname;
    public int balance;
    public int highscore;
    public Text highscore_text;
public GameObject loginPopup;
private PlayerData loadedPlayerData;
private string decryptedPassword;

public InputField password_field_login;
public InputField password_field_register;
public Text warning_text;
public Text warning_text2;
public Text nameDisplay;
private string password;

    void Start()
    {
        loginPopup.SetActive(false);
        // Load balance and highscore from PlayerPrefs
        balance = PlayerPrefs.GetInt("Total", balance);
        highscore = PlayerPrefs.GetInt("Record", highscore);
        warning_text.text = null;
        warning_text2.text = null;
    }

    // Method to update the nickname string whenever the input field changes
public void Update()
{
    nickname = nickname_field_login.text;
    password = password_field_login.text;
    nickname = nickname_field_register.text;
    password = password_field_register.text;
    highscore_text.text = "Your Highscore:\n" + highscore.ToString() + "$";
    nameDisplay.text = "Welcome to BlackJack table\n"+nickname;
    if(nickname == null)
    {
        nickname = "Player";
    }
}

    public void LoadScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
    // Save the nickname, balance, and highscore to a JSON file
public void SaveData()
{
    string nameToSave = nickname_field_register.text;
    string passwordToSave = password_field_register.text;

    if (string.IsNullOrEmpty(nameToSave) || string.IsNullOrEmpty(passwordToSave))
    {
        warning_text.text = "Nickname and Password cannot be empty!";
        Debug.LogError("Nickname and Password cannot be empty!");
        return;
    }

    string encryptedPassword = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordToSave));

    PlayerData playerData = new PlayerData
    {
        nickname = nameToSave,
        balance = balance,
        highscore = highscore,
        encryptedPassword = encryptedPassword
    };

    string json = JsonUtility.ToJson(playerData);
    var path = StandaloneFileBrowser.SaveFilePanel("Save Nickname", "", "PlayerData", "json");

    if (!string.IsNullOrEmpty(path))
    {
        File.WriteAllText(path, json);
        Debug.Log($"Data saved to {path}");
        warning_text.text = "";
    }
    else
    {
        Debug.LogError("Save cancelled or path invalid!");
    }
}

public void LoadData()
{
    var paths = StandaloneFileBrowser.OpenFilePanel("Load Nickname", "", "json", false);
    if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
    {
        string path = paths[0];

        if (File.Exists(path))
        {
string json = File.ReadAllText(path);
loadedPlayerData = JsonUtility.FromJson<PlayerData>(json);

// Decrypt password into a separate field
decryptedPassword = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(loadedPlayerData.encryptedPassword));
Debug.Log(decryptedPassword);
// Enable the login popup
loginPopup.SetActive(true);
        }
        else
        {
            Debug.LogError("File does not exist!");
        }
    }
    else
    {
        Debug.LogError("No file selected!");
    }
}
public void ConfirmLogin()
{
    string enteredName = nickname_field_login.text;
    string enteredPassword = password_field_login.text;

    if (loadedPlayerData != null &&
        enteredName == loadedPlayerData.nickname &&
        enteredPassword == decryptedPassword)
    {
        balance = loadedPlayerData.balance;
        highscore = loadedPlayerData.highscore;

        PlayerPrefs.SetInt("Total", balance);
        PlayerPrefs.SetInt("Record", highscore);
        PlayerPrefs.Save();

        highscore_text.text = "Your Highscore:\n" + highscore.ToString() + "$";
        loginPopup.SetActive(false);
        warning_text.text = "";
        Debug.Log("Login successful. Data loaded.");
    }
    else
    {
        warning_text.text = "Incorrect name or password";
        Debug.LogError("Incorrect name or password");
    }
}


    [System.Serializable]
public class PlayerData
{
    public string nickname;
    public int balance;
    public int highscore;
    public string encryptedPassword;
}

}
