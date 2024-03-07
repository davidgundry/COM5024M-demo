using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    private BT behaviourTree;
    private Blackboard blackboard;

    // Example to demonstrate getting data out of blackboard
    public int happiness;

    void Start()
    {
        // Create an example behaviour tree
        behaviourTree = new BT.Decorator.Repeater( // Repeat everything below
            new BT.Composite.Sequence( new BT[] { // Run the following steps in order, ending on failure
                new BT.Data<Vector3>.Assert((int) AgentProps.Target, (v) => true), // Always do this (currently required only so that Blackboard knows to store Vector3s)
                new BT.Test((bb) => { // Test if we are close to our target
                    Vector3 position = bb.GetValue<Vector3>((int)AgentProps.Position, typeof(Vector3));
                    Vector3 target = bb.GetValue<Vector3>((int)AgentProps.Target, typeof(Vector3));
                    return (position - target).magnitude < 10;
                }), 
                new BT.Data<int>.Set((int) AgentProps.Happiness, (a) => a + 1), // Increment our happiness
                new BT.Data<int>.Assert((int) AgentProps.Happiness, (v) => v > 10), // Test if we are happy
                new BT.Action(() => { return BT.State.Success;}), // Run some arbitrary action
                }
            ));

        // Create a blackboard for storing data for this behaviour tree
        LinkedList<Type> types = new();
        behaviourTree.AddRequiredDictionaryTypes(types);
        blackboard = new Blackboard(types);
    }

    void Update()
    {
        // Load any sensor values into blackboard
        // This could be done on update by other MonoBehaviours that generate the values
        // e.g. a target sensing behaviour
        blackboard.SetValue((int) AgentProps.Target, new Vector3(0,0,0), typeof(Vector3));
        blackboard.SetValue((int) AgentProps.Position, transform.position, typeof(Vector3));

        // Run behaviour tree on blackboard
        // Note each agent has their own blackboard, so we can run the same behaviour
        // tree for multiple agents if we wanted to
        behaviourTree.Run(blackboard);

        // Extract values from blackboard
        // This could be done on update by other MonoBehaviours that use the values
        // e.g. a UI or animation component
        happiness = blackboard.GetValue<int>((int) AgentProps.Happiness, typeof(int));
    }
}

// The properties that are defined for my agent and we can get and set on]
// our blackboard
enum AgentProps {
    Happiness,
    Target,
    Position,
}

class Blackboard {
    private Dictionary<Type, object> dicts = new();

    public Blackboard(LinkedList<Type> valueTypes)
    {
        while (valueTypes.Count > 0)
        {
            Type valueType = valueTypes.First.Value;
            valueTypes.RemoveFirst();
            Type dictToCreate = typeof(Dictionary<,>).MakeGenericType(new Type[] {typeof(int), valueType});
            dicts[valueType] = Activator.CreateInstance(dictToCreate);
        }
    }
    public TValue GetValue<TValue>(int key, Type valueType)
    {
        if (dicts == null)
            throw new Exception("Behaviour tree blackboard not initialised. Have you called InitialiseBlackboard?");
        if (dicts.ContainsKey(valueType))
        {
            var dict = dicts[valueType] as Dictionary<int, TValue>;
            if (dict.ContainsKey(key))
                return dict[key];
            return (TValue) valueType.Default();
        }
            
        else
            Debug.LogWarning("BT Blackboard does not contain dictionary for key type " + valueType.Name);
        return (TValue) valueType.Default();
    }

    public void SetValue<TValue>(int key, TValue value, Type valueType)
    {
        if (dicts == null)
            throw new Exception("Behaviour tree blackboard not initialised. Have you called InitialiseBlackboard?");
        if (!dicts.ContainsKey(valueType)) {
            Debug.LogWarning("BT Blackboard does not contain dictionary for key type " + valueType.Name);
            return;
        }
        var dict = dicts[valueType] as Dictionary<int, TValue>;
        if (dict != null)
            dict[key] = value;
    }
}

class BT {
    private static readonly bool DEBUG = true;

    public enum State {
        Success,
        Failure,
        Running,
    }
    private readonly Type type;

    public BT()
    {
        if (DEBUG)
            type = GetType();
    }

    public virtual void AddRequiredDictionaryTypes(LinkedList<Type> types) { }

    public virtual State Run(Blackboard blackboard) {
        if (DEBUG)
            Log();
        return State.Success;
    }

    protected virtual void Log() {
        Debug.Log("Running node: " +  type.Name);
    }

    public class Action : BT {
        public delegate State Callback();
        private Callback _action;
        public Action(Callback action) : base() {
            _action = action;
        }

        public override State Run(Blackboard blackboard) {
            base.Run(blackboard);
            return _action.Invoke();
        }
    }

    public class Test : BT {
        
        public delegate bool Callback(Blackboard blackboard);
        private Callback _test;
        public Test(Callback action) : base() {
            _test = action;
        }

