using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor;

public class LevelCreatorScript : MonoBehaviour
{
    // Şişe listesi, iç içe listelerden oluşan ve int değerlerini tutan bir liste.
    public static List<List<int>> bottles = new List<List<int>>();
    

    // Seçilen rastgele indekslerle prefabları eşleştir
    public static List<GameObject> sheeps = new List<GameObject>();

    // Renk dizisi, Color türünden elemanları bulunan bir dizi.
    public static Color[] ColorsList;

    // Seviye renk sayısı ve şişe sayısını tutan private tam sayı değişkenleri.
    public static int levelColorCount;
    public static int levelBottleCount;
    public static int CurGameLevel;

    [SerializeField] private Color[] liquidColors; // Renk dizisi, Unity editöründe görüntülenebilir.
    [SerializeField] public GameObject[] sheepPrefabs; // Renklerle eşleşen koyun prefabları

    // Oyun nesnelerini Unity editöründe görüntülenebilir yapmak için SerializeField kullanılmıştır.
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] public GameObject bottleHolder;
    [SerializeField] private GameObject sheepHolder;
    [SerializeField] private GameObject sheepsBottle;
    [SerializeField] private TextMeshPro levelText;

    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Canvas levelCanvas;

    public static LevelCreatorScript instance;

    // Awake metodu, Unity'nin özel bir metodu olup oyun nesneleri oluşturulduktan hemen sonra çağrılır.
    private void Awake()
    {
        instance = this;
        // liquidColors dizisi, ColorsList dizisine atanır.
        ColorsList = liquidColors;
        
    }


    public void CreateTutorialLevel()
    {
        CurGameLevel = 1;
        menuCanvas.gameObject.SetActive(false);

        // Önceki seviyenin elemanları temizlenir.
        levelText.text = "Level " + CurGameLevel;
        levelBottleCount = 3;
       //  levelColorCount = 2;

        // Sıvılar, şişeler ve koyunlar üretilir.
        StartCoroutine(GenerateObjectsAfterRemoval());
    }

    public void CheckLevel()
    {
        int childBottleCount = GameObject.Find("BottleHolder").transform.childCount;
        Debug.Log("Kalan şişe sayısı: " + childBottleCount);

        if (childBottleCount == 1)
        {
            levelCanvas.gameObject.SetActive(true);
        }
    }

    // Yeni bir seviye oluşturmak için kullanılan metot.
    public void CreateNewLevel()
    {
        RemoveOldLevel();

        levelCanvas.gameObject.SetActive(false);

        CurGameLevel++;

        // Level Kontrolleri
        if(CurGameLevel == 3)
        {
            levelBottleCount = 5;
        }
        // 3.levele geçtiğinde 5 şişe
        else if(CurGameLevel == 5)
        {
            levelBottleCount = 7;
        }
        // 5.levele geçtiğinde 7 şişe
        else if (CurGameLevel == 10)
        {
            levelBottleCount = 9;
        }
        // 10.levele geçtiğinde 9 şişe
        else if (CurGameLevel == 13)
        {
            levelBottleCount = 7;
        }
        // 13.levele geçtiğinde 7 şişe
        else if (CurGameLevel==15)
        {
            levelBottleCount = 11;
        }
        // 15.levele geçtiğinde 11 şişe
        else if (CurGameLevel==18)
        {
            levelBottleCount = 15;
        }
        // 18.levele geçtiğinde 5 şişe
        else if (CurGameLevel == 19)
        {
            levelBottleCount = 13;
        }
        // 19.levele geçtiğinde 13 şişe


        // Seviye numarası ekranda gösterilir.
        levelText.text = "Level " + CurGameLevel;

        // Coroutine kullanarak yeni objeleri oluşturma işlemlerini sırayla gerçekleştirme
        StartCoroutine(GenerateObjectsAfterRemoval());
    }

    // Önceki seviyeye ait nesnelerin temizlenmesi için kullanılan metot.
    public void RemoveOldLevel()
    {
        sheeps.Clear();

        // sheepHolder içindeki tüm çocuk nesneler silinir.
        foreach (Transform child in sheepHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // bottleHolder içindeki tüm çocuk nesneler silinir.
        foreach (Transform child in bottleHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // bottleHolder içindeki tüm çocuk nesneler silinir.
        foreach (Transform child in sheepsBottle.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

    }

    private IEnumerator GenerateObjectsAfterRemoval()
    {
        yield return new WaitForEndOfFrame(); // Bir sonraki frame'in sonunu bekleyelim

        // Şişeleri ve koyunları yeniden oluşturmak için metotlar burada çağrılabilir
        CreateLiquids();
        GenerateBottles();
        GenerateSheeps();
    }

    // Sıvıların oluşturulması için kullanılan metot.
    public void CreateLiquids()
    {
        // Rastgele renk indeksleri seçilir ve belirli işlemlerle şişelere ve koyunlara atanır.
        int [] selectedColors = GameUtils.SelectRandomNumbers(0, liquidColors.Length - 1, levelBottleCount -1, false);

        foreach (int colorIndex in selectedColors)
        {
            GameObject sheepPrefab = sheepPrefabs[colorIndex];
            sheeps.Add(sheepPrefab);
            Debug.Log(string.Join(", ", colorIndex));
        }

        selectedColors = GameUtils.RepeatElements(selectedColors, 4);
        selectedColors = GameUtils.ShuffleArray(selectedColors);

        // Renk indeksleri, belirli sayıda alt listelere bölünerek bottles listesine eklenir.
        bottles = GameUtils.DivideArray(selectedColors, 4);
        int num = GameUtils.SelectRandom(0, bottles.Count);
        bottles.Insert(num, new List<int>());    
    }

    // Şişelerin oluşturulması için kullanılan metot.
    private void GenerateBottles()
    {
        float yOffset = 2.0f; // Y ekseni üzerindeki başlangıç boşluğu
        float rowSpacing = 0.3f; // Satırlar arasındaki boşluk
        int bottlesPerGroup = 5; // Her grup içindeki şişe sayısı

        int groupCount = (levelBottleCount - 1) / bottlesPerGroup + 1;

        for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
        {
            int bottlesInThisGroup = Mathf.Min(bottlesPerGroup, levelBottleCount - groupIndex * bottlesPerGroup);

            float groupWidth = bottlesInThisGroup - 1; // Grup genişliği
            float groupXOffset = groupWidth * 0.5f; // Grup ortalamak için kullanılacak offset

            for (int i = 0; i < bottlesInThisGroup; i++)
            {
                int bottleIndex = groupIndex * bottlesPerGroup + i;

                // Şişe konumları belirlenerek oluşturulur.
                float xPos = i - groupXOffset;
                float yPos = yOffset - groupIndex * (2.0f + rowSpacing);

                GameObject bottle = Instantiate(bottlePrefab, bottleHolder.transform);
                bottle.transform.position = new Vector3(xPos, yPos);
                BottleScript bottleScript = bottle.GetComponent<BottleScript>();
                bottleScript.liquids = bottles[bottleIndex];
                bottleScript.bottleID = bottleIndex;
            }
        }
    }




    // Koyunlarýn oluþturulmasý için kullanýlan metot.
    private void GenerateSheeps()
    {
        int fullRankCount = (levelBottleCount - 1) / 4;
        float gapBetweenSheep = 1.5f;

        for (int j = 0; j < fullRankCount; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                float yPos = -3.5f - j * gapBetweenSheep;
                float xPos = -1.5f + i;
                int sheepIndex = i + 4 * j;

                if (sheepIndex < sheeps.Count)
                {
                    GameObject sheep = Instantiate(sheeps[sheepIndex], Vector3.zero, Quaternion.identity);
                    sheep.transform.SetParent(sheepHolder.transform);
                    sheep.transform.localPosition = new Vector3(xPos, yPos, 0f);
                }
            }
        }

        for (int i = 0; i < (sheeps.Count % 4); i++)
        {
            float yPos = -3.5f - fullRankCount * gapBetweenSheep;
            float xPos = (sheeps.Count % 4 - 1) * -0.5f + i;
            int sheepIndex = i + 4 * fullRankCount;

            if (sheepIndex < sheeps.Count)
            {
                GameObject sheep = Instantiate(sheeps[sheepIndex], Vector3.zero, Quaternion.identity);
                sheep.transform.SetParent(sheepHolder.transform);
                sheep.transform.localPosition = new Vector3(xPos, yPos, 0f);
            }
        }
    }

}


