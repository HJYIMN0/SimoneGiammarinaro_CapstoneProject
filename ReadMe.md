
## 📊 Diagramma delle Comunicazioni

```
┌─────────────────────────────────────────────────────────────┐
│                      GAME FLOW                              │
│                  (GameFlowManager)                          │
└───────────────────┬─────────────────────────────────────────┘
                    │
         ┌──────────┴──────────┐
         │                     │
         ▼                     ▼
┌────────────────────┐  ┌──────────────────┐
│  TaskManager       │  │  InkManager      │
│  (Singleton)       │  │  (Singleton)     │
│                    │  │                  │
│ • AddTask()        │  │ • StartDialogue()│
│ • CompleteTask()   │  │ • ContinueDialog │
│ • AreAllCompleted()│  │ • EndDialogue()  │
└────────┬───────────┘  └──────────┬───────┘
         │                         │
         │  OnInteraction          │  ShowDialogue()
         │                         │
         ▼                         ▼
    ┌────────────────────────────────────┐
    │   AbstractInteractable (BASE)      │
    │                                    │
    │ • TaskSO task                      │
    │ • EvaluateCanvaStatus()            │
    │ • ShowDialogue()                   │
    │ • InteractWithTask() [abstract]    │
    └────────────┬───────────────────────┘
                 │
         ┌───────┴────────┐
         │                │
         ▼                ▼
    [SubClass1]      [SubClass2]
    (InteractableX)  (InteractableY)
         │                │
         └───────┬────────┘
                 │ References
                 ▼
    ┌─────────────────────────────────┐
    │  PlayerInteractionController    │
    │                                 │
    │ • interactableTask              │
    │ • SetInteractableTask()         │
    │ • ClearInteractableTask()       │
    │ • HandleInteraction()           │
    └────────────┬────────────────────┘
                 │
                 ▼
    ┌─────────────────────────────────┐
    │   PlayerDialogueController      │
    │                                 │
    │ • IsDialogueActive              │
    └─────────────────────────────────┘
```

---

## 🔴 Problemi Individuati (Responsabilità Mescolate)

### 1. **AbstractInteractable fa troppe cose** ⚠️
Attualmente gestisce:
- ✅ Logica di interazione (InteractWithTask)
- ❌ **Detection del player** (OverlapSphere) → Dovrebbe stare in un Detector
- ❌ **Gestione Canvas** (istanza, posizione, attivazione) → Dovrebbe stare in UIManager
- ❌ **Comunicazione con PlayerInteractionController** → Strettamente accoppiato
- ❌ **Comunicazione con TaskManager** → Strettamente accoppiato
- ❌ **Comunicazione con InkManager** → Strettamente accoppiato

```
AbstractInteractable fa 5+ cose contemporaneamente!
```

### 2. **Accoppiamenti Stretti** 🔗

```csharp
// AbstractInteractable è tightly coupled a:
TaskManager.Instance.AddTask(task);           // ← Singleton
InkManager.Instance.ClearStoryAndTextAsset(); // ← Singleton
playerInteractionController.SetInteractable...(); // ← Direct reference

// PlayerInteractionController è coupled a:
AbstractInteractable interactableTask;        // ← Diretta dipendenza
```

### 3. **Update Loop Pesante** 💪
`AbstractInteractable.Update()` esegue `Physics.OverlapSphere` **ogni frame** = pessima performance!

---

## ✨ Refactoring Consigliato

### **Step 1: Estrai la Detection in una classe separata**

```csharp
// ✅ NUOVO: InteractionDetector.cs
public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 2f;
    public event System.Action<GameObject> OnPlayerEntered;
    public event System.Action OnPlayerExited;

    private void Update()
    {
        Collider[] player = Physics.OverlapSphere(transform.position, detectionRadius);
        if (player.Length > 0)
            OnPlayerEntered?.Invoke(player[0].gameObject);
        else
            OnPlayerExited?.Invoke();
    }
}
```

### **Step 2: Estrai la Gestione Canvas in UIManager**

```csharp
// ✅ NUOVO: InteractionUIManager.cs
public class InteractionUIManager : MonoBehaviour
{
    [SerializeField] private GameObject canvaPrefab;
    private GameObject canvaInstance;
    
    public void ShowCanvas(Vector3 pos, Quaternion rot)
    {
        if (canvaInstance == null)
            canvaInstance = Instantiate(canvaPrefab, pos, rot);
        else
        {
            canvaInstance.transform.position = pos;
            canvaInstance.SetActive(true);
        }
    }
    
    public void HideCanvas() => canvaInstance?.SetActive(false);
}
```

### **Step 3: Crea un Interaction Broker (Event-based)**

```csharp
// ✅ NUOVO: InteractionEventBus.cs
public class InteractionEventBus : MonoBehaviour
{
    public static event System.Action<AbstractInteractable> OnInteract;
    public static event System.Action<AbstractInteractable> OnPlayerNear;
    public static event System.Action OnPlayerFar;
    
    public static void BroadcastInteraction(AbstractInteractable interactable) 
        => OnInteract?.Invoke(interactable);
    
    // ... altri metodi
}
```

### **Step 4: Ripulisci AbstractInteractable**

```csharp
// ✅ SEMPLIFICATO: AbstractInteractable.cs
public abstract class AbstractInteractable : MonoBehaviour
{
    [SerializeField] protected TaskSO task;
    public TaskSO TaskSO => task;
    public bool HasBeenInteractedWith { get; protected set; }

    protected virtual void Start()
    {
        if (task != null)
            TaskManager.Instance.AddTask(task);
    }

    public abstract void InteractWithTask();

    public virtual void ShowDialogue(TextAsset dialogue, bool usesVariables)
    {
        if (dialogue != null)
            InkManager.Instance.StartDialogue(dialogue, usesVariables);
    }
}
```

---

## 📋 Architettura Proposta

```
┌─────────────────────────────────────────────┐
│           Event Bus / Message System         │  ← Loose Coupling
└─────────────────────────────────────────────┘
           ▲        ▲        ▲        ▲
           │        │        │        │
    ┌──────┴──┐ ┌──┴──────┐ ┌┴────┐ ┌┴──────┐
    │Detector │ │  UI     │ │Task │ │Dialogue│
    │Manager  │ │Manager  │ │Manager│Manager│
    └──────────┘ └─────────┘ └─────┘ └────────┘
           │        │        │        │
           └────────┼────────┼────────┘
                    ▼
         ┌──────────────────────┐
         │ AbstractInteractable │
         │   (Semplificato)     │
         └──────────────────────┘
```

---

## 🎯 Benefici del Refactoring

| Aspetto | Prima | Dopo |
|---------|-------|------|
| **Responsabilità** | 5+ compiti | 1 compito |
| **Coupling** | Strettissimo | Loose (Event-based) |
| **Testabilità** | Difficile | Facile |
| **Riusabilità** | Bassa | Alta |
| **Performance** | Physics.OverlapSphere ogni frame | Solo quando necessario |
| **Manutenibilità** | Difficile | Semplice |

Vuoi che ti faccia esempi di codice per uno di questi refactoring?