using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public int Border = 10;
    public int Width = 150;
    public int Height = 20;

    [SerializeField] private List<Rigidbody> _dices;
    [SerializeField] private GameObject _diceSpawner;
    [SerializeField] private GameObject _dicePrefab;
    private List<RiggedDice> _riggedDice;

    private bool _isRolling;

    private bool _replaying;

    private void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            AddDice();
        }

        Setup();
    }

    private void FixedUpdate()
    {
        if (!_replaying) return;
        bool diceDone = true;

        foreach (var dice in _riggedDice.Where(dice => dice.HasPhysicStepToPlay()))
        {
            diceDone = false;
            dice.PhysicsStep();
        }

        if (!diceDone) return;

        _replaying = false;
        Physics.autoSimulation = true;
        _isRolling = false;

        foreach (var dice in _dices)
        {
            dice.isKinematic = false;
            dice.detectCollisions = true;
        }
    }

    private void Setup()
    {
        _riggedDice = new List<RiggedDice>();

        foreach (var dice in _dices)
        {
            _riggedDice.Add(dice.GetComponent<RiggedDice>());
        }
    }

    private void Roll()
    {
        if (_isRolling)
        {
            return;
        }

        _isRolling = true;

        foreach (var dice in _riggedDice)
        {
            dice.Reset();
        }

        foreach (var dice in _dices)
        {
            var thrust = UnityEngine.Random.Range(24, 40);

            var forceRange = 10;
            var direction = transform.up;
            var minorXRotation = UnityEngine.Random.Range(-forceRange, forceRange);
            var minorYRotation = UnityEngine.Random.Range(-forceRange, forceRange);
            var minorZRotation = UnityEngine.Random.Range(-forceRange, forceRange);

            direction = Quaternion.Euler(minorXRotation, minorYRotation, minorZRotation) * direction;

            dice.AddForce(direction * thrust, ForceMode.Impulse);

            var spinForce = UnityEngine.Random.Range(8, 16);
            var spinVector = UnityEngine.Random.onUnitSphere;

            dice.AddTorque(spinVector * spinForce, ForceMode.Impulse);
        }

        FastForward();
    }

    private void FastForward()
    {
        Physics.autoSimulation = false;

        for (int i = 0; i < _riggedDice.Count; i++)
        {
            _riggedDice[i].OriginalPosition = _dices[i].transform.position;
            _riggedDice[i].OriginalRotation = _dices[i].transform.rotation;
        }

        bool fastfowarding = true;
        while (fastfowarding)
        {
            fastfowarding = false;

            Physics.Simulate(Time.fixedDeltaTime);

            foreach (var dice in _riggedDice)
            {
                dice.RecordStep();
            }

            foreach (var dice in _riggedDice)
            {
                fastfowarding |= dice.IsRolling();
            }
        }

        foreach (var dice in _dices)
        {
            dice.isKinematic = true;
            dice.detectCollisions = false;
        }

        foreach (var dice in _riggedDice)
        {
            dice.RotationOffset = dice.GetComponent<RiggedRotation>().GetRotationForValue(dice.DesiredRoll);
        }

        for (int i = 0; i < _dices.Count; i++)
        {
            _dices[i].transform.position = _riggedDice[i].OriginalPosition;
            _dices[i].transform.rotation = _riggedDice[i].OriginalRotation;
        }

        _replaying = true;
    }

    void OnGUI()
    {
        Rect boundary = GetWidgetBoundary(4 + _riggedDice.Count);
        GUI.Box(boundary, "Rigged Roll");

        int index = 1;
        AddButton(boundary, index++, "Roll", () => Roll());

        index++;
        for (int i = 0; i < _riggedDice.Count; i++)
        {
            AddButton(boundary, index++, "Dice " + (i + 1) + " - " + _riggedDice[i].DesiredRoll,
                () => AlterDiceValue(i));
        }
    }

    private void AddDice()
    {
        var dice = Instantiate(_dicePrefab,
            _diceSpawner.transform.position + UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(0, 10),
            UnityEngine.Random.rotationUniform);

        _dices.Add(dice.GetComponent<Rigidbody>());

        Setup();
    }

    private void AlterDiceValue(int index)
    {
        int currentDesiredRoll = (int)_riggedDice[index].DesiredRoll;
        if (currentDesiredRoll == 5)
        {
            currentDesiredRoll = 0;
        }
        else
        {
            currentDesiredRoll++;
        }

        _riggedDice[index].DesiredRoll = (DiceValueEnum)currentDesiredRoll;
    }

    private Rect GetWidgetBoundary(int num_buttons)
    {
        return new Rect(Border / 2, Border / 2, Width + Border,
            num_buttons * (Border + Height) + Height + (Border / 2));
    }

    private void AddButton(Rect boundary, int i, string s, Action action)
    {
        Rect r = new Rect(Border, boundary.y + (Border * i + Height * i), Width, Height);
        if (GUI.Button(r, s))
        {
            if (action != null)
                action();
        }
    }
}