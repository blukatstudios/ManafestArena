/*
  An interface for bodies used by an actor.
*/
using Godot;
using System;
using System.Collections.Generic;

public interface IBody {
  Actor GetActor();
  List<Node> GetHands();
  void HoldItem(int hand, IItem item);
  void ReleaseItem(int hand, IItem item);
  Node GetNode();
  void InitCam(int index);
  void Move(Vector3 movement, float moveDelta, bool ignoreAnimation = true, bool sprint = false);
  void Turn(Vector3 direction, float delta);
  void Jump();
  Speaker GetSpeaker();
  MeshInstance GetMesh();
  void Update(float delta);
  void Die();
  bool IsDead();
  List<Actor> ActorsInSight();
  Vector3 LookingDegrees(); // In case this is not as simple as spatial.GetRotationDegrees()
  void AnimationTrigger(string triggerName); // Reload, swing, crouch, etc, etc
  void ToggleCrouch();
}