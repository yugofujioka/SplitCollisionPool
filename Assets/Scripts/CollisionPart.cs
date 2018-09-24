using UnityEngine;


/// <summary>
/// 当たり判定位置
/// </summary>
public sealed class CollisionPart : MonoBehaviour {
    #region DEFINE
    /// <summary>
    /// コリジョン設定
    /// </summary>
    [System.Serializable]
    public struct CollisionData {
        public COL_FORM form;  // 形状
        public float range;    // 範囲
        public Vector2 size;   // 大きさ
        public Vector3 offset; // 平行移動
        public float angle;    // 初期回転角（deg）
    }
    #endregion


    #region MEMBER
    [SerializeField, Tooltip("コリジョン設定")]
    private CollisionData[] collisionDatas = new CollisionData[0];

    [System.NonSerialized]
    public Vector3 centerPoint = Vector3.zero;

    private Transform trans_ = null;            // Transformキャッシュ
    private Camera camera_ = null;              // 表示カメラキャッシュ
    private Transform cameraTrans = null;       // 表示カメラTransformキャッシュ
    private COL_CATEGORY category = COL_CATEGORY.PLAYER; // コリジョン分類
    private Collision[] collisions = null;      // コリジョンリスト
    private HitHandler hitHandler = null;       // 接触処理
    private int collisionCount = 0;             // コリジョン数
    private bool awake = false;                 // 起動済フラグ

    private Quaternion[] rotations = null;
    #endregion


    #region PROPERTY
    /// <summary> 起動中か </summary>
    public bool isAwake   { get { return this.awake; } }
    #endregion


    #region MAIN FUNCTION
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="category">コリジョンカテゴリ</param>
    /// <param name="hitHandler">接触処理</param>
    public void Initialize(COL_CATEGORY category, HitHandler hitHandler) {
        this.trans_ = this.transform;
        this.category = category;
        this.hitHandler = hitHandler;
        this.collisionCount = this.collisionDatas.Length;
        this.collisions = new Collision[this.collisionCount];
        this.rotations = new Quaternion[this.collisionCount];
        for (int i = 0; i < this.collisionCount; ++i)
            this.rotations[i] = Quaternion.identity;
    }

    /// <summary>
    /// 全起動
    /// </summary>
    public void WakeUp() {
        Debug.Assert(!this.awake, "CollisionPart 二重起動");

        Vector3 worldPoint = this.trans_.position;

        this.camera_ = Camera.main;
        this.cameraTrans = this.camera_.transform;

        // MEMO: カメラからの距離に応じてサイズ補正
        float actualScale = this.CalcActualScale(worldPoint, 1f);

        // コリジョン呼び出し
        this.centerPoint = this.camera_.WorldToScreenPoint(worldPoint);
        for (int i = 0; i < this.collisionCount; ++i) {
            this.collisions[i] = GameManager.collision.PickOut(this.category, this.hitHandler);
            if (this.collisions[i] != null) {
                if (this.collisionDatas[i].form == COL_FORM.CIRCLE)
                    this.collisions[i].SetCircle(this.collisionDatas[i].range * actualScale);
                else if (this.collisionDatas[i].form == COL_FORM.RECTANGLE)
                    this.collisions[i].SetRectangle(this.collisionDatas[i].size * actualScale, this.collisionDatas[i].angle);

                Vector3 offset = Quaternion.AngleAxis(this.collisions[i].angle, Vector3.forward) * this.collisionDatas[i].offset * actualScale;
                this.collisions[i].point.x = this.centerPoint.x + offset.x;
                this.collisions[i].point.y = this.centerPoint.y + offset.z;
            }
        }
        this.awake = true;
    }

    /// <summary>
    /// 全停止
    /// </summary>
    public void Sleep() {
        for (int i = 0; i < this.collisionCount; ++i) {
            if (this.collisions[i] != null) {
                // コリジョン返却
                this.collisions[i].enable = false;
                this.collisions[i] = null;
            }
        }

        this.camera_ = null; // 自分外の参照を残さない
        this.cameraTrans = null;
        this.awake = false;
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="elapsedTime">経過時間</param>
    public void Run(float elapsedTime) {
        if (!this.awake)
            return;
        
        Vector3 worldPosition = this.trans_.position;
        float actualScale = this.CalcActualScale(worldPosition, 1f);
        Quaternion rot = this.trans_.rotation;
        float angle = rot.eulerAngles.z;
        // さかさま対応
        Vector3 up = rot * Vector3.forward;
        if (up.y < 0f)
            angle = -angle;
        
        this.centerPoint = this.camera_.WorldToScreenPoint(worldPosition);
        for (int i = 0; i < this.collisionCount; ++i) {
            this.collisions[i].angle = (angle + this.collisionDatas[i].angle);
            this.rotations[i] = Quaternion.AngleAxis(angle, Vector3.forward);

            Vector3 offset = this.rotations[i] * this.collisionDatas[i].offset * actualScale;
            this.collisions[i].point.x = this.centerPoint.x + offset.x;
            this.collisions[i].point.y = this.centerPoint.y + offset.y;

            if (this.collisionDatas[i].form == COL_FORM.CIRCLE)
                this.collisions[i].SetCircle(this.collisionDatas[i].range * actualScale);
            else if (this.collisionDatas[i].form == COL_FORM.RECTANGLE)
                this.collisions[i].SetRectangle(this.collisionDatas[i].size * actualScale, this.collisions[i].angle);
        }
    }
    #endregion


    #region PRIVATE FUNCTION
    /// <summary>
    /// 高度による補正スケール値計算
    /// </summary>
    /// <param name="worldPoint">ワールド座標</param>
    /// <param name="multiplyScale">基準スケール補正</param>
    private float CalcActualScale(Vector3 worldPoint, float multiplyScale) {
        Vector3 point = Quaternion.Inverse(this.cameraTrans.rotation) * (worldPoint - this.cameraTrans.position);
        float distance = (point.z < 0f ? -point.z : point.z);
        float standardDistance = ((float)Screen.height * 0.5f) / Mathf.Tan(this.camera_.fieldOfView * 0.5f * Mathf.Deg2Rad);
        return (multiplyScale * standardDistance / distance);
    }
    #endregion
}
