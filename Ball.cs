using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarrettCodeChallenge
{
    class Ball
    {
        static Form1 f = new Form1(false);
        public bool isSelected;
        public Point pos;

        public Ball(int x, int y)
        {
            this.pos.X = x; this.pos.Y = y;
        }

        public bool colliding(Point nextPosition)
        {
            float xd = this.pos.X - nextPosition.X;
            float yd = this.pos.Y - nextPosition.Y;

            float radius = 200;
            float sqrRadius = radius * radius;

            float distSqr = (xd * xd) + (yd * yd);

            if (distSqr <= sqrRadius)
            {
                return true;
            }

            return false;
        }
    }
}
