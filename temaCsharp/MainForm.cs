﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using temaCsharp.Entities;
using temaCsharp.Library;
using temaCsharp.Library.Entities;
using temaCsharp.Util;

namespace temaCsharp
{
    public partial class MainForm : Form
    {
        // Holds state
        public HardwareSessionManager session;

        public MainForm()
        {
            session = new HardwareSessionManager();
            InitializeComponent();
            updateState();
        }

        public MainForm(HardwareSessionManager hsm)
        {
            session = hsm;
            InitializeComponent();
            // populate everything with the proper data from state
            updateState();
        }

        /**
         * Renders data from the state manager to the form components
         *
         */ 
        private void updateState()
        {
            listBox1.Items.Clear();
            treeView1.Nodes.Clear();
            if (session.components.Count > 0)
            {
                foreach (Component component in session.components)
                {
                    listBox1.Items.Add(component.Name);
                }
                int currentIndex = session.components.Last().ID + 1;
                textBox1.Text = currentIndex.ToString();
            }

            if (session.platforms.Count > 0)
            {
                foreach (String platform in session.platforms)
                {
                    comboBox1.Items.Add(platform);
                }
            }

            if (session.computers.Count() > 0)
            {
                int i = treeView1.Nodes.Count;
                foreach (Computer computer in session.computers)
                {
                    treeView1.Nodes.Add("PC " + computer.ID.ToString());
                    List<Component> components = computer.getComponents();
                    foreach (Component component in components)
                    {
                        treeView1.Nodes[i].Nodes.Add(component.Name);
                    }
                    i++;
                }
            }
            treeView1.ExpandAll();
        }

