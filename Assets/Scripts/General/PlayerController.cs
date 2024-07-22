using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Animator animator;
    private Rigidbody rb;
    private bool isGrounded;
    private bool isRunning;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (photonView.IsMine)
        {
            rb.isKinematic = false;
            // Назначаем слой "PlayerColliders" для всех коллайдеров игрока
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("PlayerColliders"));
        }
        else
        {
            rb.isKinematic = true;
            // Назначаем слой "Default" для всех коллайдеров других игроков
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            Movement();
        }
    }
    private void Movement()
    {
        // Проверка на земле
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundLayer);

        // Управление перемещением
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Поворот игрока
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Установка параметров анимации
        isRunning = moveDirection.magnitude > 0.1f;
        animator.SetBool("IsRunning", isRunning);

        // Вычисление скорости движения
        Vector3 moveVelocity = moveDirection * moveSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        //rb.MovePosition(transform.position + moveVelocity * Time.deltaTime);

        // Прыжок при нажатии кнопки Space
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            photonView.RPC("RPC_Jump", RpcTarget.All);
        }
    }
    [PunRPC]
    private void RPC_Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.velocity);
            stream.SendNext(isRunning);
        }
        else
        {
            rb.position = (Vector3)stream.ReceiveNext();
            rb.velocity = (Vector3)stream.ReceiveNext();
            isRunning = (bool)stream.ReceiveNext();
            animator.SetBool("IsRunning", isRunning);
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
