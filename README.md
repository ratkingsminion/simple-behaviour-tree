# simple-behaviour-tree
Simple behaviour tree implementation for C#, usable with Unity

Usage (Unity):

```C#
  SBT.BehaviourTree<GameObject> sbtTest;
  GUIStyle style;

  void Awake() {
    var targetDirections = new[] { Vector3.up, Vector3.right, Vector3.down, Vector3.left };
    var targetPos = transform.position;
    var targetDirIdx = -1;

    var trafficLights = new SBT.BehaviourTree<GameObject>()
      .Sequence("sequence traffic lights")
        .Do("colorize red", go => Colorize(go, Color.red))
        .Wait(0.75)
        .Do("colorize yellow", go => Colorize(go, Color.yellow))
        .Wait(0.5)
        .Do("colorize green", go => Colorize(go, Color.green))
        .Wait(0.75)
      .End();

    sbtTest = new SBT.BehaviourTree<GameObject>(gameObject)
      .Parallel()
        .Repeat().InsertTree(trafficLights)
        .Repeat().Sequence("sequence walk around")
          .Do("walk to target pos",
            go => targetPos = go.transform.position + targetDirections[targetDirIdx = (targetDirIdx + 1) % 4],
            go => {
              go.transform.position = Vector3.Lerp(go.transform.position, targetPos, sbtTest.DeltaTime * 5f);
              if ((go.transform.position - targetPos).magnitude < 0.01f) { go.transform.position = targetPos; return Status.Success; }
              return Status.Running; })
          .Wait(0.4)
        .End()
      .End();
  }

  void Update() {
    sbtTest.Tick(Time.deltaTime);
  }

  void OnGUI() {
    style ??= new(GUI.skin.label) { richText = true, fontSize = 14 };
    GUI.Label(new Rect(10, 10, 1000, 1000), sbtTest.GenerateString(true), style);
  }

  void Colorize(GameObject go, Color color) {
    Debug.Log(Time.frameCount + " <color=#" + ColorUtility.ToHtmlStringRGB(color) + ">ColorTest</color> " + name, go);
    go.GetComponent<Renderer>().material.color = color;
  }
```

Have a look at Example/BehaviourTree.NodePrint.cs to see how to add your own action nodes. To find out how to add composite nodes (like Sequence) and decorator nodes (like Invert), derive from NodeComposite/NodeDecorator instead of just Node and look at the implementation of the standard composite/decorator nodes.

## Action Nodes

* Do - Execute a generic action; if you provide a start action, it's assumed the standard status is Status.Running instead of Status.Success
* Fail - Directly fail
* Success - Directly succeed
* Wait - Wait X seconds; depends on what deltaTime you provide as argument for Tick()

## Composite Nodes

* Sequence - Iterate over the children until the child that fails
* Selector - Iterate over the children until the child that succeeds
* Parallel - Execute all the children at once until one of them fails or all of them succeed
* Race - Execute all the children at once until one of them succeeds or all of them fail
* RandomSelector - Randomly select a child and execute it

Composite nodes always need a call to End() in the hierarchy (see above).

## Decorator Nodes

* Invert - Invert the child's Status, if it's not running
* Override - Override the child's Status, if it's not running
* Repeat - Repeat the child until its curStatus is Status.Fail
* Retry - Repeat the child until its curStatus is Status.Success
