using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskOfDemiseWPF
{
    class Player
    {
        private String color = "";
        private ArrayList bodyParts = new ArrayList(5);

        public Player(String color)
        {
            this.color = color;
            bodyParts.Add("Head");
            bodyParts.Add("RightArm");
            bodyParts.Add("LeftArm");
            bodyParts.Add("RightLeg");
            bodyParts.Add("LeftLeg");
        }

        public void removeLimb(String limb)
        {
            if (bodyParts.IndexOf(limb) != -1)
            {
                bodyParts.Remove(limb);
            }
        }

        public String returnColor()
        {
            return color;
        }

        public ArrayList returnBodyParts()
        {
            return bodyParts;
        }
    }
}
