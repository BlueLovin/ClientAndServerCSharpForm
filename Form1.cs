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
    public partial class Form1 : Form
    {
        List<Ball> BallList = new List<Ball>();
        List<TcpClient> client_list;
        List<Form1> client_form_list = new List<Form1>();
        TcpListener listener;
        TcpClient client;
        int client_count;
        bool keep_going;
        bool isClient;

        const int PORT = 5000;
        const string IP_ADDRESS_STR = "127.0.0.1";
        public enum Direction
        {
            up,
            down,
            left,
            right
        }
        public Form1(bool _isClient)
        {
            InitializeComponent();
            Text = "Starrett - Server";
            timer1.Interval = 50;
            timer1.Start();
            Ball ball1 = new Ball(10,50);
            Ball ball2 = new Ball(250,100);
            Ball ball3 = new Ball(450,30);
            ball2.isSelected = true;
            BallList.Add(ball1);
            BallList.Add(ball2);
            BallList.Add(ball3);
            client_list = new List<TcpClient>();
            client_count = 0;
            if (_isClient)
            {
                Text = "Starrett - Client";
                startServerToolStripMenuItem.Enabled = false;
                startClientToolStripMenuItem.Enabled = false;
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
                if (BallList[ballID].pos.Y + 248 >= Height)//bottom boundary
                    return true;
            }
            if (direction == Direction.up)
            {
                nextPos = new Point(BallList[ballID].pos.X, BallList[ballID].pos.Y - 10);
                if (BallList[ballID].pos.Y - 30 <= 0)//top boundary
                    return true;
            }
            if (direction == Direction.right)
            {
                nextPos = new Point(BallList[ballID].pos.X + 10, BallList[ballID].pos.Y);
                if (BallList[ballID].pos.X + 225 >= Width)//right boundary
                    return true;
            }
            if (direction == Direction.left)
            {
                nextPos = new Point(BallList[ballID].pos.X - 10, BallList[ballID].pos.Y);
                if (BallList[ballID].pos.X <= 0)//left boundary
                    return true;
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
            //Thread.Sleep(200);
            if (!isColliding(direction, ballID))
            {
                try
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
                catch(Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
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
            Direction direction = new Direction();
            int ballID = getSelectedBallID(); //get currently selected ball
            bool okay = false; //prevent unwanted keypresses
            switch (e.KeyCode)
            {
                case Keys.Down:
                    direction = Direction.down;
                    okay = true;
                    MoveBall(direction, ballID);
                    break;
                case Keys.Up:
                    direction = Direction.up;
                    okay = true;
                    MoveBall(direction, ballID);
                    break;
                case Keys.Left:
                    direction = Direction.left;
                    okay = true;
                    MoveBall(direction, ballID);
                    break;
                case Keys.Right:
                    direction = Direction.right;
                    okay = true;
                    MoveBall(direction, ballID);
                    break;
            }
            if (okay)
            {
                if (!isClient)
                {
                    foreach (TcpClient clients in client_list)//if server, send direction to every client
                    {
                        if (clients.Connected)
                        {
                            StreamWriter writer = new StreamWriter(clients.GetStream());
                            writer.WriteLine(direction.ToString() + ";" + getSelectedBallID().ToString());
                            writer.Flush();
                        }
                    }
                }
                else
                {
                    if (client.Connected)
                    {
                        StreamWriter writer = new StreamWriter(client.GetStream());
                        writer.WriteLine(direction.ToString() + ";" + getSelectedBallID().ToString());
                        writer.Flush();
                    }
                }
            }
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
            try
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
            stopServerToolStripMenuItem.Enabled = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //server.Stop();
        }

        private void stopServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            keep_going = false;
            try
            {
                foreach (Form clients in client_form_list)
                {
                    clients.Close();
                }
                client_list.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem stopping the server, or client connections forcibly closed... Error: " + ex.ToString());
            }
            startServerToolStripMenuItem.Enabled = true;
            stopServerToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// initialize new client!
        /// </summary>
        private void startClientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 newInstance = new Form1(true); //create new form
            newInstance.isClient = true;
            
            for (int i = 0; i < BallList.Count; i++)
            {
                newInstance.BallList[i].pos.X = BallList[i].pos.X;
                newInstance.BallList[i].pos.Y = BallList[i].pos.Y;
                newInstance.BallList[i].isSelected = BallList[i].isSelected;
            }
            try
            {
                newInstance.client = new TcpClient("127.0.0.1", PORT);
                Thread t = new Thread(newInstance.ProcessClientTransactions);
                t.IsBackground = true;
                t.Start(newInstance.client);
                newInstance.Show(); // open new client window
                client_form_list.Add(newInstance);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem connecting to server. Is the server started?? Error: " + ex.ToString());
            }
        }

        private void ProcessClientTransactions(object tcpClient)
        {
            if (isClient)
            {
                TcpClient _client = (TcpClient)tcpClient;
                string input = string.Empty;
                StreamReader reader = null;
                StreamWriter writer = null;


                try
                {
                    reader = new StreamReader(_client.GetStream());
                    writer = new StreamWriter(_client.GetStream());

                    // Tell the server we've connected
                    //writer.WriteLine("Hello from a client! Ready to do your bidding!");
                    writer.Flush();
                    
                    while (_client.Connected)
                    {
                        input = reader.ReadLine(); // block here until we receive something from the server.
                        
                        if (input == null)
                        {
                            //DisconnectFromServer();
                        }
                        else
                        {
                            string ballIDString = input.Substring(input.IndexOf(";") + 1);
                            //very consolidated code, sorry lol. turns the first part of string transmitted into a direction enum. 
                            Direction currentDirection = (Direction)Enum.Parse(typeof(Direction), input.Substring(0, input.IndexOf(";"))); //wtf?
                            //MessageBox.Show(currentDirection.ToString());
                            MoveBall(currentDirection, Convert.ToInt32(ballIDString));
                            //MessageBox.Show(input);
                        } // end if/else


                    }
                }
                catch (Exception ex)
                {

                    // _statusTextBox.InvokeEx(stb => stb.Text += CRLF + ex.ToString());
                }
            }
        }

        private void listenForIncomingConnections()
        {
            try
            {
                keep_going = true;
                listener = new TcpListener(IPAddress.Parse(IP_ADDRESS_STR), PORT);
                listener.Start();

                while (keep_going)
                {
                    TcpClient _client = listener.AcceptTcpClient();   // blocks here until client connects
                    Thread t = new Thread(ProcessClientRequests);
                    t.IsBackground = true;
                    t.Start(_client);
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show("Problem starting the server. Error: " + se.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem starting the server. Error: " + ex.ToString());
            }
        }

        private void ProcessClientRequests(Object o)
        {
            if (!isClient)
            {
                TcpClient _client = (TcpClient)o;
                client_list.Add(_client);
                this.client_count += 1;

                string input = string.Empty;


                try
                {
                    StreamReader reader = new StreamReader(_client.GetStream());
                    StreamWriter writer = new StreamWriter(_client.GetStream());
                    while (_client.Connected)
                    {
                        input = reader.ReadLine(); // blocks here until something is received from client

                        string ballIDString = input.Substring(input.IndexOf(";") + 1);
                        Direction currentDirection = (Direction)Enum.Parse(typeof(Direction), input.Substring(0, input.IndexOf(";"))); //wtf?
                        //MessageBox.Show(currentDirection.ToString());
                        MoveBall(currentDirection, Convert.ToInt32(ballIDString));
                        //MessageBox.Show(input);

                        writer.Flush();
                    }

                }
                catch (SocketException se)
                {
                    MessageBox.Show("Problem processing client requests. Error: " + se.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                client_list.Remove(_client);
                client_count -= 1;
            }
        }
    }
}
