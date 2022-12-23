using System.Collections.Generic;

namespace RatKing.SBT {

	public partial class BehaviourTree<T> {
		/// <summary>
		/// Iterate over the children until the child that fails.
		/// Don't forget to close a compositor node with End()
		/// </summary>
		public BehaviourTree<T> Sequence(string name = null)
			=> Register(new NodeCompositeSequence(this, name));
		
		/// <summary>
		/// Iterate over the children until the child that succeeds.
		/// Don't forget to close a compositor node with End()
		/// </summary>
		public BehaviourTree<T> Selector(string name = null)
			=> Register(new NodeCompositeSelector(this, name));
		
		/// <summary>
		/// Execute all the children at once until one of them fails or all of them succeed.
		/// Don't forget to close a compositor node with End()
		/// </summary>
		public BehaviourTree<T> Parallel(string name = null)
			=> Register(new NodeCompositeParallel(this, name));
		
		/// <summary>
		/// Execute all the children at once until one of them succeeds or all of them fail.
		/// Don't forget to close a compositor node with End()
		/// </summary>
		public BehaviourTree<T> Race(string name = null)
			=> Register(new NodeCompositeRace(this, name));

		/// <summary>
		/// Randomly select a child and execute it.
		/// Don't forget to close a compositor node with End()
		/// </summary>
		public BehaviourTree<T> RandomSelector(string name = null)
			=> Register(new NodeCompositeRandomSelector(this, name));

		//
		// the nodes

		abstract class NodeComposite : Node {
			internal readonly List<Node> children = new();
			internal int childCount = 0;

			protected NodeComposite(BehaviourTree<T> tree, string name)
				: base(tree, name) { }
			
			internal override void OnRemove() {
				foreach (var c in children) { tree.UntickNode(c, true); }
			}

			internal void AddChild(Node node) {
				children.Add(node);
				++childCount;
			}
		}

		/// <summary>
		/// SEQUENCE iterates over its children one by one until the first one returns TaskStatus.Fail
		/// </summary>
		class NodeCompositeSequence : NodeComposite {
			int curIndex;

			public NodeCompositeSequence(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "sequence") { }

			protected override void OnStart() {
				curStatus = TaskStatus.Running;
				curIndex = 0;
				tree.processNodes.Add(children[curIndex]);
			}

			protected override void OnTick() {
				base.OnTick();
			}

			internal override void OnChildReport(Node child) {
				if (child.curStatus != TaskStatus.Running) {
					if (child.curStatus == TaskStatus.Fail || ++curIndex >= childCount) {
						curStatus = child.curStatus;
					}
					else {
						tree.processNodes.Add(children[curIndex]);
						tree.IsTicking = true; // continue tick
					}
				}
			}
		}

		/// <summary>
		/// SELECTOR iterates over its children one by one until the first one returns TaskStatus.Success
		/// Called FALLBACK by PandaBT
		/// </summary>
		class NodeCompositeSelector : NodeComposite {
			int curIndex;

			public NodeCompositeSelector(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "selector") { }

			protected override void OnStart() {
				curStatus = TaskStatus.Running;
				curIndex = 0;
				tree.processNodes.Add(children[curIndex]);
			}

			internal override void OnChildReport(Node child) {
				if (child.curStatus != TaskStatus.Running) {
					if (child.curStatus == TaskStatus.Success || ++curIndex >= childCount) {
						curStatus = child.curStatus;
					}
					else {
						tree.processNodes.Add(children[curIndex]);
						tree.IsTicking = true; // continue tick
					}
				}
			}
		}

		/// <summary>
		/// PARALLEL executes all its children at once, until they all return TaskStatus.Success,
		/// or one of them returns TaskStatus.Fail
		/// </summary>
		class NodeCompositeParallel : NodeComposite {
			int curSuccess;

			public NodeCompositeParallel(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "parallel") { }

			protected override void OnStart() {
				curStatus = TaskStatus.Running;
				curSuccess = 0;
				for (var i = 0; i < childCount; ++i) {
					tree.processNodes.Add(children[i]);
				}
			}

			internal override void OnChildReport(Node child) {
				switch (child.curStatus) {
					case TaskStatus.Success:
						++curSuccess;
						if (curSuccess == childCount) { curStatus = TaskStatus.Success; }
						break;
					case TaskStatus.Fail:
						curStatus = TaskStatus.Fail;
						break;
				}
			}
		}

		/// <summary>
		/// RACE executes all its children at once, until they all return TaskStatus.Fail,
		/// or one of them returns TaskStatus.Success
		/// </summary>
		class NodeCompositeRace : NodeComposite {
			int curFail;

			public NodeCompositeRace(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "race") { }

			protected override void OnStart() {
				curStatus = TaskStatus.Running;
				curFail = 0;
				for (var i = childCount - 1; i >= 0; --i) {
					tree.processNodes.Add(children[i]);
				}
			}

			internal override void OnChildReport(Node child) {
				switch (child.curStatus) {
					case TaskStatus.Success:
						curStatus = TaskStatus.Success;
						break;
					case TaskStatus.Fail:
						++curFail;
						if (curFail == childCount) { curStatus = TaskStatus.Fail; }
						break;
				}
			}
		}

		/// <summary>
		/// Randomly chooses one of its children and ticks it;
		/// Returns the same result as its child
		/// </summary>
		class NodeCompositeRandomSelector : NodeComposite {
			public NodeCompositeRandomSelector(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "random selector") { }

			protected override void OnStart() {
				tree.processNodes.Add(children[tree.random.Next() % childCount]);
			}

			internal override void OnChildReport(Node child) {
				curStatus = child.curStatus;
			}
		}
	}

}