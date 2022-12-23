using System.Collections.Generic;

namespace RatKing.SBT {

	// https://www.gamedeveloper.com/programming/behavior-trees-for-ai-how-they-work
	// inspired by fluid BT: https://github.com/ashblue/fluid-behavior-tree
	// also by PandaBT: http://www.pandabt.com/documentation/2.0.0

	public enum TaskStatus {
		Success,
		Fail,
		Running
	}

	public partial class BehaviourTree<T> {

		public abstract class Node {
			protected BehaviourTree<T> tree;
			internal string name; // for debugging purposes
			internal Node parent; // can be null, then it's at the root
			internal TaskStatus curStatus = TaskStatus.Fail;

			protected Node(BehaviourTree<T> tree, string name)
				=> (this.tree, this.name, this.parent) = (tree, name, tree.processNodes.Count > 0 ? tree.processNodes[^1] : null);

			internal virtual Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (Node)System.Activator.CreateInstance(GetType(), new object[] { otherTree, this.name });
				clone.parent = parent;
				return clone;
			}

			internal void Tick() {
				if (curStatus != TaskStatus.Running) { OnStart(); }
				OnTick();
			}

			/// <summary>
			/// Called on the start of a tick as long as the node is not Running
			/// </summary>
			protected virtual void OnStart() { }

			/// <summary>
			/// Called every tick
			/// </summary>
			protected virtual void OnTick() { }

			/// <summary>
			/// OnChildReport() gets the report from the children of this node (if it has children)
			/// Is allowed to add new processNodes, but NOT remove them! (Use tree.Untick() for that!)
			/// </summary>
			internal virtual void OnChildReport(Node child) { }

			/// <summary>
			/// Called when the node gets removed from the current Tick()
			/// </summary>
			internal virtual void OnRemove() { }
		}

		//

		readonly T target;
		readonly Stack<Node> nodesToRemove = new();
		readonly List<Node> processNodes = new();
		Node root;

		readonly System.Random random = new();

		/// <summary>
		/// The delta time fed to Tick()
		/// Warning: only use this indirectly, inside nodes via tree.DeltaTime (because subtrees don't get updated time)
		/// </summary>
		public double DeltaTime { get; private set; }

		public bool IsTicking { get; private set; }
		int tickProcessNodeIdx = 0;
		event System.Action<string> LogError;

		static string debugTab = new(' ', 1000);
		static System.Text.StringBuilder debugSB = new();

		//

#if UNITY_5_3_OR_NEWER
		public BehaviourTree(T target = default) {
			LogError = UnityEngine.Debug.LogError;
			this.target = target;
		}
#endif

		public BehaviourTree(System.Action<string> logError, T target = default) {
			LogError = logError;
			this.target = target;
		}

		//

		public string GenerateString(bool richText = false, int tabWidth = 3) {
			debugSB.Clear();
			void AddNode(Node node, int depth = 0, bool withTabs = true) {
				if (node == null) { return; }
				var withColour = richText;
				if (withColour) {
					switch (node.curStatus) {
						case TaskStatus.Success:
						case TaskStatus.Fail:	 withColour = false; break;
						case TaskStatus.Running: debugSB.Append("<color=#0f0>"); break;
					}
				}
				if (withTabs) { debugSB.Append(debugTab, 0, depth * tabWidth); }
				debugSB.Append(node.name);
				if (withColour) { debugSB.Append("</color>"); }

				if (node is NodeDecorator nd) {
					debugSB.Append(" . ");
					AddNode(nd.child, depth, false);
				}
				else {
					debugSB.AppendLine();
					if (node is NodeComposite nc) {
						foreach (var c in nc.children) { AddNode(c, depth + 1); }
					}
				}
			}
			AddNode(root);
			return debugSB.ToString();
		}

