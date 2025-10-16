using System;
using UnityEngine;

public class GameManager : GenericSingleton<GameManager>
{
    #region singleton Properties
    public override bool IsDestroyedOnLoad() => false;
    public override bool ShouldDetatchFromParent() => true;

    public Action<int, int> OnValueChanged;
    #endregion

    #region Inspector Properties
    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;

    [Header("Paranoia Settings")]
    [SerializeField][Tooltip("Minimum paranoia level before penalties are applied. Paranoia increases")]
    private int paranoiaTrigger = 75;
    [SerializeField] private int maxParanoia = 100;

    [Header("Hunger Settings")]
    [SerializeField][Tooltip("Hunger cap")]
    private int maxHunger = 100;
    [SerializeField][Tooltip("Below this value, penalties are applied")]
    private int minHunger = 25;

    [Header("Hygiene Settings")]
    [SerializeField][Tooltip("Hygiene Cap")]
    private int maxHygiene = 100;
    [SerializeField][Tooltip("Below this value, penatlies are applied")]
    private int minHygiene = 25;


    [Header("Direction Settings")]
    [SerializeField]
    [Tooltip("Desired direction value. If the player's direction is different, penalties are applied.")]
    private int desiredDirection = 50;
    [SerializeField][Tooltip("Allowed deviation from the desired direction before penalties are applied.")]
    private int directionTolerance = 25;
    [SerializeField][Tooltip("Direction cap")]
    private int maxDirection = 100;

    [Header("Climate Settings")]
    [SerializeField][Tooltip("Desired climate value. If the player's climate is different, penalties are applied.")]
    private int desiredClimate = 50;
    [SerializeField][Tooltip("Allowed deviation from the desired climate before penalties are applied.")]
    private int climateTolerance = 25;
    [SerializeField][Tooltip("Climate top cap")]
    private int maxClimate = 100;
    [SerializeField][Tooltip("Climate bottom cap")]
    private int minClimate = 0;

    [Header("Integrity Settings")]
    [SerializeField][Tooltip("Desired integrity value. If the player's integrity is significantly different, penalties are applied.")]
    private int maxIntegrity = 50;
    [SerializeField][Tooltip("Below this value, penalites are applied")]
    private int minIntegrity = 15;

    [Header("Penalty Settings")]
    [SerializeField][Tooltip("Max Penalty applied. Random Range will be applied")]
    private int valuePenalty = 10;
    [SerializeField]
    [Tooltip("Minum Increase accepted with penalty applied.")]
    private int minValueIncrease = 3;
    #endregion


    #region Private Properties
    private int day;
    
    private int energy;    
    private int paranoia;
    private int hunger;
    private int hygiene;
    private int direction;
    private int climate;
    private int integrity;

    private SaveData currentSave;
    #endregion

    #region Public Properties
    public int Day => day;
    public int Energy => energy;
    public int MaxEnergy => maxEnergy;
    public int Paranoia => paranoia;
    public int Hunger => hunger;
    public int Hygiene => hygiene;  
    public int Direction => direction;
    public int Climate => climate;
    public int Integrity => integrity;
    public int ParanoiaTrigger => paranoiaTrigger;
    #endregion



    public override void Awake()
    {
        base.Awake();

        currentSave = SaveSystem.LoadOrInitialize();

        day = currentSave.day;
        energy = currentSave.energy;
        paranoia = currentSave.paranoia;
        hunger = currentSave.hunger;
        hygiene = currentSave.hygiene;
    }

