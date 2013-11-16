using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using DiskOfDemiseWPF.Gesture;
using DiskOfDemiseWPF.Gesture.Parts;
using DiskOfDemiseWPF.EventArguments;

namespace DiskOfDemiseWPF.Gesture
{
    class GestureController
    {
        /// <summary>
        /// list of gestures to search for
        /// </summary>
        private List<Gesture> gestures = new List<Gesture>();

        /// <summary>
        /// constructor for GestureController class
        /// </summary>
        public GestureController()
        {

        }

        /// <summary>
        /// dispatched when a gesture is recognized
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        /// <summary>
        /// update each gesture
        /// </summary>
        /// <param name="skeleton"></param>
        public void UpdateGestures(Skeleton skeleton)
        {
            foreach (Gesture gesture in this.gestures)
            {
                gesture.UpdateGesture(skeleton);
            }
        }

        /// <summary>
        /// add a gesture to gestures
        /// </summary>
        /// <param name="gestureType"></param>
        /// <param name="gestureDef"></param>
        public void AddGesture(string gestureType, GestureSegment[] gestureDef)
        {
            Gesture gesture = new Gesture(gestureType, gestureDef);
            gesture.GestureRecognized += this.WhenGestureRecognized;
            this.gestures.Add(gesture);
        }

        /// <summary>
        /// handles GestureRecognized event and resets all gestures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WhenGestureRecognized(object sender, GestureEventArgs e)
        {
            if (this.GestureRecognized != null)
            {
                this.GestureRecognized(this, e);
            }
            foreach (Gesture g in this.gestures)
            {
                g.Reset();
            }
        }
    }
}
