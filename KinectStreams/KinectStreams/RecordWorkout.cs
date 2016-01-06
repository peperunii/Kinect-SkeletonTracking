using Bitseps;
using Microsoft.Kinect;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;

namespace KinectStreams
{
    /*This class will record any movement - if provided a list of desired points to follow. The movement vector will be calculated after the exercise is performed twise.*/
    public class RecordMovement
    {
        /*Set threshold for movement difference detection*/
        double angleDiff = 16; //35; // for merging two vectors
        //double distanceDiff = 70;
        int numberOfAngleDetectionZones = 16; //10;//16; // // 8;
        int MOVEMENT_VECTOR_MIN_DISTANCE_IN_PIX = 10;

        int curNumberOfFrame = 0;
        int numberOfFramesForMovementDetection = 9;

        List<JointType> jointsOfInterest;

        Dictionary<JointType, int> jointDirs;
        Dictionary<JointType, Point3D>[] lastPositions;
        Dictionary<JointType, List<MovementVector>> jointMovementVectors;

        //bool escape = false;
        /*Constructor*/
        public RecordMovement(List<JointType> jointsOfInterestToFollow, int numberOfFramesForDetection)
        {
            jointsOfInterest = new List<JointType>(jointsOfInterestToFollow);
            numberOfFramesForMovementDetection = numberOfFramesForDetection;
            lastPositions = new Dictionary<JointType, Point3D>[numberOfFramesForDetection];
            jointDirs = new Dictionary<JointType, int>();
            jointMovementVectors = new Dictionary<JointType, List<MovementVector>>();

            foreach (var joint in jointsOfInterest)
            {
                jointDirs.Add(joint, 0);

                var listMovementVectors = new List<MovementVector>();
                listMovementVectors.Add(new MovementVector());

                jointMovementVectors.Add(joint, listMovementVectors);
            }

            for (int i = 0; i < numberOfFramesForDetection; i++)
            {
                var jointDict = new Dictionary<JointType, Point3D>();
                foreach (var joint in jointsOfInterest)
                {
                    jointDict.Add(joint, new Point3D());
                }

                lastPositions[i] = jointDict;
            }
        }

        /*This function will return the movement vector if the exercise is detected twise. Otherwise - empty dictionary*/
        public Dictionary<JointType, List<MovementVector>> GetMovementVectors(BodyStructure body)
        {

            foreach (var joint in body.currentFrameBody)
            {
                if (jointsOfInterest.Contains(joint.Key))
                {
                    lastPositions[0][joint.Key] = joint.Value;
                }
            }

            var jointDirChange = IsDirChanged();

            UpdateMovementVectors(jointDirChange);

            curNumberOfFrame++;
            return this.jointMovementVectors;//movementVector;
           
        }

        private void UpdateMovementVectors(Dictionary<JointType, bool> jointDirChange)
        {
            foreach (var change in jointDirChange)
            {
                if (change.Value == true) // if a dir is changed
                {
                    var joint = change.Key;
                    int currentNumberOfVectors = jointMovementVectors[joint].Count;
                    var lastCoordinates = jointMovementVectors[joint][currentNumberOfVectors - 1]._endPoint;

                    var newVector = new MovementVector(new Point3D(lastCoordinates.X, lastCoordinates.Y, lastCoordinates.Z), lastPositions[0][joint]);


                    bool CheckIfNewVectorIsNeeded = checkNewVector(newVector, joint);
                    if (CheckIfNewVectorIsNeeded == true)
                    {
                        jointMovementVectors[joint].Add(newVector);
                        
                  }
                    else
                    {
                        var lastPoint = jointMovementVectors[joint][currentNumberOfVectors - 1];
                        //Batev: var lastPoint = new Point3D();
                        jointMovementVectors[joint][currentNumberOfVectors - 1]._endPoint = lastPositions[0][joint];
                         var angle = Math3DHelper.AngleBetweenTwoPointsInDegrees(lastPoint._startPoint, lastPositions[0][joint]);
                        //Batev: var angle = Math3DHelper.AngleBetweenTwoPointsInDegrees(lastPoint, lastPositions[0][joint]);
                        jointMovementVectors[joint][currentNumberOfVectors - 1]._angle = angle;
                         jointMovementVectors[joint][currentNumberOfVectors - 1]._distance = Math3DHelper.DistanceBetwenTwoPoints(lastPoint._startPoint, lastPositions[0][joint]);
                        //Batev:  jointMovementVectors[joint][currentNumberOfVectors - 1]._distance = Math3DHelper.DistanceBetwenTwoPoints(lastPoint, lastPositions[0][joint]);
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
                int currentNumberOfVectors = jointMovementVectors[joint].Count;
                if (Math.Abs(jointMovementVectors[joint][currentNumberOfVectors - 1]._angle - newVector._angle) < angleDiff)
                {
                    isNeeded = false;
                }
            }

            return isNeeded;
        }

        Dictionary<JointType, bool> IsDirChanged()
        {
            Dictionary<JointType, bool> isDirChanged = new Dictionary<JointType, bool>();

            if (curNumberOfFrame > numberOfFramesForMovementDetection)
            {
                foreach (var joint in jointsOfInterest)
                {
                    bool curJointDirChange = false;

                    // TODO swift
                    double angle = Math3DHelper.AngleBetweenTwoPointsInDegrees(lastPositions[numberOfFramesForMovementDetection - 1][joint], lastPositions[0][joint]);
                    //Batev: var lastPoint = new Point3D();
                    //Batev: double angle = Math3DHelper.AngleBetweenTwoPointsInDegrees(lastPoint, lastPositions[0][joint]);


                    int curDir = (int)(angle / (360 / numberOfAngleDetectionZones));
                    //Console.WriteLine("{0}", curDir);


                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
                for (int i = numberOfFramesForMovementDetection - 1; i > 0; i--)
                {
                    lastPositions[i][joint] = lastPositions[i - 1][joint];
                }
            }
        }
    }
}
