using System.Collections.Generic;

namespace RatKing.SBT {

	public partial class BehaviourTree<T> {
		/// <summary>
		/// Invert the child's TaskStatus, if it's not running
		/// </summary>
		public BehaviourTree<T> Invert(string name = null)
			=> Register(new NodeDecoratorInvert(this, name));
		
		/// <summary>
		/// Override the child's TaskStatus, if it's not running
		/// </summary>
		public BehaviourTree<T> Override(TaskStatus status = TaskStatus.Success)
			=> Register(new NodeDecoratorOverride(this, null, status));
		
		/// <summary>
		/// Override the child's TaskStatus, if it's not running
		/// </summary>
		public BehaviourTree<T> Override(string name, TaskStatus status = TaskStatus.Success)
			=> Register(new NodeDecoratorOverride(this, name, status));
		
		/// <summary>
		/// Repeat the child until it returns TaskStatus.Fail
		/// </summary>
		public BehaviourTree<T> Repeat(string name = null)
			=> Register(new NodeDecoratorRepeat(this, name));

		/// <summary>
		/// Repeat the child until it returns TaskStatus.Success
		/// </summary>
		public BehaviourTree<T> Retry(string name = null)
			=> Register(new NodeDecoratorRetry(this, name));

		//
		// the nodes

		abstract class NodeDecorator : Node {
			internal Node child;

			protected NodeDecorator(BehaviourTree<T> tree, string name)
				: base(tree, name) { }

			protected override void OnStart() {
				tree.processNodes.Add(child);
			}
			
			internal override void OnRemove() {
				tree.UntickNode(child, true);
			}
		}

		/// <summary>
		/// INVERT returns TaskStatus.Success if its child returns TaskStatus.Fail, and vice versa,
		/// but it keeps the TaskStatus.Running status
		/// </summary>
		class NodeDecoratorInvert : NodeDecorator {
			public NodeDecoratorInvert(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "invert") { }

			internal override void OnChildReport(Node child) {
				curStatus = child.curStatus switch {
					TaskStatus.Success => TaskStatus.Fail,
					TaskStatus.Fail => TaskStatus.Success,
					_ => child.curStatus,
				};
			}
		}

		/// <summary>
		/// OVERRIDE returns the specified TaskStatus, as long as its child doesn't return TaskStatus.Running
		/// </summary>
		class NodeDecoratorOverride : NodeDecorator {
			protected TaskStatus fixedStatus;

			public NodeDecoratorOverride(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "override") { }

			internal NodeDecoratorOverride(BehaviourTree<T> tree, string name, TaskStatus status)
				: base(tree, name ?? "override")
				=> fixedStatus = status;

			internal override Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (NodeDecoratorOverride)base.Clone(otherTree, parent);
				clone.fixedStatus = fixedStatus;
				return clone;
			}

			internal override void OnChildReport(Node child) {
				curStatus = child.curStatus == TaskStatus.Running ? TaskStatus.Running : fixedStatus;
			}
		}

		/// <summary>
		/// REPEAT is running as long as its child doesn't return TaskStatus.Fail
		/// </summary>
		class NodeDecoratorRepeat : NodeDecorator {
			public NodeDecoratorRepeat(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "repeat") { }

			protected override void OnStart() {
				curStatus = TaskStatus.Running;
			}

			protected override void OnTick() {
				if (child.curStatus != TaskStatus.Running) { tree.processNodes.Add(child); }
			}

			internal override void OnChildReport(Node child) {
				if (child.curStatus == TaskStatus.Fail) { curStatus = TaskStatus.Fail; }
			}
		}

		/// <summary>
		/// RETRY is running as long as its child doesn't return TaskStatus.Success
		/// </summary>
		class NodeDecoratorRetry: NodeDecorator {
			public NodeDecoratorRetry(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "retry") { }

			protected override void OnStart() {
				curStatus = TaskStatus.Running;
			}

			protected override void OnTick() {
				if (child.curStatus != TaskStatus.Running) { tree.processNodes.Add(child); }
			}

			internal override void OnChildReport(Node child) {
				if (child.curStatus == TaskStatus.Success) { curStatus = TaskStatus.Success; }
			}
		}
	}

}