using UnityEngine;


/// <summary>
/// 弾丸
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Bullet : MonoBehaviour {
    private float speed = 200f;            // 弾速（pix./sec.）
    private Vector3 direct = Vector3.down; // 進行方向
    private Collision col = null; // 借りたコリジョン


    /// <summary>
    /// カメラ外へ行ったら破棄
    /// </summary>
    void OnBecameInvisible() {
        if (this.col != null)
            this.col.Sleep();
        Object.Destroy(this.gameObject);
    }

    void Update() {
        Vector3 point = this.transform.localPosition;
        point += this.direct * (this.speed * Time.deltaTime);

        // 移動継続
        this.transform.localPosition = point;
        // Sprite座標からScreen座標に落とす
        point.x += (float)Screen.width * 0.5f;
        point.y += (float)Screen.height * 0.5f;
        // コリジョン座標更新
        this.col.point = point;
    }

    /// <summary>
    /// 射撃開始
    /// </summary>
    /// <param name="direct">方向(XY)</param>
    /// <param name="speed">速度（pix./sec.）</param>
    public void Shoot(Vector3 direct, float speed, COL_CATEGORY category) {
        this.direct = direct;
        this.speed = speed;
        // コリジョン呼び出し
        this.col = GameManager.collision.PickOut(category, HitCallback);
        this.col.SetCircle(30f);
    }
    
    /// <summary>
    /// 接触コールバック
    /// </summary>
    /// <param name="atk">影響を与えるコリジョン</param>
    /// <param name="def">影響を受けるコリジョン</param>
    private static void HitCallback(Collision atk, Collision def) {
        //atk.Sleep(); // 自身のコリジョンの返却
    }
}