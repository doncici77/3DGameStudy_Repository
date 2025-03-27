using UnityEngine;
using UnityEngine.UI;

public class GameSettingUIManager1 : MonoBehaviour
{
    public GameObject SettingsObj;

    public Text resolutionText;
    public Text graphicsQualityText;
    public Text fullScreenText;

    private int resolutionIndex = 0;
    private int qualityIndex = 0;
    private bool isFullScreen = true;

    private string[] resolutions = { "1280x720", "1920x1080", "2560x1440", "3840x2160" };
    private string[] qualityOptions = { "Low", "Normal", "High" };

    public void OnApplySettingsClick()
    {
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        ApplySettings();
        SaveSettings();
    }

    private void ApplySettings()
    {
        string[] res = resolutions[resolutionIndex].Split('x');
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, isFullScreen);
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("GraphicsQualityIndex", qualityIndex);
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        qualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex", 1);
    }

    public void OnResolutionLeftClick()
    {
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        resolutionIndex = Mathf.Max(0, resolutionIndex - 1);
        UpdateResolutionText();
    }

    public void OnResolutionRightClick()
    {
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);
        UpdateResolutionText();
    }

    public void OnGraphicsLeftClick()
    {
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        qualityIndex = Mathf.Max(0, qualityIndex - 1);
        UpdateGraphicsQulityText();
    }

    public void OnGraphicsRightClick()
    {
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        qualityIndex = Mathf.Min(qualityOptions.Length - 1, qualityIndex + 1);
        UpdateGraphicsQulityText();
    }

    public void OnFullScreenToGgleClick()
    {
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        isFullScreen = !isFullScreen;
        UpdateFullScreenText();
    }

    private void UpdateResolutionText()
    {
        resolutionText.text = resolutions[resolutionIndex];
    }

    private void UpdateGraphicsQulityText()
    {
        graphicsQualityText.text = qualityOptions[qualityIndex];
    }

    private void UpdateFullScreenText()
    {
        fullScreenText.text = isFullScreen ? "On" : "Off";
    }

    public void OnSettings()
    {
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        SettingsObj.SetActive(true);
    }

    public void OffSettings()
    {
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        SettingsObj.SetActive(false);
    }
}
