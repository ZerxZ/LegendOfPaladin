using Godot.Collections;

namespace 勇者传说.classes;

public interface IDataSave
{
    Dictionary ToDictionary();
    void FromDictionary(Dictionary dictionary);
}