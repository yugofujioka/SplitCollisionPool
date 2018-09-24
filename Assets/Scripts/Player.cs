using UnityEngine;


public class Player : MonoBehaviour {
    public GameObject bulletPrefab = null;
    public float shotSpan = 0.08f;
    public float shotSpeed = 1000f;

    private float actTime = 0f;
    private CollisionPart col = null;

    void Start() {
        // コリジョンの呼び出し
        this.col = this.GetComponentInChildren<CollisionPart>(true);
        this.col.Initialize(COL_CATEGORY.PLAYER, HitCallback);
        this.col.WakeUp();
    }

    void OnDestroy() {
        // コリジョンの返却
        this.col.Sleep();
    }

    void Update() {
        float elapsedTime = Time.deltaTime;
        this.col.Run(elapsedTime);

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
            bl.Shoot(Vector3.up, this.shotSpeed, COL_CATEGORY.PL_BULLET);
            this.actTime -= this.shotSpan;
        }
    }
    
    /// <summary>
    /// 接触コールバック
    /// </summary>
    /// <param name="atk">影響を与えるコリジョン</param>
    /// <param name="def">影響を受けるコリジョン</param>
    private void HitCallback(Collision atk, Collision def) {
        // atkに自身、defに相手（この場合は敵）が受け渡される
        Debug.Log("PLAYER HIT !!!");

        //atk.Sleep(); // 自身のコリジョンの返却
    }
}