		/// <summary>
		/// Call this to tick the tree and traverse its nodes
		/// </summary>
		public TaskStatus Tick(double deltaTime) {
			DeltaTime = deltaTime;

			IsTicking = true;
			if (processNodes.Count == 0) {
				processNodes.Add(root);
			}

			while (IsTicking && processNodes.Count > 0) {
				tickProcessNodeIdx = 0;

				for (; tickProcessNodeIdx < processNodes.Count; ++tickProcessNodeIdx) { // list can increase during iteration
					processNodes[tickProcessNodeIdx].Tick();
				}

				IsTicking = false;

				for (--tickProcessNodeIdx; tickProcessNodeIdx >= 1; --tickProcessNodeIdx) {
					var node = processNodes[tickProcessNodeIdx];
					node.parent.OnChildReport(node);
					if (node.curStatus != TaskStatus.Running) { UntickNode(node); }
				}

				if (processNodes[0].curStatus != TaskStatus.Running) {
					UntickNode(processNodes[0]);
				}

				while (nodesToRemove.Count > 0) {
					var node = nodesToRemove.Pop();
					if (processNodes.Remove(node)) { node.OnRemove(); }
				}

				// TODO: optionally break after X iterations?
			}
			
			IsTicking = false;

			if (processNodes.Count == 0) { return TaskStatus.Success; }
			else if (processNodes.Count == 1) { return processNodes[0].curStatus; }
			return TaskStatus.Running;
		}

		void UntickNode(Node node, bool setToFail = false) {
			if (!nodesToRemove.Contains(node)) {
				if (setToFail) { node.curStatus = TaskStatus.Fail; }
				nodesToRemove.Push(node);
			}
		}

		/// <summary>
		/// Stop the current tick
		/// </summary>
		public void Reset() {
			IsTicking = false;
			tickProcessNodeIdx = 0;
			foreach (var n in processNodes) { n.curStatus = TaskStatus.Fail; }
			foreach (var n in nodesToRemove) { n.curStatus = TaskStatus.Fail; }
			processNodes.Clear();
			nodesToRemove.Clear();
		}

		//

		// building the hierarchy

		BehaviourTree<T> Register(Node node) {
			root ??= node;
			if (processNodes.Count > 0) {
				var last = processNodes.Count - 1;
				if (processNodes[last] is NodeComposite c) { c.AddChild(node); }
				else if (processNodes[last] is NodeDecorator d) { d.child = node; processNodes.RemoveAt(last); }
			}
			if (node is NodeComposite || node is NodeDecorator) { processNodes.Add(node); }
			return this;
		}

		//

		/// <summary>
		/// Insert another Behaviour Tree with the same target type
		/// </summary>
		public BehaviourTree<T> InsertTree(BehaviourTree<T> other) {
			if (other == null || other.root == null) { return this; }

			var clonedRoot = other.root.Clone(this, processNodes.Count > 0 ? processNodes[^1] : null);
			Register(clonedRoot);

			var stack = new Stack<(Node original, Node clone)>();
			stack.Push((other.root, clonedRoot));
			while (stack.Count > 0) {
				var (original, clone) = stack.Pop();
				if (original is NodeComposite oc && clone is NodeComposite cc) {
					foreach (var c in oc.children) {
						cc.AddChild(c.Clone(this, clone));
						stack.Push((c, cc.children[^1]));
					}
				}
				else if (original is NodeDecorator od && clone is NodeDecorator cd) {
					cd.child = od.child.Clone(this, clone);
					stack.Push((od.child, cd.child));
				}
			}

			if (clonedRoot is NodeComposite) {
				processNodes.RemoveAt(processNodes.Count - 1);
			}

			return this;
		}

		/// <summary>
		/// Always call this at the end of a compositor node's children list
		/// </summary>
		public BehaviourTree<T> End() {
			if (processNodes.Count == 0) { LogError("Malformed Behaviour Tree: too many End() calls!"); return this; }
			var last = processNodes.Count - 1;
			if (processNodes[last] is not NodeComposite nc) { LogError("Malformed Behaviour Tree: End() used without composite node!"); return this; }
			if (nc.childCount == 0) { LogError("Malformed Behaviour Tree: composite node with no children!"); return this; }
			processNodes.RemoveAt(last);
			return this;
		}

		/// <summary>
		/// Stops and resets the tree, clearing its root node
		/// </summary>
		public BehaviourTree<T> ClearNodes() {
			Reset();
			root = null;
			return this;
		}
	}

}