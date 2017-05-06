using System.Collections.Generic;
using System.Diagnostics;

public class PatternGenerator
{
    private readonly double m_AllowedTimeByFrame = 16d;
    private readonly int m_PatternDiscoveredToSave = 10000;

    private int m_PatternDiscoveredSinceSave;
    private Stopwatch m_Stopwatch = new Stopwatch();

    private List<string> m_PatternsToDiscover = new List<string>();
    private List<Pattern> m_PatternsDiscoveredButNotSaved = new List<Pattern>();
    public Dictionary<string, Pattern> m_Patterns = new Dictionary<string, Pattern>();

    public int PatternsCount { get { return m_Patterns.Count; } }
    public int RemainingPatternsToDiscover { get { return m_PatternsToDiscover.Count; } }

    private Pattern PopulatePatternDict(string aParttern)
    {
        Pattern newPattern = GeneratePatternData(aParttern);

        if (m_Patterns.ContainsKey(newPattern.Id))
            GameRules.Log(newPattern.Id + " " + aParttern);

        m_Patterns.Add(newPattern.Id, newPattern);

        for(int i = 0; i < newPattern.ResultsInList.Count; i++)
        {
            List<string> formatedResults = GameRules.PatternResultToPattern(newPattern.ResultsInList[i]);

            foreach (string formatedResult in formatedResults)
            {
                if (!m_Patterns.ContainsKey(formatedResult) && !m_PatternsToDiscover.Contains(formatedResult))
                {
                    m_PatternsToDiscover.Add(formatedResult);
                }
            }
        }

        return newPattern;
    }

    private Pattern GeneratePatternData(string aPattern)
    {
        CamelsMovement camelsMovement = new CamelsMovement(aPattern);
        Pattern pattern = new Pattern(aPattern, camelsMovement.GetCamelsResults());
        return pattern;
    }  

    private void GeneratePatterns()
    {
        if (m_Stopwatch.IsRunning || m_PatternsToDiscover.Count == 0)
        {
            return;
        }

        m_Stopwatch.Start();
        while (m_PatternsToDiscover.Count != 0 && m_Stopwatch.ElapsedMilliseconds < m_AllowedTimeByFrame)
        {
            m_PatternsDiscoveredButNotSaved.Add(PopulatePatternDict(m_PatternsToDiscover[0]));
            m_PatternsToDiscover.Remove(m_PatternsToDiscover[0]);
            if (m_PatternsDiscoveredButNotSaved.Count > m_PatternDiscoveredToSave)
            {
                SaveNewPatterns();
            }
        }

        m_Stopwatch.Stop();
    }

    private void SaveNewPatterns()
    {
        GameRules.Log("Je me save");
        SaveManager.Instance.Save(m_PatternsDiscoveredButNotSaved, false);
        m_PatternsDiscoveredButNotSaved.Clear();
    }

    public void Init()
    {
        m_PatternsToDiscover.Add(";ABCDE;;+;;+");
    }

    public void Update()
    {
        m_Stopwatch.Reset();
        GeneratePatterns();
    }

    public void SaveLastPatterns()
    {
        SaveNewPatterns();
    }
}
