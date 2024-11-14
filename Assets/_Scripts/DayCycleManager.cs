using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DayCycleManager : NetworkBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    public NetworkVariable<float> _currentTime = new(0);
    public NetworkVariable<bool> DayActive { get; private set; } = new(true);

    private static int _numDays = 3;
    private NetworkVariable<int> _currentDay = new(1);
    private static float _dayLength = 120;

    private static float _winAmount = 50;

    [SerializeField] private TextMeshProUGUI _timeText;

    [SerializeField] private Transform _sun;
    [SerializeField] private Animator _dayComplete;
    [SerializeField] private TextMeshProUGUI _dayText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartDay()
    {
        // DayActive.Value = true;
    }

    private void Update()
    {
        if (!DayActive.Value)
        {
            return;
        }

        int flooredTime = Mathf.FloorToInt(_currentTime.Value);
        int seconds = flooredTime % 60;
        int minutes = flooredTime / 60;
        _timeText.text = minutes.ToString("00") + " : " + seconds.ToString("00");

        float sunXRotaion = Mathf.Lerp(140, 160, _currentTime.Value / _dayLength);
        _sun.rotation = Quaternion.Euler(sunXRotaion, -30, 0);

        _dayText.text = "Day " + _currentDay.Value;

        if (!IsServer)
        {
            return;
        }

        if (DayActive.Value)
        {
            _currentTime.Value += Time.deltaTime;
        }

        if (_currentTime.Value >= _dayLength && _currentDay.Value < _numDays)
        {
            StartCoroutine(NextDay());
        }
        else if (_currentTime.Value >= _dayLength)
        {
            EndGame();
        }
    }

    private IEnumerator NextDay(float pauseTime = 10)
    {
        Stop();
        _dayComplete.SetTrigger("Fade");

        yield return new WaitForSeconds(pauseTime);

        DayActive.Value = true;
        _currentTime.Value = 0;
        _currentDay.Value++;
    }

    public void Stop()
    {
        Customer.MakeAllCustomersLeave();
        DayActive.Value = false;
    }

    private void EndGame()
    {
        if (ScoreManager.Instance.Score > _winAmount)
        {
            RestaurantManager.Instance.Win();
        }
        else
        {
            RestaurantManager.Instance.Lose();
        }

        Stop();
    }
}
