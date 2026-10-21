using UnityEngine;
using UnityEngine.SceneManagement;

public class StreetEmpireMenuController : MonoBehaviour
{
    public string gameSceneName = "StreetEmpirePrototype_Generated";
    public GameObject settingsPanel;

    public void StartGame() { LoadGameScene(); }

    // Save/load is not implemented yet, so Continue currently opens the gameplay scene too.
    public void ContinueGame() { LoadGameScene(); }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadGameScene()
    {
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            Debug.LogWarning("Сцена '" + gameSceneName + "' пока не добавлена в Build Settings. Создай Assets/Scenes/" + gameSceneName + ".unity.");
    }
}
