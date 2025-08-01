using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainBackgroundVideo;
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject creditsMenu;
    public GameObject openingCutscene;

    void Start()
    {
        // Initial state
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainBackgroundVideo.SetActive(true);
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
        openingCutscene.SetActive(false);
    }

    public void ShowSettings()
    {
        mainBackgroundVideo.SetActive(true);
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        creditsMenu.SetActive(false);
        openingCutscene.SetActive(false);
    }

    public void ShowCredits()
    {
        mainBackgroundVideo.SetActive(true);
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(true);
        openingCutscene.SetActive(false);
    }

    public void PlayGame()
    {
        mainBackgroundVideo.SetActive(false);
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
        openingCutscene.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Game is exiting...");
        Application.Quit();
    }

}
