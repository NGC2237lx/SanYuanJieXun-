using UnityEngine;

public class FireMoney : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("旋转速度 (度/秒)")]
    public float rotationSpeed = 180f;
    
    [Tooltip("是否随机旋转速度")]
    public bool randomizeSpeed = true;
    
    [Tooltip("最小随机旋转速度")]
    public float minRandomSpeed = 90f;
    
    [Tooltip("最大随机旋转速度")]
    public float maxRandomSpeed = 270f;
    
    [Header("浮动效果")]
    [Tooltip("是否启用上下浮动效果")]
    public bool enableFloatEffect = true;
    
    [Tooltip("浮动幅度")]
    public float floatAmplitude = 0.5f;
    
    [Tooltip("浮动频率")]
    public float floatFrequency = 1f;
    
    [Header("缩放效果")]
    [Tooltip("是否启用缩放效果")]
    public bool enableScaleEffect = true;
    
    [Tooltip("初始缩放 (0-1)")]
    public float initialScale = 0.1f;
    
    [Tooltip("缩放速度")]
    public float scaleSpeed = 2f;
    
    [Tooltip("最大缩放")]
    public float maxScale = 1f;
    
    private Vector3 originalPosition;
    private float randomOffset;
    private bool isScalingUp = true;
    private Transform childTransform; // 用于旋转的子物体

    private void Start()
    {
        // 初始化随机旋转速度
        if (randomizeSpeed)
        {
            rotationSpeed = Random.Range(minRandomSpeed, maxRandomSpeed);
        }
        
        // 随机浮动偏移量
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
        
        // 记录原始位置
        originalPosition = transform.position;
        
        // 如果有子物体，旋转子物体而不是父物体
        if (transform.childCount > 0)
        {
            childTransform = transform.GetChild(0);
        }
        else
        {
            childTransform = transform;
        }
        
        // 初始化缩放
        if (enableScaleEffect)
        {
            childTransform.localScale = Vector3.one * initialScale;
        }
    }

    private void Update()
    {
        // 旋转效果
        RotateObject();
        
        // 浮动效果
        if (enableFloatEffect)
        {
            FloatEffect();
        }
        
        // 缩放效果
        if (enableScaleEffect)
        {
            ScaleEffect();
        }
    }

    private void RotateObject()
    {
        // 绕Y轴旋转
        childTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void FloatEffect()
    {
        // 使用正弦函数创建上下浮动效果
        float newY = originalPosition.y + Mathf.Sin((Time.time + randomOffset) * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void ScaleEffect()
    {
        // 获取当前缩放
        Vector3 currentScale = childTransform.localScale;
        
        // 根据方向缩放
        if (isScalingUp)
        {
            currentScale += Vector3.one * scaleSpeed * Time.deltaTime;
            if (currentScale.x >= maxScale)
            {
                currentScale = Vector3.one * maxScale;
                isScalingUp = false;
            }
        }
        else
        {
            currentScale -= Vector3.one * scaleSpeed * Time.deltaTime;
            if (currentScale.x <= initialScale)
            {
                currentScale = Vector3.one * initialScale;
                isScalingUp = true;
            }
        }
        
        childTransform.localScale = currentScale;
    }
}