#!/bin/sh

EDITOR_BASE="/Applications/Unity/Hub/Editor"

if [ ! -d Documentation ]; then
  mkdir Documentation
  LATEST_EDITOR=`ls $EDITOR_BASE | sort -V | tail -n 1`
  cp -r $EDITOR_BASE/$LATEST_EDITOR/Documentation/en/ScriptReference/LowLevelPhysics* Documentation
fi

if [ ! -d LowLevelSamples ]; then
    git clone git@github.com:Unity-Technologies/PhysicsExamples2D.git
    mv PhysicsExamples2D/PhysicsCore2D LowLevelSamples
    rm -rf PhysicsExamples2D
fi
