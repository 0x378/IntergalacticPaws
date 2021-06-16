using UnityEngine;

public class EndMenuInterface : MonoBehaviour
{
    [SerializeField] private GameObject winConditionTitle;
    [SerializeField] private GameObject loseConditionTitle;

    private bool winCondition = false;

    private void Start()
    {
        winConditionTitle.SetActive(false);
        loseConditionTitle.SetActive(false);
    }

    public void SetCondition(bool isWin)
    {
        winCondition = isWin;
    }

    private void Update()
    {
        if (winCondition)
        {
            winConditionTitle.SetActive(true);
            loseConditionTitle.SetActive(false);
        }
        else
        {
            winConditionTitle.SetActive(false);
            loseConditionTitle.SetActive(true);
        }
    }
}
