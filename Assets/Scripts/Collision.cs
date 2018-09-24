using UnityEngine;


/// <summary>
/// コリジョン種別
/// </summary>
public enum COL_CATEGORY {
    PLAYER,    // 味方本体
    ENEMY,     // 敵本体
    PL_BULLET, // 味方弾
    EN_BULLET, // 敵弾

    MAX,
}

/// <summary>
/// コリジョン型
/// </summary>
public enum COL_FORM {
    CIRCLE,    // 円形
    RECTANGLE, // 矩形
}

/// <summary>
/// 接触コールバック
/// </summary>
/// <param name="atk">影響を与えるコリジョン</param>
/// <param name="def">影響を受けるコリジョン</param>
public delegate void HitHandler(Collision atk, Collision def);

/// <summary>
/// 当たり判定
/// </summary>
public sealed class Collision {
    public bool enable = false;                         // 有効フラグ
    public COL_CATEGORY category = COL_CATEGORY.PLAYER; // コリジョンの分類
    public COL_FORM form = COL_FORM.CIRCLE;             // 形状
    public Vector2 point = Vector2.zero;                // 座標
    public float range = 0f;                            // 最大半径
    public Vector2 size = Vector2.zero;                 // 幅と高さ（※矩形のみ有効）
    public float angle = 0f;                            // 回転角（※矩形のみ有効）
    public HitHandler hitHandler = null;                // 接触コールバック
#if DEBUG
    public bool hit = false;    // 接触した
#endif


    /// <summary>
    /// 起動
    /// </summary>
    /// <param name="category">判定カテゴリ</param>
    /// <param name="hitHandler">衝突処理</param>
    public void WakeUp(COL_CATEGORY category, HitHandler hitHandler) {
        this.enable = true;
        this.hitHandler = hitHandler;
        this.category = category;
        this.SetCircle(10f); // デフォ設定
    }

    /// <summary>
    /// 停止（返却）
    /// </summary>
    public void Sleep() {
        this.enable = false;
        this.hitHandler = null;
    }

    /// <summary>
    /// 円形設定
    /// </summary>
    /// <param name="range">半径</param>
    public void SetCircle(float range) {
        this.form = COL_FORM.CIRCLE;
        this.size.x = this.size.y = 0f;
        this.range = range;
        this.angle = 0f;
    }

    /// <summary>
    /// 矩形設定
    /// </summary>
    /// <param name="size">サイズ</param>
    /// <param name="angle">回転角</param>
    public void SetRectangle(Vector2 size, float angle = 0f) {
        this.form = COL_FORM.RECTANGLE;
        this.size = size;
        this.range = size.magnitude * 0.5f;
        this.angle = angle;
    }

    /// <summary>
    /// 矩形設定
    /// </summary>
    /// <param name="x">横幅</param>
    /// <param name="y">縦幅</param>
    /// <param name="angle">回転角</param>
    public void SetRectangle(float x, float y, float angle = 0f) {
        this.form = COL_FORM.RECTANGLE;
        this.size = new Vector2(x, y);
        this.range = size.magnitude * 0.5f;
        this.angle = angle;
    }
}
