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
    public Button movingImageButton; // Le bouton qui repr�sentera l'image
    private bool isObjectDisplayed = false;

    public float batteryIncreaseAmount = 10f;
    private int nightCounter = 1; // Compteur pour suivre la nuit actuelle
    private int money = 0; // Montant total d'argent accumul�

    private int maxAppearancesPerNight = 5; // Nombre maximum d'apparitions pour la premi�re nuit

    private int clickCount = 0; // Nombre de clics sur l'image
    private const int maxClicksToMove = 20; // Nombre de clics requis pour d�placer l'image

    public float moveAmount = 10f; // Valeur du d�placement du bouton (modifiable dans l'Inspector)

    private bool isMovingAllowed = true; // Permet de contr�ler quand le bouton peut bouger

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
        UpdateMoneyText(); // Mise � jour initiale de l'affichage de l'argent
        StartCoroutine(HandleMovingImage()); // Commence � g�rer les apparitions de l'image

        // Assure-toi que movingImageButton est bien assign� dans l'�diteur Unity
        if (movingImageButton != null)
        {
            movingImageButton.onClick.AddListener(OnMovingImageClick); // Ajoute l'�couteur de clic
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
        AddMoney(); // Ajouter de l'argent � la fin de chaque nuit
        nightCounter++;
        AdjustBatteryDecrementTime(); // Ajuster la vitesse de d�charge de la batterie
        maxAppearancesPerNight += 2; // Augmente le nombre maximum d'apparitions � chaque nuit
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
        batteryDecrementTime = Mathf.Max(10f, 100f - (10f * (nightCounter - 1))); // R�duit de 10 secondes chaque nuit, minimum 10 secondes
    }

    void AddMoney()
    {
        int moneyEarned = 100 * (int)Mathf.Pow(2, nightCounter - 1); // Double les gains chaque nuit
        money += moneyEarned;
        UpdateMoneyText(); // Met � jour l'affichage de l'argent
    }

    private IEnumerator HandleMovingImage()
    {
        while (true)
        {
            // V�rifier si l'heure est 00h00, 01h00, 05h00 ou 06h00
            if (hourCounter == 0 || hourCounter == 1 || hourCounter == 5 || hourCounter == 6)
            {
                // Si l'heure est l'une des heures interdites, attend que l'heure change avant de permettre � l'objet d'appara�tre
                while (hourCounter == 0 || hourCounter == 1 || hourCounter == 5 || hourCounter == 6)
                {
                    yield return null; // Attend jusqu'� ce que l'heure change
                }
            }

            // Le bouton appara�t
            if (movingImageButton != null)
            {
                movingImageButton.gameObject.SetActive(true); // Le bouton appara�t
                yield return new WaitForSeconds(5f); // Le bouton reste visible pendant 5 secondes
                movingImageButton.gameObject.SetActive(false); // Le bouton dispara�t
            }

            // Attendre un temps al�atoire entre 5 et 20 secondes avant de r�appara�tre
            float waitTime = Random.Range(5f, 20f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    // Handler pour le clic sur le bouton
    void OnMovingImageClick()
    {
        clickCount++;

        // Si le bouton a �t� cliqu� 20 fois et que le d�placement est autoris�, d�place-le de 300 unit�s vers la gauche
        if (clickCount >= maxClicksToMove && isMovingAllowed)
        {
            Vector3 currentPosition = movingImageButton.transform.position;
            currentPosition.x -= 300f; // D�place le bouton � gauche de 300 unit�s
            movingImageButton.transform.position = currentPosition;

            clickCount = 0; // R�initialise le compteur de clics apr�s le d�placement

            // Emp�che tout nouveau d�placement jusqu'� ce qu'il ait attendu 10 secondes
            isMovingAllowed = false;
            StartCoroutine(WaitForNextMove());
        }
    }

    // Coroutine pour attendre 10 secondes avant de permettre un nouveau d�placement
    private IEnumerator WaitForNextMove()
    {
        yield return new WaitForSeconds(10f); // Attente de 10 secondes

        // Permet � nouveau de d�placer l'image apr�s le d�lai
        isMovingAllowed = true;
    }
}
