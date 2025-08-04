using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 控制相机的移动速度
    public float moveSpeed = 4f;
    // 控制相机的上升下降速度
    public float ascendSpeed = 2f;
    // 控制相机的旋转速度
    public float rotationSpeed = 2f;


    private void LateUpdate()
    {
        // 获取玩家输入
        float moveX = Input.GetAxis("Horizontal");  // A/D 控制左右
        float moveZ = Input.GetAxis("Vertical");    // W/S 控制前后
        float moveY = 0f;

        // Shift 键按下时，控制下移
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveY = -1f;  // 向下移动
        }
        // 空格键按下时，控制上升
        if (Input.GetKey(KeyCode.Space))
        {
            moveY = 1f;  // 向上移动
        }

        // 获取相机的局部方向
        Vector3 forward = transform.forward;  // 前方向
        forward.y = 0f;  // 防止上下方向影响
        forward.Normalize();  // 保证前方向是单位向量

        Vector3 right = transform.right;  // 右方向
        right.y = 0f;  // 防止上下方向影响
        right.Normalize();  // 保证右方向是单位向量

        // 相机的移动（基于相机当前的视角）
        Vector3 move = (forward * moveZ + right * moveX + Vector3.up * moveY) * moveSpeed * Time.deltaTime;
        //transform.Translate(move, Space.World);

        // 旋转相机的角度
        float rotateX = Input.GetAxis("Mouse X") * rotationSpeed;
        float rotateY = -Input.GetAxis("Mouse Y") * rotationSpeed;

        // 更新相机旋转（上下、左右）
        //transform.Rotate(0, rotateX, 0, Space.World);
        //Camera.main.transform.Rotate(rotateY, 0, 0);
    }
}
