#if TOOLS
namespace queen.behaviour_tree;

using System.Collections.Generic;
using Godot;

[Tool]
public static class BehaviourTreePluginModule
{

    private struct TypeValues
    {
        public string TypeName = "";
        public string BaseNode = "Node";
        public string ScriptPath = "";
        public string IconPath = "";

        public TypeValues(){}
    }
    const string ROOT = "res://addons/squiggle_zone/modules/behaviour_tree/";

    private static readonly List<TypeValues> _registered_types = new();
    public static void RegisterTypes(EditorPlugin ep)
    {
        Add(ep, new TypeValues(){       // BehaviourTree.cs
            TypeName="BehaviourTree",
            ScriptPath="BehaviourTree.cs",
            IconPath="icons/bt-root.svg"
        });

        // Composition
        Add(ep, new TypeValues(){       // BTSelect.cs
            TypeName="BTSelect",
            ScriptPath="composition/BTSelect.cs",
            IconPath="icons/bt-select.svg"
        });
        Add(ep, new TypeValues(){       // BTSequence.cs
            TypeName="BTSequence",
            ScriptPath="composition/BTSequence.cs",
            IconPath="icons/bt-sequence.svg"
        });
        Add(ep, new TypeValues(){       // BTSelectStar.cs
            TypeName="BTSelectStar",
            ScriptPath="composition/BTSelectStar.cs",
            IconPath="icons/bt-select-star.svg"
        });
        Add(ep, new TypeValues(){       // BTSequenceStar.cs
            TypeName="BTSequenceStar",
            ScriptPath="composition/BTSequenceStar.cs",
            IconPath="icons/bt-sequence-star.svg"
        });

        // Decoration
        Add(ep, new TypeValues(){       // BTFailer.cs
            TypeName="BTFailer",
            ScriptPath="decoration/BTFailer.cs",
            IconPath="icons/bt-decorator.svg"
        });
        Add(ep, new TypeValues(){       // BTInverter.cs
            TypeName="BTInverter",
            ScriptPath="decoration/BTInverter.cs",
            IconPath="icons/bt-decorator.svg"
        });
        Add(ep, new TypeValues(){       // BTLimiter.cs
            TypeName="BTLimiter",
            ScriptPath="decoration/BTLimiter.cs",
            IconPath="icons/bt-decorator.svg"
        });
        Add(ep, new TypeValues(){       // BTSucceeder.cs
            TypeName="BTSucceeder",
            ScriptPath="decoration/BTSucceeder.cs",
            IconPath="icons/bt-decorator.svg"
        });

        // Leaves - Actions
        var actions = ActionsModule.GetRegisterTypes();
        foreach(var a in actions)
        {
            Add(ep, new TypeValues(){       // BTAction.cs
                TypeName=a.GetFile(),
                ScriptPath=$"leaves/action/{a}",
                IconPath="icons/bt-action.svg"
            });
        }

        // Leaves - Conditions
        var conditions = ConditionsModule.GetRegisterTypes();
        foreach(var c in conditions)
        {
            Add(ep, new TypeValues(){       // BTAction.cs
                TypeName=c.GetFile(),
                ScriptPath=$"leaves/condition/{c}",
                IconPath="icons/bt-query.svg"
            });
        }
        // res://addons/squiggle_zone/modules/behaviour_tree/leaves/condition/BTCondition.cs

    }

    public static void UnregisterTypes(EditorPlugin ep)
    {
        foreach (var type in _registered_types)
            Remove(ep, type);
    }

    private static void Add(EditorPlugin ep, TypeValues values)
    {
        var script = GD.Load<Script>($"{ROOT}{values.ScriptPath}");
        Texture2D icon = null;
        if(values.IconPath != "") icon = GD.Load<Texture2D>($"{ROOT}{values.IconPath}");
        ep.AddCustomType(values.TypeName, values.BaseNode, script, icon);
        _registered_types.Add(values);
    } 

    private static void Remove(EditorPlugin ep, TypeValues values)
    {
        ep.RemoveCustomType(values.TypeName);
    }



    

}
#endif