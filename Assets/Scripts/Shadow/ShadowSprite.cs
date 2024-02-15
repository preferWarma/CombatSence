using UnityEngine;
// ReSharper disable Unity.InefficientPropertyAccess

namespace Shadow
{
    // 挂载在残影游戏对象上
    public class ShadowSprite : MonoBehaviour
    {
        [Header("残影相关设置")]
        [Tooltip("残影持续时间")] public float showTime = 0.5f;
        [Tooltip("开始显示残影的时间")] public float showTimeStart = 0.1f;
        [Tooltip("不透明度初始值")][Range(0, 1)] public float initialAlpha = 1f;
        [Tooltip("不透明度减少倍数")] public float alphaDecreaseMultiplier = 0.6f;
        [Tooltip("影子的颜色")] public Color shadowColor;
        
        private Transform player;   // 玩家
        private SpriteRenderer thisSpriteRenderer;    // 当前图像
        private SpriteRenderer playerSpriteRenderer;    // 玩家图像
        private Color color;    // 颜色
        private float alpha;  // 不透明度
        
        private void OnEnable()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            thisSpriteRenderer = GetComponent<SpriteRenderer>();
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            
            alpha = initialAlpha;
            thisSpriteRenderer.sprite = playerSpriteRenderer.sprite;    // 设置残影图像
            thisSpriteRenderer.flipX = playerSpriteRenderer.flipX;    // 设置残影的翻转
            thisSpriteRenderer.flipY = playerSpriteRenderer.flipY;    // 设置残影的翻转
            transform.position = player.position;   // 设置残影位置
            transform.localScale = player.localScale;   // 设置残影大小
            transform.rotation = player.rotation;   // 设置残影旋转
            showTimeStart = Time.time;  // 设置开始显示残影的时间
        }

        private void FixedUpdate()
        {
            alpha *= alphaDecreaseMultiplier;   // 不透明度减少
            color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, alpha);
            thisSpriteRenderer.color = color;   // 设置颜色
            if (showTime + showTimeStart < Time.time)    // 判断是否超过持续时间
            {
                // 放回对象池
                ShadowPool.Instance.Recycle(gameObject);
            }
        }
    }
}
