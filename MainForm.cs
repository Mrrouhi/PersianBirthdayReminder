using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace PersianBirthdayReminder
{
    public partial class MainForm : Form
    {
        private List<Customer> customers = new List<Customer>();
        private string dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PersianBirthdayData.dat");
        private PersianCalendar pc = new PersianCalendar();

        public MainForm()
        {
            InitializeComponent();
            LoadData();
            InitializeUI();
            CheckTodayBirthdays();
        }

        private void InitializeUI()
        {
            // تنظیمات اصلی فرم
            this.Text = "یادآوری تولد مشتریان (تقویم شمسی)";
            this.BackColor = Color.FromArgb(10, 30, 60); // آبی تیره
            this.ForeColor = Color.White;
            this.Font = new Font("IranSans", 10, FontStyle.Regular);
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += MainForm_FormClosing;

            // ایجاد تب کنترل اصلی
            TabControl mainTabControl = new TabControl();
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Appearance = TabAppearance.FlatButtons;
            mainTabControl.ItemSize = new Size(120, 30);
            mainTabControl.SizeMode = TabSizeMode.Fixed;
            mainTabControl.SelectedIndex = 0;
            mainTabControl.BackColor = Color.FromArgb(20, 40, 80);

            // تب ورود اطلاعات
            TabPage inputTab = new TabPage("ورود اطلاعات مشتری");
            inputTab.BackColor = Color.FromArgb(20, 40, 80);
            InitializeInputTab(inputTab);

            // تب نمایش اطلاعات
            TabPage viewTab = new TabPage("لیست مشتریان");
            viewTab.BackColor = Color.FromArgb(20, 40, 80);
            InitializeViewTab(viewTab);

            // تب یادآوری
            TabPage reminderTab = new TabPage("تولدهای امروز");
            reminderTab.BackColor = Color.FromArgb(20, 40, 80);
            InitializeReminderTab(reminderTab);

            mainTabControl.TabPages.Add(inputTab);
            mainTabControl.TabPages.Add(viewTab);
            mainTabControl.TabPages.Add(reminderTab);

            this.Controls.Add(mainTabControl);
        }

        private void InitializeInputTab(TabPage tabPage)
        {
            // عنوان بخش
            Label titleLabel = new Label();
            titleLabel.Text = "ثبت اطلاعات جدید";
            titleLabel.Font = new Font("IranSans", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(100, 150, 255); // رنگ نیلی
            titleLabel.Size = new Size(300, 40);
            titleLabel.Location = new Point(350, 20);
            tabPage.Controls.Add(titleLabel);

            // فیلد نام
            Label nameLabel = new Label();
            nameLabel.Text = "نام کامل مشتری:";
            nameLabel.Location = new Point(700, 80);
            nameLabel.ForeColor = Color.White;
            nameLabel.Font = new Font("IranSans", 10, FontStyle.Bold);
            tabPage.Controls.Add(nameLabel);

            TextBox nameTextBox = new TextBox();
            nameTextBox.Location = new Point(400, 80);
            nameTextBox.Size = new Size(280, 30);
            nameTextBox.BackColor = Color.FromArgb(40, 60, 100);
            nameTextBox.ForeColor = Color.White;
            nameTextBox.BorderStyle = BorderStyle.FixedSingle;
            nameTextBox.Tag = "name";
            tabPage.Controls.Add(nameTextBox);

            // فیلد تلفن
            Label phoneLabel = new Label();
            phoneLabel.Text = "شماره تلفن:";
            phoneLabel.Location = new Point(700, 130);
            phoneLabel.ForeColor = Color.White;
            tabPage.Controls.Add(phoneLabel);

            TextBox phoneTextBox = new TextBox();
            phoneTextBox.Location = new Point(400, 130);
            phoneTextBox.Size = new Size(280, 30);
            phoneTextBox.BackColor = Color.FromArgb(40, 60, 100);
            phoneTextBox.ForeColor = Color.White;
            phoneTextBox.BorderStyle = BorderStyle.FixedSingle;
            phoneTextBox.Tag = "phone";
            tabPage.Controls.Add(phoneTextBox);

            // فیلد تاریخ تولد (شمسی)
            Label birthLabel = new Label();
            birthLabel.Text = "تاریخ تولد (شمسی):";
            birthLabel.Location = new Point(700, 180);
            birthLabel.ForeColor = Color.White;
            birthLabel.Font = new Font("IranSans", 10, FontStyle.Bold);
            tabPage.Controls.Add(birthLabel);

            TextBox birthTextBox = new TextBox();
            birthTextBox.Location = new Point(400, 180);
            birthTextBox.Size = new Size(280, 30);
            birthTextBox.BackColor = Color.FromArgb(40, 60, 100);
            birthTextBox.ForeColor = Color.White;
            birthTextBox.BorderStyle = BorderStyle.FixedSingle;
            birthTextBox.PlaceholderText = "مثال: 1375/05/15";
            birthTextBox.Tag = "birthdate";
            tabPage.Controls.Add(birthTextBox);

            // فیلد توضیحات
            Label descLabel = new Label();
            descLabel.Text = "توضیحات اضافی:";
            descLabel.Location = new Point(700, 230);
            descLabel.ForeColor = Color.White;
            tabPage.Controls.Add(descLabel);

            TextBox descTextBox = new TextBox();
            descTextBox.Location = new Point(400, 230);
            descTextBox.Size = new Size(280, 100);
            descTextBox.Multiline = true;
            descTextBox.BackColor = Color.FromArgb(40, 60, 100);
            descTextBox.ForeColor = Color.White;
            descTextBox.BorderStyle = BorderStyle.FixedSingle;
            descTextBox.Tag = "description";
            tabPage.Controls.Add(descTextBox);

            // دکمه ذخیره
            Button saveButton = new Button();
            saveButton.Text = "ذخیره اطلاعات";
            saveButton.Location = new Point(450, 350);
            saveButton.Size = new Size(180, 40);
            saveButton.BackColor = Color.FromArgb(30, 70, 120);
            saveButton.ForeColor = Color.White;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.Font = new Font("IranSans", 10, FontStyle.Bold);
            saveButton.Click += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(birthTextBox.Text))
                {
                    MessageBox.Show("لطفاً نام و تاریخ تولد را وارد نمایید", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime birthDate;
                try
                {
                    birthDate = ParsePersianDate(birthTextBox.Text);
                }
                catch
                {
                    MessageBox.Show("تاریخ وارد شده معتبر نیست", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Customer newCustomer = new Customer
                {
                    Name = nameTextBox.Text,
                    Phone = phoneTextBox.Text,
                    BirthDate = birthDate,
                    Description = descTextBox.Text,
                    Congratulated = false
                };

                customers.Add(newCustomer);
                SaveData();

                nameTextBox.Clear();
                phoneTextBox.Clear();
                birthTextBox.Clear();
                descTextBox.Clear();

                MessageBox.Show("اطلاعات مشتری با موفقیت ذخیره شد", "ذخیره شد", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            tabPage.Controls.Add(saveButton);
        }

        private DateTime ParsePersianDate(string persianDate)
        {
            string[] parts = persianDate.Split('/');
            if (parts.Length != 3)
                throw new FormatException("فرمت تاریخ صحیح نیست");

            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            // تبدیل تاریخ شمسی به میلادی
            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

        private string ToPersianDateString(DateTime date)
        {
            return $"{pc.GetYear(date)}/{pc.GetMonth(date):00}/{pc.GetDayOfMonth(date):00}";
        }

        private void InitializeViewTab(TabPage tabPage)
        {
            // عنوان بخش
            Label titleLabel = new Label();
            titleLabel.Text = "لیست تمام مشتریان";
            titleLabel.Font = new Font("IranSans", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(100, 150, 255);
            titleLabel.Size = new Size(300, 40);
            titleLabel.Location = new Point(350, 20);
            tabPage.Controls.Add(titleLabel);

            // دیتا گرید ویو
            DataGridView dataGridView = new DataGridView();
            dataGridView.Location = new Point(50, 80);
            dataGridView.Size = new Size(900, 450);
            dataGridView.BackgroundColor = Color.FromArgb(20, 40, 80);
            dataGridView.BorderStyle = BorderStyle.FixedSingle;
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 60, 100);
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("IranSans", 10, FontStyle.Bold);
            dataGridView.DefaultCellStyle.BackColor = Color.FromArgb(40, 70, 110);
            dataGridView.DefaultCellStyle.ForeColor = Color.White;
            dataGridView.DefaultCellStyle.Font = new Font("IranSans", 9);
            dataGridView.RowHeadersVisible = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.Tag = "customersGrid";
            dataGridView.CellDoubleClick += CustomersGrid_CellDoubleClick;

            // ستون‌ها
            dataGridView.Columns.Add("Name", "نام مشتری");
            dataGridView.Columns.Add("Phone", "تلفن");
            dataGridView.Columns.Add("BirthDate", "تاریخ تولد");
            dataGridView.Columns.Add("Description", "توضیحات");
            dataGridView.Columns.Add("Edit", "ویرایش");
            dataGridView.Columns.Add("Delete", "حذف");

            // دکمه‌های ویرایش و حذف
            DataGridViewButtonColumn editButtonColumn = new DataGridViewButtonColumn();
            editButtonColumn.Name = "Edit";
            editButtonColumn.Text = "ویرایش";
            editButtonColumn.UseColumnTextForButtonValue = true;
            dataGridView.Columns[4] = editButtonColumn;

            DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn();
            deleteButtonColumn.Name = "Delete";
            deleteButtonColumn.Text = "حذف";
            deleteButtonColumn.UseColumnTextForButtonValue = true;
            dataGridView.Columns[5] = deleteButtonColumn;

            tabPage.Controls.Add(dataGridView);

            // دکمه بارگذاری مجدد
            Button refreshButton = new Button();
            refreshButton.Text = "بارگذاری مجدد لیست";
            refreshButton.Location = new Point(400, 550);
            refreshButton.Size = new Size(200, 40);
            refreshButton.BackColor = Color.FromArgb(30, 70, 120);
            refreshButton.ForeColor = Color.White;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.Font = new Font("IranSans", 10, FontStyle.Bold);
            refreshButton.Click += (sender, e) => RefreshCustomersGrid(dataGridView);
            tabPage.Controls.Add(refreshButton);

            RefreshCustomersGrid(dataGridView);
        }

        private void RefreshCustomersGrid(DataGridView dataGridView)
        {
            dataGridView.Rows.Clear();
            foreach (Customer customer in customers)
            {
                dataGridView.Rows.Add(
                    customer.Name,
                    customer.Phone,
                    ToPersianDateString(customer.BirthDate),
                    customer.Description
                );
            }
        }

        private void CustomersGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridView grid = (DataGridView)sender;
            if (e.ColumnIndex == grid.Columns["Edit"].Index)
            {
                EditCustomer(e.RowIndex);
            }
            else if (e.ColumnIndex == grid.Columns["Delete"].Index)
            {
                DeleteCustomer(e.RowIndex);
            }
        }

        private void EditCustomer(int index)
        {
            if (index < 0 || index >= customers.Count) return;

            Customer customer = customers[index];

            Form editForm = new Form();
            editForm.Text = "ویرایش مشتری";
            editForm.Size = new Size(500, 400);
            editForm.StartPosition = FormStartPosition.CenterScreen;
            editForm.BackColor = Color.FromArgb(20, 40, 80);
            editForm.ForeColor = Color.White;
            editForm.Font = new Font("IranSans", 10);

            // فیلد نام
            Label nameLabel = new Label();
            nameLabel.Text = "نام کامل:";
            nameLabel.Location = new Point(350, 30);
            editForm.Controls.Add(nameLabel);

            TextBox nameTextBox = new TextBox();
            nameTextBox.Text = customer.Name;
            nameTextBox.Location = new Point(150, 30);
            nameTextBox.Size = new Size(180, 30);
            nameTextBox.BackColor = Color.FromArgb(40, 60, 100);
            nameTextBox.ForeColor = Color.White;
            editForm.Controls.Add(nameTextBox);

            // فیلد تلفن
            Label phoneLabel = new Label();
            phoneLabel.Text = "تلفن:";
            phoneLabel.Location = new Point(350, 80);
            editForm.Controls.Add(phoneLabel);

            TextBox phoneTextBox = new TextBox();
            phoneTextBox.Text = customer.Phone;
            phoneTextBox.Location = new Point(150, 80);
            phoneTextBox.Size = new Size(180, 30);
            phoneTextBox.BackColor = Color.FromArgb(40, 60, 100);
            phoneTextBox.ForeColor = Color.White;
            editForm.Controls.Add(phoneTextBox);

            // فیلد تاریخ تولد
            Label birthLabel = new Label();
            birthLabel.Text = "تاریخ تولد (شمسی):";
            birthLabel.Location = new Point(350, 130);
            editForm.Controls.Add(birthLabel);

            TextBox birthTextBox = new TextBox();
            birthTextBox.Text = ToPersianDateString(customer.BirthDate);
            birthTextBox.Location = new Point(150, 130);
            birthTextBox.Size = new Size(180, 30);
            birthTextBox.BackColor = Color.FromArgb(40, 60, 100);
            birthTextBox.ForeColor = Color.White;
            editForm.Controls.Add(birthTextBox);

            // فیلد توضیحات
            Label descLabel = new Label();
            descLabel.Text = "توضیحات:";
            descLabel.Location = new Point(350, 180);
            editForm.Controls.Add(descLabel);

            TextBox descTextBox = new TextBox();
            descTextBox.Text = customer.Description;
            descTextBox.Location = new Point(150, 180);
            descTextBox.Size = new Size(180, 100);
            descTextBox.Multiline = true;
            descTextBox.BackColor = Color.FromArgb(40, 60, 100);
            descTextBox.ForeColor = Color.White;
            editForm.Controls.Add(descTextBox);

            // دکمه ذخیره
            Button saveButton = new Button();
            saveButton.Text = "ذخیره تغییرات";
            saveButton.Location = new Point(180, 300);
            saveButton.Size = new Size(150, 40);
            saveButton.BackColor = Color.FromArgb(30, 70, 120);
            saveButton.ForeColor = Color.White;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.Click += (sender, e) =>
            {
                try
                {
                    customer.Name = nameTextBox.Text;
                    customer.Phone = phoneTextBox.Text;
                    customer.BirthDate = ParsePersianDate(birthTextBox.Text);
                    customer.Description = descTextBox.Text;

                    SaveData();
                    editForm.DialogResult = DialogResult.OK;
                    editForm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطا در ذخیره تغییرات: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            editForm.Controls.Add(saveButton);

            editForm.ShowDialog();
        }

        private void DeleteCustomer(int index)
        {
            if (index < 0 || index >= customers.Count) return;

            DialogResult result = MessageBox.Show(
                $"آیا از حذف مشتری '{customers[index].Name}' مطمئن هستید؟",
                "تأیید حذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                customers.RemoveAt(index);
                SaveData();

                // پیدا کردن و رفرش کردن گرید
                foreach (Control control in this.Controls)
                {
                    if (control is TabControl tabControl)
                    {
                        foreach (TabPage tabPage in tabControl.TabPages)
                        {
                            if (tabPage.Text == "لیست مشتریان")
                            {
                                foreach (Control tabControl2 in tabPage.Controls)
                                {
                                    if (tabControl2 is DataGridView dataGridView && (string)dataGridView.Tag == "customersGrid")
                                    {
                                        RefreshCustomersGrid(dataGridView);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitializeReminderTab(TabPage tabPage)
        {
            // عنوان بخش
            Label titleLabel = new Label();
            titleLabel.Text = "تولدهای امروز";
            titleLabel.Font = new Font("IranSans", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(100, 150, 255);
            titleLabel.Size = new Size(300, 40);
            titleLabel.Location = new Point(350, 20);
            tabPage.Controls.Add(titleLabel);

            // لیست تولدها
            ListBox birthdaysList = new ListBox();
            birthdaysList.Location = new Point(50, 80);
            birthdaysList.Size = new Size(900, 400);
            birthdaysList.BackColor = Color.FromArgb(40, 70, 110);
            birthdaysList.ForeColor = Color.White;
            birthdaysList.Font = new Font("IranSans", 10);
            birthdaysList.BorderStyle = BorderStyle.FixedSingle;
            birthdaysList.Tag = "birthdaysList";
            tabPage.Controls.Add(birthdaysList);

            // دکمه تبریک گفته شد
            Button congratulatedButton = new Button();
            congratulatedButton.Text = "تبریک گفته شد";
            congratulatedButton.Location = new Point(400, 500);
            congratulatedButton.Size = new Size(200, 40);
            congratulatedButton.BackColor = Color.FromArgb(30, 100, 30);
            congratulatedButton.ForeColor = Color.White;
            congratulatedButton.FlatStyle = FlatStyle.Flat;
            congratulatedButton.Font = new Font("IranSans", 10, FontStyle.Bold);
            congratulatedButton.Click += (sender, e) =>
            {
                if (birthdaysList.SelectedIndex >= 0 && birthdaysList.SelectedIndex < customers.Count)
                {
                    customers[birthdaysList.SelectedIndex].Congratulated = true;
                    SaveData();
                    CheckTodayBirthdays();
                }
            };
            tabPage.Controls.Add(congratulatedButton);

            // دکمه بارگذاری مجدد
            Button refreshButton = new Button();
            refreshButton.Text = "بررسی مجدد";
            refreshButton.Location = new Point(400, 550);
            refreshButton.Size = new Size(200, 40);
            refreshButton.BackColor = Color.FromArgb(30, 70, 120);
            refreshButton.ForeColor = Color.White;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.Font = new Font("IranSans", 10, FontStyle.Bold);
            refreshButton.Click += (sender, e) => CheckTodayBirthdays();
            tabPage.Controls.Add(refreshButton);
        }

        private void CheckTodayBirthdays()
        {
            DateTime today = DateTime.Now;
            string todayPersian = ToPersianDateString(today).Substring(5); // فقط ماه و روز

            List<Customer> todayBirthdays = new List<Customer>();

            foreach (Customer customer in customers)
            {
                string customerBirthPersian = ToPersianDateString(customer.BirthDate).Substring(5);
                if (customerBirthPersian == todayPersian && !customer.Congratulated)
                {
                    todayBirthdays.Add(customer);
                }
            }

            // پیدا کردن لیست باکس و آپدیت آن
            foreach (Control control in this.Controls)
            {
                if (control is TabControl tabControl)
                {
                    foreach (TabPage tabPage in tabControl.TabPages)
                    {
                        if (tabPage.Text == "تولدهای امروز")
                        {
                            foreach (Control tabControl2 in tabPage.Controls)
                            {
                                if (tabControl2 is ListBox listBox && (string)listBox.Tag == "birthdaysList")
                                {
                                    listBox.Items.Clear();
                                    if (todayBirthdays.Count == 0)
                                    {
                                        listBox.Items.Add("امروز هیچ تولدی برای یادآوری وجود ندارد");
                                    }
                                    else
                                    {
                                        foreach (Customer customer in todayBirthdays)
                                        {
                                            listBox.Items.Add($"{customer.Name} - تلفن: {customer.Phone} - توضیحات: {customer.Description}");
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadData()
        {
            if (File.Exists(dataFilePath))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (FileStream fs = new FileStream(dataFilePath, FileMode.Open))
                    {
                        customers = (List<Customer>)formatter.Deserialize(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطا در بارگذاری داده‌ها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveData()
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream(dataFilePath, FileMode.Create))
                {
                    formatter.Serialize(fs, customers);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره داده‌ها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveData();
        }

        [Serializable]
        public class Customer
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public DateTime BirthDate { get; set; }
            public string Description { get; set; }
            public bool Congratulated { get; set; }
        }
    }
}