        // Event handlers

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        /**
         * 
         * Deletes a component and marks it for deletion from db
         * 
         */ 
        private void button2_Click(object sender, EventArgs e)
        {
            int selectedComponent = listBox1.SelectedIndex;
            if (selectedComponent > -1)
            {
                int targetedId = session.components[selectedComponent].ID;
                session.componentsToDelete.Add(targetedId);

                session.components.RemoveAt(selectedComponent);
                listBox1.Items.RemoveAt(selectedComponent);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        /**
         * Adds a component to the list view
         * 
         */
        private void button1_Click(object sender, EventArgs e)
        {
            // Validate input
            int id;
            int currentComponentIndex = 1;
            if (session.components.Count > 0)
            {
                currentComponentIndex = session.components.Last().ID + 1;
            }

            Boolean idok, nameok, platfok;
            idok = nameok = platfok = true;
            String validationMsg = "";

            if (!Int32.TryParse(textBox1.Text, out id) && id > currentComponentIndex)
            {
                validationMsg += "`" +id + "` is not a valid component ID.";
                idok = false;
            }
            
            if (String.IsNullOrEmpty(textBox2.Text.Trim()))
            {
                validationMsg += "`" + textBox2.Text.Trim() + "` is not a valid component name.";
                nameok = false;
            }
            String componentName = textBox2.Text.Trim();

            if (String.IsNullOrEmpty(comboBox1.Text.Trim()))
            {
                validationMsg += "`" + comboBox1.Text.Trim() + "` is not a valid platform.";
                platfok = false;
            }
            String componentPlatform = comboBox1.Text;

            // If all is well
            if (idok && nameok && platfok)
            {
                // Add component to data store
                Component newComponent = new Component(id, componentName, componentPlatform);

                if (!HardwareUtil.isStringInArray(listBox1.Items, newComponent.Name))
                {
                    listBox1.Items.Add(newComponent.Name);
                    session.components.Add(newComponent);
                }
                else
                {
                    HardwareUtil.validationAlert("`" + newComponent.Name + "` already exists in the component store.");
                }
            }
            else
            {
                HardwareUtil.validationAlert(validationMsg);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {  
        }

        /**
         * Tests the compatibility of 2 components or a component and a PC
         * 
         */ 
        private void button3_Click(object sender, EventArgs e)
        {
            // if we have a pc 
            if (treeView1.Nodes.Count > 0)
            {
                // if we have selected a component
                int selectedIndex = listBox1.SelectedIndex;
                if (selectedIndex > -1)
                {
                    // if we have selected a node
                    if (treeView1.SelectedNode != null)
                    {
                        Component selectedComponent = session.components[selectedIndex];
                        List<int> ls = HardwareUtil.getAddrOfNode(treeView1.SelectedNode);

                        if (HardwareCompatibilityManager.isCompatible(selectedComponent, session.computers[ls[0]]))
                        {
                            HardwareUtil.compatibilityInfo("The component is compatible!");
                        } else {
                            HardwareUtil.compatibilityInfo("The component is not compatible.");
                        }
                    }
                }
            }
        }

        /**
         * Creates a new PC in the tree view
         * 
         */ 
        private void button5_Click(object sender, EventArgs e)
        {
            int currentIndex = 1;
            if (session.computers.Count > 0)
            {
                currentIndex = session.computers.Last().ID + 1;
            }

            Computer c = new Computer(currentIndex, "INTEL", new List<Component>());
            session.computers.Add(c);
            treeView1.Nodes.Add("PC " + c.ID.ToString());
            treeView1.ExpandAll();
        }

        /**
         * Adds the component to the selected PC
         * 
         */ 
        private void button4_Click(object sender, EventArgs e)
        {
            // if we have a pc 
            if (treeView1.Nodes.Count > 0)
            {
                // if we have selected a component
                int selectedComponent = listBox1.SelectedIndex;
                if (selectedComponent > -1)
                {
                    // if we have selected a node
                    if (treeView1.SelectedNode != null)
                    {
                        // this is done poorly but works for now
                        List<int> ls = HardwareUtil.getAddrOfNode(treeView1.SelectedNode);
                
                        // Add a node to tree and add its associated component to the session
                        if (HardwareCompatibilityManager.isCompatible(session.components[selectedComponent], session.computers[ls[0]]))
                        {
                            treeView1.Nodes[ls[0]].Nodes.Add(session.components[selectedComponent].Name);
                            session.computers[ls[0]].addComponent(session.components[selectedComponent]);
                        }
                        else {
                            String msg = String.Format(
                                "Component `{0}` (platform `{1}`) is not compatible with PC {2} (platform `{3}`).",
                                session.components[selectedComponent].Name,
                                session.components[selectedComponent].Platform,
                                session.computers[ls[0]].ID,
                                session.computers[ls[0]].Platform
                                );

                            HardwareUtil.compatibilityAlert(msg);
                        }
                        treeView1.ExpandAll();
                    }
                }
            }
        }

        /**
         * Removes the component from its parent PC
         * 
         */ 
        private void button6_Click(object sender, EventArgs e)
        {
            // if we have a pc 
            if (treeView1.Nodes.Count > 0)
            {
                // if we have selected a node
                if (treeView1.SelectedNode != null)
                {
                    // getAddrOfNode should be done better
                    List<int> ls = HardwareUtil.getAddrOfNode(treeView1.SelectedNode);
                    if (ls.Count > 1)
                    {
                        treeView1.Nodes.Remove(treeView1.SelectedNode);
                        session.computers[ls[1]].removeComponent(ls[0]);
                    }
                    treeView1.ExpandAll();
                }
            }
        }

        /**
         * Opens a file save dialog and saves a report formatted as a text file
         * 
         */ 
        private void button8_Click(object sender, EventArgs e)
        {
            Stream stream;
            SaveFileDialog sfd = new SaveFileDialog();

            // show other files in directory; by default the file will be called report.txt
            sfd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            sfd.FileName = "report.txt";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if ((stream = sfd.OpenFile()) != null)
                {
                    String path   = Path.GetFullPath(sfd.FileName);
                    String report = session.getReportAsString();

                    // Close the stream as the process will be unable to write to 
                    // it with WriteAllText while opened as stream
                    stream.Close();
                    File.WriteAllText(path, report);
                }
            }
        }

        /**
         * 
         * Writes state to the db --should ideally also contain a fallback to file-based serialisation
         */ 
        private void button7_Click(object sender, EventArgs e)
        {
            // TODO:: deal with situation in which an OleDb driver is not present on pc
            try
            {
                var connectionString = Properties.Settings.Default.Database;
                session.saveState(new OleDbConnection(connectionString));
                session.retrieveState(new OleDbConnection(connectionString));
                updateState();
                HardwareUtil.savingSuccess("You have successfully persisted application data to the database!");
            }
            catch (Exception x) {
                MessageBox.Show(x.Message);
            }
        }

        /**
         * Deletes a PC from the session and marks it for deletion from the db
         * 
         */ 
        private void button9_Click(object sender, EventArgs e)
        {
            // if we have a pc 
            if (treeView1.Nodes.Count > 0)
            {
                // if we have selected a node
                if (treeView1.SelectedNode != null)
                {
                    // getAddrOfNode should be done better
                    List<int> ls = HardwareUtil.getAddrOfNode(treeView1.SelectedNode);
                    if (
                        ls.Count == 1 &&
                        HardwareUtil.confirmDeleteComputer("Are you sure you want to delete this computer?")
                        )
                    {
                        // Queue ID for deletion
                        session.computersToDelete.Add(session.computers[ls[0]].ID);
                        treeView1.Nodes.Remove(treeView1.SelectedNode);
                        session.computers.Remove(session.computers[ls[0]]);
                    }
                    treeView1.ExpandAll();
                }
            }
        }

        private void pieChartControl1_Load(object sender, EventArgs e)
        {
            
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void pieChartControl1_Load_1(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            PieChartCategory[] statsFormData = HardwareUtil.getStatsAsChart(session.getPlatformShare());
            StatsForm statsForm = new StatsForm(statsFormData);
            statsForm.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            String logdata = HardwareUtil.getLogsAsString();
            LogsForm logsForm = new LogsForm(logdata);
            logsForm.Show();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.Show();
        }
    }
}
