using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ClockAndBatterySystem : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI batteryText;
    public TextMeshProUGUI nightText; // Texte pour afficher "Nuit X"
    public TextMeshProUGUI moneyText; // Texte pour afficher l'argent

    private float hourCounter = 0f;
    private float timeUpdateInterval = 10f;
    private float startTime;

    private bool resetHourInProgress = false;
    private bool isWaitingAfterReset = false;

    private float battery = 100f;
    private float batteryDecrementTime = 100f;
    private bool batteryResetInProgress = false;
    private float resetTimer = 0f;
    private float startBatteryTime = 0f;

    public Button toggleButton;
    public GameObject objectToDisplay;
    public Button movingImageButton; // Le bouton qui représentera l'image
    private bool isObjectDisplayed = false;

    public float batteryIncreaseAmount = 10f;
    private int nightCounter = 1; // Compteur pour suivre la nuit actuelle
    private int money = 0; // Montant total d'argent accumulé

    private int maxAppearancesPerNight = 5; // Nombre maximum d'apparitions pour la première nuit

    private int clickCount = 0; // Nombre de clics sur l'image
    private const int maxClicksToMove = 20; // Nombre de clics requis pour déplacer l'image

    public float moveAmount = 10f; // Valeur du déplacement du bouton (modifiable dans l'Inspector)

    private bool isMovingAllowed = true; // Permet de contrôler quand le bouton peut bouger

    void Start()
    {
        startTime = Time.time;
        startBatteryTime = Time.time;

        if (timeText == null)
        {
            timeText = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        }

        if (batteryText == null)
        {
            batteryText = GameObject.Find("BatteryText").GetComponent<TextMeshProUGUI>();
        }

        if (nightText == null)
        {
            nightText = GameObject.Find("NightText").GetComponent<TextMeshProUGUI>();
        }

        if (moneyText == null)
        {
            moneyText = GameObject.Find("MoneyText").GetComponent<TextMeshProUGUI>();
        }

        if (objectToDisplay != null)
        {
            objectToDisplay.SetActive(true);
            StartCoroutine(DisplayObjectFor3Seconds());
        }

        if (toggleButton != null)
            toggleButton.onClick.AddListener(OnToggleButtonClick);

        DisplayNightText();
        UpdateMoneyText(); // Mise à jour initiale de l'affichage de l'argent
        StartCoroutine(HandleMovingImage()); // Commence à gérer les apparitions de l'image

        // Assure-toi que movingImageButton est bien assigné dans l'éditeur Unity
        if (movingImageButton != null)
        {
            movingImageButton.onClick.AddListener(OnMovingImageClick); // Ajoute l'écouteur de clic
        }
    }

    void Update()
    {
        if (isWaitingAfterReset)
        {
            return;
        }

        if (Time.time - startTime >= timeUpdateInterval)
        {
            if (hourCounter < 6 && !resetHourInProgress)
            {
                hourCounter++;
            }

            if (hourCounter >= 6 && !resetHourInProgress)
            {
                resetHourInProgress = true;
                StartCoroutine(ResetHourAndBattery());
            }

            startTime = Time.time;
        }

        if (batteryResetInProgress)
        {
            resetTimer += Time.deltaTime;
            if (resetTimer >= 3f)
            {
                batteryResetInProgress = false;
                resetTimer = 0f;
            }
        }
        else
        {
            if (Time.time - startBatteryTime >= 3f)
            {
                DecrementBattery();
            }
        }

        UpdateTimeText();
        UpdateBatteryText();
    }

    void UpdateTimeText()
    {
        string formattedTime = hourCounter.ToString("00") + ":00";
        if (timeText != null)
        {
            timeText.text = formattedTime;
        }
    }

    void UpdateBatteryText()
    {
        if (batteryText != null)
        {
            batteryText.text = Mathf.Round(battery).ToString() + "%";
        }
    }

    void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + money.ToString();
        }
    }

    private IEnumerator ResetHourAndBattery()
    {
        yield return new WaitForSeconds(1f);
        hourCounter = 0f;
        resetHourInProgress = false;
        ResetBattery();
        AddMoney(); // Ajouter de l'argent à la fin de chaque nuit
        nightCounter++;
        AdjustBatteryDecrementTime(); // Ajuster la vitesse de décharge de la batterie
        maxAppearancesPerNight += 2; // Augmente le nombre maximum d'apparitions à chaque nuit
        DisplayNightText();
        StartCoroutine(DisplayObjectFor6Seconds());
        isWaitingAfterReset = true;

        yield return new WaitForSeconds(6f);

        isWaitingAfterReset = false;
    }

    void ResetBattery()
    {
        battery = 100f;
        batteryResetInProgress = true;
    }

    void DecrementBattery()
    {
        if (battery > 0)
        {
            battery -= (Time.deltaTime / batteryDecrementTime) * 100;
        }
        else
        {
            battery = 0;
        }
    }

    private IEnumerator DisplayObjectFor3Seconds()
    {
        yield return new WaitForSeconds(3f);
        if (objectToDisplay != null)
        {
            objectToDisplay.SetActive(false);
        }
    }

    private IEnumerator DisplayObjectFor6Seconds()
    {
        if (objectToDisplay != null)
        {
            objectToDisplay.SetActive(true);
            yield return new WaitForSeconds(6f);
            objectToDisplay.SetActive(false);
        }
    }

    void OnToggleButtonClick()
    {
        if (battery < 100f)
        {
            battery += batteryIncreaseAmount;
            if (battery > 100f)
            {
                battery = 100f;
            }
        }
    }

    void DisplayNightText()
    {
        if (nightText != null)
        {
            nightText.text = "Nuit " + nightCounter;
            nightText.gameObject.SetActive(true);
            StartCoroutine(HideNightTextAfterDelay());
        }
    }

    private IEnumerator HideNightTextAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (nightText != null)
        {
            nightText.gameObject.SetActive(false);
        }
    }

    void AdjustBatteryDecrementTime()
    {
        batteryDecrementTime = Mathf.Max(10f, 100f - (10f * (nightCounter - 1))); // Réduit de 10 secondes chaque nuit, minimum 10 secondes
    }

    void AddMoney()
    {
        int moneyEarned = 100 * (int)Mathf.Pow(2, nightCounter - 1); // Double les gains chaque nuit
        money += moneyEarned;
        UpdateMoneyText(); // Met à jour l'affichage de l'argent
    }

    private IEnumerator HandleMovingImage()
    {
        while (true)
        {
            // Vérifier si l'heure est 00h00, 01h00, 05h00 ou 06h00
            if (hourCounter == 0 || hourCounter == 1 || hourCounter == 5 || hourCounter == 6)
            {
                // Si l'heure est l'une des heures interdites, attend que l'heure change avant de permettre à l'objet d'apparaître
                while (hourCounter == 0 || hourCounter == 1 || hourCounter == 5 || hourCounter == 6)
                {
                    yield return null; // Attend jusqu'à ce que l'heure change
                }
            }

            // Le bouton apparaît
            if (movingImageButton != null)
            {
                movingImageButton.gameObject.SetActive(true); // Le bouton apparaît
                yield return new WaitForSeconds(5f); // Le bouton reste visible pendant 5 secondes
                movingImageButton.gameObject.SetActive(false); // Le bouton disparaît
            }

            // Attendre un temps aléatoire entre 5 et 20 secondes avant de réapparaître
            float waitTime = Random.Range(5f, 20f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    // Handler pour le clic sur le bouton
    void OnMovingImageClick()
    {
        clickCount++;

        // Si le bouton a été cliqué 20 fois et que le déplacement est autorisé, déplace-le de 300 unités vers la gauche
        if (clickCount >= maxClicksToMove && isMovingAllowed)
        {
            Vector3 currentPosition = movingImageButton.transform.position;
            currentPosition.x -= 300f; // Déplace le bouton à gauche de 300 unités
            movingImageButton.transform.position = currentPosition;

            clickCount = 0; // Réinitialise le compteur de clics après le déplacement

            // Empêche tout nouveau déplacement jusqu'à ce qu'il ait attendu 10 secondes
            isMovingAllowed = false;
            StartCoroutine(WaitForNextMove());
        }
    }

    // Coroutine pour attendre 10 secondes avant de permettre un nouveau déplacement
    private IEnumerator WaitForNextMove()
    {
        yield return new WaitForSeconds(10f); // Attente de 10 secondes

        // Permet à nouveau de déplacer l'image après le délai
        isMovingAllowed = true;
    }
}
