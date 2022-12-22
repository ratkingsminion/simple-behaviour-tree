# simple-behaviour-tree
Simple behaviour tree implementation for C#, usable with Unity

Usage (Unity):

```C#
    SBT.BehaviourTree<GameObject> tree;
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

      tree = new SBT.BehaviourTree<GameObject>(gameObject)
        .Parallel()
          .Repeat().InsertTree(trafficLights)
          .Repeat().Sequence("sequence walk around")
            .Do("walk to target pos",
              go => targetPos = go.transform.position + targetDirections[targetDirIdx = (targetDirIdx + 1) % 4],
              go => {
                go.transform.position = Vector3.Lerp(go.transform.position, targetPos, (float)tree.DeltaTime * 5f);
                if (Vector3.Distance(go.transform.position, targetPos) < 0.01f) {
                  go.transform.position = targetPos;
                  return true;
                }
                return false;
              })
            .Wait(0.4)
          .End()
        .End();
    }

    void Update() {
      tree.Tick(Time.deltaTime);
    }

    void OnGUI() {
      style ??= new(GUI.skin.label) { richText = true, fontSize = 14 };
      GUI.Label(new Rect(10, 10, 1000, 1000), tree.GenerateString(true), style);
    }

    void Colorize(GameObject go, Color color) {
      Debug.Log(Time.frameCount + " <color=#" + ColorUtility.ToHtmlStringRGB(color) + ">ColorTest</color> " + name, go);
      go.GetComponent<Renderer>().material.color = color;
    }
  }
```
