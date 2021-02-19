using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace classe_24_09
{
    public partial class Form1 : Form
    {
        Stopwatch stopWatch = new Stopwatch();
        Stopwatch stopWatch2 = new Stopwatch();
        StreamWriter ficheiro;
        string data;
        double offset, voltage, reference, amplitude, error, frequency, manual, t, input, position, ramp, step = 0.0, speed, Kp = 0.0, Ki = 0.0, Kd = 0.0;
        string pwm_value;
        int a = 0, time, b = 0;
        char type = 'd';
        Random rnd = new Random();
        int stepRandom, controller;

        // ----------------------   INITIALIZATING FUNCTIONS    -----------------------------
        public Form1()
        {
            InitializeComponent();
            InitPorts();
            Initchart();
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }

        // ---------------------------   CONNECTING TO SERIALPORT ---------------------------
        private void button1_Click(object sender, EventArgs e) //conect
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                button1.Text = "Connect";
                chart1.Series["Series1"].Points.Clear();
                chart1.Series["Series2"].Points.Clear();
                chart2.Series["Series1"].Points.Clear();
                chart2.Series["Series2"].Points.Clear();
                chart3.Series["Series1"].Points.Clear();
                //chart4.Series["Series1"].Points.Clear();
                if (a == 1)
                {
                    ficheiro.Close();
                    a = 0;
                }
            }
            else
            {
                serialPort1.Open();
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();
                button1.Text = "Disconnect";
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.PortName = comboBox1.Text;
        }
        private void InitPorts()
        {
            string[] Ports = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            for (int i = 0; i < Ports.Length; i++)
            {
                comboBox1.Items.Add(Ports[i]);
            }
        }
        // ----------------------------------------------------------------------------------

        //GRAPHICS SCALE - DONE
        private void Initchart()
        {
            chart1.ChartAreas[0].AxisY.Maximum = 400;
            chart1.ChartAreas[0].AxisY.Minimum = -400;
            chart2.ChartAreas[0].AxisY.Maximum = 120;
            chart2.ChartAreas[0].AxisY.Minimum = -120;
            chart3.ChartAreas[0].AxisY.Maximum = 10;
            chart3.ChartAreas[0].AxisY.Minimum = -10;
            chart4.ChartAreas[0].AxisY.Maximum = 6;
            chart4.ChartAreas[0].AxisY.Minimum = -6;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            chart3.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

            chart4.ChartAreas[0].AxisX.Maximum = 1.5;
            chart4.ChartAreas[0].AxisX.Minimum = 0.5;
            chart4.Series["Series1"].Points.AddXY(0, 0);

            chart2.ChartAreas[0].AxisX.Title = "Time(s)";
            chart2.ChartAreas[0].AxisY.Title = "Speed(rpm)";

            chart1.ChartAreas[0].AxisX.Title = "Time(s)";
            chart1.ChartAreas[0].AxisY.Title = "Position(degrees)";

            chart3.ChartAreas[0].AxisX.Title = "Time(s)";
            chart3.ChartAreas[0].AxisY.Title = "Error";

            chart4.ChartAreas[0].AxisY.Title = "Voltage(V)";
        }


        //GETTING DATA FROM MICROCONTROLLER 
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Invoke(new EventHandler(GetSerialDataFromArduino));
        }
        private void GetSerialDataFromArduino(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    data = serialPort1.ReadLine();
                    var part = data.Split(' ');
                    if (part.Length == 5)
                    {
                        t = Convert.ToDouble(part[0]) / 1000.0;
                        time = Convert.ToInt32(part[0]) / 1000;
                        position = (Convert.ToDouble(part[2]) % 360);
                        speed = Convert.ToDouble(part[3]);
                        input = Convert.ToDouble(part[2]);
                        //error = Convert.ToDouble(part[4]);
                        if (error > 10) error = 10;
                        voltage = Convert.ToDouble(part[4]);
                    }
                    else
                    {
                        richTextBox1.AppendText(data);
                        richTextBox1.ScrollToCaret();
                    }
                    b = 1;
                }
            }
            catch { }

        }


        // ---------------------------  MOTOR ---------------------------------------------
        //PWM VALUE - DONE
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            voltage = trackBar1.Value;
            voltage = (voltage / 4095) * 6;
            double percent = (voltage / 6.0) * 100.0;
            if (serialPort1.IsOpen)
            {
                pwm_value = trackBar1.Value.ToString();
                serialPort1.Write("T" + pwm_value + "\r");
                label2.Text = voltage.ToString("0.00");
            }
        }
        //STOP MOTOR - DONE
        private void button4_Click(object sender, EventArgs e)
        {
            trackBar1.Enabled = false;
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("S" + "\r");
                checkBox3.Checked = false;
            }
        }
        //START MOTOR - DONE
        private void button3_Click(object sender, EventArgs e)
        {

            trackBar1.Enabled = true;
            if (serialPort1.IsOpen)
            {
                pwm_value = trackBar1.Value.ToString();
                serialPort1.Write("I" + pwm_value + "\r");
                checkBox3.Checked = true;
            }
        }

        //CLOCKWISE DIRECTION - DONE
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("C" + "\r");
            }
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
                checkBox1.Checked = true;
            }
            richTextBox1.AppendText("The motor is spinning clockwise \r");
            richTextBox1.ScrollToCaret();
        }

        //ANTI-CLOCKWISE DIRECTION - DONE
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("A" + "\r");
            }
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
                checkBox2.Checked = true;
            }
            richTextBox1.AppendText("The motor is spinning anti-clockwise \r");
            richTextBox1.ScrollToCaret();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            offset = Convert.ToDouble(textBox7.Text);
        }



        /*try
        {
        catch
        }
        {
        }*/


        //-------------------------- INPUTS------------------------------------------------

        //GET PARAMETERS FOR INPUTS      


        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("Z" + "\r");
            }
        }

        //MANUAL
        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                checkBox4.Checked = false;
                checkBox6.Checked = false;
                checkBox5.Checked = false;
                checkBox7.Checked = true;
                trackBar2.Enabled = true;
                textBox2.Enabled = false;
                textBox1.Enabled = false;
            }
            else
            {
                trackBar2.Enabled = false;
                textBox2.Enabled = true;
                textBox1.Enabled = true;
                checkBox7.Checked = false;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (checkBox11.Checked)
            {
                stepRandom = rnd.Next(-6, 6);
                if (stepRandom > 6)
                {
                    stepRandom = 6;
                }
                if (stepRandom < -6)
                {
                    stepRandom = -6;
                }
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("R" + stepRandom + "\r");
                    //serialPort1.Write("R" + stepRandom + /*type +*/ "\r");
                }
            }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            amplitude = Convert.ToDouble(textBox2.Text);
        }

        //SET GAINS OF THE CONTROLLER
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("G" + textBox4.Text + textBox5.Text + textBox6.Text + "\r");
                }
            }
            catch { }
        }

        // TUNE BUTTON
        private void button7_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("L" + controller + "\r");
            }
            textBox4.Text = Kp.ToString();
            textBox5.Text = Ki.ToString();
            textBox6.Text = Kd.ToString();
        }

        // CONTROLLER SELECTOR
        #region ControllerAndInputSelection

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == 0) // NONE
            {
                controller = 0;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("S" + "\r");
                }
            }
            if (listBox1.SelectedIndex == 1) //P    
            {
                controller = 1;
                textBox4.Enabled = true;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                if (type == 's')
                {
                    Kp = 0.0706;
                }
                else if (type == 'd')
                {
                    Kp = 0.0931;
                }
            }
            if (listBox1.SelectedIndex == 2) //PI
            {
                controller = 2;
                textBox4.Enabled = true;
                textBox5.Enabled = true;
                textBox6.Enabled = false;
                if (type == 's')
                {
                    Kp = 0.0124;
                    Ki = 0.4977;
                }
                else if (type == 'd')
                {
                    Kp = 0.0883;
                    Ki = 0.0138;
                }
            }
            if (listBox1.SelectedIndex == 3) //PD
            {
                controller = 3;
                textBox4.Enabled = true;
                textBox5.Enabled = false;
                textBox6.Enabled = true;
                if (type == 's')
                {
                    Kp = 0.08;
                    Kd = -0.0011;
                }
                else if (type == 'd')
                {
                    Kp = 0.104;
                    Kd = 0.0050;
                }
            }
            if (listBox1.SelectedIndex == 4) //PID
            {
                controller = 4;
                textBox4.Enabled = true;
                textBox5.Enabled = true;
                textBox6.Enabled = true;
                if (type == 's')
                {
                    Kp = 0.0623;
                    Ki = 1.2400;
                    Kd = 1.16e-5;
                }
                else if (type == 'd')
                {
                    Kp = 0.1020;
                    Ki = 0.0218;
                    Kd = 0.0047;
                }
            }
            textBox4.Text = Kp.ToString();
            textBox5.Text = Ki.ToString();
            textBox6.Text = Kd.ToString();
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("L" + controller + "\r");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            frequency = Convert.ToDouble(textBox1.Text);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            manual = trackBar2.Value / 100.0;
            if (checkBox8.Checked) //input in degree
            {
                manual = manual * 360.0;
            }
            else if (checkBox9.Checked) //input in deg/s
            {
                manual = manual * 108;
            }
            reference = manual;
        }

        //DEGREE
        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
            {
                checkBox9.Checked = false;
                checkBox8.Checked = true;
                label4.Text = "degrees";
                type = 'd';
            }
        }

        //SPEED
        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked)
            {
                checkBox8.Checked = false;
                checkBox9.Checked = true;
                label4.Text = "RPM";
                type = 's';
            }
        }

        //CHECKBOX STEP   
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                checkBox5.Checked = false;
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                checkBox4.Checked = true;
            }
        }

        //CHECKBOX RAMP
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                checkBox4.Checked = false;
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                checkBox5.Checked = true;
            }
        }

        //CHECKBOX SINE
        private void checkBox6_CheckedChanged_1(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                checkBox5.Checked = false;
                checkBox4.Checked = false;
                checkBox7.Checked = false;
                checkBox6.Checked = true;
            }
        }
        #endregion

        //TIMER
        #region TimerData
        private void timer1_Tick(object sender, EventArgs e)
        {
            stopWatch.Start();
            double seconds = stopWatch.ElapsedMilliseconds / 1000.0;
            //******************************************************************************

            label2.Text = voltage.ToString("0.00");

            //RAMP -DONE     
            #region RampInput
            if (checkBox5.Checked)
            {
                double T = 1 / (2.0 * frequency);
                double m;
                if (checkBox8.Checked) //input in degree
                {
                    m = 360.0 / T;
                    ramp = seconds * m;
                    ramp = ramp % amplitude;
                }
                else if (checkBox9.Checked) //input in deg/s
                {
                    m = 110.0 / T;
                    ramp = seconds * m;
                    ramp = ramp % amplitude;
                }
                reference = ramp - amplitude * offset / 100;
                /*
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("O" + ramp + type + "\r");
                }*/
            }
            #endregion
            //SINE - DONE        
            #region SineInput
            if (checkBox6.Checked)
            {
                double sine = Math.Sin(2 * Math.PI * seconds * frequency) * amplitude;
                reference = sine - amplitude * offset / 100;
                /*if (serialPort1.IsOpen)
                {
                    serialPort1.Write("O" + sine + type + "\r");
                }*/
            }
            #endregion

            //RANDOMSTEPS - DONE
            /*if (checkBox11.Checked)
            {                
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("R" + stepRandom + type + "\r");
                }
            }*/

            //STEP - DONE
            #region StepInput
            if (checkBox4.Checked)
            {
                double wo = 2 * Math.PI * frequency;
                double senos = 0;
                for (int n = 1; n <= 100; n++)
                {
                    senos = senos + (Math.Sin((2 * n - 1) * wo * seconds) / (2 * n - 1));
                }
                step = (amplitude / 2) + (2 * amplitude / Math.PI) * senos - amplitude * offset / 100;
                reference = step;
                /*if (serialPort1.IsOpen)
                {
                    serialPort1.Write("O" + step + type + "\r");
                }*/
            }
            #endregion
            // CHARTS AND CONTROLLER DATA SEND
            #region ChartsAndController
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("P" + controller + type + reference + "\r");
                chart1.Series["Series1"].Points.AddXY(t, position);
                chart2.Series["Series1"].Points.AddXY(t, speed);
                if (voltage < 3 && voltage > -3)
                {
                    chart4.Series["Series1"].Color = Color.Yellow;
                }
                else if (voltage < -3)
                {
                    chart4.Series["Series1"].Color = Color.Red;
                }
                else chart4.Series["Series1"].Color = Color.Green;
                chart4.Series["Series1"].Points.ElementAt(0).SetValueY(voltage);

                chart4.Invalidate();

                if (t >= 5)
                {
                    //chart1.Series["Series1"].Points.RemoveAt(0);
                    //chart1.Series["Series2"].Points.RemoveAt(0);
                    //chart2.Series["Series1"].Points.RemoveAt(0);
                    //chart2.Series["Series2"].Points.RemoveAt(0);
                    //chart3.Series["Series1"].Points.RemoveAt(0);

                    chart1.ChartAreas[0].AxisX.Maximum = Math.Round(t, 2);
                    chart1.ChartAreas[0].AxisX.Minimum = Math.Round(t - 5, 2);

                    chart2.ChartAreas[0].AxisX.Maximum = Math.Round(t, 2);
                    chart2.ChartAreas[0].AxisX.Minimum = Math.Round(t - 5, 2);

                    chart3.ChartAreas[0].AxisX.Maximum = Math.Round(t, 2);
                    chart3.ChartAreas[0].AxisX.Minimum = Math.Round(t - 5, 2);
                }

                if (type == 'd')
                {
                    chart1.Series["Series2"].Points.AddXY(t, input);
                    chart2.Series["Series2"].Points.Clear();
                }
                else if (type == 's')
                {
                    chart2.Series["Series2"].Points.AddXY(t, input);
                    chart1.Series["Series2"].Points.Clear();
                }
                chart3.Series["Series1"].Points.AddXY(t, error);
            }

            if (a == 1 && b == 1)
            {
                ficheiro.WriteLine(t + " " + input + " " + position + " " + speed + " " + error + " " + voltage);
            }
            #endregion
        }
        #endregion
        //------------------------------------ FILES ---------------------------------------
        //GENERATE THE FILE
        #region GenerateFile
        private void button5_Click(object sender, EventArgs e)
        {
            checkBox10.Enabled = true;
            a = 1;
            string path;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                path = folderBrowserDialog1.SelectedPath;
                path = path + "\\" + textBox3.Text;
                ficheiro = new StreamWriter(path + ".txt");
            }
        }
        #endregion
        //SAVING THE FILE
        #region SavingFile
        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            ficheiro.Close();
            a = 0;
        }
        #endregion
    }
}
