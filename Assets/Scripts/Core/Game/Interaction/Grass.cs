using DG.Tweening;
using UnityEngine;

public class Grass : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeAngle = 10f;       // Góc nghiêng tối đa
    public float shakeDuration = 0.3f;   // Thời gian rung
    public float returnDuration = 0.4f;  // Thời gian trở lại
    public Ease easeType = Ease.OutQuad;

    private Quaternion defaultRotation;

    void Start()
    {
        defaultRotation = transform.localRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Shake(other.transform.position);
    }

    void Shake(Vector3 playerPos)
    {
        // Xác định hướng va chạm -> nếu player ở trái thì nghiêng ngược lại
        float dir = (playerPos.x < transform.position.x) ? -1f : 1f;

        // Hủy tween cũ nếu đang chạy
        transform.DOKill();

        // Xoay quanh trục Z
        transform.DOLocalRotate(
            new Vector3(
                transform.localEulerAngles.x,
                transform.localEulerAngles.y,
                shakeAngle * dir
            ),
            shakeDuration
        )
        .SetEase(easeType)
        .OnComplete(() =>
        {
            // Quay về trạng thái ban đầu
            transform.DOLocalRotateQuaternion(defaultRotation, returnDuration)
                     .SetEase(Ease.OutBack);
        });
    }
}