    #region Statistic Methods
    #region Day
    /// <summary>
    /// Adds the specified amount to the current value.
    /// This method does not overwrite the value directly.
    /// </summary>
    /// <param name="amount">The value to add to the current total.</param>
    public void IncreaseDay(int amount)
    {
        int previousDay = day;
        day += amount;
        if (day != previousDay)
        { 
            OnValueChanged?.Invoke(previousDay, day);
        }
    }
    #endregion
    #region Energy
    /// <summary>
    /// Adds the specified amount to the current value.
    /// This method does not overwrite the value directly.
    /// </summary>
    /// <param name="amount">The value to add to the current total.</param>
    public void IncreaseEnergy(int amount)
    {
        int previousEnergy = energy;
        energy = Mathf.Clamp(energy + amount, 0, maxEnergy);
        if (CompareValues(previousEnergy, energy))
        {
            if (paranoia > paranoiaTrigger && energy > previousEnergy)//Only if we're increasing energy
            {
                energy = Mathf.Clamp(energy - (UnityEngine.Random.Range(0, valuePenalty)),
                                    previousEnergy + minValueIncrease,
                                    maxEnergy);
            }
            OnValueChanged?.Invoke(energy, maxEnergy);
        }
    }
    #endregion
    #region Paranoia
    /// <summary>
    /// Adds the specified amount to the current value.
    /// This method does not overwrite the value directly.
    /// </summary>
    /// <param name="amount">The value to add to the current total.</param>
    public void IncreaseParanoia(int amount)
    {
        int previousParanoia = paranoia;
        paranoia = Mathf.Clamp(paranoia + amount, 0, maxParanoia);
        if (CompareValues(previousParanoia, paranoia))
        {
            OnValueChanged?.Invoke(previousParanoia, maxParanoia);
        }
    }
    #endregion
    #region Hunger
    /// <summary>
    /// Adds the specified amount to the current value.
    /// This method does not overwrite the value directly.
    /// </summary>
    /// <param name="amount">The value to add to the current total.</param>

    public void IncreaseHunger(int amount)
    {
        int previousHunger = hunger; 
        hunger = Mathf.Clamp(hunger + amount, 0, maxHunger);

        if (CompareValues(previousHunger, hunger))
        {
            //Apply penalties only if hunger is increasing
            if (hunger > previousHunger)
            {
                int penaltyCounter = 0;
                if (IsHygieneOutOfTolerance()) penaltyCounter++;
                if (IsClimateOutOfTolerance()) penaltyCounter++;

                //if penaltyCounter is 0, no penalties are applied
                if (penaltyCounter > 0)
                {
                    hunger = Mathf.Clamp(hunger - (UnityEngine.Random.Range(0, valuePenalty) * penaltyCounter),
                    previousHunger + minValueIncrease, //making sure hunger doesn't decrease below previous value
                    maxHunger);
                }
            }

            OnValueChanged?.Invoke(hunger, maxHunger);
        }
    }
    #endregion
    #region Hygiene
    /// <summary>
    /// Adds the specified amount to the current value.
    /// This method does not overwrite the value directly.
    /// </summary>
    /// <param name="amount">The value to add to the current total.</param>

    public void IncreaseHygiene(int amount)
    {
        int previousHygiene = hygiene;
        hygiene = Mathf.Clamp(hygiene + amount, minHygiene, maxHygiene);
        if (CompareValues(previousHygiene, hygiene))
        {
            //Apply penalties only if hygiene is increasing
            if (hygiene > previousHygiene)
            {             
                int penaltyCounter = 0;
                if (IsClimateOutOfTolerance()) penaltyCounter++;
                if (IsIntegrityOutOfTolerance()) penaltyCounter++;

                if (penaltyCounter > 0)
                {
                    hygiene = Mathf.Clamp(hygiene - (UnityEngine.Random.Range(0, valuePenalty) * penaltyCounter),
                    previousHygiene + minValueIncrease, //making sure hygiene doesn't decrease below previous value
                    maxHygiene);
                }
            }

            OnValueChanged?.Invoke(previousHygiene, maxHygiene);
        }
    }
    #endregion
    #region Climate

