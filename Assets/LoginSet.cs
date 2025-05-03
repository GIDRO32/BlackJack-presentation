using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginSet : MonoBehaviour
{
    [Header("UI Fields")]
    public InputField nickname_field_register;
    public InputField password_field_register;
    public InputField nickname_field_login;
    public InputField password_field_login;
    public Text warning_text;
    public Text nameDisplay;
    public GameObject loginPopup;

    [Header("Player Data")]
    public int balance;
    public int highscore;

    private PlayerData loadedPlayerData;
    private string decryptedPassword;

    // 🔷 WebGL file upload interface
#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void ShowFileUpload(string gameObjectName, string methodName);
#endif

    void Start()
    {
        // ✅ Null-safe welcome message
        if (loadedPlayerData == null || string.IsNullOrEmpty(loadedPlayerData.nickname))
        {
            nameDisplay.text = "Welcome to BlackJack table\nPlayer";
        }
        else
        {
            nameDisplay.text = "Welcome to BlackJack table\n" + loadedPlayerData.nickname;
        }
    }
    public void LoadScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
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

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL-specific logic
    string base64Json = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
    Application.ExternalCall("DownloadFileFromUnity", base64Json, "PlayerData.json");
#else
    // Desktop-specific logic
    var path = SFB.StandaloneFileBrowser.SaveFilePanel("Save Nickname", "", "PlayerData", "json");

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
#endif
}



    // 🔷 WebGL file load trigger (use this instead of normal LoadData in WebGL)
public void LoadData()
{
#if UNITY_WEBGL && !UNITY_EDITOR
    // Trigger file upload dialog in browser (handled by .jslib)
    Application.ExternalCall("ShowFileUpload", gameObject.name, "OnFileLoadedFromJS");
#else
    var paths = SFB.StandaloneFileBrowser.OpenFilePanel("Open Player Data", "", "json", false);
    if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
    {
        string json = File.ReadAllText(paths[0]);
        LoadPlayerFromJson(json);
    }
    else
    {
        Debug.LogError("Load cancelled or path invalid!");
    }
#endif
}

private void LoadPlayerFromJson(string json)
{
    loadedPlayerData = JsonUtility.FromJson<PlayerData>(json);
    if (loadedPlayerData == null)
    {
        Debug.LogError("Failed to parse JSON.");
        return;
    }

    decryptedPassword = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(loadedPlayerData.encryptedPassword));
    loginPopup.SetActive(true);
    Debug.Log("Player data loaded. Awaiting login credentials.");
}
public void OnFileLoadedFromJS(string base64Json)
{
    try
    {
        string json = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(base64Json));
        LoadPlayerFromJson(json);
    }
    catch (System.Exception ex)
    {
        Debug.LogError("Failed to load player data from WebGL file: " + ex.Message);
    }
}
    public void AttemptLogin()
    {
        string enteredName = nickname_field_login.text;
        string enteredPassword = password_field_login.text;

        if (loadedPlayerData == null)
        {
            warning_text.text = "No data loaded.";
            return;
        }

        if (enteredName == loadedPlayerData.nickname && enteredPassword == decryptedPassword)
        {
            loginPopup.SetActive(false);
            nameDisplay.text = "Welcome to BlackJack table\n" + loadedPlayerData.nickname;
        }
        else
        {
            warning_text.text = "Incorrect name or password";
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
