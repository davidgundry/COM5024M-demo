using System;
using System.Collections;
using System.Data;
using UnityEngine;
using UnityEngine.AI;

enum CurveType {
    Quadratic,
    Exponential,
    Logistic,
    Logit,
}

enum InputFactor {
    Ammo,
    Health,
    Age,
    Happiness
}

struct Curve {
    readonly public CurveType type;
    public readonly float m, k ,b, d;
}

struct DecisionInput {
    public readonly InputFactor input;
    public readonly Curve curve;
}

class DecisionFactor {
    private readonly DecisionInput[] _inputs;

    public DecisionFactor(DecisionInput[] inputs)
    {
        _inputs = inputs;
    }

    public float Calculate()
    {
        float product = 0;
        for(int i=0;i<_inputs.Length;i++)
            product *= ProcessInput(_inputs[i]);
        return product;
    }
    
    float ProcessInput(DecisionInput input)
    {
        return Curve(input.curve, FindInput(input.input));
    }

    float Curve(Curve curve, float input)
    {
        //TODO: implement each curve function transforming input
        return input;
    }
    float FindInput(InputFactor input)
    {
        //TODO: implement how we get our original input values.
        return 0;
    }
}

enum ActionType {
    Idle,
    Run,
    Jump,
    Shoot,
    Eat
}

class Action
{
    public readonly ActionType action;
    private readonly DecisionFactor[] _considerations;
    private readonly float _weight;
    public float Utility { 
        get { 
            float product = 0;
            for(int i=0;i<_considerations.Length;i++)
                product *= _considerations[i].Calculate();
            return product * _weight;
        }
    }

    public Action(DecisionFactor[] considerations, float weight)
    {
        _considerations = considerations;
        _weight = weight;
    }
}

class UtilityAIAgent
{
    private Action[] _actions;

    public UtilityAIAgent(Action[] actions)
    {
        _actions = actions;
    }

    public ActionType PickAction()
    {
        Array.Sort(_actions, (a, b) => (int)((a.Utility - b.Utility)*100));
        //TODO: pick one of top options by weighted random
        return _actions[0].action;
    } 
}

public class UtilityAI : MonoBehaviour
{
    NavMeshAgent nma;
    UtilityAIAgent uai;

    // Start is called before the first frame update
    void Start()
    {
        nma = GetComponent<NavMeshAgent>();        
        uai = new UtilityAIAgent(new Action[0]);
    }

    // Update is called once per frame
    void Update()
    {
        ActionType actionToPerform = uai.PickAction();
        Act(actionToPerform);
    }
    
    void Act(ActionType actionToPerform)
    {
        // TODO: act based on actionToPerform
    }
}
