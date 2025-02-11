using System;
using System.Collections.Generic;

namespace LogicLink;

public class SelectedParts
{
    public bool NoneSelected => !Head && !Trigger1 && !Trigger2;
    public bool AllSelected => Head && Trigger1 && (Trigger2 || !UseTwoInputs);

    bool _head;
    bool _trigger1;
    bool _trigger2;

    public bool Head
    {
        get { return _head; }
        set { _head = value; }
    }
    public bool Trigger1
    {
        get { return _trigger1; }
        set { _trigger1 = value; }
    }
    public bool Trigger2
    {
        get { return _trigger2 && UseTwoInputs; }
        set { _trigger2 = value; }
    }

    public bool UseTwoInputs;

    public SelectedParts(bool useTwoInputs)
    {
        UseTwoInputs = useTwoInputs;
    }
}
