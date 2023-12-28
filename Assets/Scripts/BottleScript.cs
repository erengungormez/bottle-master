using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Rendering.FilterWindow;

public class BottleScript : MonoBehaviour
{
    public List<int> liquids = new();
    public int bottleID;
    int i;
    public static bool isSelectionInProgress = false; // Seçim süreci devam ediyor mu?

    private Transform sheepHolder;
    private Transform bottleHolder;
    private List<GameObject> sheepsInHolder = new List<GameObject>();
    private List<GameObject> bottlesInHolder = new List<GameObject>();

    [SerializeField] private Transform liquidSprite;
    [SerializeField] private AnimationCurve RotateMultiplerCurve;
    [SerializeField] private AnimationCurve FillAmountCurve;

    private Material _liquidMat;
    private bool isSelected;
    private int curFillAmount = 0;
    private int animFrame = 0;
    private Vector2 startPos;
    private BottleScript targetBottle;
    private int liquidTransferCount = 0;

    private bool isMoving = false; // Hareket kontrolü
    private bool sheepMoved = false;
    private Vector3 targetPosition; // Koyunun hedef konumu
    private float moveSpeed = 5.0f; // Hareket hızı

    private void Start()
    {  

    _liquidMat = liquidSprite.GetComponent<SpriteRenderer>().material;
        startPos = transform.position;
        BottleCreateLiquids();

        // Sahnedeki SheepHolder objesini alıyoruz
        sheepHolder = GameObject.Find("SheepHolder").transform;

        // SheepHolder altındaki tüm child objeleri bir listeye ekliyoruz
        foreach (Transform child in sheepHolder)
        {
            sheepsInHolder.Add(child.gameObject);
        }


    }

    private void Update()
    {
        // Şişenin doluluğunu kontrol et
        if (IsFull() && IsSameColor() && !sheepMoved && transform.parent.name == "BottleHolder")
        {
            int selectedBottleColorIndex = GetColorIndex(i); // Seçilen şişenin renk indeksi
            Debug.Log("Seçilen şişe doldu ve rengi: " + selectedBottleColorIndex);
            MoveBottleToSheep(selectedBottleColorIndex);
        }
    }

    private void BottleCreateLiquids()
    {
        RecalculateLiquidColors();
        float fillVal = Mathf.Lerp(-0.9f, 0.4f, liquids.Count * 0.25f);
        _liquidMat.SetFloat("_FillAmount", fillVal);
        curFillAmount = liquids.Count;
    }

    private void RecalculateLiquidColors()
    {
        for (int i = 0; i < liquids.Count; i++)
        {
            _liquidMat.SetColor("_c" + (i + 1), LevelCreatorScript.ColorsList[liquids[i]]);
        }
    }

    public void BottleClicked()
    {
        if (isSelectionInProgress)
        {
            return;
        }

        if (isSelected)
        {
            isSelected = false;
            isSelectionInProgress = true;
            InvokeRepeating(nameof(StopSelection), 0f, 0.01f);
        }
        else
        {
            isSelected = true;
            isSelectionInProgress = true;
            InvokeRepeating(nameof(BottleSelected), 0f, 0.01f);
        }
    }

    private bool IsFull()
    {
        return liquids.Count == 4;
    }

    private bool IsSameColor()
    {
        if (liquids.Count == 0)
        {
            return false;
        }

        int firstColor = liquids[0];

        for (int i = 1; i < liquids.Count; i++)
        {
            if (liquids[i] != firstColor)
            {
                return false;
            }
        }

        return true;
    }

    public int GetColorIndex(int i)
    {
        if (i >= 0 && i < liquids.Count)
        {
            // İstenen indeksteki renk indeksini döndür
            return liquids[i];
        }

        // Geçersiz bir indeks istendiğinde -1 döndür
        return -1;
    }

