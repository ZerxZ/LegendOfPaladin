using Godot;
using Godot.Collections;

namespace 勇者传说.classes;

public interface IDataSave
{
    Dictionary<string,Variant> ToDictionary();
    void FromDictionary(Dictionary<string,Variant> dictionary);
}