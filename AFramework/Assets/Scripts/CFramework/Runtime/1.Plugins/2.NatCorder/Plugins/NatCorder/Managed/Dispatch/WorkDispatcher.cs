/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Dispatch {

	using System;
	using System.Threading;
	using Queue = System.Collections.Generic.List<System.Action>;
	
	public sealed class WorkDispatcher : MainDispatcher {

		#region --Op vars--
		private bool running = true;
		private Thread thread;
		private readonly EventWaitHandle queueSync = new AutoResetEvent(false);
		#endregion


		#region --Dispatcher--

		public WorkDispatcher () : base() {}

		public override void Dispatch (Action action) {
			base.Dispatch(action);
			queueSync.Set();
		}

		public override void Dispose () {
			base.Dispose();
			thread.Join();
		}
		#endregion


		#region --Operations--

		protected override void Start () {
			thread = new Thread(Update);
			thread.Start();
		}

		protected override void Update () {
			for (;;) {
				queueSync.WaitOne();
				base.Update();
				if (!running)
					break;
			}
		}

		protected override void Stop () {
			running = false;
		}
		#endregion
	}
}

