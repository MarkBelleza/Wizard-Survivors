using UnityEngine;
using TMPro;
public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public EnemySpawner enemySpawner;


    // Update is called once per frame
    void Update()
    {
        if (enemySpawner.GetisSpawning())
        {
            scoreText.text = "AURA: " + EnemyManager.Instance.GetCurrentPoints().ToString();
        }
    }
}
