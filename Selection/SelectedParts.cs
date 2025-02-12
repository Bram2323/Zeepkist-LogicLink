using System;

namespace LogicLink.Selection;

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

    public string ToUidString()
    {
        string head = _head ? "1" : "0";
        string trigger1 = _trigger1 ? "1" : "0";
        string trigger2 = _trigger2 ? "1" : "0";
        string useTwoInputs = UseTwoInputs ? "1" : "0";
        return $"{head}{trigger1}{trigger2}{useTwoInputs}";
    }

    public static SelectedParts FromUidString(string uid)
    {
        if (uid.Length != 4)
        {
            throw new ArgumentException($"Tried to parse invalid string to SelectedParts! String: {uid}");
        }

        bool head = uid[0] == '1';
        bool trigger1 = uid[1] == '1';
        bool trigger2 = uid[2] == '1';
        bool useTwoInputs = uid[3] == '1';

        SelectedParts selectedParts = new(useTwoInputs)
        {
            Head = head,
            Trigger1 = trigger1,
            Trigger2 = trigger2
        };

        return selectedParts;
    }
}
