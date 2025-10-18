using System.Collections;
using UnityEngine;

public class PlayerTemperatureController : MonoBehaviour
{
    public enum TemperatureState
    {
        Cold,
        Comfortable,
        Hot
    }

    [SerializeField] private int temperatureDecrease = 5;
    [SerializeField] private int temperatureIncrease = 5;
    [SerializeField] private float timerInterval = 5f;

    private GameManager _gm;

    private TemperatureState playerTemperatureState = TemperatureState.Comfortable;

    private void Start()
    {
        _gm = GameManager.Instance;
        StartCoroutine(TemperatureRoutine());
    }

    public IEnumerator TemperatureRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timerInterval);
            switch(playerTemperatureState)
            { 
                case TemperatureState.Cold:
                    _gm.IncreaseClimate(-temperatureDecrease);
                    break;

                case TemperatureState.Hot:
                    _gm.IncreaseClimate(temperatureIncrease);
                    break;

                case TemperatureState.Comfortable:
                    // No change in climate
                    break;
            }
        }
    }

    public void SetTemperatureState(TemperatureState newState)
    {
        if (playerTemperatureState != newState)
        {
            StopAllCoroutines();
            playerTemperatureState = newState;
            StartCoroutine(TemperatureRoutine());            
        }
    }
}
