using System.Collections.Generic;

namespace RatKing.SBT {

	public partial class BehaviourTree<T> {
		
		public BehaviourTree<T> Fail(string name = null) => Register(new NodeFail(this, name));

		public BehaviourTree<T> Success(string name = null) => Register(new NodeSuccess(this, name));

		// generic actions
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Func<Status> action) {
			return Do((string)null, action);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Func<Status> action) {
			var node = new NodeActionSimple(this, name, action);
			return Register(node);
		}

		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action action, Status result) {
			return Do((string)null, action, result);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action action, Status result) {
			var node = new NodeActionSimple(this, name, () => { action(); return result; });
			return Register(node);
		}

		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Func<bool> action) {
			return Do((string)null, action);
		}
		
		/// <summary>
		/// Execute an action; if the action returns false, the Status is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Func<bool> action) {
			var node = new NodeActionSimple(this, name, () => action() ? Status.Success : Status.Fail);
			return Register(node);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the Status is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(System.Action action, bool result = true) {
			return Do((string)null, action, result);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the Status is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action action, bool result = true) {
			var node = new NodeActionSimple(this, name, result
				? () => { action(); return Status.Success; }
				: () => { action(); return Status.Fail; });
			return Register(node);
		}

		// generic actions, targeted
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Func<T, Status> action) {
			return Do((string)null, action);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Func<T, Status> action) {
			var node = new NodeTargetedActionSimple(this, name, action);
			return Register(node);
		}

		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> action, Status result) {
			return Do((string)null, action, result);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> action, Status result) {
			var node = new NodeTargetedActionSimple(this, name, target => { action(target); return result; });
			return Register(node);
		}

		/// <summary>
		/// Execute an action; if the action returns false, the Status is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(System.Func<T, bool> action) {
			return Do((string)null, action);
		}
		
		/// <summary>
		/// Execute an action; if the action returns false, the Status is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Func<T, bool> action) {
			var node = new NodeTargetedActionSimple(this, name, target => action(target) ? Status.Success : Status.Fail);
			return Register(node);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the Status is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> action, bool result = true) {
			return Do((string)null, action, result);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the Status is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> action, bool result = true) {
			var node = new NodeTargetedActionSimple(this, name, result
				? target => { action(target); return Status.Success; }
				: target => { action(target); return Status.Fail; });
			return Register(node);
		}

		// generic actions with start for initialisation
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action start, System.Func<Status> action) {
			return Do((string)null, start, action);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action start, System.Func<Status> action) {
			var node = new NodeAction(this, name, start, action);
			return Register(node);
		}

		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action start, System.Action action, Status result) {
			return Do((string)null, start, action, result);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action start, System.Action action, Status result) {
			var node = new NodeAction(this, name, start, () => { action(); return result; });
			return Register(node);
		}

		/// <summary>
		/// Execute an action; if the action returns false, the Status is set to Running
		/// </summary>
		public BehaviourTree<T> Do(System.Action start, System.Func<bool> action) {
			return Do((string)null, start, action);
		}
		
		/// <summary>
		/// Execute an action; if the action returns false, the Status is set to Running
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action start, System.Func<bool> action) {
			var node = new NodeAction(this, name, start, () => action() ? Status.Success : Status.Running);
			return Register(node);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the Status is set to Running
		/// </summary>
		public BehaviourTree<T> Do(System.Action start, System.Action action, bool result = false) {
			return Do((string)null, start, action, result);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the Status is set to Running
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action start, System.Action action, bool result = false) {
			var node = new NodeAction(this, name, start, result
				? () => { action(); return Status.Success; }
				: () => { action(); return Status.Running; });
			return Register(node);
		}

		// generic actions, targeted
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> start, System.Func<T, Status> action) {
			return Do((string)null, start, action);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> start, System.Func<T, Status> action) {
			var node = new NodeTargetedAction(this, name, start, action);
			return Register(node);
		}

		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> start, System.Action<T> action, Status result) {
			return Do((string)null, start, action, result);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> start, System.Action<T> action, Status result) {
			var node = new NodeTargetedAction(this, name, start, target => { action(target); return result; });
			return Register(node);
		}

		/// <summary>
		/// Execute an action; if the action returns false, the Status is set to Running
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> start, System.Func<T, bool> action) {
			return Do((string)null, start, action);
		}
		
		/// <summary>
		/// Execute an action; if the action returns false, the Status is set to Running
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> start, System.Func<T, bool> action) {
			var node = new NodeTargetedAction(this, name, start, target => action(target) ? Status.Success : Status.Running);
			return Register(node);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the Status is set to Running
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> start, System.Action<T> action, bool result = false) {
			return Do((string)null, start, action, result);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the Status is set to Running
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> start, System.Action<T> action, bool result = false) {
			var node = new NodeTargetedAction(this, name, start, result
				? target => { action(target); return Status.Success; }
				: target => { action(target); return Status.Running; });
			return Register(node);
		}

		// special actions

		/// <summary>
		/// Returns Success when it finishes waiting
		/// </summary>
		public BehaviourTree<T> Wait(double waitTime) =>
			Register(new NodeWait(this, null, waitTime));
		
		/// <summary>
		/// Returns Success when it finishes waiting
		/// </summary>
		public BehaviourTree<T> Wait(string name, double waitTime) =>
			Register(new NodeWait(this, name, waitTime));

		//
		// the nodes

		/// <summary>
		/// FAIL
		/// </summary>
		class NodeFail : Node {
			public NodeFail(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "fail") { }

			protected override void OnStart() {
				curStatus = Status.Fail;
			}
		}

		/// <summary>
		/// SUCCESS
		/// </summary>
		class NodeSuccess : Node {
			public NodeSuccess(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "success") { }

			protected override void OnStart() {
				curStatus = Status.Success;
			}
		}

		//

		/// <summary>
		/// ACTION SIMPLE has a Tick callback only
		/// </summary>
		class NodeActionSimple : Node {
			protected System.Func<Status> action;

			public NodeActionSimple(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "action") { }

			internal NodeActionSimple(BehaviourTree<T> tree, string name, System.Func<Status> action)
				: base(tree, name ?? "action")
				=> this.action = action;

			internal override Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (NodeActionSimple)base.Clone(otherTree, parent);
				clone.action = action;
				return clone;
			}

			protected override void OnTick() {
				curStatus = action();
			}
		}

		/// <summary>
		/// ACTION has both a Start and a Tick callback
		/// </summary>
		class NodeAction : Node {
			protected System.Action actionStart;
			protected System.Func<Status> actionRun;

			public NodeAction(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "action") { }

			internal NodeAction(BehaviourTree<T> tree, string name, System.Action actionStart, System.Func<Status> actionRun)
				: base(tree, name ?? "action")
				=> (this.actionStart, this.actionRun) = (actionStart, actionRun);

			internal override Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (NodeAction)base.Clone(otherTree, parent);
				clone.actionStart = actionStart;
				clone.actionRun = actionRun;
				return clone;
			}

			protected override void OnStart() {
				actionStart();
			}

			protected override void OnTick() {
				curStatus = actionRun();
			}
		}

		/// <summary>
		/// ACTION SIMPLE has a Tick callback only
		/// </summary>
		class NodeTargetedActionSimple : Node {
			protected System.Func<T, Status> action;

			public NodeTargetedActionSimple(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "action") { }

			internal NodeTargetedActionSimple(BehaviourTree<T> tree, string name, System.Func<T, Status> action)
				: base(tree, name ?? "action")
				=> this.action = action;

			internal override Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (NodeTargetedActionSimple)base.Clone(otherTree, parent);
				clone.action = action;
				return clone;
			}

			protected override void OnTick() {
				curStatus = action(tree.target);
			}
		}

		/// <summary>
		/// ACTION has both a Start and a Tick callback
		/// </summary>
		class NodeTargetedAction : Node {
			protected System.Action<T> actionStart;
			protected System.Func<T, Status> actionRun;

			public NodeTargetedAction(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "action") { }

			internal NodeTargetedAction(BehaviourTree<T> tree, string name, System.Action<T> actionStart, System.Func<T, Status> actionRun)
				: base(tree, name ?? "action")
				=> (this.actionStart, this.actionRun) = (actionStart, actionRun);

			internal override Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (NodeTargetedAction)base.Clone(otherTree, parent);
				clone.actionStart = actionStart;
				clone.actionRun = actionRun;
				return clone;
			}

			protected override void OnStart() {
				actionStart(tree.target);
			}

			protected override void OnTick() {
				curStatus = actionRun(tree.target);
			}
		}

		/// <summary>
		/// WAIT waits X seconds
		/// </summary>
		class NodeWait : Node {
			double waitTime;
			double curTime;

			public NodeWait(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "wait") { }

			internal NodeWait(BehaviourTree<T> tree, string name, double waitTime)
				: base(tree, name ?? "wait " + waitTime.ToString("0.##"))
				=> this.waitTime = waitTime;

			internal override Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (NodeWait)base.Clone(otherTree, parent);
				clone.waitTime = waitTime;
				return clone;
			}

			protected override void OnStart() {
				curTime = waitTime;
			}

			protected override void OnTick() {
				curTime -= tree.DeltaTime;
				curStatus = curTime <= 0.0 ? Status.Success : Status.Running;
			}
		}
	}

}