using UnityEngine;

public class SearchModeManager : MonoBehaviour
{
    public static SearchModeManager Instance;

    public enum SearchMode
    {
        AStar,
        BFS
    }

    public SearchMode currentMode = SearchMode.AStar;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            currentMode = SearchMode.AStar;
            Debug.Log("Switched to A*");
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            currentMode = SearchMode.BFS;
            Debug.Log("Switched to BFS");
        }
    }
}