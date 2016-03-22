using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel;
using System.Data.SqlClient;
using System.Configuration;

namespace ExcelReader
{
    public partial class Form1 : Form
    {
        SqlCommand cmd;
        SqlDataAdapter adp;
        SqlCommandBuilder cmdBD;
        DataSet sDs;
        DataTable sDt;

        DataTable InsDt;
        string dateFrom;
        string dateTo;
        private DataSet ds;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                button2.Enabled = true;
            }
        }

        //process
        private void button2_Click(object sender, EventArgs e)
        {
            var file = new FileInfo(textBox1.Text);
            using (var stream = new FileStream(textBox1.Text, FileMode.Open))
            {
                IExcelDataReader reader = null;
                if (file.Extension == ".xls")
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);

                }
                else if (file.Extension == ".xlsx")
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                if (reader == null)
                    return;
                reader.IsFirstRowAsColumnNames = firstRowNamesCheckBox.Checked;
                ds = reader.AsDataSet();

                var tablenames = GetTablenames(ds.Tables);
                sheetCombo.DataSource = tablenames;

                if (tablenames.Count > 0)
                {
                    sheetCombo.SelectedIndex = 0;
                    button3.Enabled = true;
                }


                //dataGridView1.DataSource = ds;
                //dataGridView1.DataMember
            }
        }
        private IList<string> GetTablenames(DataTableCollection tables)
        {
            var tableList = new List<string>();
            foreach (var table in tables)
            {
                tableList.Add(table.ToString());
            }

            return tableList;
        }
        private void SelectTable()
        {
            var tablename = sheetCombo.SelectedItem.ToString();

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = ds; // dataset
            dataGridView1.DataMember = tablename;

            InsDt = new DataTable();
            InsDt = ds.Tables[tablename];

            //GetValues(ds, tablename);
        }
        private void sheetCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectTable();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //adp.Update(InsDt);
            string strQuery;
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    using (SqlConnection con = new SqlConnection(GlobalVar.con))
                    {
                        strQuery = @"Insert Into " + GlobalVar.staffTB + " Values(" + GlobalVar.insertedStaff + ")";
                        using (SqlCommand cmd = new SqlCommand(strQuery, con))
                        {

                            cmd.Parameters.AddWithValue("@StaffDate", row.Cells["StaffDate"].Value);
                            cmd.Parameters.AddWithValue("@RN", row.Cells["RN"].Value);
                            cmd.Parameters.AddWithValue("@NA", row.Cells["NA"].Value);
                            cmd.Parameters.AddWithValue("@Hour", row.Cells["Hour"].Value);
                            con.Open();
                            cmd.ExecuteNonQuery();

                        }
                    }
                }
                MessageBox.Show("Save data to database successed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.ToString());
            }

        }
        private DataTable GetDataToDT(string cmdStr, string conStr)
        {
            DataTable rDt = new DataTable();
            SqlConnection con = new SqlConnection(conStr);

            try
            {
                con.Open();
                cmd = new SqlCommand(cmdStr, con);
                adp = new SqlDataAdapter(cmd);
                cmdBD = new SqlCommandBuilder(adp);
                sDs = new DataSet();
                adp.Fill(sDs);
                rDt = sDs.Tables[0];
            }
            catch
            {

            }
            finally
            {
                con.Close();
            }

            return rDt;
        }

        private void LoadData()
        {
            dateFrom = dateTimePicker1.Value.Date.ToString(GlobalVar.dateFormate);
            dateTo = dateTimePicker2.Value.Date.ToString(GlobalVar.dateFormate);
            string sql = "SELECT " + GlobalVar.seletedStaff + " FROM ORStaff WHERE " + GlobalVar.convertStaffDate + " >= '" + dateFrom + "' and " + GlobalVar.convertStaffDate + " <= '" + dateTo + "' order by StaffDate ";
            SqlConnection connection = new SqlConnection(GlobalVar.con);
            connection.Open();
            cmd = new SqlCommand(sql, connection);
            adp = new SqlDataAdapter(cmd);
            cmdBD = new SqlCommandBuilder(adp);
            sDs = new DataSet();
            adp.Fill(sDs, "Stores");
            sDt = sDs.Tables["Stores"];
            connection.Close();
            dataGridView1.DataSource = sDs.Tables["Stores"];
            dataGridView1.ReadOnly = true;
            save_btn.Enabled = false;
            new_btn.Enabled = false;
            this.dataGridView1.Columns["Id"].Visible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            if (dataGridView1.Rows.Count > 0)
            {
                delete_btn.Enabled = true;
                new_btn.Enabled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void save_btn_Click(object sender, EventArgs e)
        {
            adp.Update(sDt);
            MessageBox.Show("Save Data successed.");
        }

        private void new_btn_Click(object sender, EventArgs e)
        {
            dataGridView1.ReadOnly = false;
            save_btn.Enabled = true;
            new_btn.Enabled = false;
            delete_btn.Enabled = false;
        }

        private void delete_btn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to delete data from gridview ?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                //adp.Update(sDt);
                string strCmd = "delete " + GlobalVar.staffTB + " Where " + GlobalVar.convertStaffDate + " >= '" + dateFrom + "' and " + GlobalVar.convertStaffDate + " <= '" + dateTo + "' ";
                if (executeQuery(strCmd))
                {
                    LoadData();
                    MessageBox.Show("Delete data successed.");

                }
            }
        }

        void LoadEnableButton()
        {
            delete_btn.Enabled = false;
            button2.Enabled = false;
            new_btn.Enabled = false;
            save_btn.Enabled = false;
            button3.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadEnableButton();
        }

        private bool executeQuery(string strCmd)
        {
            bool status = false;
            try
            {
                using (SqlConnection con = new SqlConnection(GlobalVar.con))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(strCmd, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                status = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
            return status;
        }

    }
}
