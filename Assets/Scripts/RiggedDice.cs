using System.Collections.Generic;
using UnityEngine;

public class RiggedDice : MonoBehaviour
{
    private Rigidbody _rigidBody;
    
    private List<Vector3> _positions = new ();
    private List<Quaternion> _rotations = new ();

    public Quaternion RotationOffset = Quaternion.identity;
    public Quaternion OriginalRotation;
    public Vector3 OriginalPosition;
    public DiceValueEnum DesiredRoll;

    private int _stepIndex;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        InitializeOriginalValues();
    }

    private void InitializeOriginalValues()
    {
        OriginalPosition = transform.position;
        OriginalRotation = transform.rotation;
    }

    public void Reset()
    {
        _stepIndex = 0;
        RotationOffset = Quaternion.identity;
        _positions.Clear();
        _rotations.Clear();
        InitializeOriginalValues();
    }

    public void RecordStep()
    {
        _positions.Add(transform.position);
        _rotations.Add(transform.rotation);
    }

    public bool IsRolling()
    {
        return !IsVelocityZero() || !IsAngularVelocityZero();
    }

    private bool IsVelocityZero()
    {
        return Mathf.Approximately(_rigidBody.velocity.sqrMagnitude, 0);
    }

    private bool IsAngularVelocityZero()
    {
        return Mathf.Approximately(_rigidBody.angularVelocity.sqrMagnitude, 0);
    }

    public void PhysicsStep()
    {
        if (!HasPhysicStepToPlay()) return;
        
        transform.position = _positions[_stepIndex];
        transform.rotation = _rotations[_stepIndex] * RotationOffset;
        _stepIndex++;
    }

    public bool HasPhysicStepToPlay()
    {
        return _stepIndex < _positions.Count;
    }
}