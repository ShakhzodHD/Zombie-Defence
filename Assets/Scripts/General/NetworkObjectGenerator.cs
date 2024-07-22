using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI; // Добавляем пространство имен для работы с TextMeshPro

public class SyncObjectGenerator : MonoBehaviourPunCallbacks
{
    public GameObject[] objectPrefabs; // Массив префабов объектов, из которых случайно будет выбираться объект
    public float spawnInterval = 3f; // Интервал между появлениями объектов
    public float minScale = 0.5f; // Минимальный масштаб объекта
    public float maxScale = 2.0f; // Максимальный масштаб объекта
    public float minRotationY = 0f; // Минимальный угол поворота по Y
    public float maxRotationY = 360f; // Максимальный угол поворота по Y
    private float timer; // Таймер для отслеживания интервалов
    private int objectsSpawnedCount; // Счетчик сгенерированных объектов

    public Text countText; // Ссылка на Text для вывода счетчика на UI

    private bool isReady = false;

    void Start()
    {
        timer = spawnInterval;
        objectsSpawnedCount = 0;
        UpdateCountUI();
    }

    void Update()
    {
        if (!isReady) return;
        if (PhotonNetwork.IsMasterClient) // Проверяем, являемся ли мы хозяином комнаты
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                SpawnObject();
                timer = spawnInterval;
            }
        }
    }

    void SpawnObject()
    {
        // случайный прфеаб из массива 
        int randomIndex = Random.Range(0, objectPrefabs.Length);

        // случайный размер
        float scale = Random.Range(minScale, maxScale);

        // случайный угол поворота по Y
        float rotationY = Random.Range(minRotationY, maxRotationY);

        // текущая позиция этого GameObject (где прикреплен скрипт)
        Vector3 basePosition = transform.position;

        // Генерируется объект на всех клиентах
        Vector3 spawnPosition = new Vector3(Random.Range(-10f, 10f), basePosition.y, Random.Range(-10f, 10f)); // Только случайные координаты по x и z, высота по y равна высоте GameObject
        Quaternion spawnRotation = Quaternion.Euler(0f, rotationY, 0f); // Поворачиваем по Y на случайный угол

        // Создается объект на всех клиентах
        GameObject spawnedObject = PhotonNetwork.Instantiate(objectPrefabs[randomIndex].name, spawnPosition, spawnRotation);

        // Отправляем информацию о масштабе объекта всем клиентам
        photonView.RPC("SyncScale", RpcTarget.AllBuffered, spawnedObject.GetPhotonView().ViewID, scale);

        // счетчик сгенерированных объектов
        objectsSpawnedCount++;
        UpdateCountUI();
    }

    // RPC-метод для синхронизации масштаба объекта
    [PunRPC]
    void SyncScale(int viewID, float scale)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        obj.transform.localScale = new Vector3(scale, scale, scale);
    }

    // Метод для обновления текста счетчика на UI
    void UpdateCountUI()
    {
        if (countText != null)
        {
            countText.text = "Objects Spawned: " + objectsSpawnedCount.ToString();
        }
    }
    public void StartGenerate()
    {
        isReady = true;
    }
}
