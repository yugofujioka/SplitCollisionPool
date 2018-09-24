using UnityEngine;

namespace TaskSystem {
	#region DEFINE
	// タスクに対する指令宣言
	public delegate bool OrderHandler<T>(T obj, int no);    // true:継続, false:終了
	public delegate int MatchHandler<T>(T obj);             // 1:HIT, 0:MISS, -1:BREAK
	#endregion


	/// <summary>
	/// タスク管理
	/// </summary>
	/// <typeparam name="T">管理する型</typeparam>
	public sealed class TaskSystem<T> {
		#region MEMBER
		private Task<T> top = null;    // 先端
        private Task<T> end = null;    // 終端
    
        private int capacity = 0;      // 最大タスク数
        private int freeCount = -1;    // 空きタスクインデックス
        private int actCount = 0;      // 稼動タスク数
        private Task<T>[] taskPool = null;    // 生成された全タスク
        private Task<T>[] activeTask = null;  // 待機中のプール
		#endregion


		#region PROPERTY
		// 稼動数
		public int count { get { return this.actCount; } }
		#endregion


		#region MAIN FUNCTION
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="capacity">最大タスク接続数</param>
		public TaskSystem(int capacity) {
            this.capacity = capacity;
            this.taskPool = new Task<T>[this.capacity];
            this.activeTask = new Task<T>[this.capacity];
            for (int i = 0; i < this.capacity; ++i) {
                this.taskPool[i] = new Task<T>();
                this.activeTask[i] = this.taskPool[i];
            }
            this.freeCount = this.capacity;
        }

		/// <summary>
		/// タスクの全消去
		/// </summary>
		public void Clear() {
            this.freeCount = this.capacity;
            this.actCount = 0;
            this.top = null;
            this.end = null;
    
            for (int i = 0; i < this.capacity; ++i) {
                this.taskPool[i].prev = null;
                this.taskPool[i].next = null;
                this.activeTask[i] = this.taskPool[i];
            }
        }

		/// <summary>
		/// タスク接続
		/// </summary>
		/// <param name="item"></param>
        public void Attach(T item) {
            Debug.Assert(item != null, "アタッチエラー");
            Debug.Assert(this.freeCount > 0, "キャパシティオーバー");
    
            Task<T> task = this.activeTask[this.freeCount - 1];
            task.item = item;
    
            if (this.actCount > 0) {
                task.Attach(this.end, null);
                this.end = task;
            } else {
                task.Attach(null, null);
                this.end = task; this.top = task;
            }
    
            --this.freeCount;
            ++this.actCount;
        }

		/// <summary>
		/// 接続解除
		/// 外部からの割込による解除は許容せず指令による終了判定で戻す
		/// </summary>
		/// <param name="task">解除するタスク</param>
        internal void Detach(Task<T> task) {
            if (task == this.top)
                this.top = task.next;
            if (task == this.end)
                this.end = task.prev;
            task.Detach();
    
            --this.actCount;
            ++this.freeCount;
            this.activeTask[this.freeCount-1] = task;
        }

		/// <summary>
		/// 先端タスクの切断
		/// </summary>
		public T PickOutFirst() {
			Task<T> task = this.top;
			if (task == null) {
				Debug.Assert(false, "\"TaskSystem.PickOutFirst\" 失敗");
				return default(T);
			}

			this.top = this.top.next;
			this.Detach(task);

			return task.item;
		}
		
		/// <summary>
		/// 終端タスクの切断
		/// </summary>
		public T PickOutLast() {
			Task<T> task = this.end;
			if (task == null) {
				Debug.Assert(false, "\"TaskSystem.PickOutLast\" 失敗");
				return default(T);
			}

			this.end = this.end.prev;
			this.Detach(task);

			return task.item;
		}

		/// <summary>
		/// 全タスクに実行
		/// </summary>
		/// <param name="action">実行関数</param>
		public void Action(System.Action<T> action) {
			for (Task<T> task = this.top; task != null && this.actCount > 0; task = task.next)
				action(task.item);
		}

        /// <summary>
		/// 全タスクに指令
		/// </summary>
		/// <param name="order">指令コールバック</param>
        public void Order(OrderHandler<T> order) {
            int no = 0;
            Task<T> now = null;
            for (Task<T> task = this.top; task != null && this.actCount > 0;) {
                now = task;
                task = task.next;
                if (!order(now.item, no))
                    this.Detach(now);
                ++no;
            }
        }

		/// <summary>
		/// 条件に合うタスクに対して指令を行う
		/// </summary>
		/// <param name="match">条件式</param>
		/// <param name="order">指令</param>
		public void ParticularOrder(MatchHandler<T> match, OrderHandler<T> order) {
			int no = 0;
			Task<T> now = null;
			for (Task<T> task = this.top; task != null && this.actCount > 0;) {
				// MEMO: 切断されても良い様に最初にノードを更新する
				now = task;
				task = task.next;

				int ret = match(now.item);
				// 中断
				if (ret < 0)
					break;
				// HIT
				if (ret > 0) {
					if (!order(now.item, no))
						this.Detach(now);
				}
				++no;
			}
		}
		#endregion
	}
}