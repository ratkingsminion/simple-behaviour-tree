using System.Collections.Generic;

namespace RatKing.SBT {

	public partial class BehaviourTree<T> {
		/// <summary>
		/// Invert the child's Status, if it's not running
		/// </summary>
		public BehaviourTree<T> Invert(string name = null)
			=> Register(new NodeDecoratorInvert(this, name));

		/// <summary>
		/// Override the child's Status, if it's not running
		/// </summary>
		public BehaviourTree<T> Override(Status status = Status.Success)
			=> Register(new NodeDecoratorOverride(this, null, status));

		/// <summary>
		/// Override the child's Status, if it's not running
		/// </summary>
		public BehaviourTree<T> Override(string name, Status status = Status.Success)
			=> Register(new NodeDecoratorOverride(this, name, status));

		/// <summary>
		/// Repeat the child until it returns Status.Fail
		/// </summary>
		public BehaviourTree<T> Repeat(string name = null)
			=> Register(new NodeDecoratorRepeat(this, name));

		/// <summary>
		/// Repeat the child until it returns Status.Success
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
				tree.TickNode(child);
			}

			internal override void OnRemove() {
				tree.UntickNode(child, true);
			}
		}

		/// <summary>
		/// INVERT returns Status.Success if its child returns Status.Fail, and vice versa,
		/// but it keeps the Status.Running status
		/// </summary>
		class NodeDecoratorInvert : NodeDecorator {
			public NodeDecoratorInvert(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "invert") { }

			internal override void OnChildReport(Node child) {
				curStatus = child.curStatus switch {
					Status.Success => Status.Fail,
					Status.Fail => Status.Success,
					_ => child.curStatus,
				};
			}
		}

		/// <summary>
		/// OVERRIDE returns the specified Status, as long as its child doesn't return Status.Running
		/// </summary>
		class NodeDecoratorOverride : NodeDecorator {
			protected Status fixedStatus;

			public NodeDecoratorOverride(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "override") { }

			internal NodeDecoratorOverride(BehaviourTree<T> tree, string name, Status status)
				: base(tree, name ?? "override")
				=> fixedStatus = status;

			internal override Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (NodeDecoratorOverride)base.Clone(otherTree, parent);
				clone.fixedStatus = fixedStatus;
				return clone;
			}

			internal override void OnChildReport(Node child) {
				curStatus = child.curStatus == Status.Running ? Status.Running : fixedStatus;
			}
		}

		/// <summary>
		/// REPEAT is running as long as its child doesn't return Status.Fail
		/// </summary>
		class NodeDecoratorRepeat : NodeDecorator {
			public NodeDecoratorRepeat(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "repeat") { }

			protected override void OnStart() {
				curStatus = Status.Running;
			}

			protected override void OnTick() {
				if (child.curStatus != Status.Running) { tree.TickNode(child); }
			}

			internal override void OnChildReport(Node child) {
				if (child.curStatus == Status.Fail) { curStatus = Status.Fail; }
			}
		}

		/// <summary>
		/// RETRY is running as long as its child doesn't return Status.Success
		/// </summary>
		class NodeDecoratorRetry : NodeDecorator {
			public NodeDecoratorRetry(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "retry") { }

			protected override void OnStart() {
				curStatus = Status.Running;
			}

			protected override void OnTick() {
				if (child.curStatus != Status.Running) { tree.TickNode(child); }
			}

			internal override void OnChildReport(Node child) {
				if (child.curStatus == Status.Success) { curStatus = Status.Success; }
			}
		}
	}

}