using Meta.XR.BuildingBlocks;
using Meta.XR.MultiplayerBlocks.Shared;
using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTransferOwnershipOnSelect : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the game's gravity simulation influences this networked game object.
    /// </summary>
    /// <remarks>Must be set before the game object <c>Awake()</c> method is called.</remarks>
    public bool UseGravity;
    private Grabbable _grabbable;
    private Rigidbody _rigidbody;
    private ITransferOwnership _transferOwnership;

    private void Awake()
    {
        _grabbable = GetComponentInChildren<Grabbable>();

        if (_grabbable == null)
        {
            throw new InvalidOperationException("Object requires a Grabbable component");
        }

        _grabbable.WhenPointerEventRaised += OnPointerEventRaised;

        _transferOwnership = this.GetInterfaceComponent<ITransferOwnership>();
        if (_transferOwnership == null)
        {
            throw new InvalidOperationException("Object requires an ITransferOwnership component");
        }

        if (!UseGravity)
        {
            return;
        }
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            throw new InvalidOperationException("Object requires a Rigidbody component when useGravity enabled");
        }
    }

    private void OnDestroy()
    {
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised -= OnPointerEventRaised;
        }
    }

    private void OnPointerEventRaised(PointerEvent pointerEvent)
    {
        if (_grabbable == null || pointerEvent.Type != PointerEventType.Select)
        {
            return;
        }

        if (_grabbable.SelectingPointsCount == 1)
        {
            if (!_transferOwnership.HasOwnership())
            {
                _transferOwnership.TransferOwnershipToLocalPlayer();
            }
        }
    }

    private void LateUpdate()
    {
        if (_transferOwnership.HasOwnership() && UseGravity)
        {
            // When network objects transferring ownership during interactions from ISDK, we need to guarantee a proper
            // kinematic state. We recommend developers to use RigidbodyKinematicLocker for other custom isKinematic controls.
            _rigidbody.isKinematic = _rigidbody.IsLocked();
        }
    }
}