    public void MoveBottleToSheep(int selectedBottleColorIndex)
    {
            foreach (GameObject obj in sheepsInHolder)
            {
                int tagNumber;
                if (int.TryParse(obj.tag, out tagNumber)) // GameObject'in tag'ini integer'a dönüştür
                {
                    if (selectedBottleColorIndex == tagNumber) // Liste elemanı ile tag sayısı eşleşirse
                    {
                        Debug.Log("Hareket edilecek koyun: " + tagNumber);
                        Debug.Log("Şişe hareket ediyor...");

                        // Koyunun konumunu al
                        Vector3 sheepPosition = obj.transform.position;

                        // Şişenin koyunun pozisyonuna doğru hareket etmesi
                        StartCoroutine(MoveToSheep(sheepPosition, obj));

                        break;
                    }
                }
            }
    }

    private IEnumerator MoveToSheep(Vector3 targetPosition, GameObject sheep)
    {
        sheepMoved = true;

        float waitTimeBeforeMove = 3f; // Hareket etmeden önce bekleme süresi
        float duration = 1.5f; // Hareketin süresi
        float checkLevelDelay = 2f; // Hareketin tamamlanmasından sonra beklenen süre

        // Bekleme süresi
        yield return new WaitForSeconds(waitTimeBeforeMove);

        // "SheepsBottle" nesnesini Find işlemi ile bulma
        GameObject sheepsBottleObject = GameObject.Find("SheepsBottle");

        if (sheepsBottleObject != null)
        {
            // Şişe objesini "SheepsBottle" nesnesinin altına taşıma işlemi
            transform.parent = sheepsBottleObject.transform;

            Debug.Log("Şişe taşındı.");


            if (sheep != null)
            {
                // Hareketin süresince Şişe objesini doğrudan Sheep nesnesine taşıma işlemi
                float elapsedTime = 0.0f;
                Vector3 startingPos = transform.position;

                while (elapsedTime < duration)
                {
                    transform.position = Vector3.Lerp(startingPos, targetPosition, (elapsedTime / duration));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Particle System'i bul
                ParticleSystem sheepParticleSystem = sheep.GetComponentInChildren<ParticleSystem>();

                if (sheepParticleSystem != null)
                {
                    // Particle System'i aktifleştir
                    sheepParticleSystem.Play();

                    yield return new WaitForSeconds(duration); // Belirli bir süre bekle

                    // Particle System'i tekrar durdur
                    sheepParticleSystem.Stop();
                    LevelCreatorScript.instance.CheckLevel(); // Şişe sayısını kontrol et;
                }
                else
                {
                    Debug.LogError("Sheep objesinde Particle System bulunamadı.");
                }

                // Şişe objesini Sheep nesnesinin altından çıkarma
                transform.parent = GameObject.Find("SheepsBottle").transform;
            }
            else
            {
                Debug.LogError("Sheep nesnesi bulunamadı.");
            }
        }
        else
        {
            Debug.LogError("SheepsBottle nesnesi bulunamadı.");
        }

        // Bekleme süresi
        yield return new WaitForSeconds(checkLevelDelay);

        sheepMoved = false;
        
    }

    private void BottleSelected()
    {
        if (animFrame < 20)
        {
            transform.position += new Vector3(0, 0.02f, 0);
            animFrame++;
        }
        else
        {
            CancelInvoke();
            animFrame = 0;
            isSelectionInProgress = false; // Süreç devam etmiyor
        }
    }

    private void StopSelection()
    {
        if (animFrame < 20)
        {
            transform.position -= new Vector3(0, 0.02f, 0);
            animFrame++;
        }
        else
        {
            CancelInvoke();
            animFrame = 0;
            isSelectionInProgress = false; // Süreç devam etmiyor
        }
    }

    public void LiquidTransfer(BottleScript bottle, List<int> liqToTransfer)
    {

        targetBottle = bottle;
        if (liqToTransfer.Count == 0)
        {
            isSelected = false;
            isSelectionInProgress = true;
            InvokeRepeating(nameof(StopSelection), 0f, 0.01f);
        }
        
        else
        {
            liquidTransferCount = liqToTransfer.Count;
            liquids.RemoveRange(liquids.Count - liquidTransferCount, liquidTransferCount);
            LevelCreatorScript.bottles[bottleID] = liquids;
            targetBottle.liquids.AddRange(liqToTransfer);
            LevelCreatorScript.bottles[targetBottle.bottleID] = targetBottle.liquids;
            isSelected = true;
            isSelectionInProgress = true;
            InvokeRepeating(nameof(AnimateLiquidTransfer), 0f, 0.01f);
        }
    }

    private Vector3 CalculateTargetPos(Vector3 targetPosition)
    {
        if ((targetPosition.x - transform.position.x) > 0)
        {
            return targetPosition + new Vector3(-0.85f, 1.6f, 0f);
        }
        else
        {
            return targetPosition + new Vector3(0.85f, 1.6f, 0f);
        }
    }

    private void AnimateLiquidTransfer()
    {
        Vector3 targetPos = CalculateTargetPos(targetBottle.transform.position);
        int rotMultiplier = (targetBottle.transform.position.x - transform.position.x > 0) ? -1 : 1;
        int targetAngle = TargetAngleCalculator(4 - liquids.Count);

        // Animate Moving To Spill Position
        if (animFrame < 30)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, animFrame / 29f);
            animFrame++;
        }
        // Animate Rotation Until Spill Starts
        else if (animFrame < 90) 
        {
            float curFrame = (animFrame - 29f) / 60f;
            float rotIncrement = Mathf.Lerp(0, TargetAngleCalculator(4 - (liquids.Count + liquidTransferCount)), curFrame);
            transform.eulerAngles = new Vector3(0, 0, rotIncrement) * rotMultiplier;
            float radAngle = transform.eulerAngles.z * Mathf.Deg2Rad;
            _liquidMat.SetFloat("_RotationMultiplier", RotateMultiplerCurve.Evaluate(radAngle));
            animFrame++;
        }
        // Animate Spilling Process
        else if (animFrame < 90 + liquidTransferCount * 60)
        {
            float curFrame = (animFrame - 89f) / (liquidTransferCount * 60f);
            float rotIncrement = Mathf.Lerp(TargetAngleCalculator(4 - (liquids.Count + liquidTransferCount)), targetAngle, curFrame);
            transform.eulerAngles = new Vector3(0, 0, rotIncrement) * rotMultiplier;
            float radAngle = transform.eulerAngles.z * Mathf.Deg2Rad;
            _liquidMat.SetFloat("_RotationMultiplier", RotateMultiplerCurve.Evaluate(radAngle));
            _liquidMat.SetFloat("_FillAmount", FillAmountCurve.Evaluate(radAngle));
            float targetFillAmount = -0.9f + 0.325f * targetBottle.liquids.Count;
            float targetFillStartAmount = -0.9f + 0.325f * (targetBottle.liquids.Count - liquidTransferCount);
            float targetFill = Mathf.Lerp(targetFillStartAmount , targetFillAmount, curFrame);
            targetBottle._liquidMat.SetFloat("_FillAmount", targetFill);
            targetBottle.RecalculateLiquidColors();
            animFrame++;
        }

        else if (animFrame < 150 + liquidTransferCount * 90)
        {
            float curFrame = (animFrame - (29f + liquidTransferCount * 90)) / 120;
            float rotIncrement = Mathf.Lerp(targetAngle, 0, curFrame);
            transform.eulerAngles = new Vector3(0, 0, rotIncrement) * rotMultiplier;
            transform.position = Vector3.Lerp(targetPos, startPos, curFrame);
            float radAngle = transform.eulerAngles.z * Mathf.Deg2Rad;
            _liquidMat.SetFloat("_RotationMultiplier", RotateMultiplerCurve.Evaluate(radAngle));
            animFrame++;
        }        
        else
        {                     
            isSelected = false;
            isSelectionInProgress = false;
            CancelInvoke();
            animFrame = 0;
        }
    }

    private int TargetAngleCalculator(int liquidCount)
    {
        return liquidCount switch
        {
            0 => 70,
            1 => 80,
            2 => 85,
            3 => 92,
            _ => 105
        };
    }
}