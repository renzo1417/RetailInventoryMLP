using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetailInventoryMLP
{
    public partial class Form1 : Form
    {
        private readonly InventoryEngine engine = new InventoryEngine();
        private const string CsvFilePath = "inventory_data.csv";
        private const string LogFilePath = "training_log.csv";
        private string currentCsvPath = CsvFilePath;

        public Form1()
        {
            InitializeComponent();

            // Default view on startup
            dgvResults.DataSource = engine.MetricsTable;
        }

        // 1a. Load Default CSV Button
        private void btnLoadData_Click(object sender, EventArgs e)
        {
            try
            {
                currentCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CsvFilePath);
                engine.LoadAndPreprocessCsv(currentCsvPath);
                lblDataStatus.Text = string.Format("Default Loaded: {0} items ({1} train, {2} test)",
                    engine.AllRows.Count, engine.TrainSet.Count, engine.TestSet.Count);
                rbData.Checked = true;
                dgvResults.DataSource = engine.DataPreviewTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading default dataset: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 1b. Browse Custom CSV Button (Allows testing independent datasets)
        private void btnBrowseCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Dataset CSV";
                ofd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        currentCsvPath = ofd.FileName;
                        engine.LoadAndPreprocessCsv(currentCsvPath);
                        lblDataStatus.Text = string.Format("Loaded {0}: {1} items ({2} train, {3} test)",
                            Path.GetFileName(currentCsvPath), engine.AllRows.Count, engine.TrainSet.Count, engine.TestSet.Count);
                        rbData.Checked = true;
                        dgvResults.DataSource = engine.DataPreviewTable;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading selected dataset: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // 2. Train Models Button (Asynchronous to keep UI fully responsive)
        private async void btnTrainAll_Click(object sender, EventArgs e)
        {
            if (!engine.IsDataLoaded)
            {
                currentCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CsvFilePath);
                engine.LoadAndPreprocessCsv(currentCsvPath);
                lblDataStatus.Text = string.Format("Dataset Loaded: {0} items ({1} train, {2} test)",
                    engine.AllRows.Count, engine.TrainSet.Count, engine.TestSet.Count);
            }

            int hidden, epochs;
            double lr;
            if (!int.TryParse(txtHidden.Text, out hidden) || hidden < 1 ||
                !double.TryParse(txtLearningRate.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out lr) || lr <= 0 ||
                !int.TryParse(txtEpochs.Text, out epochs) || epochs < 1)
            {
                MessageBox.Show("Please enter valid positive numbers for Hidden Neurons, Learning Rate, and Epochs.",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            Cursor = Cursors.WaitCursor;
            lblDataStatus.Text = "Training models in background (please wait)...";
            dgvResults.DataSource = null; // Detach to avoid cross-thread binding conflicts

            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFilePath);
                await Task.Run(() => engine.TrainAndEvaluateAll(hidden, lr, epochs, logPath));

                lblDataStatus.Text = string.Format("Trained {0} epochs (H={1}, LR={2:F2})", epochs, hidden, lr);
                rbMetrics.Checked = true;
                dgvResults.DataSource = engine.MetricsTable;

                // Automatically test single item predictor
                btnPredictItem_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Training error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
                Cursor = Cursors.Default;
            }
        }

        // 3. Hyperparameter Tuning Button (Asynchronous)
        private async void btnTune_Click(object sender, EventArgs e)
        {
            if (!engine.IsDataLoaded)
            {
                currentCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CsvFilePath);
                engine.LoadAndPreprocessCsv(currentCsvPath);
                lblDataStatus.Text = string.Format("Dataset Loaded: {0} items ({1} train, {2} test)",
                    engine.AllRows.Count, engine.TrainSet.Count, engine.TestSet.Count);
            }

            SetControlsEnabled(false);
            Cursor = Cursors.WaitCursor;
            lblDataStatus.Text = "Running hyperparameter tests (3 trials)...";
            dgvResults.DataSource = null;

            try
            {
                await Task.Run(() => engine.RunHyperparameterTuning());
                lblDataStatus.Text = "Hyperparameter tests completed.";
                rbTuning.Checked = true;
                dgvResults.DataSource = engine.TuningTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tuning error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
                Cursor = Cursors.Default;
            }
        }

        // 4. Test Single Item Predictor Button
        private void btnPredictItem_Click(object sender, EventArgs e)
        {
            if (!engine.AreModelsTrained)
            {
                MessageBox.Show("Please click '2. Train & Evaluate Models' first.",
                    "Models Not Trained", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double stock, sales, expiry, margin;
            if (!double.TryParse(txtStockQty.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out stock) || stock < 0 ||
                !double.TryParse(txtDailySales.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out sales) || sales < 0 ||
                !double.TryParse(txtDaysExpiry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out expiry) || expiry < 0 ||
                !double.TryParse(txtProfitMargin.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out margin) || margin < 0)
            {
                MessageBox.Show("Please enter valid non-negative numeric values for all 4 item fields.",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string binRes, mcRes, mlRes;
            engine.PredictSingleItem(stock, sales, expiry, margin, out binRes, out mcRes, out mlRes);

            txtOutBinary.Text = "Binary: " + binRes;
            txtOutMultiClass.Text = "Multi-Class: " + mcRes;
            txtOutMultiLabel.Text = "Multi-Label: " + mlRes;
        }

        // Clean Radio Button Switcher for the Right-Side Table
        private void ViewSelector_Changed(object sender, EventArgs e)
        {
            if (rbMetrics.Checked) dgvResults.DataSource = engine.MetricsTable;
            else if (rbLogs.Checked) dgvResults.DataSource = engine.EpochLogTable;
            else if (rbTuning.Checked) dgvResults.DataSource = engine.TuningTable;
            else if (rbData.Checked) dgvResults.DataSource = engine.DataPreviewTable;
        }

        // Helper to disable controls during long-running tasks
        private void SetControlsEnabled(bool enabled)
        {
            btnLoadData.Enabled = enabled;
            btnBrowseCsv.Enabled = enabled;
            btnTrainAll.Enabled = enabled;
            btnTune.Enabled = enabled;
            btnPredictItem.Enabled = enabled;
            txtHidden.Enabled = enabled;
            txtLearningRate.Enabled = enabled;
            txtEpochs.Enabled = enabled;
        }
    }
}
