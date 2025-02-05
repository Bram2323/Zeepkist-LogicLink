using System;
using System.Collections.Generic;

namespace LogicLink;

public class SelectedParts
{
    public bool NoneSelected => !Head && !Trigger1 && !Trigger2;

    public bool Head;
    public bool Trigger1;
    public bool Trigger2;
}
