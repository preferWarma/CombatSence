using System.Collections.Generic;
using UnityEngine;

namespace Shadow
{
    public class ShadowPool : MonoBehaviour
    {
        public static ShadowPool Instance { get; private set; }

        [Tooltip("残影预制体")] public GameObject shadowPrefab;
        [Tooltip("残影数量")] public int shadowCount = 10;
        
        private readonly Queue<GameObject> shadowQueue = new();  // 残影队列(对象池)

        private void Awake()
        {
            Instance = this;
            // 初始化
            FillPool();
        }

        private void FillPool() // 初始化对象池
        {
            for (var i = 1; i <= shadowCount; i++)
            {
                var shadow = Instantiate(shadowPrefab, transform);
                Recycle(shadow);
            }
        }
        
        // 从对象池中获取残影
        public GameObject Allocate()
        {
            if (shadowQueue.Count == 0)
            {
                FillPool();
            }
            var res = shadowQueue.Dequeue();
            res.SetActive(true);
            return res;
        }
        
        // 将残影放回对象池
        public void Recycle(GameObject shadow)
        {
            shadow.SetActive(false);
            shadowQueue.Enqueue(shadow);    // 入队
        }
    }
}
