using UnityEngine;
using TMPro;
using System;

public class TrainingUI : MonoBehaviour
{
    public TextMeshProUGUI episodeText;       // TextMeshPro reference to display episode information
    public TextMeshProUGUI trainingTimeText;    // TextMeshPro reference to display training time
    public TextMeshProUGUI successfulParkText;  // TextMeshPro reference to display number of successful parks

    private float startTime;            // Start time of this session
    private int episodeCount;           // Number of episodes in this session
    private float cumulativeTime;       // Total training time from previous sessions
    private int successfulParkCount;    // Number of successful parks in this session

    // PlayerPrefs keys
    private const string EpisodeKey = "EpisodeCount";
    private const string TimeKey = "CumulativeTime";
    private const string SuccessfulParksKey = "SuccessfulParks";

    void Start()
    {
        /*/ For resetting timers and counters!
        PlayerPrefs.SetInt(EpisodeKey, -1);
        float currentSessionTime = Time.realtimeSinceStartup - startTime;
        cumulativeTime += currentSessionTime;
        PlayerPrefs.SetFloat(TimeKey, 0f);
        PlayerPrefs.SetInt(SuccessfulParksKey, 0);
        */

        // Load saved values; if not found, start at zero
        episodeCount = PlayerPrefs.GetInt(EpisodeKey, 0);
        cumulativeTime = PlayerPrefs.GetFloat(TimeKey, 0f);
        successfulParkCount = PlayerPrefs.GetInt(SuccessfulParksKey, 0);

        // Set the start time for the new session
        startTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        // Display episode count on UI
        episodeText.text = "Episode: " + episodeCount.ToString();

        // Calculate elapsed time: cumulative time from previous sessions plus this session's time
        float currentSessionTime = Time.realtimeSinceStartup - startTime;
        float totalElapsed = cumulativeTime + currentSessionTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalElapsed);
        trainingTimeText.text = string.Format("Training Time: {0:D2}:{1:D2}:{2:D2}",
                                               timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

        // Display the number of successful parks on UI
        successfulParkText.text = "Successful Parks: " + successfulParkCount.ToString();
    }

    // Can be called from other scripts to increment the episode count (e.g., from ParkingAgent)
    public void IncrementEpisode()
    {
        episodeCount++;
    }

    // Can be called from other scripts to increment the successful park count
    public void IncrementSuccessfulParks()
    {
        successfulParkCount++;
    }

    // Save the current episode count, training time, and successful park count to PlayerPrefs when the application quits or is disabled
    void OnApplicationQuit()
    {
        SaveValues();
    }

    void OnDisable()
    {
        SaveValues();
    }

    // Saves the current session's progress to PlayerPrefs
    private void SaveValues()
    {
        PlayerPrefs.SetInt(EpisodeKey, episodeCount);
        float currentSessionTime = Time.realtimeSinceStartup - startTime;
        cumulativeTime += currentSessionTime;
        PlayerPrefs.SetFloat(TimeKey, cumulativeTime);
        PlayerPrefs.SetInt(SuccessfulParksKey, successfulParkCount);
        PlayerPrefs.Save();
    }
}
