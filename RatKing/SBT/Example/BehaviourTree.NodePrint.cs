#if UNITY_5_3_OR_NEWER
using UnityEngine;

// putting our node(s) in RatKing.SBT makes things easier
namespace RatKing.SBT {

	public partial class BehaviourTree<T> {
		/// <summary>
		/// Print a fixed debug message
		/// </summary>
		public BehaviourTree<T> Print(string message) // offering a method without debug name can make the tree more concise
			=> Print((string)null, message);

		/// <summary>
		/// Print a fixed debug message
		/// </summary>
		public BehaviourTree<T> Print(string name, string message) // offering a method with debug name can help with debugging via GenerateString()
			=> Register(new NodePrintFixed(this, name) { message = message });

		/// <summary>
		/// Print a dynamic debug message
		/// </summary>
		public BehaviourTree<T> Print(System.Func<string> message) // offering a method without debug name can make the tree more concise
			=> Print((string)null, message);

		/// <summary>
		/// Print a dynamic debug message
		/// </summary>
		public BehaviourTree<T> Print(System.Func<T, string> message)
			=> Print((string)null, message);

		/// <summary>
		/// Print a dynamic debug message
		/// </summary>
		public BehaviourTree<T> Print(string name, System.Func<string> message) // offering a method with debug name can help with debugging via GenerateString()
			=> Register(new NodePrintDynamic(this, name) { message = _ => message() });

		/// <summary>
		/// Print a dynamic debug message
		/// </summary>
		public BehaviourTree<T> Print(string name, System.Func<T, string> message) // offering a method with debug name can help with debugging via GenerateString()
			=> Register(new NodePrintDynamic(this, name) { message = message });

		// the nodes

		/// <summary>
		/// PRINT a fixed message
		/// </summary>
		public class NodePrintFixed : Node {
			internal string message;

			// always declare the standard constructor of a Node
			public NodePrintFixed(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "print") { }

			// the clone method is needed if your Node has member variables
			internal override Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (NodePrintFixed)base.Clone(otherTree, parent);
				// copy the member variables here
				clone.message = message;
				return clone;
			}

			//protected override void OnStart() {
			//	// override start for initialisation things, this is useful if the node's status is Status.Running
			//}

			// OnTick() is called every time the node 
			protected override void OnTick() {
				Debug.Log(message, tree.target as Object);
				curStatus = Status.Success; // set the status here
			}
		}

		/// <summary>
		/// PRINT a dynamic message
		/// </summary>
		public class NodePrintDynamic : Node {
			internal System.Func<T, string> message;

			// always declare the standard constructor of a Node
			public NodePrintDynamic(BehaviourTree<T> tree, string name)
				: base(tree, name ?? "print") { }

			// the clone method is needed if your Node has member variables
			internal override Node Clone(BehaviourTree<T> otherTree, Node parent) {
				var clone = (NodePrintDynamic)base.Clone(otherTree, parent);
				// copy the member variables here
				clone.message = message;
				return clone;
			}

			//protected override void OnStart() {
			//	// override start for initialisation things, this is useful if the node's status is Status.Running
			//}

			// OnTick() is called every time the node 
			protected override void OnTick() {
				Debug.Log(message(tree.target), tree.target as Object);
				curStatus = Status.Success; // set the status here
			}
		}
	}

}
#endif