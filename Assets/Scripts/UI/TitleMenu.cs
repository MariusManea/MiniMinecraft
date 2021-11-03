using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public GameObject mainMenuObject;
    public GameObject settingsObject;

    [Header("Main Menu UI Elements")]
    public TextMeshProUGUI seedField;

    [Header("Settings Menu UI Elements")]
    public Slider viewDistanceSlider;
    public TextMeshProUGUI viewDistanceText;
    public Slider mouseSlider;
    public TextMeshProUGUI mouseText;
    public Toggle threadingToggle;
    public Toggle chunkToggle;
    public TMP_Dropdown clouds;

    Settings settings;
    private void Awake()
    {
        if (!File.Exists(Application.dataPath + "/settings.cfg"))
        {
            settings = new Settings();
            string jsonExport = JsonUtility.ToJson(settings);
            File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);
        }
        else
        {
            string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
            settings = JsonUtility.FromJson<Settings>(jsonImport);
        }
    }

    public void StartGame()
    {
        VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode()) / VoxelData.WorldSizeInChunks;

        SceneManager.LoadScene("MinecraftWorld", LoadSceneMode.Single);
    }

    public void EnterSettings()
    {
        viewDistanceSlider.value = settings.viewDistance;
        UpdateViewDistanceSlider();

        mouseSlider.value = settings.mouseSensitivity;
        UpdateMouseSlider();

        threadingToggle.isOn = settings.enableThreading;
        chunkToggle.isOn = settings.enableAnimatedChunks;

        clouds.value = (int)settings.clouds;

        mainMenuObject.SetActive(false);
        settingsObject.SetActive(true);
    }

    public void LeaveSettings()
    {
        settings.viewDistance = (int)viewDistanceSlider.value;
        settings.mouseSensitivity = mouseSlider.value;
        settings.enableThreading = threadingToggle.isOn;
        settings.enableAnimatedChunks = chunkToggle.isOn;
        settings.clouds = (CloudStyle)clouds.value;

        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        settingsObject.SetActive(false);
        mainMenuObject.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void UpdateViewDistanceSlider()
    {
        viewDistanceText.text = "View Distance: " + viewDistanceSlider.value;
    }

    public void UpdateMouseSlider()
    {
        mouseText.text = "Mouse Sensitivity: " + mouseSlider.value.ToString("F1");
    }
}
