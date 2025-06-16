using System.Collections.Generic;
using RepoXR.UI.Expressions;
using UnityEngine;

namespace RepoXR.Managers;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public bool headlampEnabled;
    public List<ExpressionPart.Expression> activeExpressions = [];
    
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Resets all data for the current run
    /// </summary>
    public void ResetData()
    {
        headlampEnabled = false;
        activeExpressions.Clear();
    }
}