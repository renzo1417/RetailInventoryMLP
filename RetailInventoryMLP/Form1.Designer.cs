namespace RetailInventoryMLP
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.GroupBox grpControls;
        private System.Windows.Forms.Button btnLoadData;
        private System.Windows.Forms.Button btnBrowseCsv;
        private System.Windows.Forms.Label lblDataStatus;

        private System.Windows.Forms.Label lblHidden;
        private System.Windows.Forms.TextBox txtHidden;
        private System.Windows.Forms.Label lblLR;
        private System.Windows.Forms.TextBox txtLearningRate;
        private System.Windows.Forms.Label lblEpochs;
        private System.Windows.Forms.TextBox txtEpochs;
        private System.Windows.Forms.Button btnTrainAll;
        private System.Windows.Forms.Button btnTune;

        private System.Windows.Forms.Label lblStock;
        private System.Windows.Forms.TextBox txtStockQty;
        private System.Windows.Forms.Label lblSales;
        private System.Windows.Forms.TextBox txtDailySales;
        private System.Windows.Forms.Label lblExpiry;
        private System.Windows.Forms.TextBox txtDaysExpiry;
        private System.Windows.Forms.Label lblMargin;
        private System.Windows.Forms.TextBox txtProfitMargin;
        private System.Windows.Forms.Button btnPredictItem;

        private System.Windows.Forms.TextBox txtOutBinary;
        private System.Windows.Forms.TextBox txtOutMultiClass;
        private System.Windows.Forms.TextBox txtOutMultiLabel;

        private System.Windows.Forms.GroupBox grpResults;
        private System.Windows.Forms.RadioButton rbMetrics;
        private System.Windows.Forms.RadioButton rbLogs;
        private System.Windows.Forms.RadioButton rbTuning;
        private System.Windows.Forms.RadioButton rbData;
        private System.Windows.Forms.DataGridView dgvResults;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grpControls = new System.Windows.Forms.GroupBox();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.btnBrowseCsv = new System.Windows.Forms.Button();
            this.lblDataStatus = new System.Windows.Forms.Label();

            this.lblHidden = new System.Windows.Forms.Label();
            this.txtHidden = new System.Windows.Forms.TextBox();
            this.lblLR = new System.Windows.Forms.Label();
            this.txtLearningRate = new System.Windows.Forms.TextBox();
            this.lblEpochs = new System.Windows.Forms.Label();
            this.txtEpochs = new System.Windows.Forms.TextBox();
            this.btnTrainAll = new System.Windows.Forms.Button();
            this.btnTune = new System.Windows.Forms.Button();

            this.lblStock = new System.Windows.Forms.Label();
            this.txtStockQty = new System.Windows.Forms.TextBox();
            this.lblSales = new System.Windows.Forms.Label();
            this.txtDailySales = new System.Windows.Forms.TextBox();
            this.lblExpiry = new System.Windows.Forms.Label();
            this.txtDaysExpiry = new System.Windows.Forms.TextBox();
            this.lblMargin = new System.Windows.Forms.Label();
            this.txtProfitMargin = new System.Windows.Forms.TextBox();
            this.btnPredictItem = new System.Windows.Forms.Button();

            this.txtOutBinary = new System.Windows.Forms.TextBox();
            this.txtOutMultiClass = new System.Windows.Forms.TextBox();
            this.txtOutMultiLabel = new System.Windows.Forms.TextBox();

            this.grpResults = new System.Windows.Forms.GroupBox();
            this.rbMetrics = new System.Windows.Forms.RadioButton();
            this.rbLogs = new System.Windows.Forms.RadioButton();
            this.rbTuning = new System.Windows.Forms.RadioButton();
            this.rbData = new System.Windows.Forms.RadioButton();
            this.dgvResults = new System.Windows.Forms.DataGridView();

            this.grpControls.SuspendLayout();
            this.grpResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.SuspendLayout();

            // 
            // grpControls (Left Sidebar - Width 330)
            // 
            this.grpControls.Controls.Add(this.btnLoadData);
            this.grpControls.Controls.Add(this.btnBrowseCsv);
            this.grpControls.Controls.Add(this.lblDataStatus);
            this.grpControls.Controls.Add(this.lblHidden);
            this.grpControls.Controls.Add(this.txtHidden);
            this.grpControls.Controls.Add(this.lblLR);
            this.grpControls.Controls.Add(this.txtLearningRate);
            this.grpControls.Controls.Add(this.lblEpochs);
            this.grpControls.Controls.Add(this.txtEpochs);
            this.grpControls.Controls.Add(this.btnTrainAll);
            this.grpControls.Controls.Add(this.btnTune);
            this.grpControls.Controls.Add(this.lblStock);
            this.grpControls.Controls.Add(this.txtStockQty);
            this.grpControls.Controls.Add(this.lblSales);
            this.grpControls.Controls.Add(this.txtDailySales);
            this.grpControls.Controls.Add(this.lblExpiry);
            this.grpControls.Controls.Add(this.txtDaysExpiry);
            this.grpControls.Controls.Add(this.lblMargin);
            this.grpControls.Controls.Add(this.txtProfitMargin);
            this.grpControls.Controls.Add(this.btnPredictItem);
            this.grpControls.Controls.Add(this.txtOutBinary);
            this.grpControls.Controls.Add(this.txtOutMultiClass);
            this.grpControls.Controls.Add(this.txtOutMultiLabel);
            this.grpControls.Location = new System.Drawing.Point(14, 12);
            this.grpControls.Size = new System.Drawing.Size(326, 685);
            this.grpControls.TabStop = false;
            this.grpControls.Text = "Control Panel";

            // 1. Dataset Buttons (Default vs Browse)
            this.btnLoadData.Location = new System.Drawing.Point(16, 26);
            this.btnLoadData.Size = new System.Drawing.Size(142, 32);
            this.btnLoadData.Text = "Load Default CSV";
            this.btnLoadData.UseVisualStyleBackColor = true;
            this.btnLoadData.Click += new System.EventHandler(this.btnLoadData_Click);

            this.btnBrowseCsv.Location = new System.Drawing.Point(168, 26);
            this.btnBrowseCsv.Size = new System.Drawing.Size(142, 32);
            this.btnBrowseCsv.Text = "Browse Custom CSV";
            this.btnBrowseCsv.UseVisualStyleBackColor = true;
            this.btnBrowseCsv.Click += new System.EventHandler(this.btnBrowseCsv_Click);

            this.lblDataStatus.AutoSize = true;
            this.lblDataStatus.Location = new System.Drawing.Point(16, 66);
            this.lblDataStatus.Text = "Dataset: Not loaded.";

            // Hyperparameters
            this.lblHidden.AutoSize = true;
            this.lblHidden.Location = new System.Drawing.Point(16, 102);
            this.lblHidden.Text = "Hidden Neurons:";
            this.txtHidden.Location = new System.Drawing.Point(174, 99);
            this.txtHidden.Size = new System.Drawing.Size(136, 22);
            this.txtHidden.Text = "8";

            this.lblLR.AutoSize = true;
            this.lblLR.Location = new System.Drawing.Point(16, 132);
            this.lblLR.Text = "Learning Rate:";
            this.txtLearningRate.Location = new System.Drawing.Point(174, 129);
            this.txtLearningRate.Size = new System.Drawing.Size(136, 22);
            this.txtLearningRate.Text = "0.15";

            this.lblEpochs.AutoSize = true;
            this.lblEpochs.Location = new System.Drawing.Point(16, 162);
            this.lblEpochs.Text = "Epochs:";
            this.txtEpochs.Location = new System.Drawing.Point(174, 159);
            this.txtEpochs.Size = new System.Drawing.Size(136, 22);
            this.txtEpochs.Text = "1000";

            this.btnTrainAll.Location = new System.Drawing.Point(16, 193);
            this.btnTrainAll.Size = new System.Drawing.Size(294, 35);
            this.btnTrainAll.Text = "2. Train & Evaluate Models";
            this.btnTrainAll.UseVisualStyleBackColor = true;
            this.btnTrainAll.Click += new System.EventHandler(this.btnTrainAll_Click);

            this.btnTune.Location = new System.Drawing.Point(16, 234);
            this.btnTune.Size = new System.Drawing.Size(294, 32);
            this.btnTune.Text = "3. Run Hyperparameter Tests";
            this.btnTune.UseVisualStyleBackColor = true;
            this.btnTune.Click += new System.EventHandler(this.btnTune_Click);

            // Single Item Inputs
            this.lblStock.AutoSize = true;
            this.lblStock.Location = new System.Drawing.Point(16, 288);
            this.lblStock.Text = "Stock Quantity:";
            this.txtStockQty.Location = new System.Drawing.Point(174, 285);
            this.txtStockQty.Size = new System.Drawing.Size(136, 22);
            this.txtStockQty.Text = "25";

            this.lblSales.AutoSize = true;
            this.lblSales.Location = new System.Drawing.Point(16, 318);
            this.lblSales.Text = "Daily Sales:";
            this.txtDailySales.Location = new System.Drawing.Point(174, 315);
            this.txtDailySales.Size = new System.Drawing.Size(136, 22);
            this.txtDailySales.Text = "30";

            this.lblExpiry.AutoSize = true;
            this.lblExpiry.Location = new System.Drawing.Point(16, 348);
            this.lblExpiry.Text = "Days to Expiry:";
            this.txtDaysExpiry.Location = new System.Drawing.Point(174, 345);
            this.txtDaysExpiry.Size = new System.Drawing.Size(136, 22);
            this.txtDaysExpiry.Text = "45";

            this.lblMargin.AutoSize = true;
            this.lblMargin.Location = new System.Drawing.Point(16, 378);
            this.lblMargin.Text = "Profit Margin %:";
            this.txtProfitMargin.Location = new System.Drawing.Point(174, 375);
            this.txtProfitMargin.Size = new System.Drawing.Size(136, 22);
            this.txtProfitMargin.Text = "35";

            this.btnPredictItem.Location = new System.Drawing.Point(16, 409);
            this.btnPredictItem.Size = new System.Drawing.Size(294, 34);
            this.btnPredictItem.Text = "Test Single Item Prediction";
            this.btnPredictItem.UseVisualStyleBackColor = true;
            this.btnPredictItem.Click += new System.EventHandler(this.btnPredictItem_Click);

            // Live Results Boxes
            this.txtOutBinary.Location = new System.Drawing.Point(16, 455);
            this.txtOutBinary.ReadOnly = true;
            this.txtOutBinary.Size = new System.Drawing.Size(294, 22);
            this.txtOutBinary.Text = "Binary: (Not tested)";

            this.txtOutMultiClass.Location = new System.Drawing.Point(16, 485);
            this.txtOutMultiClass.ReadOnly = true;
            this.txtOutMultiClass.Size = new System.Drawing.Size(294, 22);
            this.txtOutMultiClass.Text = "Multi-Class: (Not tested)";

            this.txtOutMultiLabel.Location = new System.Drawing.Point(16, 515);
            this.txtOutMultiLabel.ReadOnly = true;
            this.txtOutMultiLabel.Size = new System.Drawing.Size(294, 22);
            this.txtOutMultiLabel.Text = "Multi-Label: (Not tested)";

            // 
            // grpResults (Right Side Table Viewer)
            // 
            this.grpResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpResults.Controls.Add(this.rbMetrics);
            this.grpResults.Controls.Add(this.rbLogs);
            this.grpResults.Controls.Add(this.rbTuning);
            this.grpResults.Controls.Add(this.rbData);
            this.grpResults.Controls.Add(this.dgvResults);
            this.grpResults.Location = new System.Drawing.Point(350, 12);
            this.grpResults.Size = new System.Drawing.Size(734, 685);
            this.grpResults.TabStop = false;
            this.grpResults.Text = "View Results & Analysis";

            // Radio Buttons to switch table views cleanly without tabs
            this.rbMetrics.AutoSize = true;
            this.rbMetrics.Checked = true;
            this.rbMetrics.Location = new System.Drawing.Point(20, 28);
            this.rbMetrics.Text = "1. Evaluation Metrics";
            this.rbMetrics.UseVisualStyleBackColor = true;
            this.rbMetrics.CheckedChanged += new System.EventHandler(this.ViewSelector_Changed);

            this.rbLogs.AutoSize = true;
            this.rbLogs.Location = new System.Drawing.Point(180, 28);
            this.rbLogs.Text = "2. Epoch Loss Log";
            this.rbLogs.UseVisualStyleBackColor = true;
            this.rbLogs.CheckedChanged += new System.EventHandler(this.ViewSelector_Changed);

            this.rbTuning.AutoSize = true;
            this.rbTuning.Location = new System.Drawing.Point(330, 28);
            this.rbTuning.Text = "3. Hyperparameter Tests";
            this.rbTuning.UseVisualStyleBackColor = true;
            this.rbTuning.CheckedChanged += new System.EventHandler(this.ViewSelector_Changed);

            this.rbData.AutoSize = true;
            this.rbData.Location = new System.Drawing.Point(510, 28);
            this.rbData.Text = "4. Normalized Dataset";
            this.rbData.UseVisualStyleBackColor = true;
            this.rbData.CheckedChanged += new System.EventHandler(this.ViewSelector_Changed);

            // DataGridView Grid
            this.dgvResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.AllowUserToDeleteRows = false;
            this.dgvResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvResults.BackgroundColor = System.Drawing.Color.White;
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Location = new System.Drawing.Point(16, 62);
            this.dgvResults.ReadOnly = true;
            this.dgvResults.RowHeadersVisible = false;
            this.dgvResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvResults.Size = new System.Drawing.Size(702, 608);

            // 
            // Form1 Window Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1096, 710);
            this.Controls.Add(this.grpControls);
            this.Controls.Add(this.grpResults);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart Retail Inventory Predictor — Custom MLP Neural Network";

            this.grpControls.ResumeLayout(false);
            this.grpControls.PerformLayout();
            this.grpResults.ResumeLayout(false);
            this.grpResults.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
