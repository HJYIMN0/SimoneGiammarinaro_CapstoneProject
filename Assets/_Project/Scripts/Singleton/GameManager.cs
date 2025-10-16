using System;
using UnityEngine;

public class GameManager : GenericSingleton<GameManager>
{
    public override bool IsDestroyedOnLoad() => false;
    public override bool ShouldDetatchFromParent() => true;

    public Action<int, int> OnValueChanged;

    [Header("MaxValues Allowed")]
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int maxParanoia = 100;
    [SerializeField] private int maxHunger = 100;
    [SerializeField] private int maxHygiene = 100;

    [Header("MinValues Allowed")]
    [SerializeField] private int minHunger = 25;
    [SerializeField] private int minHygiene = 25;
    [SerializeField][Tooltip("Minimum paranoia level before penalties are applied. Paranoia increases")]
    private int paranoiaTrigger = 75;

    [Header("Direction Settings")]
    [SerializeField]
    [Tooltip("Desired direction value. If the player's direction is different, penalties are applied.")]
    private int desiredDirection = 50;
    [SerializeField][Tooltip("Allowed deviation from the desired direction before penalties are applied.")]
    private int directionTolerance = 25;

    [Header("Climate Settings")]
    [SerializeField][Tooltip("Desired climate value. If the player's climate is different, penalties are applied.")]
    private int desiredClimate = 50;
    [SerializeField][Tooltip("Allowed deviation from the desired climate before penalties are applied.")]
    private int climateTolerance = 25;

    [Header("Integrity Settings")]
    [SerializeField][Tooltip("Desired integrity value. If the player's integrity is significantly different, penalties are applied.")]
    private int desiredIntegrity = 50;
    [SerializeField][Tooltip("Allowed deviation from the desired integrity before penalties are applied.")]
    private int integrityTolerance = 25;

    [Header("Penalty Settings")]
    [SerializeField][Tooltip("Max Penalty applied. Random Range will be applied")]
    private int valuePenalty = 10;
    [SerializeField]
    [Tooltip("Minum Increase accepted with penalty applied.")]
    private int minValueIncrease = 3;



    private int day;
    
    private int energy;    
    private int paranoia;
    private int hunger;
    private int hygiene;
    private int direction;
    private int climate;
    private int integrity;
    public int Day => day;
    public int Energy => energy;
    public int Paranoia => paranoia;
    public int Hunger => hunger;
    public int Hygiene => hygiene;  
    public int Direction => direction;
    public int Climate => climate;


    private SaveData currentSave;

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

    public void ChangeDay(int amount)
    {
        int previousDay = day;
        day += amount;
        if (day != previousDay)
        { 
            OnValueChanged?.Invoke(previousDay, day);
        }
    }

    public void ChangeEnergy(int amount)
    {
        int previousEnergy = energy;
        energy = Mathf.Clamp(energy + amount, 0, maxEnergy);
        if (CompareValues(previousEnergy, energy))
        {
            if (paranoia > paranoiaTrigger)
            {
                energy = Mathf.Clamp(energy - (UnityEngine.Random.Range(0, valuePenalty)), 0, maxEnergy);
            }
            OnValueChanged?.Invoke(energy, maxEnergy);
        }
    }       

    public void ChangeParanoia(int amount)
    {
        int previousParanoia = paranoia;
        paranoia = Mathf.Clamp(paranoia + amount, 0, maxParanoia);
        if (CompareValues(previousParanoia, paranoia))
        {
            OnValueChanged?.Invoke(previousParanoia, maxParanoia);
        }
    }

    public void ChangeHunger(int amount)
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

    public void ChangeHygiene(int amount)
    {
        int previousHygiene = hygiene;
        hygiene = Mathf.Clamp(hygiene + amount, 0, maxHygiene);
        if (CompareValues(previousHygiene, hygiene))
        {
            if (hygiene > previousHygiene)
            {
                //Apply penalties only if hygiene is increasing

            }
            OnValueChanged?.Invoke(previousHygiene, maxHygiene);
        }
    }

    private bool CompareValues(int previous, int current) => previous != current;

    private bool IsValueOutOfTolerance(int value, int desiredValue, int valueTolerance) 
    {
        return Mathf.Abs(value - desiredValue) > valueTolerance;
    }

    private bool IsHygieneOutOfTolerance() => hygiene <= minHygiene;

    private bool IsClimateOutOfTolerance() => IsValueOutOfTolerance(climate, desiredClimate, climateTolerance);
}
