using UnityEngine;

public class PlayerActionSystem : GenericSingleton<PlayerActionSystem>
{
    public override bool IsDestroyedOnLoad() => true;
    public override bool ShouldDetatchFromParent() => true;

    private GameManager _gm;

    [Header("Energy Settings")]
    [SerializeField] private int sleepEnergyRecover = 20;

    [Header("Hunger Settings")]
    [SerializeField] private int energyToEat = 10;
    [SerializeField] private int hungerIncrease = 20;

    [Header("Hygiene Settings")]
    [SerializeField] private int energyToWash = 30;
    [SerializeField] private int hygieneIncrease = 40;

    [Header("Direction Settings")]
    [SerializeField] private int energyToWork = 20;
    [SerializeField] private int directionIncrease = 15;

    [Header("Integrity Settings")]
    [SerializeField] private int energyToRepair = 25;
    [SerializeField] private int integrityIncrease = 30;

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
        _gm.IncreaseHunger(hungerIncrease);
    }
    
    public bool CanWash() => HasEnoughEnergy(energyToWash);
    public void Wash()
    {
        if (!CanWash()) return;
        _gm.IncreaseEnergy(-energyToWash);
        _gm.IncreaseHygiene(hygieneIncrease);
    }

    public bool CanWork() => HasEnoughEnergy(energyToWork);
    public void Work()
    {
        if (!CanWork()) return;
        _gm.IncreaseEnergy(-energyToWork);
        _gm.IncreaseDirection(directionIncrease);
    }

    public void ConsultMap()
    {
        if (!CanWork()) return;
        _gm.IncreaseEnergy(-energyToWork);
        _gm.IncreaseDirection(-directionIncrease);
    }

    public bool CanRepair() => HasEnoughEnergy(energyToRepair);
    public void Repair()
    {
        if (!CanRepair()) return;
        _gm.IncreaseEnergy(-energyToRepair);
        _gm.IncreaseIntegrity(integrityIncrease);
    }
}
