using System;
using UnityEngine;

public class GameManager : GenericSingleton<GameManager>
{
    public override bool IsDestroyedOnLoad() => false;
    public override bool ShouldDetatchFromParent() => true;

    public Action<int, int> OnValueChanged;

    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int maxParanoia = 100;
    [SerializeField] private int maxHunger = 100;
    [SerializeField] private int maxHygiene = 100;

    [SerializeField] private int minHunger = 25;
    [SerializeField] private int minHygiene = 25;
    [SerializeField][Tooltip("Minimum paranoia level before penalties are applied. Paranoia increases")]
    private int minParanoia = 75;

    [SerializeField][Tooltip("Penalty applied when value is low.")]
    private int valuePenalty = 10;

    private int day;
    
    private int energy;    
    private int paranoia;
    private int hunger;
    private int hygiene;
    private int direction;
    public int Day => day;
    public int Energy => energy;
    public int Paranoia => paranoia;
    public int Hunger => hunger;
    public int Hygiene => hygiene;  


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
        OnValueChanged?.Invoke(previousDay, day);
    }

    public void ChangeEnergy(int amount)
    {
        int previousEnergy = energy;
        energy = Mathf.Clamp(energy + amount, 0, maxEnergy);
        OnValueChanged?.Invoke(previousEnergy, energy);
    }

    public void ChangeParanoia(int amount)
    {
        int previousParanoia = paranoia;
        paranoia = Mathf.Clamp(paranoia + amount, 0, maxParanoia);
        OnValueChanged?.Invoke(previousParanoia, paranoia);
    }

    public void ChangeHunger(int amount)
    {
        int previousHunger = hunger;
        if (Hygiene < minHygiene)
        {
            amount -= valuePenalty;
        }
        hunger = Mathf.Clamp(hunger + amount, 0, maxHunger);
        OnValueChanged?.Invoke(previousHunger, hunger);
    }

    public void ChangeHygiene(int amount)
    {
        int previousHygiene = hygiene;
        hygiene = Mathf.Clamp(hygiene + amount, 0, maxHygiene);
        OnValueChanged?.Invoke(previousHygiene, hygiene);
    }
}