        public override State Run(Blackboard blackboard) {
            base.Run(blackboard);
            if (_test.Invoke(blackboard))
                return State.Success;
            return State.Failure;
        }
    }

    public class Data<TValue> : BT {
        protected readonly int key;
        protected readonly Type valueType;

        public Data(int key) : base() {
            this.key = key;
            valueType = GetType().GenericTypeArguments[0];
        }

        public override void AddRequiredDictionaryTypes(LinkedList<Type> list) {
            list.AddLast(valueType);
        }

        public override State Run(Blackboard blackboard) { { return base.Run(blackboard); } }

        public class Set : Data<TValue> {
            private Callback _callback;
            public delegate TValue Callback(TValue current);

            public Set(int key, Callback callback) : base(key) {
                _callback = callback;
            }

            public override State Run(Blackboard blackboard) {
                base.Run(blackboard);
                TValue current = blackboard.GetValue<TValue>(key, valueType);
                blackboard.SetValue(key, _callback.Invoke(current), valueType);
                return State.Success;
            }
        }

        public class Assert : Data<TValue> {
            private Callback _callback;
            public delegate bool Callback(TValue value);

            public Assert(int key, Callback assertion) : base(key) {
                _callback = assertion;
            }

            public override State Run(Blackboard blackboard) {
                base.Run(blackboard);
                TValue result = blackboard.GetValue<TValue>(key, valueType);
                if (_callback.Invoke(result))
                    return State.Success;
                else
                    return State.Failure;
            }
        }
    }

    public abstract class Composite : BT {
        protected int RunningChild {get; set;}
        protected readonly BT[] children;

        public Composite(BT[] children) {
            this.children = children;
        }

        public override void AddRequiredDictionaryTypes(LinkedList<Type> list) {
            for (int i=0;i<children.Length;i++)
                children[i].AddRequiredDictionaryTypes(list);
        }

        public class Sequence : Composite {
            public Sequence(BT[] children) : base(children) {}

            public override State Run(Blackboard blackboard) {
                base.Run(blackboard);
                while (RunningChild < children.Length)
                {
                    State result = children[RunningChild].Run(blackboard);
                    if (result == State.Running)
                        return State.Running;
                    if (result == State.Failure)
                    {
                        RunningChild = 0;
                        return State.Failure;
                    }
                    RunningChild++;
                }
                RunningChild = 0;
                return State.Success;
            }
        }

        public class Selector: Composite {
            public Selector(BT[] children) : base(children) {}
            public override State Run(Blackboard blackboard) {
                base.Run(blackboard);
                while (RunningChild < children.Length)
                {
                    State result = children[RunningChild].Run(blackboard);
                    if (result == State.Running)
                        return State.Running;
                    if (result == State.Success)
                    {
                        RunningChild = 0;
                        return State.Success;
                    }
                    RunningChild++;
                }
                return State.Failure;
            }
        }
    }

    public abstract class Decorator : BT {
        protected BT child;

        public Decorator(BT child) {
            this.child = child;
        }

        public override void AddRequiredDictionaryTypes(LinkedList<Type> list) {
            child.AddRequiredDictionaryTypes(list);
        }

        public class Dynamic : Decorator {
            public Dynamic(BT child): base (child) {}
            public void Set(BT child) { this.child = child; }
        }

        public class Inverter : Decorator {
            public Inverter(BT child) : base(child) {}
            public override State Run(Blackboard blackboard) {
                base.Run(blackboard);
                State result = child.Run(blackboard);
                if (result == State.Success)
                    return State.Failure;
                if (result == State.Failure)
                    return State.Success;
                return State.Running;
            }
        }

        public class Succeeder : Decorator {
            public Succeeder(BT child) : base(child) {}
            public override State Run(Blackboard blackboard) {
                base.Run(blackboard);
                State result = child.Run(blackboard);
                if (result == State.Running)
                    return State.Running;
                return State.Success;
            }
        }

        public class Repeater : Decorator {
            public Repeater(BT child) : base(child) {}
            public override State Run(Blackboard blackboard) {
                base.Run(blackboard);
                child.Run(blackboard);
                return State.Running;
            }
        }

        public class RepeatUntilSuccess : Decorator {
            public RepeatUntilSuccess(BT child) : base(child) {}

            public override State Run(Blackboard blackboard) {
                base.Run(blackboard);
                State result = child.Run(blackboard);
                if (result == State.Success)
                    return State.Success;
                return State.Running;
            }
        }

        public class RepeatN : Decorator {
            private int _count;
            private readonly int _limit;

            public RepeatN(BT child, int limit) : base(child) {
                _limit = limit;
            }

            public override State Run(Blackboard blackboard) {
                base.Run(blackboard);
                State result = child.Run(blackboard);
                if (result != State.Running)
                    _count++;
                if (_count >= _limit) {
                    _count = 0;
                    return result;
                }
                return State.Running;
            }
        }
    }
}
