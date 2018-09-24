using UnityEngine;


[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour {
    public static CollisionPool collision;

#if DEBUG
	public bool displayCollision = false;
#endif

	void Awake() {
        GameManager.collision = new CollisionPool();
        GameManager.collision.Initialize();
		Application.targetFrameRate = 60;
    }

	void OnDestroy() {
        GameManager.collision.Final();
	}

	void Start() {
        // 1m = 1pixの位置にカメラを移動させる
        Camera cam = Camera.main;
        float dist = ((float)Screen.height * 0.5f) / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        cam.transform.localPosition = new Vector3(0f, 0f, -dist);
    }

    void LateUpdate() {
        float elapsedTime = Time.deltaTime;

		// コリジョンの更新（判定）
#if DEBUG
		GameManager.collision.debugCamera = (this.displayCollision ? Camera.main : null);
#endif
		GameManager.collision.Proc(elapsedTime);
    }
}
