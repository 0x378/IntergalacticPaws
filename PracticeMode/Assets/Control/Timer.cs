using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Timer : MonoBehaviour
{
    [SerializeField] [Range(1, 600)] [Tooltip("Match duration is in seconds.")] int matchDuration;
    [SerializeField] [Range(1, 10)] float countdown;
    [SerializeField] [Range(10, 20)] float domeSpeed;
    float minutes, seconds, milliseconds, timeRemaining, countdownRemaining;
    [SerializeField] GameObject timerTextObj, countdownTextObj, UI_HTP, dome, boss, surviveBossTextObj, enemyNumberTextObj;
    TextMeshProUGUI timerTextTMP, countdownTextTMP, surviveBossTextTMP, enemyNumberTextTMP;
    int numOfEnemiesAlive;
    public Timer()
    {

    }
    public Timer(int matchDurationInSeconds)
    {
        timeRemaining = matchDuration = matchDurationInSeconds;
    }
    void Start()
    {
        timeRemaining = matchDuration + countdown;
        countdownRemaining = countdown;
        minutes = Mathf.FloorToInt(matchDuration / 60);
        seconds = Mathf.FloorToInt(matchDuration % 60);
        timerTextTMP = timerTextObj.GetComponent<TextMeshProUGUI>();
        countdownTextTMP = countdownTextObj.GetComponent<TextMeshProUGUI>();
        surviveBossTextTMP = surviveBossTextObj.GetComponent<TextMeshProUGUI>();
        enemyNumberTextTMP = enemyNumberTextObj.GetComponent<TextMeshProUGUI>();
        timerTextObj.SetActive(false);
        boss.SetActive(false);
        surviveBossTextObj.SetActive(false);
    }
    void Update()
    {
        numOfEnemiesAlive = GameManager.Instance.NumberOfEnemiesAlive();
        StartCoroutine(countdownTimer());
    }
    IEnumerator playMatchTimer()
    {
        if (timerTextObj.activeSelf == false) timerTextObj.SetActive(true);
        if (timeRemaining > 0f) timeRemaining -= Time.deltaTime;
        buildTimerText(timeRemaining);

        if (dome.activeSelf && ((timeRemaining <= matchDuration / 2f + 5f) || (GameManager.targetSelector.enemiesAreDestroyed)))
        {
            StartCoroutine(halfTime());
        }

        yield return new WaitUntil(() => timeRemaining <= 0f);
        matchTimerExpired();
    }
    IEnumerator countdownTimer()
    {
        if (countdownRemaining > 0f) countdownRemaining -= Time.deltaTime;
        countdownTextTMP.text = string.Format("{0}", Mathf.CeilToInt(countdownRemaining));
        yield return new WaitUntil(() => countdownRemaining <= 0f);
        countdownTextTMP.text = string.Format("Go!");
        Invoke("disableCountdownText", 1f);
        StartCoroutine(playMatchTimer());
    }
    IEnumerator halfTime()
    {
        boss.SetActive(true);
        Vector3 initial = dome.transform.position;
        Vector3 target = new Vector3(initial.x, initial.y + 100f, initial.z);
        dome.transform.position = Vector3.MoveTowards(initial, target, domeSpeed * Time.smoothDeltaTime);
        if (countdownTextObj.activeSelf == false)
        {
            countdownTextTMP.fontSize = 180;
            countdownTextTMP.text = "Gravity Warning!";
            countdownTextObj.transform.position += Vector3.up * Time.smoothDeltaTime;
            countdownTextObj.SetActive(true);
        }
        yield return new WaitUntil(() => dome.transform.position.y >= target.y);
        dome.SetActive(false);
        countdownTextObj.SetActive(false);
        GameManager.Instance.ToggleGravityMode();
        surviveBossTextObj.SetActive(true);
    }
    void buildTimerText(float t)
    {
        minutes = Mathf.FloorToInt(t / 60);
        seconds = Mathf.FloorToInt(t % 60);
        milliseconds = (t % 1) * 1000;
        if (minutes > 0)
        {
            timerTextTMP.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
        else if (t <= 0)
        {
            timerTextTMP.text = "0:00:000";
        }
        else
        {
            timerTextTMP.text = string.Format("{0:0}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
        enemyNumberTextTMP.text = string.Format("{0}/10", numOfEnemiesAlive);
    }
    void matchTimerExpired()
    {
        GameManager.Instance.Halt();
    }
    void disableCountdownText()
    {
        countdownTextObj.SetActive(false);
    }
    public bool isTimerActive()
    {
        return (timeRemaining > 0f && timeRemaining < matchDuration);
    }
    public bool pastHalfTime()
    {
        return (timeRemaining <= (matchDuration / 2));
    }
    public float returnTimeRemainingPercent()
    {
        return timeRemaining / matchDuration;
    }
    public int returnTimeRemainingSeconds()
    {
        return Mathf.CeilToInt(timeRemaining);
    }
}