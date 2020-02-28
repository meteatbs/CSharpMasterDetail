using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace MasterDetailsCRUDAPP
{

    public partial class Form1 : Form
    {
        int inEmpID = 0;
        bool isDefaultImage = true;
        string strConnectionString = @"Data Source=DESKTOP-84LNPPD;Initial Catalog=MasterDetailsDB;Integrated Security=True", strPreviousImage = "";
        OpenFileDialog ofd = new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Clear();
        }
        void Clear()
        {
            txtEmpCode.Text = txtEmpCode.Text = "";
            cmbPosition.SelectedIndex = cmbGender.SelectedIndex = 0;
            dtpDOB.Value = DateTime.Now;
            rbtRegular.Checked = true;
            if (dgvEmpCompany.DataSource == null)
                dgvEmpCompany.Rows.Clear();
            else
                dgvEmpCompany.DataSource = (dgvEmpCompany.DataSource as DataTable).Clone();
            inEmpID = 0;
            btnSave.Text = "Save";
            btnDelete.Enabled = false;
            pbxPhoto.Image = Image.FromFile(Application.StartupPath + "\\Images\\photo.jpg");
            isDefaultImage = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PositionComboBoxFill();
            FillEmployeeDataGridView();
            Clear();
        }
        void PositionComboBoxFill()
        {
            using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Position", sqlCon);
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                DataRow topItem = dtbl.NewRow();
                topItem[0] = 0;
                topItem[1] = "-Select-";
                dtbl.Rows.InsertAt(topItem, 0);
                cmbPosition.ValueMember = dgvcmbPosition.ValueMember = "PositionID";
                cmbPosition.DisplayMember = dgvcmbPosition.DisplayMember = "Position";
                cmbPosition.DataSource = dtbl;
                dgvcmbPosition.DataSource = dtbl.Copy();

            }
        }

        private void btnImageClear_Click(object sender, EventArgs e)
        {
            pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\photo.jpg");
            isDefaultImage = true;
            strPreviousImage = "";
        }
        //Details


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateMasterDetailForm())
            {
                int _EmpID = 0;
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlCommand sqlCmd = new SqlCommand("EmployeeAddOrEdit", sqlCon);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("@EmpID", inEmpID);
                    sqlCmd.Parameters.AddWithValue("@EmpCode", txtEmpCode.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@EmpName", txtEmpName.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@PositionID", Convert.ToInt32(cmbPosition.SelectedValue));
                    sqlCmd.Parameters.AddWithValue("@DOB", dtpDOB.Value);
                    sqlCmd.Parameters.AddWithValue("@Gender", cmbGender.Text);
                    sqlCmd.Parameters.AddWithValue("@State", rbtRegular.Checked ? "Regular" : "Contractual");
                    if (isDefaultImage)
                        sqlCmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                    else if (inEmpID > 0 && strPreviousImage != "")
                        sqlCmd.Parameters.AddWithValue("@ImagePath", strPreviousImage);
                    else
                        sqlCmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    _EmpID = Convert.ToInt32(sqlCmd.ExecuteScalar());

                }
                using (SqlConnection sqlCon=new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    foreach (DataGridViewRow dgvRow in dgvEmpCompany.Rows)
                    {
                        if (dgvRow.IsNewRow) break;
                        else
                        {
                            SqlCommand sqlCmd = new SqlCommand("EmpCompanyAddOrEdit", sqlCon);
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            sqlCmd.Parameters.AddWithValue("@EmpCmpID", Convert.ToInt32(dgvRow.Cells["dgvtxtEmpCompID"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvtxtEmpCompID"].Value));
                            sqlCmd.Parameters.AddWithValue("@EmpID", _EmpID);
                            sqlCmd.Parameters.AddWithValue("@CompanyName", dgvRow.Cells["dgvtxtCompanyName"].Value == DBNull.Value ? "" : dgvRow.Cells["dgvtxtCompanyName"].Value);
                            sqlCmd.Parameters.AddWithValue("@PositionID", Convert.ToInt32(dgvRow.Cells["dgvcmbPosition"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvcmbPosition"].Value));
                            sqlCmd.Parameters.AddWithValue("@ExpYear", Convert.ToInt32(dgvRow.Cells["dgvtxtYear"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvtxtYear"].Value));
                            sqlCmd.ExecuteNonQuery();
                        }
                    }
                }
                FillEmployeeDataGridView();
                Clear();
                MessageBox.Show("Submitted Succesfully");

            }
        }
        bool ValidateMasterDetailForm()
        {
            bool _isValid = true;
            if (txtEmpName.Text.Trim() == "")
            {
                MessageBox.Show("Employee name is required");
                _isValid = false;
            }//Add more validation if needed
            return _isValid;
        }
        string SaveImage(string _imagePath)
        {
            string _fileName = Path.GetFileNameWithoutExtension(_imagePath);
            string _extension = Path.GetExtension(_imagePath);
            //Shorten image name
            _fileName = _fileName.Length <= 15 ? _fileName : _fileName.Substring(0, 15);
            _fileName = _fileName + DateTime.Now.ToString("yymmssfff") + _extension;
            pbxPhoto.Image.Save(Application.StartupPath + "\\Images\\" + _fileName);
            return _fileName;
        }

        private void btnImageBrowse_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Images(.jpg,.png)|*.;*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pbxPhoto.Image = new Bitmap(ofd.FileName);
                isDefaultImage = false;
                strPreviousImage = "";
            }

        }

        private void tabControl_DoubleClick(object sender, EventArgs e)
        {

        }

        private void dgvEmployee_DoubleClick(object sender, EventArgs e)
        {
            if (dgvEmployee.CurrentRow.Index != -1)
            {
                DataGridViewRow _dgvCurrentRow = dgvEmployee.CurrentRow;
                inEmpID = Convert.ToInt32(_dgvCurrentRow.Cells[0].Value);
                using(SqlConnection sqlCon=new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("EmployeeViewByID", sqlCon);
                    sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sqlDa.SelectCommand.Parameters.AddWithValue("@EmpID", inEmpID);
                    DataSet ds = new DataSet();
                    sqlDa.Fill(ds);

                    //Master fill
                    DataRow dr = ds.Tables[0].Rows[0];
                    txtEmpCode.Text = dr["EmpCode"].ToString();
                    txtEmpName.Text = dr["EmpName"].ToString();
                    cmbPosition.SelectedValue = Convert.ToInt32(dr["PositionID"].ToString());
                    dtpDOB.Value = Convert.ToDateTime(dr["DOB"].ToString());
                    cmbGender.Text = dr["Gender"].ToString();
                    if (dr["State"].ToString() == "Regular")
                        rbtRegular.Checked = true;
                    else
                        rbtContractual.Checked = true;
                    if (dr["ImagePath"] == DBNull.Value)
                    {
                        pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\photo.jpg");
                        isDefaultImage = true;
                    }
                    else
                    {
                        pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\"+dr["ImagePath"].ToString());
                        strPreviousImage = dr["ImagePath"].ToString();
                        isDefaultImage = false;
                    }
                    dgvEmpCompany.AutoGenerateColumns = false;
                    dgvEmpCompany.DataSource = ds.Tables[1];
                    btnDelete.Enabled = true;
                    btnSave.Text = "Update";
                    tabControl.SelectedIndex = 0;
                }
            }
        }

        private void dgvEmpCompany_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dgvRow = dgvEmpCompany.CurrentRow;
            if (dgvRow.Cells["dgvtxtEmpCompID"].Value != DBNull.Value)
            {
                if (MessageBox.Show("Are you sure to delete this record?", "Master Detail CRUD", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                    {
                        sqlCon.Open();
                        SqlCommand sqlCmd = new SqlCommand("EmployeeCompanyDelete", sqlCon);
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("@EmpCmpID", Convert.ToInt32(dgvRow.Cells["dgvtxtEmpCompID"].Value));
                        sqlCmd.ExecuteNonQuery();
                    }
                }
                else
                    e.Cancel = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete this record?", "Master Detail CRUD", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlCommand sqlCmd = new SqlCommand("EmpCompanyDelete", sqlCon);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("@EmpID", inEmpID);
                    
                    sqlCmd.ExecuteNonQuery();
                    Clear();
                    FillEmployeeDataGridView();
                    MessageBox.Show("Deleted Succesfully");
                }
            }
        }

        void FillEmployeeDataGridView()
        {
            using(SqlConnection sqlCon=new SqlConnection(strConnectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("EmployeeViewAll", sqlCon);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                dgvEmployee.DataSource = dtbl;
                dgvEmployee.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvEmployee.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvEmployee.Columns[0].Visible = false;


            }
        }
          
    }
}
