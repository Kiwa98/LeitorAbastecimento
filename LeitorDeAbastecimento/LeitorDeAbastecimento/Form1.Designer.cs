using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;


using Newtonsoft.Json;
using System;

using System.Windows.Forms;
using ZXing;
using ZXing.QrCode;

namespace LeitorDeAbastecimento
{
    public partial class Form1 : Form
    {
        private string connectionString = "Server=localhost;Database=abastecimento;Uid=root;Pwd=17119807;";
        private Timer timer;

      
        private void InitializeTimer()
        {
            timer = new Timer();
            timer.Interval = 5000; // Intervalo de 5 segundos (5000 milissegundos)
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.textBoxCodigoManual = new System.Windows.Forms.TextBox();
            this.labelCodigoManual = new System.Windows.Forms.Label();
            this.buttonProcessarManual = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();

            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true; // Torna o DataGridView apenas para leitura
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.Size = new System.Drawing.Size(776, 300);
            this.dataGridView1.TabIndex = 0;

            // 
            // textBoxCodigoManual
            // 
            this.textBoxCodigoManual.Location = new System.Drawing.Point(12, 340);
            this.textBoxCodigoManual.Name = "textBoxCodigoManual";
            this.textBoxCodigoManual.Size = new System.Drawing.Size(500, 23);
            this.textBoxCodigoManual.TabIndex = 1;

            // 
            // labelCodigoManual
            // 
            this.labelCodigoManual.AutoSize = true;
            this.labelCodigoManual.Location = new System.Drawing.Point(12, 320);
            this.labelCodigoManual.Name = "labelCodigoManual";
            this.labelCodigoManual.Size = new System.Drawing.Size(54, 15);
            this.labelCodigoManual.TabIndex = 2;
            this.labelCodigoManual.Text = "Código:";

            // 
            // buttonProcessarManual
            // 
            this.buttonProcessarManual.Location = new System.Drawing.Point(518, 340);
            this.buttonProcessarManual.Name = "buttonProcessarManual";
            this.buttonProcessarManual.Size = new System.Drawing.Size(110, 23);
            this.buttonProcessarManual.TabIndex = 3;
            this.buttonProcessarManual.Text = "Processar Código";
            this.buttonProcessarManual.UseVisualStyleBackColor = true;
            this.buttonProcessarManual.Click += new System.EventHandler(this.buttonProcessarManual_Click);

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonProcessarManual);
            this.Controls.Add(this.labelCodigoManual);
            this.Controls.Add(this.textBoxCodigoManual);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form1";
            this.Text = "Leitor de Abastecimento";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox textBoxCodigoManual;
        private System.Windows.Forms.Label labelCodigoManual;
        private System.Windows.Forms.Button buttonProcessarManual;

        private void LoadData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT opNumero, produto, quantidade, data FROM OPs";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar dados do banco de dados: " + ex.Message);
            }
        }

        private void buttonProcessarManual_Click(object sender, EventArgs e)
        {
            string manualInput = textBoxCodigoManual.Text;

            try
            {
                // Decodificar o JSON do input manual
                var qrData = JsonConvert.DeserializeObject<QrData>(manualInput);

                // Atualizar quantidade no banco de dados
                AtualizarQuantidadeNoBanco(qrData.OP, qrData.Quantidade);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao processar código manual: " + ex.Message);
            }
        }

        private void AtualizarQuantidadeNoBanco(string opNumero, int quantidade)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE OPs SET quantidade = quantidade + @quantidade WHERE opNumero = @opNumero";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@opNumero", opNumero);
                        cmd.Parameters.AddWithValue("@quantidade", quantidade);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"Quantidade atualizada com sucesso para a OP: {opNumero}");
                LoadData(); // Atualizar DataGridView após atualização
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar quantidade no banco de dados: " + ex.Message);
            }
        }

        private class QrData
        {
            public string OP { get; set; }
            public int Quantidade { get; set; }
        }
    }
}
