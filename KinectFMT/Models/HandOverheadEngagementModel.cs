using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.Toolkit.Input;

namespace KinectFMT.Models
{
    //NOT NEED NOW
    public class HandOverheadEngagementModel : IKinectEngagementManager
    {
        bool stopped = true;
        BodyFrameReader bodyReader;
        Body[] bodies;
        bool engagementPeopleHaveChanged;
        List<BodyHandPair> handsToEngage;
        private int engagedPeopleAllowed;

        public HandOverheadEngagementModel(int engagedPeopleAllowed)
        {
            EngagedPeopleAllowed = engagedPeopleAllowed;
            var sensor = KinectSensor.GetDefault();
            bodyReader = sensor.BodyFrameSource.OpenReader();
            bodyReader.FrameArrived += BodyReader_FrameArrived;
            sensor.Open();
            bodies = new Body[bodyReader.BodyFrameSource.BodyCount];
            handsToEngage = new List<BodyHandPair>();
        }

        public int EngagedPeopleAllowed
        {
            get
            {
                return engagedPeopleAllowed;
            }
            set
            {
                if (value > 2 || value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", value, "This engagement manager requires 0 to 2 people to be set as the EngagedPeopleAllowed");
                }

                engagedPeopleAllowed = value;
            }
        }

        public bool EngagedBodyHandPairsChanged()
        {
            return engagementPeopleHaveChanged;
        }

        public IReadOnlyList<BodyHandPair> KinectManualEngagedHands
        {
            get
            {
                return KinectCoreWindow.KinectManualEngagedHands;
            }
        }

        public void StartManaging()
        {
            stopped = false;
            bodyReader.IsPaused = false;
        }

        public void StopManaging()
        {
            stopped = true;
            bodyReader.IsPaused = true;
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs args)
        {
            bool gotData = false;

            using (var frame = args.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(bodies);
                    gotData = true;
                }
            }

            if (gotData && !stopped)
            {
                TrackEngagedPlayersViaHandOverHead();
            }
        }

        private static bool IsHandOverhead(JointType jointType, Body body)
        {
            return (body.Joints[jointType].Position.Y >
                    body.Joints[JointType.Head].Position.Y);
        }

        private static bool IsHandBelowHip(JointType jointType, Body body)
        {
            return (body.Joints[jointType].Position.Y <
                    body.Joints[JointType.SpineBase].Position.Y);
        }

        private void TrackEngagedPlayersViaHandOverHead()
        {
            engagementPeopleHaveChanged = false;
            var currentlyEngagedHands = KinectCoreWindow.KinectManualEngagedHands;
            handsToEngage.Clear();

            // check to see if anybody who is currently engaged should be disengaged
            foreach (var bodyHandPair in currentlyEngagedHands)
            {
                var bodyTrackingId = bodyHandPair.BodyTrackingId;
                foreach (var body in bodies)
                {
                    if (body.TrackingId == bodyTrackingId)
                    {
                        // check for disengagement
                        JointType engagedHandJoint =
                            (bodyHandPair.HandType == HandType.LEFT) ? JointType.HandLeft : JointType.HandRight;
                        bool toBeDisengaged = IsHandBelowHip(engagedHandJoint, body);

                        if (toBeDisengaged)
                        {
                            engagementPeopleHaveChanged = true;
                        }
                        else
                        {
                            handsToEngage.Add(bodyHandPair);
                        }
                    }
                }
            }

            // check to see if anybody should be engaged, if not already engaged
            foreach (var body in bodies)
            {
                if (handsToEngage.Count < engagedPeopleAllowed)
                {
                    bool alreadyEngaged = false;
                    foreach (var bodyHandPair in handsToEngage)
                    {
                        alreadyEngaged = (body.TrackingId == bodyHandPair.BodyTrackingId);
                    }

                    if (!alreadyEngaged)
                    {
                        // check for engagement
                        if (IsHandOverhead(JointType.HandLeft, body))
                        {
                            // engage the left hand
                            handsToEngage.Add(
                                new BodyHandPair(body.TrackingId, HandType.LEFT));
                            engagementPeopleHaveChanged = true;
                        }
                        else if (IsHandOverhead(JointType.HandRight, body))
                        {
                            // engage the right hand
                            handsToEngage.Add(
                                new BodyHandPair(body.TrackingId, HandType.RIGHT));
                            engagementPeopleHaveChanged = true;
                        }
                    }
                }
            }

            if (engagementPeopleHaveChanged)
            {
                BodyHandPair firstPersonToEngage = null;
                BodyHandPair secondPersonToEngage = null;

                Debug.Assert(handsToEngage.Count <= 2, "handsToEngage should be <= 2");

                switch (handsToEngage.Count)
                {
                    case 0:
                        break;
                    case 1:
                        firstPersonToEngage = handsToEngage[0];
                        break;
                    case 2:
                        firstPersonToEngage = handsToEngage[0];
                        secondPersonToEngage = handsToEngage[1];
                        break;
                }

                switch (EngagedPeopleAllowed)
                {
                    case 1:
                        KinectCoreWindow.SetKinectOnePersonManualEngagement(firstPersonToEngage);
                        break;
                    case 2:
                        KinectCoreWindow.SetKinectTwoPersonManualEngagement(firstPersonToEngage, secondPersonToEngage);
                        break;
                }
            }
        }
    }
}