    /// <summary>
    /// Adds the specified amount to the current value.
    /// This method does not overwrite the value directly.
    /// </summary>
    /// <param name="amount">The value to add to the current total.</param>
    public void IncreaseClimate(int amount)
    {
        int previousClimate = climate;
        climate = Mathf.Clamp(climate + amount, minClimate, maxClimate);
        if (CompareValues(previousClimate, climate))
        {
            //Apply penalties only if climate is increasing
            if (climate > previousClimate)
            {
                int penaltyCounter = 0;
                if (IsIntegrityOutOfTolerance()) penaltyCounter++;
                if (IsDirectionOutOfTolerance()) penaltyCounter++;
                if (penaltyCounter > 0)
                {
                    climate = Mathf.Clamp(climate - (UnityEngine.Random.Range(0, valuePenalty) * penaltyCounter),
                    previousClimate + minValueIncrease, //making sure climate doesn't decrease below previous value
                    maxClimate);
                }
            }
            OnValueChanged?.Invoke(climate, desiredClimate);
        }
    }
    #endregion
    #region Direction   
    /// <summary>
    /// Adds the specified amount to the current value.
    /// This method does not overwrite the value directly.
    /// </summary>
    /// <param name="amount">The value to add to the current total.</param>
    public void IncreaseDirection(int amount)
    {
        int previousDirection = direction;
        direction = Mathf.Clamp(direction + amount, 0, 100);
        if (CompareValues(previousDirection, direction))
        {
            //Apply penalties only if direction is increasing
            if (direction > previousDirection)
            {
                int penaltyCounter = 0;
                if (IsHygieneOutOfTolerance()) penaltyCounter++;
                if (IsHungerOutOfTolerance()) penaltyCounter++;
                if (penaltyCounter > 0)
                {
                    direction = Mathf.Clamp(direction - (UnityEngine.Random.Range(0, valuePenalty) * penaltyCounter),
                    previousDirection + minValueIncrease, //making sure direction doesn't decrease below previous value
                    maxDirection);
                }
            }
            OnValueChanged?.Invoke(direction, desiredDirection);
        }
    }
    #endregion
    #region Integrity
    /// <summary>
    /// Adds the specified amount to the current value.
    /// This method does not overwrite the value directly.
    /// </summary>
    /// <param name="amount">The value to add to the current total.</param>
    public void IncreaseIntegrity(int amount)
    {
        int previousIntegrity = integrity;
        integrity = Mathf.Clamp(integrity + amount, 0, maxIntegrity);
        if (CompareValues(previousIntegrity, integrity))
        {
            //Apply penalties only if integrity is increasing
            if (integrity > previousIntegrity)
            {
                int penaltyCounter = 0;
                if (IsHungerOutOfTolerance()) penaltyCounter++;
                if (IsDirectionOutOfTolerance()) penaltyCounter++;
                if (penaltyCounter > 0)
                {
                    integrity = Mathf.Clamp(integrity - (UnityEngine.Random.Range(0, valuePenalty) * penaltyCounter),
                    previousIntegrity + minValueIncrease, //making sure integrity doesn't decrease below previous value
                    maxIntegrity);
                }
            }
            OnValueChanged?.Invoke(integrity, maxIntegrity);
        }
    }
    #endregion
    #endregion

    #region Value Comparison Methods
    private bool CompareValues(int previous, int current) => previous != current;

    private bool IsValueOutOfTolerance(int value, int desiredValue, int valueTolerance) 
    {
        return Mathf.Abs(value - desiredValue) > valueTolerance;
    }

    public bool IsHygieneOutOfTolerance() => hygiene <= minHygiene;
    public bool IsHungerOutOfTolerance() => hunger <= minHunger;
    public bool IsIntegrityOutOfTolerance() => integrity <= minIntegrity;
    public bool IsClimateOutOfTolerance() => IsValueOutOfTolerance(climate, desiredClimate, climateTolerance);
    public bool IsDirectionOutOfTolerance() => IsValueOutOfTolerance(direction, desiredDirection, directionTolerance);
    #endregion
}
