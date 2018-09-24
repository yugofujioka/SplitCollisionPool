
namespace TaskSystem {
    /// <summary>
	/// タスクシステムで管理されるタスク
	/// 外部公開は不要
	/// </summary>
	/// <typeparam name="T">管理する型</typeparam>
    internal sealed class Task<T> {
		public T item = default(T);
        public Task<T> prev = null;
        public Task<T> next = null;


		/// <summary>
		/// 接続処理
		/// </summary>
		/// <param name="prev">接続する前のノード</param>
		/// <param name="next">接続する後ろのノード</param>
		public void Attach(Task<T> prev, Task<T> next) {
            this.prev = prev;
            this.next = next;
            if (prev != null)
                prev.next = this;
            if (next != null)
                next.prev = this;
        }

        /// <summary>
		/// 切断処理
		/// </summary>
        public void Detach() {
            if (this.prev != null)
                this.prev.next = this.next;
            if (this.next != null)
                this.next.prev = this.prev;

            this.prev = null;
            this.next = null;
        }
    }
}