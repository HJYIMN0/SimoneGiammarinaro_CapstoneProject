using UnityEngine;

public class PlayerActionSystem : GenericSingleton<PlayerActionSystem>
{
    public override bool IsDestroyedOnLoad() => true;
    public override bool ShouldDetatchFromParent() => true;

    private GameManager _gm;

    [Header("Energy Settings")]
    private int sleepEnergyRecover = 20;

    [Header("Hunger Settings")]
    private int energyToEat = 10;
    private int hungerIncrease = 20;

    [Header("Hygiene Settings")]
    private int energyToWash = 30;
    private int hygieneIncrease = 40;
    private void Start()
    {
        _gm = GameManager.Instance;
    }

    public bool CanSleep() => _gm.Paranoia < _gm.ParanoiaTrigger;
    public bool HasEnoughEnergy(int amountNeeded)
    {
        return _gm.Energy >= amountNeeded;
    }
    public void Sleep()
    {
        if (!CanSleep()) return;
        _gm.IncreaseDay(1);
        _gm.IncreaseEnergy(_gm.Paranoia < _gm.ParanoiaTrigger ? _gm.MaxEnergy : sleepEnergyRecover);
    }

    public bool CanEat() => HasEnoughEnergy(energyToEat);
    public void Eat()
    {
        if (!CanEat()) return;
        _gm.IncreaseEnergy(-energyToEat);
        _gm.IncreaseHunger(20);
    }
    
    public bool CanWash() => HasEnoughEnergy(energyToWash);
    public void Wash()
    {
        if (!CanWash()) return;
        _gm.IncreaseEnergy(-energyToWash);
        _gm.IncreaseHygiene(hygieneIncrease);
    }
}
