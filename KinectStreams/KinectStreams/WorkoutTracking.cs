using Bitseps;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Fitnematic.Core.BenchPressTest
{
    public enum SkeletonBoneState
    {
        Matched,
        Unmatched,
        Expected
    }

    public enum TrackCompareState
    {
        Start,
        OnTrack,
        OutTrack,
        End,
        Warning
    }

    public class SkeletonFrameValidationData
    {
        public TrackCompareState TrackState { get; set; }

        public Dictionary<JointType, SkeletonBoneState> BoneStates { get; set; }

        public string Status { get; set; }

        public SkeletonFrameValidationData()
        {

        }
    }

    public class WorkoutTracking
    {
        /*This algorithm can work in two modes:
          - mode 1: using angles and distances - the result is calculated after a change of direction
          - mode 2: using only angles - the result is calculated during the current movement, but could be more inacurate in compare to mode 1.*/

        /*Thresholds and algorithm dependencies*/
        double angleDiff = 16; //22; //35; // for merging two consecutive vectors
        int MOVEMENT_VECTOR_MIN_DISTANCE_IN_PIX = 4;
        bool USE_DISTANCE_APPROXIMATION = false;

        int _workingMode = 1;
        int _skippedFramesForMode2 = 11;
        double _angleThreshold = 16; // 35;
        double _distanceThreshold = 70; //40; // 70;
        int _calculatedFramesSinceStart = 0;
        int _numberOfAngleDetectionZones = 16; //10 //16; // // 8;
        const int numberOfFramesForDirCalculation =  9;

        List<JointType> jointsOfInterest;
        Dictionary<JointType, int> jointDirs;
        Dictionary<JointType, Point3D>[] lastPositions;
        TrackCompareState prevState = TrackCompareState.OutTrack;
        Dictionary<JointType, MovementVector> jointMovementVectors;

        /*Constructor*/
        public WorkoutTracking(List<JointType> pointsOfInterest, int workingMode = 1)
        {
            _workingMode = workingMode;
            //_angleThreshold = angleThresh;
            //_distanceThreshold = distanceThresh;
            jointsOfInterest = new List<JointType>(pointsOfInterest);

            lastPositions = new Dictionary<JointType, Point3D>[numberOfFramesForDirCalculation];
            jointDirs = new Dictionary<JointType, int>();
            jointMovementVectors = new Dictionary<JointType, MovementVector>();

            foreach (var joint in jointsOfInterest)
            {
                jointDirs.Add(joint, 0);

                var movementVectors = new MovementVector();

                jointMovementVectors.Add(joint, movementVectors);
            }

            for (int i = 0; i < numberOfFramesForDirCalculation; i++)
            {
                var jointDict = new Dictionary<JointType, Point3D>();
                foreach (var joint in jointsOfInterest)
                {
                    jointDict.Add(joint, new Point3D());
                }

                lastPositions[i] = jointDict;
            }
        }

        public SkeletonFrameValidationData ValidateCurrentSkeletonFrame(Dictionary<JointType, List<MovementVector>> serverWorkoutVectors, BodyStructure body)
        {
            var result = new SkeletonFrameValidationData();

            foreach (var joint in body.currentFrameBody)
            {
                if (jointsOfInterest.Contains(joint.Key))
                {
                    lastPositions[0][joint.Key] = joint.Value;
                }
            }
            var jointDirChange = IsDirChanged();

            UpdateMovementVectors(jointDirChange);

            result.BoneStates = new Dictionary<JointType, SkeletonBoneState>();

            CalculateVectors(serverWorkoutVectors, body, result);

            return result;
        }

        Dictionary<JointType, bool> IsDirChanged()
        {
            Dictionary<JointType, bool> isDirChanged = new Dictionary<JointType, bool>();

            if (_calculatedFramesSinceStart > numberOfFramesForDirCalculation)
            {
                foreach (var joint in jointsOfInterest)
                {
                    bool curJointDirChange = false;

                    double angle = Math3DHelper.AngleBetweenTwoPointsInDegrees(lastPositions[0][joint], lastPositions[numberOfFramesForDirCalculation - 1][joint]);
                    //Batev: var lastPoint = new Point3D();
                    //Batev: double angle = Math3DHelper.AngleBetweenTwoPointsInDegrees(lastPoint, lastPositions[0][joint]);

                    int curDir = (int)(angle / (360 / _numberOfAngleDetectionZones));

                    // !!!!!!!!!!!!!!!!!!!!!

                    /*TODO: complicated logic about angle zones and their overlaping*/
                    if (curDir != jointDirs[joint])// - 1 && Math.Abs(curDir - jointDirs[joint]) < numberOfAngleDetectionZones - 1)
                    {
                        curJointDirChange = true;
                        jointDirs[joint] = curDir;
                    }

                    isDirChanged.Add(joint, curJointDirChange);
                }
            }
            ShiftRegister();

            return isDirChanged;
        }

        private void ShiftRegister()
        {
            foreach (var joint in jointsOfInterest)
            {
                for (int i = numberOfFramesForDirCalculation - 1; i > 0; i--)
                {
                    lastPositions[i][joint] = lastPositions[i - 1][joint];
                }
            }
        }

        private void UpdateMovementVectors(Dictionary<JointType, bool> jointDirChange)
        {
            foreach (var change in jointDirChange)
            {
                if (_workingMode == 1)
                {
                    if (change.Value == true) // if a dir is changed
                    {
                        var joint = change.Key;
                        //int currentNumberOfVectors = jointMovementVectors[joint].Count();
                        var lastCoordinates = jointMovementVectors[joint]._endPoint;

                        var newVector = new MovementVector(new Point3D(lastCoordinates.X, lastCoordinates.Y, lastCoordinates.Z), lastPositions[0][joint]);

                        bool CheckIfNewVectorIsNeeded = checkNewVector(newVector, joint);
                        if (CheckIfNewVectorIsNeeded == true)
                        {
                            jointMovementVectors[joint] = newVector;
                        }
                        else
                        {
                             var lastPoint = jointMovementVectors[joint];
                            //Batev: var lastPoint = new Point3D();

                            jointMovementVectors[joint]._endPoint = lastPositions[0][joint];
                            var angle = Math3DHelper.AngleBetweenTwoPointsInDegrees(lastPositions[0][joint], lastPoint._startPoint);
                            //Batev: var angle = Math3DHelper.AngleBetweenTwoPointsInDegrees(lastPoint, lastPositions[0][joint]);

                            if (angle < 0)
                            {
                                angle = 360 + angle;
                            }
                            jointMovementVectors[joint]._angle = angle;
                            jointMovementVectors[joint]._distance = Math3DHelper.DistanceBetwenTwoPoints(lastPoint._startPoint, lastPositions[0][joint]);
                            //Batev: jointMovementVectors[joint]._distance = Math3DHelper.DistanceBetwenTwoPoints(lastPoint, lastPositions[0][joint]);
                        }
                    }
                }
                // for working mode 2 - we replace the vector every time. We have to skip several frames in order to have better angle calculation
                else if (_workingMode == 2)
                {
                    if (_calculatedFramesSinceStart % _skippedFramesForMode2 == 0)
                    {
                        var joint = change.Key;
                        var lastCoordinates = jointMovementVectors[joint]._endPoint;
                        var newVector = new MovementVector(new Point3D(lastCoordinates.X, lastCoordinates.Y, lastCoordinates.Z), lastPositions[0][joint]);
                        jointMovementVectors[joint] = newVector;
                    }
                }
            }
        }

        private bool checkNewVector(MovementVector newVector, JointType joint)
        {
            bool isNeeded = true;

            if (newVector._distance < MOVEMENT_VECTOR_MIN_DISTANCE_IN_PIX)
            {
                isNeeded = false;
            }
            else
            {
                //int currentNumberOfVectors = jointMovementVectors[joint].Count();
                if (Math.Abs(jointMovementVectors[joint]._angle - newVector._angle) < angleDiff)
                {
                    isNeeded = false;
                }
            }

            return isNeeded;
        }

        public void CalculateVectors(Dictionary<JointType, List<MovementVector>> serverPoints, BodyStructure body, SkeletonFrameValidationData result)
        {
            //We take the last calculated vector for each joint and compare it with the serverRecorded ones

            foreach (var joint in jointsOfInterest)
            {
                bool isFoundInServerVectors = false;

                var movementVectorLast = jointMovementVectors[joint];

                foreach (var serverVector in serverPoints[joint])
                {
                    switch (_workingMode)
                    {
                        case 1:
                            /*since the difference in pixels is almost linear to the distance to the sensor - we can use the following approximation for distance*/
                            if (USE_DISTANCE_APPROXIMATION)
                            {
                                _distanceThreshold = 3 * ((movementVectorLast._startPoint.Z + movementVectorLast._endPoint.Z) / 2 - (serverVector._startPoint.Z + serverVector._endPoint.Z) / 2);
                            }

                            if (Math.Abs(serverVector._angle - movementVectorLast._angle) < _angleThreshold)
                            {
                                if (Math.Abs(serverVector._distance - movementVectorLast._distance) < _distanceThreshold)
                                {
                                    /*if we have a match for both angle and distance*/
                                    isFoundInServerVectors = true;
                                    break;
                                }
                            }
                            break;
                        case 2:
                            if (Math.Abs(serverVector._angle - movementVectorLast._angle) < _angleThreshold)
                            {
                                /*if we have a match for both angle*/
                                isFoundInServerVectors = true;
                            }
                            break;
                    }
                    if (isFoundInServerVectors == true) break;
                }

                if (isFoundInServerVectors)
                {
                    result.BoneStates.Add(joint, SkeletonBoneState.Matched);
                }
                else
                {
                    result.BoneStates.Add(joint, SkeletonBoneState.Unmatched);
                }
            }

            _calculatedFramesSinceStart++;
        }
    }
}