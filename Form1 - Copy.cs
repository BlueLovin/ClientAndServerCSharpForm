using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarrettCodeChallenge
{
    public partial class Form1copy : Form
    {
        List<Ball> BallList = new List<Ball>();
        List<TcpClient> client_list;
        TcpListener listener;
        TcpClient client;
        int client_count;
        bool keep_going;
        int port = 5000;
        public enum Direction
        {
            up,
            down,
            left,
            right
        }
        public Form1copy(bool isConnected)
        {
            InitializeComponent();
            timer1.Interval = 50;
            timer1.Start();
            Ball ball1 = new Ball(10,50);
            Ball ball2 = new Ball(250,100);
            Ball ball3 = new Ball(450,20);
            ball2.isSelected = true;
            BallList.Add(ball1);
            BallList.Add(ball2);
            BallList.Add(ball3);
            client_list = new List<TcpClient>();
            client_count = 0;
            if (isConnected)
            {
                startServerToolStripMenuItem.Enabled = false;
            }
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.SuspendLayout();
            // Create pen.
            Pen blackPen = new Pen(Color.Black, 3);
            Pen selectedPen = new Pen(Color.Green, 5);


            foreach (Ball balls in BallList)
            {
                Rectangle rect = new Rectangle(balls.pos.X, balls.pos.Y, 200, 200);
                //Rectangle center = new Rectangle(balls.pos.X + 95, balls.pos.Y + 95, 10, 10);
                if (balls.isSelected)
                {
                    e.Graphics.DrawEllipse(selectedPen, rect);
                    //e.Graphics.DrawEllipse(selectedPen, center); // center of ball
                    //e.Graphics.DrawLine(selectedPen, new Point(balls.pos.X + 100, balls.pos.Y + 100), new Point(balls.pos.X + 200, balls.pos.Y + 100)); //radius line
                }
                else
                {
                    e.Graphics.DrawEllipse(blackPen, rect);
                    //e.Graphics.DrawEllipse(selectedPen, center);

                }
            }
            this.ResumeLayout();
        }
        /// <summary>
        /// woo wheee
        /// </summary>
        /// <param name="direction">which direction are we moving?</param>
        /// <param name="ballID">what ball are we moving?</param>
        /// <returns>if a ball is colliding or not</returns>
        private bool isColliding(Direction direction, int ballID)
        {
            Point nextPos = new Point(0, 0);

            if (direction == Direction.down)
            {
                nextPos = new Point(BallList[ballID].pos.X, BallList[ballID].pos.Y + 10);
                if (BallList[ballID].pos.Y + 248 >= Height)
                    return true;
            }
            if (direction == Direction.up)
            {
                nextPos = new Point(BallList[ballID].pos.X, BallList[ballID].pos.Y - 10);
                if (BallList[ballID].pos.Y - 30 <= 0)
                    return true;
            }
            if (direction == Direction.right)
            {
                nextPos = new Point(BallList[ballID].pos.X + 10, BallList[ballID].pos.Y);

            }
            if (direction == Direction.left)
            {
                nextPos = new Point(BallList[ballID].pos.X - 10, BallList[ballID].pos.Y);

            }

            foreach (Ball ball in BallList)
            {
                if (ball != BallList[ballID])
                {
                    if (ball.colliding(nextPos))
                        return true;
                }
            }
            return false;
        }

        private void MoveBall(Direction direction, int ballID)
        {
            if (!isColliding(direction, ballID))
            {
                switch (direction)
                {
                    case Direction.up:
                        BallList[ballID].pos.Y -= 10;
                        break;
                    case Direction.down:
                        BallList[ballID].pos.Y += 10;
                        break;
                    case Direction.left:
                        BallList[ballID].pos.X -= 10;
                        break;
                    case Direction.right:
                        BallList[ballID].pos.X += 10;
                        break;
                }
            }
        }
        private int getSelectedBallID()
        {
            for (int i = 0; i < BallList.Count; i++)
            {
                if (BallList[i].isSelected)
                    return i;
            }
            return 99;
        }

        /// <summary>
        /// handle keypresses
        /// </summary>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int ballID = getSelectedBallID(); //get currently selected ball
            switch (e.KeyCode)
            {
                case Keys.Down:
                    MoveBall(Direction.down, ballID);
                    break;
                case Keys.Up:
                    MoveBall(Direction.up, ballID);
                    break;
                case Keys.Left:
                    MoveBall(Direction.left, ballID);
                    break;
                case Keys.Right:
                    MoveBall(Direction.right, ballID);
                    break;
            }
            //if (e.KeyCode == Keys.Down)
            //{
            //    y += 15;
            //}

            //if (e.KeyCode == Keys.Up)
            //{
            //    y -= 15;
            //}

            //if (e.KeyCode == Keys.Left)
            //{
            //    x -= 15;
            //}

            //if (e.KeyCode == Keys.Right)
            //{
            //    x += 15;
            //}
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Refresh();
        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {


        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.Show();
            ab.TopMost = true;
        }

        private void selectOtherBallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < BallList.Count; i++)
            {
                if (BallList[i].isSelected)
                {
                    BallList[i].isSelected = false; //unselect current ball

                    //if not at the end of the list, select the next ball
                    if(i != BallList.Count - 1)
                    {
                        BallList[i + 1].isSelected = true;
                    }
                    else //select first ball if at end of the list
                    {
                        BallList[0].isSelected = true;
                    }
                    break;
                }
            }
        }

        private void startServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*try
            {
                client_count = 0;
                client_list.Clear();

                Thread t = new Thread(listenForIncomingConnections);
                t.Name = "Server Listener Thread";
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem starting server.");
            }
            startServerToolStripMenuItem.Enabled = false;
            stopServerToolStripMenuItem.Enabled = true;*/
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //server.Stop();
        }

        private void stopServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //server.Stop();
            startServerToolStripMenuItem.Enabled = true;
            stopServerToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// initialize new client!
        /// </summary>
        private void startClientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 newInstance = new Form1(true); //create new form
            newInstance.Show(); // open new client window
        }

        //private void ProcessClientTransactions(object tcpClient)
        //{
        //    TcpClient client = (TcpClient)tcpClient;
        //    string input = string.Empty;
        //    StreamReader reader = null;
        //    StreamWriter writer = null;


        //    try
        //    {
        //        reader = new StreamReader(client.GetStream());
        //        writer = new StreamWriter(client.GetStream());

        //        // Tell the server we've connected
        //        writer.WriteLine("Hello from a client! Ready to do your bidding!");
        //        writer.Flush();

        //        while (client.Connected)
        //        {
        //            input = reader.ReadLine(); // block here until we receive something from the server.
        //            if (input == null)
        //            {
        //                //DisconnectFromServer();
        //            }
        //            else
        //            {
        //                switch (input)
        //                {

        //                    default:
        //                        {
        //                            // _statusTextBox.InvokeEx(stb => stb.Text += CRLF + " Received from Server: " + input);
        //                            MoveBall((Direction)Enum.Parse(typeof(Direction), input), getSelectedBallID());
                                    
        //                            break;
        //                        }
        //                } // end switch
        //            } // end if/else


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //}

        //private void listenForIncomingConnections()
        //{
        //    try
        //    {
        //        keep_going = true;
        //        listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
        //        listener.Start();

        //        while (keep_going)
        //        {
        //            TcpClient client = listener.AcceptTcpClient();   // blocks here until client connects
        //            Thread t = new Thread(ProcessClientRequests);
        //            t.IsBackground = true;
        //            t.Start(client);
        //        }
        //    }
        //    catch (SocketException se)
        //    {
        //        MessageBox.Show("Problem starting the server. Error: " + se.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Problem starting the server. Error: " + ex.ToString());
        //    }
        //}

        //private void ProcessClientRequests(Object o)
        //{
        //    TcpClient client = (TcpClient)o;
        //    client_list.Add(client);
        //    client_count += 1;


        //    string input = string.Empty;


        //    try
        //    {
        //        StreamReader reader = new StreamReader(client.GetStream());
        //        StreamWriter writer = new StreamWriter(client.GetStream());
        //        while (client.Connected)
        //        {
        //            input = reader.ReadLine(); // blocks here until something is received from client
        //            switch (input)
        //            {
                        


        //                default:  // default case acts as echo server
        //                    {
        //                        try
        //                        {
        //                            MoveBall((Direction)Enum.Parse(typeof(Direction), input), getSelectedBallID());
        //                        }
        //                        catch(Exception ex)
        //                        {

        //                        }
        //                        MessageBox.Show("Server received: " + input);
        //                        writer.WriteLine("Server received: " + input);
        //                        writer.Flush();

        //                        break;
        //                    }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Problem processing client requests. Error: " + ex.ToString());
        //    }

        //    client_list.Remove(client);
        //    client_count -= 1;

        //    //if (client_count == 0)
        //    //{
        //    //    _statusTextBox.InvokeEx(stb => stb.Text = string.Empty);
        //    //}
        //}

        private void sendmessagetohostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (client.Connected)
            {
                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.WriteLine("shitfuckers!");
                writer.Flush();
            }
        }
    }
}
