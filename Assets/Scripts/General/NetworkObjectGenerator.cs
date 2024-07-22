using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI; // ��������� ������������ ���� ��� ������ � TextMeshPro

public class SyncObjectGenerator : MonoBehaviourPunCallbacks
{
    public GameObject[] objectPrefabs; // ������ �������� ��������, �� ������� �������� ����� ���������� ������
    public float spawnInterval = 3f; // �������� ����� ����������� ��������
    public float minScale = 0.5f; // ����������� ������� �������
    public float maxScale = 2.0f; // ������������ ������� �������
    public float minRotationY = 0f; // ����������� ���� �������� �� Y
    public float maxRotationY = 360f; // ������������ ���� �������� �� Y
    private float timer; // ������ ��� ������������ ����������
    private int objectsSpawnedCount; // ������� ��������������� ��������

    public Text countText; // ������ �� Text ��� ������ �������� �� UI

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
        if (PhotonNetwork.IsMasterClient) // ���������, �������� �� �� �������� �������
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
        // ��������� ������ �� ������� 
        int randomIndex = Random.Range(0, objectPrefabs.Length);

        // ��������� ������
        float scale = Random.Range(minScale, maxScale);

        // ��������� ���� �������� �� Y
        float rotationY = Random.Range(minRotationY, maxRotationY);

        // ������� ������� ����� GameObject (��� ���������� ������)
        Vector3 basePosition = transform.position;

        // ������������ ������ �� ���� ��������
        Vector3 spawnPosition = new Vector3(Random.Range(-10f, 10f), basePosition.y, Random.Range(-10f, 10f)); // ������ ��������� ���������� �� x � z, ������ �� y ����� ������ GameObject
        Quaternion spawnRotation = Quaternion.Euler(0f, rotationY, 0f); // ������������ �� Y �� ��������� ����

        // ��������� ������ �� ���� ��������
        GameObject spawnedObject = PhotonNetwork.Instantiate(objectPrefabs[randomIndex].name, spawnPosition, spawnRotation);

        // ���������� ���������� � �������� ������� ���� ��������
        photonView.RPC("SyncScale", RpcTarget.AllBuffered, spawnedObject.GetPhotonView().ViewID, scale);

        // ������� ��������������� ��������
        objectsSpawnedCount++;
        UpdateCountUI();
    }

    // RPC-����� ��� ������������� �������� �������
    [PunRPC]
    void SyncScale(int viewID, float scale)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        obj.transform.localScale = new Vector3(scale, scale, scale);
    }

    // ����� ��� ���������� ������ �������� �� UI
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
