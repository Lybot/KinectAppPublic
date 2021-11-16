using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Util;
using Microsoft.Kinect;

namespace KinectFMT.Models
{
    public class CollectionArModels:ObservableCollection<ArModel>
    {

        public bool Contains(ulong trackingId)
        {
            foreach (var aRModel in this)
            {
                if (aRModel.TrackingId == trackingId)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Update all actions
        /// </summary>
        /// <param name="leftSwipeAction">action for left hand swipe</param>
        /// <param name="rightSwipeAction">action for right hand swipe</param>
        /// <param name="clapAction">action for clap</param>
        /// <param name="leftClenchingAction">action for left clench</param>
        /// <param name="rightClenchingAction">action for right clench</param>
        public void UpdateActions(Action leftSwipeAction, Action rightSwipeAction, Action clapAction, Action leftClenchingAction, Action rightClenchingAction)
        {
            foreach (var arModel in this)
            {
                arModel.LeftSwipeAction = leftSwipeAction;
                arModel.LeftClenchingAction = leftClenchingAction;
                arModel.RightClenchingAction = rightClenchingAction;
                arModel.RightSwipeAction = rightSwipeAction;
                arModel.ClapAction = clapAction;
            }
        }
        /// <summary>
        /// Update only swipes and clap actions, clenching will change masks (has default actions)
        /// </summary>
        /// <param name="leftSwipeAction">action for left hand swipe</param>
        /// <param name="rightSwipeAction">action for right hand swipe</param>
        /// <param name="clapAction">action for clap</param>
        public void UpdateActions(Action leftSwipeAction, Action rightSwipeAction, Action clapAction)
        {
            if (Count == 0)
                return;
            foreach (var arModel in this)
            {
                arModel.LeftSwipeAction = leftSwipeAction;
                arModel.LeftClenchingAction = arModel.PreviousSource;
                arModel.RightClenchingAction = arModel.NextSource;
                arModel.RightSwipeAction = rightSwipeAction;
                arModel.ClapAction = clapAction;
            }
        }
        public void UpdateArModel(ulong trackingId, ColorSpacePoint head, ColorSpacePoint rightArm, ColorSpacePoint leftArm, Vector4 orientationHead, Vector4 orientationLeftArm, Vector4 orientationRightArm)
        {
            if (Count == 0)
                return;
            foreach (var aRModel in this)
            {
                if (aRModel.TrackingId == trackingId)
                {
                    aRModel.UpdatePosition(head, rightArm, leftArm, orientationHead,  orientationLeftArm, orientationRightArm);
                    return;
                }
            }
        }

        public void ClearArModels()
        {
            foreach (var model in this)
            {
                model.Source = null;
                model.SourceNumber = -1;
            }
        }
        public void UpdateArModel(ulong trackingId, string type, double progress)
        {
            if (Count==0)
                return;
            foreach (var aRModel in this)
            {
                if (aRModel.TrackingId == trackingId)
                {
                    if (type == "leftSwipe")
                        aRModel.LeftSwipeGesture = progress;
                    if (type == "rightSwipe")
                        aRModel.RightSwipeGesture = progress;
                    if (type == "clap")
                        aRModel.ClapGesture = progress;
                    if (type == "leftClenching")
                        aRModel.LeftClenchingGesture = progress;
                    if (type == "rightClenching")
                        aRModel.RightClenchingGesture = progress;
                    return;
                }
            }
        }

        public void DeleteArModel(ulong trackingId)
        {
            foreach (var aRModel in this)
            {
                if (aRModel.TrackingId == trackingId)
                {
                    Remove(aRModel);
                    return;
                }
            }
        }
    }
}
