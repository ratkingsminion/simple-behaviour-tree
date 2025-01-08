// #define SBT_OPTIMIZED
using System.Collections.Generic;
using System.IO;

namespace RatKing.SBT {

	// https://www.gamedeveloper.com/programming/behavior-trees-for-ai-how-they-work
	// inspired by fluid BT: https://github.com/ashblue/fluid-behavior-tree
	// also by PandaBT: http://www.pandabt.com/documentation/2.0.0

	public enum Status {
		Success,
		Fail,
		Running
	}

	public partial class BehaviourTree<T> {

		public abstract class Node {
			protected BehaviourTree<T> tree;
			internal string name; // for debugging purposes
			internal Node parent; // can be null, then it's at the root
#if SBT_OPTIMIZED
			internal Status curStatus = Status.Fail;
#else
			internal Status _curStatus = Status.Fail;
			internal int lastChangeTick;

			internal Status curStatus {
				get { return _curStatus; }
				set { _curStatus = value; lastChangeTick = tree.tickCounter; }
			}
#endif
			internal bool isProcessing;
			internal int curTick = -1;

			protected Node(BehaviourTree<T> tree, string name)
				=> (this.tree, this.name, this.parent) = (tree, name, tree.processNodes.Count > 0 ? tree.processNodes[^1] : null);

			internal virtual Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (Node)System.Activator.CreateInstance(GetType(), new object[] { otherTree, this.name });
				clone.parent = parent;
				return clone;
			}

			internal virtual void Tick() {
				if (curStatus != Status.Running) { OnStart(); }
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
			/// OnChildReport() gets the report from the children of this node (if it has children).
			/// To add a new node for processing, use tree.TickNode(); to remove a node, use tree.UntickNode()
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
		
		/// <summary>
		/// a tree with more than one root is technically malformed, but can be useful when using InsertTree()
		/// </summary>
		readonly List<Node> roots = new();

		readonly System.Random random = new();

		/// <summary>
		/// The delta time fed to Tick()
		/// Warning: only use this indirectly, inside nodes via tree.DeltaTime (because subtrees don't get updated time)
		/// </summary>
		public double DeltaTime { get; private set; }
		public float DeltaTimeF => (float)DeltaTime;

		public bool IsTicking { get; private set; }
		int tickProcessNodeIdx = 0;
		int tickCounter = 0;
		event System.Action<string> LogError;

		static readonly string debugTab = new(' ', 1000);
		static readonly System.Text.StringBuilder debugSB = new();

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

		public string GenerateString(bool richText = false, int tabWidth = 3, int rootIdx = 0, int coloredAgeTicks = 30) {
			debugSB.Clear();
			void AddNode(Node node, bool richText, int depth = 0, int tabMul = 1) {
				if (node == null) { return; }
				if (tabWidth != 0) { debugSB.Append(debugTab, 0, depth * tabWidth * tabMul); }
#if SBT_OPTIMIZED
				var colored = richText && node.curStatus == Status.Running;
				if (colored) {
					debugSB.Append("<color=#ffff00>");
				}
#else
				var age = coloredAgeTicks > 0 ? System.Math.Clamp(tickCounter - node.lastChangeTick, 0, coloredAgeTicks) / (float)coloredAgeTicks : 1.0;
				var colored = richText && node.curStatus != Status.Fail && age < 1.0;
				if (colored) {
					var lerp = ((int)(age * 0xff)).ToString("x2");
					debugSB.Append("<color=#").Append(lerp).Append("ff").Append(lerp).Append(">");
				}
#endif
				debugSB.Append(node.name);
				if (colored) { debugSB.Append("</color>"); }

				if (node is NodeDecorator nd) {
					debugSB.Append(" . ");
					AddNode(nd.child, richText, depth, 0);
				}
				else {
					debugSB.AppendLine();
					if (node is NodeComposite nc) {
						foreach (var c in nc.children) { AddNode(c, richText, depth + 1); }
					}
				}
			}
			if (roots.Count - 1 >= rootIdx) { AddNode(roots[rootIdx], richText); }
			return debugSB.ToString();
		}

		/// <summary>
		/// Call this to tick the tree and traverse its nodes
		/// </summary>
		public Status Tick(double deltaTime, int rootIdx = 0) {
			DeltaTime = deltaTime;

			IsTicking = true;
			if (processNodes.Count == 0) {
				TickNode(roots[rootIdx]);
			}

			++tickCounter;
			while (IsTicking && processNodes.Count > 0) {
				tickProcessNodeIdx = 0;

				for (; tickProcessNodeIdx < processNodes.Count; ++tickProcessNodeIdx) { // list can increase during iteration
					processNodes[tickProcessNodeIdx].Tick();
				}

				IsTicking = false;

				for (--tickProcessNodeIdx; tickProcessNodeIdx >= 1; --tickProcessNodeIdx) {
					var node = processNodes[tickProcessNodeIdx];
					node.parent.OnChildReport(node);
					if (node.curStatus != Status.Running) { UntickNode(node); }
				}
				if (processNodes[0].curStatus != Status.Running) {
					UntickNode(processNodes[0]);
				}

				while (nodesToRemove.Count > 0) {
					var node = nodesToRemove.Pop();
					if (processNodes.Remove(node)) { node.OnRemove(); }
				}
			}

			IsTicking = false;

			if (processNodes.Count == 0) { return Status.Success; }
			else if (processNodes.Count == 1) { return processNodes[0].curStatus; }
			return Status.Running;
		}

		void TickNode(Node node) {
			if (node.curTick != tickCounter) {
				node.curTick = tickCounter;
				node.isProcessing = true;
				processNodes.Add(node);
			}
		}

		void UntickNode(Node node, bool setToFail = false) {
			if (node.isProcessing) {
				node.isProcessing = false;
				if (setToFail) { node.curStatus = Status.Fail; }
				nodesToRemove.Push(node);
			}
		}

		/// <summary>
		/// Stop the current tick
		/// </summary>
		public void Reset() {
			IsTicking = false;
			tickProcessNodeIdx = 0;
			foreach (var n in processNodes) { n.isProcessing = false; n.curTick = -1; n.curStatus = Status.Fail; }
			foreach (var n in nodesToRemove) { n.isProcessing = false; n.curTick = -1; n.curStatus = Status.Fail; }
			processNodes.Clear();
			nodesToRemove.Clear();
		}

		//

		// building the hierarchy

		BehaviourTree<T> Register(Node node) {
			if (processNodes.Count > 0) {
				var last = processNodes.Count - 1;
				if (processNodes[last] is NodeComposite c) { c.AddChild(node); }
				else if (processNodes[last] is NodeDecorator d) { d.child = node; processNodes.RemoveAt(last); }
			}
			else {
				roots.Add(node);
			}
			if (node is NodeComposite || node is NodeDecorator) { processNodes.Add(node); }
			return this;
		}

		//

		/// <summary>
		/// Insert another Behaviour Tree with the same target type
		/// </summary>
		public BehaviourTree<T> InsertTree(BehaviourTree<T> other) {
			if (other == null || other.roots.Count == 0) { return this; }

			foreach (var otherRoot in other.roots) {
				var clonedRoot = otherRoot.Clone(this, processNodes.Count > 0 ? processNodes[^1] : null);
				Register(clonedRoot);

				var stack = new Stack<(Node original, Node clone)>();
				stack.Push((otherRoot, clonedRoot));
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

				if (clonedRoot is NodeComposite || clonedRoot is NodeDecorator) {
					processNodes.RemoveAt(processNodes.Count - 1);
				}
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
			roots.Clear();
			return this;
		}
	}

}