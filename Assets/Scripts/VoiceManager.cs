using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;

public class VoiceManager : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> actions = new Dictionary<string, System.Action>();

    void Start()
    {
        // Add voice commands and their corresponding actions here
        actions.Add("doom", ActivateDoomAbility);

        // Initialize the KeywordRecognizer with the commands
        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnCommandRecognized;
        keywordRecognizer.Start();
    }

    private void OnCommandRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("You said: " + args.text);
        actions[args.text].Invoke();
    }

    void ActivateDoomAbility()
    {
        Debug.Log("DOOM ability activated!");
        if (EnemyManager.Instance.GetCurrentPoints() >= 500)
        {
            EnemyManager.Instance.DestroyAllEnemies();
            EnemyManager.Instance.RemovePoints(500); // Subtract points after using nuke
        }
        
    }

    private void OnDestroy()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }

}
