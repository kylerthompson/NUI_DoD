using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using DiskOfDemiseWPF;
using DiskOfDemiseWPF.Gesture;

namespace DiskOfDemiseWPF.EventArguments
{    
    /// <summary>
    /// Gesture event arguments
    /// </summary>
    class GestureEventArgs : EventArgs
    {
        /// <summary>
        /// constructor for GestureEventArgs class
        /// </summary>
        /// <param name="gestureType"></param>
        /// <param name="trackingId"></param>
        public GestureEventArgs(string gestureType, int trackingId)
        {
            this.gestureType = gestureType;
            this.trackingId = trackingId;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string gestureType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int trackingId { get; set; }
    }
    
}
