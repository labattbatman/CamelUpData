using UnityEngine;
using System.Collections.Generic;

public class test : MonoBehaviour {

    PatternGenerator m_PatternGenerator = new PatternGenerator();
    float maxFloat = 1.0f;
    float timer;
    public void Start()
    {
        m_PatternGenerator.Init();

        //foreach (var test in GameRules.ResultToPattern(";ABCD;;;;;;;;;;;;;E"))
        //    Debug.Log(test);
    }

    public void Update()
    {
        AllPoutine();
    }

    public void AllPoutine()
    {
        if (m_PatternGenerator.PatternsCount < 10010 && m_PatternGenerator.RemainingPatternsToDiscover > 0)
        {
            m_PatternGenerator.Update();
        }
        else
        {
            m_PatternGenerator.SaveLastPatterns();
            string log = string.Format("{0} {1} {2}", Time.timeSinceLevelLoad, m_PatternGenerator.PatternsCount, m_PatternGenerator.RemainingPatternsToDiscover);
            Debug.Log(log);
            Debug.Break();
        }
    }
}
