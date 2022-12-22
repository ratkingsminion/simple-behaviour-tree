using System.Collections.Generic;

namespace RatKing.SBT {

	public partial class BehaviourTree<T> {
		
		public BehaviourTree<T> Fail(string name = null) => Register(new NodeFail(this, name));

		public BehaviourTree<T> Success(string name = null) => Register(new NodeSuccess(this, name));

		// generic actions
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Func<TaskStatus> action) {
			return Do((string)null, action);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Func<TaskStatus> action) {
			var node = new NodeActionSimple(this, name, action);
			return Register(node);
		}

		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action action, TaskStatus result) {
			return Do((string)null, action, result);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action action, TaskStatus result) {
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
		/// Execute an action; if the action returns false, the TaskStatus is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Func<bool> action) {
			var node = new NodeActionSimple(this, name, () => action() ? TaskStatus.Success : TaskStatus.Fail);
			return Register(node);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the TaskStatus is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(System.Action action, bool result = true) {
			return Do((string)null, action, result);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the TaskStatus is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action action, bool result = true) {
			var node = new NodeActionSimple(this, name, result
				? () => { action(); return TaskStatus.Success; }
				: () => { action(); return TaskStatus.Fail; });
			return Register(node);
		}

		// generic actions, targeted
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Func<T, TaskStatus> action) {
			return Do((string)null, action);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Func<T, TaskStatus> action) {
			var node = new NodeTargetedActionSimple(this, name, action);
			return Register(node);
		}

		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> action, TaskStatus result) {
			return Do((string)null, action, result);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> action, TaskStatus result) {
			var node = new NodeTargetedActionSimple(this, name, target => { action(target); return result; });
			return Register(node);
		}

		/// <summary>
		/// Execute an action; if the action returns false, the TaskStatus is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(System.Func<T, bool> action) {
			return Do((string)null, action);
		}
		
		/// <summary>
		/// Execute an action; if the action returns false, the TaskStatus is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Func<T, bool> action) {
			var node = new NodeTargetedActionSimple(this, name, target => action(target) ? TaskStatus.Success : TaskStatus.Fail);
			return Register(node);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the TaskStatus is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> action, bool result = true) {
			return Do((string)null, action, result);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the TaskStatus is set to Fail
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> action, bool result = true) {
			var node = new NodeTargetedActionSimple(this, name, result
				? target => { action(target); return TaskStatus.Success; }
				: target => { action(target); return TaskStatus.Fail; });
			return Register(node);
		}

		// generic actions with start for initialisation
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action start, System.Func<TaskStatus> action) {
			return Do((string)null, start, action);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action start, System.Func<TaskStatus> action) {
			var node = new NodeAction(this, name, start, action);
			return Register(node);
		}

		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action start, System.Action action, TaskStatus result) {
			return Do((string)null, start, action, result);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action start, System.Action action, TaskStatus result) {
			var node = new NodeAction(this, name, start, () => { action(); return result; });
			return Register(node);
		}

		/// <summary>
		/// Execute an action; if the action returns false, the TaskStatus is set to Running
		/// </summary>
		public BehaviourTree<T> Do(System.Action start, System.Func<bool> action) {
			return Do((string)null, start, action);
		}
		
		/// <summary>
		/// Execute an action; if the action returns false, the TaskStatus is set to Running
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action start, System.Func<bool> action) {
			var node = new NodeAction(this, name, start, () => action() ? TaskStatus.Success : TaskStatus.Running);
			return Register(node);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the TaskStatus is set to Running
		/// </summary>
		public BehaviourTree<T> Do(System.Action start, System.Action action, bool result = false) {
			return Do((string)null, start, action, result);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the TaskStatus is set to Running
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action start, System.Action action, bool result = false) {
			var node = new NodeAction(this, name, start, result
				? () => { action(); return TaskStatus.Success; }
				: () => { action(); return TaskStatus.Running; });
			return Register(node);
		}

		// generic actions, targeted
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> start, System.Func<T, TaskStatus> action) {
			return Do((string)null, start, action);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> start, System.Func<T, TaskStatus> action) {
			var node = new NodeTargetedAction(this, name, start, action);
			return Register(node);
		}

		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> start, System.Action<T> action, TaskStatus result) {
			return Do((string)null, start, action, result);
		}
		
		/// <summary>
		/// Execute an action
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> start, System.Action<T> action, TaskStatus result) {
			var node = new NodeTargetedAction(this, name, start, target => { action(target); return result; });
			return Register(node);
		}

		/// <summary>
		/// Execute an action; if the action returns false, the TaskStatus is set to Running
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> start, System.Func<T, bool> action) {
			return Do((string)null, start, action);
		}
		
		/// <summary>
		/// Execute an action; if the action returns false, the TaskStatus is set to Running
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> start, System.Func<T, bool> action) {
			var node = new NodeTargetedAction(this, name, start, target => action(target) ? TaskStatus.Success : TaskStatus.Running);
			return Register(node);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the TaskStatus is set to Running
		/// </summary>
		public BehaviourTree<T> Do(System.Action<T> start, System.Action<T> action, bool result = false) {
			return Do((string)null, start, action, result);
		}
		
		/// <summary>
		/// Execute an action; if the result is set to false, the TaskStatus is set to Running
		/// </summary>
		public BehaviourTree<T> Do(string name, System.Action<T> start, System.Action<T> action, bool result = false) {
			var node = new NodeTargetedAction(this, name, start, result
				? target => { action(target); return TaskStatus.Success; }
				: target => { action(target); return TaskStatus.Running; });
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
				curStatus = TaskStatus.Fail;
			}
		}

		/// <summary>
		/// SUCCESS
		/// </summary>
		class NodeSuccess : Node {
			public NodeSuccess(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "success") { }

			protected override void OnStart() {
				curStatus = TaskStatus.Success;
			}
		}

		//

		/// <summary>
		/// ACTION SIMPLE has a Tick callback only
		/// </summary>
		class NodeActionSimple : Node {
			protected System.Func<TaskStatus> action;

			public NodeActionSimple(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "action") { }

			internal NodeActionSimple(BehaviourTree<T> tree, string name, System.Func<TaskStatus> action)
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
			protected System.Func<TaskStatus> actionRun;

			public NodeAction(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "action") { }

			internal NodeAction(BehaviourTree<T> tree, string name, System.Action actionStart, System.Func<TaskStatus> actionRun)
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
			protected System.Func<T, TaskStatus> action;

			public NodeTargetedActionSimple(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "action") { }

			internal NodeTargetedActionSimple(BehaviourTree<T> tree, string name, System.Func<T, TaskStatus> action)
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
			protected System.Func<T, TaskStatus> actionRun;

			public NodeTargetedAction(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "action") { }

			internal NodeTargetedAction(BehaviourTree<T> tree, string name, System.Action<T> actionStart, System.Func<T, TaskStatus> actionRun)
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
				curStatus = curTime <= 0.0 ? TaskStatus.Success : TaskStatus.Running;
			}
		}
	}

}