using UnityEngine;


public class Enemy : MonoBehaviour {
    public GameObject bulletPrefab = null; // 弾丸のPrefab
    public float shotSpan = 0.1f;  // 射撃間隔（sec.）
    public float shotSpeed = 400f; // 射撃速度（pix./sec.）

    private float actTime = 0f;    // 稼働時間（sec.）
    private CollisionPart[] col = null;

    void Start() {
        // コリジョンの呼び出し
        this.col = this.GetComponentsInChildren<CollisionPart>(true);
        for (int i = 0; i < this.col.Length; ++i) {
            this.col[i].Initialize(COL_CATEGORY.ENEMY, HitCallback);
            this.col[i].WakeUp();
        }
    }

    void OnDestroy() {
        // コリジョンの返却
        for (int i = 0; i < this.col.Length; ++i)
            this.col[i].Sleep();
    }

    void Update() {
        float elapsedTime = Time.deltaTime;
        for (int i = 0; i < this.col.Length; ++i)
            this.col[i].Run(elapsedTime);

        this.actTime += elapsedTime;
        if (this.actTime >= this.shotSpan) {
            GameObject go = Object.Instantiate<GameObject>(this.bulletPrefab);
            Vector3 position = this.transform.position;
            position = Camera.main.WorldToScreenPoint(position);
            position.x -= (float)Screen.width * 0.5f;
            position.y -= (float)Screen.height * 0.5f;
            position.z = 0f;
            go.transform.position = position;
            Bullet bl = go.GetComponent<Bullet>();
            bl.Shoot(Vector3.down, this.shotSpeed, COL_CATEGORY.EN_BULLET);
            this.actTime -= this.shotSpan;
        }
    }
    
    /// <summary>
    /// 接触コールバック
    /// </summary>
    /// <param name="atk">影響を与えるコリジョン</param>
    /// <param name="def">影響を受けるコリジョン</param>
    private static void HitCallback(Collision atk, Collision def) {
        // atkに自身、defに相手（この場合は敵）が受け渡される
        Debug.LogWarning("ENEMY HIT !!!");

        //atk.Sleep(); // 自身のコリジョンの返却
    }
}